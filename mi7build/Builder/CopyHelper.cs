using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mi7build {
    class CopyHelper {
        public static void Copy(string sourceDirectory, string targetDirectory) {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget;
            if (!Directory.Exists(targetDirectory)) {
                diTarget = Directory.CreateDirectory(targetDirectory);
            } else {
                diTarget = new DirectoryInfo(targetDirectory);
            }

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target) {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles()) {
                if (!fi.Name.StartsWith(".")) {
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
                if (!diSourceSubDir.Name.StartsWith(".")) {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir, nextTargetSubDir);
                }
            }
        }

        public static void Remove(string directory) {
            Directory.Delete(directory, true);
        }

    }
}
