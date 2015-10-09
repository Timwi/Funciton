using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

[assembly: AssemblyTitle("FuncitonInterpreter")]
[assembly: AssemblyDescription("Interprets Funciton programs.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("Funciton Interpreter")]
[assembly: AssemblyCopyright("Copyright © Timwi 2011")]
[assembly: ComVisible(false)]
[assembly: Guid("a08d877d-0559-47ba-a9be-8c08d3e81851")]
[assembly: AssemblyVersion("1.0.9999.9999")]
[assembly: AssemblyFileVersion("1.0.9999.9999")]

namespace Funciton
{
    static class FuncitonMainProgram
    {
        static int CommandSwitchesHelp()
        {
            Console.Error.WriteLine(@"Usages:");
            Console.Error.WriteLine();
            Console.Error.WriteLine(@"    • FuncitonInterpreter [-t[func] [-t[func] ...]] sourceFile [sourceFile ...]");
            Console.Error.WriteLine(@"        Interprets a Funciton program. The source files are expected to contain exactly one program and otherwise only function declarations.");
            Console.Error.WriteLine(@"        Additional options permissible in this mode:");
            Console.Error.WriteLine(@"        • -i[int]     Pretends that the specified integer was passed in through STDIN (and ignores the actual STDIN).");
            Console.Error.WriteLine(@"        • -s[str]     Pretends that the specified string was passed in through STDIN (and ignores the actual STDIN).");
            Console.Error.WriteLine();
            Console.Error.WriteLine(@"    • FuncitonInterpreter -a[func] [-a[func] ...] sourceFile [sourceFile ...]");
            Console.Error.WriteLine(@"        Outputs an expression for the specified function(s) “func”, or the main program if no function is specified.");
            Console.Error.WriteLine();
            Console.Error.WriteLine(@"    • FuncitonInterpreter -c<target> sourceFile [sourceFile ...]");
            Console.Error.WriteLine(@"        Compiles the specified Funciton program to a .NET executable. “target” specifies the path and filename of the executable file to generate.");
            Console.Error.WriteLine();
            Console.Error.WriteLine(@"    • FuncitonInterpreter -k sourceFile [sourceFile ...]");
            Console.Error.WriteLine(@"        Checks the specified program for compile errors.");
            Console.Error.WriteLine();
            return 1;
        }

        static int Main(string[] args)
        {
            try { Console.OutputEncoding = Encoding.UTF8; }
            catch { }

            var sourceFiles = new List<string>();
            var checkErrorsOnly = false;
            var ignoreSwitches = false;
            var analyseFunctions = new List<string>();
            var traceFunctions = new List<string>();
            var waitAtEnd = false;
            string compileTo = null;

            foreach (var arg in args)
            {
                if (!ignoreSwitches && arg.StartsWith("-a"))
                    analyseFunctions.Add(arg.Substring(2));
                else if (!ignoreSwitches && arg.StartsWith("-t"))
                    traceFunctions.Add(arg.Substring(2));
                else if (!ignoreSwitches && arg == "-k")
                    checkErrorsOnly = true;
                else if (!ignoreSwitches && arg == "-w")
                    waitAtEnd = true;
                else if (arg.StartsWith("-c"))
                {
                    if (compileTo != null)
                    {
                        Console.Error.WriteLine("You cannot specify multiple “-c” switches. Please specify only one.");
                        return CommandSwitchesHelp();
                    }
                    compileTo = arg.Substring(2);
                }
                else if (!ignoreSwitches && arg.StartsWith("-i"))
                {
                    if (FuncitonLanguage.PretendStdin != null)
                    {
                        Console.Error.WriteLine("You cannot use “-i” and “-s” together or multiple copies of those switches.");
                        return CommandSwitchesHelp();
                    }
                    try { FuncitonLanguage.PretendStdin = BigInteger.Parse(arg.Substring(2)); }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                        return CommandSwitchesHelp();
                    }
                }
                else if (!ignoreSwitches && arg.StartsWith("-s"))
                {
                    if (FuncitonLanguage.PretendStdin != null)
                    {
                        Console.Error.WriteLine("You cannot use “-i” and “-s” together or multiple copies of those switches.");
                        return CommandSwitchesHelp();
                    }
                    FuncitonLanguage.PretendStdin = FuncitonLanguage.StringToInteger(arg.Substring(2));
                }
                else if (!ignoreSwitches && arg == "--")
                    ignoreSwitches = true;
                else if (!ignoreSwitches && arg.StartsWith("-"))
                {
                    Console.Error.WriteLine("Unrecognised switch: “{0}”.", arg);
                    return CommandSwitchesHelp();
                }
                else
                {
                    var dir = Path.GetDirectoryName(arg);
                    var matches = Directory.GetFiles(dir.Length < 1 ? "." : dir, Path.GetFileName(arg), SearchOption.TopDirectoryOnly);
                    if (matches.Length == 0)
                    {
                        Console.WriteLine("“{0}” not found.", arg);
                        return 1;
                    }
                    sourceFiles.AddRange(matches);
                }
            }

            if ((traceFunctions.Count > 0 ? 1 : 0) + (analyseFunctions.Count > 0 ? 1 : 0) + (checkErrorsOnly ? 1 : 0) + (compileTo != null ? 1 : 0) > 1)
            {
                Console.WriteLine("Command-line error: You cannot use “-t”, “-a”, “-k” and “-c” together. Please specify only one of these.");
                return CommandSwitchesHelp();
            }

            if (sourceFiles.Count == 0)
                return CommandSwitchesHelp();

            try
            {
                if (analyseFunctions.Count > 0)
                {
                    Console.Write(FuncitonLanguage.AnalyseFunctions(sourceFiles, analyseFunctions));
                }
                else
                {
                    var program = FuncitonLanguage.CompileFiles(sourceFiles);
                    if (checkErrorsOnly)
                        Console.WriteLine("Program parses without errors.");
                    else if (compileTo != null)
                        FuncitonCompiler.CompileTo(program, compileTo);
                    else
                        Console.Write(program.Run(traceFunctions.Count == 0 ? null : traceFunctions));
                }
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

            if (waitAtEnd)
            {
                Console.WriteLine("Done.");
                Console.ReadLine();
            }

            return 0;
        }
    }
}
