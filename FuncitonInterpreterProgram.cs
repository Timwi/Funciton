using System;
using System.Collections.Generic;
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
        static int CommandSwitchesHelp()
        {
            Console.Error.WriteLine("Usage: FuncitonInterpreter [-c] sourceFile [sourceFile [sourceFile ...]]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("You must specify at least one source file. One of them must contain a program, all others must contain only library functions.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("-c    Compile only, don’t run.");
            Console.Error.WriteLine();
            return 1;
        }

        static int Main(string[] args)
        {
            var sourceFiles = new List<string>();
            var compileOnly = false;
            var ignoreSwitches = false;

            foreach (var arg in args)
            {
                if (!ignoreSwitches && arg == "-c")
                    compileOnly = true;
                else if (!ignoreSwitches && arg == "--")
                    ignoreSwitches = true;
                else if (!ignoreSwitches && arg.StartsWith("-"))
                {
                    Console.Error.WriteLine("Unrecognised switch: “{0}”.", arg);
                    return CommandSwitchesHelp();
                }
                else
                    sourceFiles.Add(arg);
            }

            if (sourceFiles.Count == 0)
                return CommandSwitchesHelp();

            foreach (var file in sourceFiles)
                if (!File.Exists(file))
                {
                    Console.WriteLine("“{0}” doesn’t exist.", file);
                    return 1;
                }

            try
            {
                var compiled = FuncitonLanguage.CompileFiles(sourceFiles);
                if (compileOnly)
                    Console.WriteLine("Program parses without errors.");
                else
                    Console.WriteLine(compiled.Run());
            }
            catch (ParseErrorException pe)
            {
                foreach (var error in pe.Errors)
                {
                    if (error.SourceFile == null)
                        Console.Error.WriteLine("Error: {0}", error.Message);
                    else if (error.Line == null)
                        Console.Error.WriteLine("{0}: Error: {1}", error.SourceFile, error.Message);
                    else if (error.Character == null)
                        Console.Error.WriteLine("{0}({1}): Error: {2}", error.SourceFile, error.Line.Value + 1, error.Message);
                    else
                        Console.Error.WriteLine("{0}({1},{2}): Error: {3}", error.SourceFile, error.Line.Value + 1, error.Character.Value + 1, error.Message);
                }
                return 1;
            }
            return 0;
        }
    }
}
