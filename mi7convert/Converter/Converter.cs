using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace mi7shared {
    public class Converter {
        List<Color> colorMapList = new List<Color>();
        private int _imageWidth = 0;

        public string TgaToPng(string file) {
            string path = "";
            if (File.Exists(file)) {
                try {
                    ImageMagick.MagickImage image;
                    string fileNameFull = file;
                    string fileName = Path.GetFileNameWithoutExtension(fileNameFull);
                    path = Path.GetDirectoryName(fileNameFull);
                    int RealWidth = -1;
                    using (var fileStream = File.OpenRead(fileNameFull)) {
                        byte[] streamBuffer = new byte[fileStream.Length];
                        fileStream.Read(streamBuffer, 0, (int)fileStream.Length);

                        Header header = new Header(streamBuffer);
                        ImageDescription imageDescription = new ImageDescription(streamBuffer, header.GetImageIDLength());
                        RealWidth = imageDescription.GetRealWidth();
                    }

                    using (var fileStream = File.OpenRead(fileNameFull)) {
                        image = new ImageMagick.MagickImage(fileStream, ImageMagick.MagickFormat.Tga);
                    }

                    image.Format = ImageMagick.MagickFormat.Png32;
                    if (RealWidth > 0 && RealWidth != image.Width) {
                        int height = image.Height;
                        image = (ImageMagick.MagickImage)image.Clone(RealWidth, height);
                    }

                    /*ImageMagick.IMagickImage Blue = image.Separate(ImageMagick.Channels.Blue).First();
                    ImageMagick.IMagickImage Red = image.Separate(ImageMagick.Channels.Red).First();
                    image.Composite(Red, ImageMagick.CompositeOperator.Replace, ImageMagick.Channels.Blue);
                    image.Composite(Blue, ImageMagick.CompositeOperator.Replace, ImageMagick.Channels.Red);*/

                    //string newFileName = Path.Combine(path, fileName + ".png");
                    image.Write(fileNameFull);
                }
                catch (Exception exp) {
                    Console.WriteLine(exp.Message);
                    throw;
                }

            }
            return path;
        }

        public bool PngToTga(string fileNameFull) {
            if (File.Exists(fileNameFull)) {
                colorMapList.Clear();
                try {
                    //string fileNameFull = openFileDialog.FileName;
                    string fileName = Path.GetFileNameWithoutExtension(fileNameFull);
                    string path = Path.GetDirectoryName(fileNameFull);
                    //fileName = Path.Combine(path, fileName);
                    ImageMagick.MagickImage image;
                    ImageMagick.MagickImage image_temp;

                    using (var fileStream = File.OpenRead(fileNameFull)) {
                        image = new ImageMagick.MagickImage(fileStream);
                    }
                    using (var fileStream = File.OpenRead(fileNameFull)) {
                        //image = new ImageMagick.MagickImage(fileStream);
                        image_temp = new ImageMagick.MagickImage(fileStream);
                    }
                    int newWidth = image.Width;
                    int newHeight = image.Height;
                    this._imageWidth = newWidth;

                    ImageMagick.Pixel pixel = (ImageMagick.Pixel)image.GetPixels().GetPixel(0, 0);
                    //pixel = new ImageMagick.Pixel(0, 0, 4);
                    bool transparent = false;
                    //if (pixel.Channels == 4 && pixel[3] < 256) transparent = true;

                    image.ColorType = ImageMagick.ColorType.Palette;
                    if (image.ColorSpace != ImageMagick.ColorSpace.sRGB) {
                        image = image_temp;
                        //image.ColorSpace = ImageMagick.ColorSpace.sRGB;
                        //ImageMagick.Pixel pixel = image.GetPixels().GetPixel(0, 0);
                        ushort[] p;
                        if (pixel[2] > 256) {
                            if (pixel.Channels == 4) p = new ushort[] { pixel[0], pixel[1], (ushort)(pixel[2] - 256), pixel[3] };
                            else p = new ushort[] { pixel[0], pixel[1], (ushort)(pixel[2] - 256) };
                        } else if (pixel.Channels == 4) {
                            p = new ushort[] { pixel[0], pixel[1], (ushort)(pixel[2] + 256), pixel[3] };
                        } else {
                            p = new ushort[] { pixel[0], pixel[1], (ushort)(pixel[2] + 256) };
                            transparent = true;
                        }
                        image.GetPixels().SetPixel(0, 0, p);
                        pixel = (ImageMagick.Pixel)image.GetPixels().GetPixel(0, 0);
                        image.ColorType = ImageMagick.ColorType.Palette;
                        pixel = (ImageMagick.Pixel)image.GetPixels().GetPixel(0, 0);
                        if (image.ColorSpace != ImageMagick.ColorSpace.sRGB) {
                            Console.WriteLine("Изображение не должно быть монохромным и должно быть в формате 32bit: " + fileNameFull);
                            return false;
                        }
                    }
                    for (int i = 0; i < image.ColormapSize; i++) {
                        //Color tempColor = image.GetColormap(i);
                        //colorMapList.Add(tempColor.ToArgb().ToString());
                        //Color tempColor2 = Color.FromArgb(Int32.Parse(colorMapList[i]));
                        var tempColor = image.GetColormap(i).ToByteArray();
                        //Color tempColor2 = Color.fr
                        colorMapList.Add(Color.FromArgb(tempColor[3], tempColor[0], tempColor[1], tempColor[2]));
                    }
                    if (transparent && colorMapList.Count == 2) {
                        colorMapList[0] = Color.FromArgb(0, colorMapList[0].R, colorMapList[0].G, colorMapList[0].B);
                        colorMapList[1] = Color.FromArgb(0, colorMapList[1].R, colorMapList[1].G, colorMapList[1].B);
                    }
                    string newFileName = Path.Combine(path, fileName + ".tga");
                    image.Settings.ColorType = ImageMagick.ColorType.Palette;
                    image.Write(newFileName, ImageMagick.MagickFormat.Tga);
                    ImageFix(newFileName);
                    return true;
                }
                catch (Exception exp) {
                    Console.WriteLine("Не верный формат исходного файла: " +  exp.Message);
                    throw;
                }
            }
            return false;
        }

        private void ImageFix(string fileNameFull) {
            if (File.Exists(fileNameFull)) {
                try {
                    //string fileNameFull = openFileDialog.FileName;
                    string fileName = Path.GetFileNameWithoutExtension(fileNameFull);
                    string path = Path.GetDirectoryName(fileNameFull);
                    //fileName = Path.Combine(path, fileName);

                    //ImageMagick.MagickImage image = new ImageMagick.MagickImage(fileNameFull, ImageMagick.MagickFormat.Tga);

                    // читаем картинку в массив
                    using (var fileStream = File.OpenRead(fileNameFull)) {
                        byte[] streamBuffer = new byte[fileStream.Length];
                        fileStream.Read(streamBuffer, 0, (int)fileStream.Length);

                        Header header = new Header(streamBuffer);
                        ImageDescription imageDescription = new ImageDescription(streamBuffer, header.GetImageIDLength());

                        int ColorMapCount = header.GetColorMapCount(); // количество цветов в карте
                        byte ColorMapEntrySize = header.GetColorMapEntrySize(); // битность цвета
                        byte ImageIDLength = header.GetImageIDLength(); // длина описания
                        ColorMap ColorMap = new ColorMap(streamBuffer, ColorMapCount, ColorMapEntrySize, 18 + ImageIDLength);

                        int ColorMapLength = ColorMap._colorMap.Length;
                        ImageData imageData = new ImageData(streamBuffer, 18 + ImageIDLength + ColorMapLength);

                        Footer footer = new Footer();

                        #region fix
                        header.SetImageIDLength(46);
                        imageDescription.SetSize(46, this._imageWidth);
                        //imageDescription.SetSize(46, header.Width);

                        int colorMapCount = ColorMap.ColorMapCount;
                        //if (checkBox_Color256.Checked && !checkBox_32bit.Checked)
                        //{
                        //    colorMapCount = 256;
                        //    header.SetColorMapCount(colorMapCount);
                        //    if (!checkBox_32bit.Checked) ColorMap.SetColorCount(colorMapCount);
                        //}
                        bool argb_brga = true;
                        colorMapCount = 256;
                        header.SetColorMapCount(colorMapCount);
                        byte colorMapEntrySize = 32;

                        ColorMap.RestoreColor(colorMapList);
                        ColorMap.ColorsFix(argb_brga, colorMapCount, colorMapEntrySize);
                        header.SetColorMapEntrySize(32);
                        #endregion

                        int newLength = 18 + header.GetImageIDLength() + ColorMap._colorMap.Length + imageData._imageData.Length;
                        //if (checkBox_Footer.Checked) newLength = newLength + footer._footer.Length;
                        byte[] newTGA = new byte[newLength];

                        header._header.CopyTo(newTGA, 0);
                        int offset = header._header.Length;

                        imageDescription._imageDescription.CopyTo(newTGA, offset);
                        offset = offset + imageDescription._imageDescription.Length;

                        ColorMap._colorMap.CopyTo(newTGA, offset);
                        offset = offset + ColorMap._colorMap.Length;

                        imageData._imageData.CopyTo(newTGA, offset);
                        offset = offset + imageData._imageData.Length;

                        //if (checkBox_Footer.Checked) footer._footer.CopyTo(newTGA, offset);

                        if (newTGA != null && newTGA.Length > 0) {
                            string newFileName = Path.Combine(path, fileName + ".png");

                            using (var fileStreamTGA = File.OpenWrite(newFileName)) {
                                fileStreamTGA.Write(newTGA, 0, newTGA.Length);
                                fileStreamTGA.Flush();
                            }
                        }
                    }

                    try {
                        File.Delete(fileNameFull);
                    }
                    catch (Exception) {
                    }

                }
                catch (Exception exp) {
                    Console.WriteLine("Ошибка открытия файла: " + exp.Message);
                    throw;
                }
            }
        }
    }
}
