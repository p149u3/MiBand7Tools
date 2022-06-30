using System;
using System.IO;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace mi7build {
    class Program {
        static async Task Main(string[] args) {
            await BuildMain.Process(args);
        }
        
    }
}
