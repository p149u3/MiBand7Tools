using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mi7shared {
    class ImageData {
        public byte[] _imageData;

        /// <summary>Запоминаем оставшуюся часть файла, после карты цветов</summary>
        /// <param name="streamBuffer">Набор байт файла</param>
        /// <param name="offset">Смещение от начала файла до конца карты цветов</param>
        public ImageData(byte[] streamBuffer, int offset) {
            int lenght = streamBuffer.Length - offset;
            _imageData = new byte[lenght];
            Array.Copy(streamBuffer, offset, _imageData, 0, lenght);
        }
    }
}
