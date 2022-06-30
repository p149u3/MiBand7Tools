using System;
using System.Threading.Tasks;

namespace mi7convert {
    class Program {
        static async Task Main(string[] args) {
            await ConvertMain.Process(args);
        }
    }
}
