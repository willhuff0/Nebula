using CommandLine;
using Nebula.Interop;

namespace Nebula;

public class Options {
    [Option('i', "interopPassword")]
    public string InteropPassword { get; set; }
}

public class Nebula {
    public void Main(string[] args) {
        Options options = Parser.Default.ParseArguments<Options>(args).Value;
        JRPCServer.Start(options.InteropPassword);
    }
}