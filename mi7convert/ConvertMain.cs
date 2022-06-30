using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mi7shared;

namespace mi7convert {
    class ConvertMain {
        public static async Task Process(string[] args) {
            RootCommand rootCommand = new RootCommand(
                description: "Converts an image file from one format to another.");

            var packCommand = new Command("pack", "Pack to MiBand7 format");
            var fileArgument = new Argument<string[]>("files", "List of files or directories.");
            packCommand.AddArgument(fileArgument);
            rootCommand.Add(packCommand);
            var unPackCommand = new Command("unpack", "Unpack to png");
            var file2Argument = new Argument<string[]>("files", "List of files or directories.");
            unPackCommand.AddArgument(fileArgument);
            rootCommand.Add(unPackCommand);
            packCommand.SetHandler((file) => {
                Pack(file);
            }, fileArgument);

            unPackCommand.SetHandler((file) => {
                UnPack(file);
            }, fileArgument);

            await rootCommand.InvokeAsync(args);
        }
        private static void Pack(string[] paths) {
            if (paths.Length > 0) {
                Converter converter = new Converter();
                foreach (string path in paths) {
                    if (Directory.Exists(path)) {
                        string[] pngs = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
                        foreach (string png in pngs) {
                            try {
                                Console.WriteLine("Packing " + png);
                                converter.PngToTga(png);
                            }
                            catch (Exception e) {
                                Console.WriteLine("Cannot pack " + png + " : " + e.Message);
                            }
                        }
                    } else if (File.Exists(path)) {
                        try {
                            Console.WriteLine("Packing " + path);
                            converter.PngToTga(path);
                        }
                        catch (Exception e) {
                            Console.WriteLine("Cannot pack " + path + " : " + e.Message);
                        }
                    }
                }
            }
        }

        private static void UnPack(string[] paths) {
            if (paths.Length > 0) {
                Converter converter = new Converter();
                foreach (string path in paths) {
                    if (Directory.Exists(path)) {
                        string[] pngs = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
                        foreach (string png in pngs) {
                            try {
                                Console.WriteLine("Unpacking " + png);
                                converter.TgaToPng(png);
                            }
                            catch (Exception e) {
                                Console.WriteLine("Cannot unpack " + png + " : " + e.Message);
                            }
                        }
                    } else if (File.Exists(path)) {
                        try {
                            Console.WriteLine("Unpacking " + path);
                            converter.TgaToPng(path);
                        }
                        catch (Exception e) {
                            Console.WriteLine("Cannot unpack " + path + " : " + e.Message);
                        }

                    }
                }
            }
        }
    }
}
