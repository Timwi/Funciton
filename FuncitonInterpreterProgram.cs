using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("FuncitonInterpreter")]
[assembly: AssemblyDescription("Interprets Funciton programs.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("Funciton Interpreter")]
[assembly: AssemblyCopyright("Copyright © Timwi 2011")]
[assembly: ComVisible(false)]
[assembly: Guid("a08d877d-0559-47ba-a9be-8c08d3e81851")]
[assembly: AssemblyVersion("1.0.9999.9999")]
[assembly: AssemblyFileVersion("1.0.9999.9999")]

namespace FuncitonInterpreter
{
    static class FuncitonInterpreterProgram
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Specify one or more Funciton source files as command-line arguments.");
                Console.WriteLine("One of them must contain a program, all others must contain only library functions.");
                return 1;
            }

            foreach (var arg in args)
                if (!File.Exists(arg))
                {
                    Console.WriteLine("“{0}” doesn’t exist.", arg);
                    return 1;
                }

            Console.WriteLine(FuncitonLanguage.CompileFiles(args).Run());
            return 0;
        }
    }
}
