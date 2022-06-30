using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mi7shared;

namespace mi7build {
    class BuildMain {
        const string AppJS = "app.js";
        const string AppJson = "app.json";
        const string TempDir = "temp";

        public static async Task Process(string[] args) {
            RootCommand rootCommand = new RootCommand(
                description: "Pack or unpack watchface.");
            var buildCommand = new Command("pack", "Pack to MiBand7 format");
            var dirArgument = new Argument<string>("directory", "Input directory");
            buildCommand.AddArgument(dirArgument);
            var outOption = new Option<string>("--out", "Output file");
            buildCommand.AddOption(outOption);
            buildCommand.SetHandler((dir, outf) => {
                Build(dir, outf);
            }, dirArgument, outOption);
            rootCommand.Add(buildCommand);
            var unPackCommand = new Command("unpack", "Unpack watchface");
            var fileArgument = new Argument<string>("file", "Watchface bin file");
            unPackCommand.AddArgument(fileArgument);
            var outDirOption = new Option<string>("--out", "Output directory");
            unPackCommand.AddOption(outDirOption);
            unPackCommand.SetHandler((file, dir) => {
                UnPack(file, dir);
            }, fileArgument, outDirOption);
            rootCommand.Add(unPackCommand);

            await rootCommand.InvokeAsync(args);
        }

        private static void Build(string directory, string outFile) {
            Console.WriteLine("Start packing...");
            Console.WriteLine("Checking structure...");
            if (!Directory.Exists(directory)) {
                Console.WriteLine("Directory " + directory + " not found!");
                return;
            }
            string appJSPath = Path.Combine(directory, AppJS);
            if (!File.Exists(appJSPath)) {
                Console.WriteLine("No app.js in directory " + directory);
                return;
            }
            string appJsonPath = Path.Combine(directory, AppJson);
            if (!File.Exists(appJsonPath)) {
                Console.WriteLine("No app.json in directory " + directory);
                return;
            }
            string watchfaceJSPath;
            try {
                JObject data = JObject.Parse(File.ReadAllText(appJsonPath));
                JObject module = data.Value<JObject>("module");
                JObject watchface = module.Value<JObject>("watchface");
                watchfaceJSPath = watchface.Value<string>("path");
                if (!watchfaceJSPath.EndsWith(".js"))
                    watchfaceJSPath += ".js";
            } catch (Exception e) {
                Console.WriteLine("Error parsing app.js: " + e.Message);
                return;
            }
            if (watchfaceJSPath.Length == 0 || !File.Exists(Path.Combine(directory, watchfaceJSPath))) {
                Console.WriteLine("Not found watchface js file: " + Path.Combine(directory, watchfaceJSPath));
                return;
            }
            try {
                Console.WriteLine("Copying to temp...");
                string buildDir = CopyToTemp(directory);
                //Console.WriteLine("Minifying js code...");
                //MinifyJS(buildDir);
                Console.WriteLine("Converting images...");
                ConvertToWFFormat(buildDir);
                Console.WriteLine("Zipping files...");
                DirectoryInfo dir = new DirectoryInfo(directory);
                string binFile = outFile?.Length > 0 ? outFile : "./" + dir.Name + ".bin";
                Zip(buildDir, binFile);
                Console.WriteLine("Cleaning up...");
                Cleanup(buildDir);
                Console.WriteLine("Watchface built: " + Path.GetFileName(binFile));
            } catch (Exception e) {
                Console.WriteLine("Error buidling watchface: " + e.Message);
            }
        }

        private static string CopyToTemp(string directory) {
            DirectoryInfo dir = new DirectoryInfo(directory);
            string tempPath = Path.Combine(dir.Parent.FullName, TempDir);
            if (Directory.Exists(tempPath)) {
                CopyHelper.Remove(tempPath);
            }
            CopyHelper.Copy(directory, tempPath);
            return tempPath;
        }

        private static void MinifyJS(string path) {
            var minifier = new Microsoft.Ajax.Utilities.Minifier();
            string[] jss = Directory.GetFiles(path, "*.js", SearchOption.AllDirectories);
            foreach (string js in jss) {
                string content = File.ReadAllText(js);
                string min = minifier.MinifyJavaScript(content);
                File.WriteAllText(js, min);
            }
            /*string[] jsons = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
            foreach (string json in jsons) {
                string content = File.ReadAllText(json);
                var obj = JObject.Parse(content);
                string min = JsonConvert.SerializeObject(obj);
                File.WriteAllText(json, min);
            }*/
        }

        private static void ConvertToWFFormat(string path) {
            Converter cv = new Converter();
            string[] pngs = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
            foreach (string png in pngs) {
                cv.PngToTga(png);
            }
        }
        private static void ConvertFromWFFormat(string path) {
            Converter cv = new Converter();
            string[] pngs = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
            foreach (string png in pngs) {
                cv.TgaToPng(png);
            }
        }

        private static void Zip(string path, string outfile) {
            if (File.Exists(outfile)) {
                File.Delete(outfile);
            }
            ZipFile.CreateFromDirectory(path, outfile);
        }
        private static void UnZip(string path, string outdir) {
            if (Directory.Exists(outdir)) {
                CopyHelper.Remove(outdir);
            }
            ZipFile.ExtractToDirectory(path, outdir);
        }

        private static void Cleanup(string directory) {
            if (Directory.Exists(directory)) {
                CopyHelper.Remove(directory);
            }
        }

        private static void UnPack(string file, string outDir) {
            try {
                Console.WriteLine("Starting unpack...");
                if (!File.Exists(file)) {
                    Console.WriteLine("No such file " + file);
                    return;
                }
                string dir = Path.GetDirectoryName(file);
                string filename = Path.GetFileNameWithoutExtension(file);
                string outd = outDir != null ? outDir : Path.Combine(dir, filename);
                Console.WriteLine("Unzipping files...");
                UnZip(file, outDir);
                Console.WriteLine("Converting images...");
                ConvertFromWFFormat(outDir);
            } catch (Exception e) {
                Console.WriteLine("Error unpacking watchface: " + e.Message);
            }

        }
    }
}
