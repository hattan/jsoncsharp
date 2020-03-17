using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

namespace jsoncsharp
{
    class Program
    {
        static void Info(string message, bool verbose = false)
        {
            if (verbose)
            {
                Console.WriteLine($"INFO-{DateTime.Now.ToLongTimeString()}: {message}");
            }
        }

        static void Write(string message, bool verbose=false)
        {
            if (verbose)
            {
                Console.WriteLine(message);
            }
        }
        static void Error(string message)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"INFO-{DateTime.Now.ToLongTimeString()}: {message}");
            Console.ForegroundColor = old;
        }

        static void Success(string message)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            
            Console.Write($"\n{message}");
            
            Console.ForegroundColor = old;
        }

        /// <summary>
        /// Convert JSON to C# Models.
        /// </summary>
        /// <param name="input">Json document path.</param>
        /// <param name="output">C# file path. (default is {json filename}.cs </param>
        /// <param name="namespace">C# namespace. (default is DefaultNameSpace)</param>
        /// <param name="force">Ignore checking for existence of output file.</param>
        /// <param name="verbose">Show verbose logs.</param>
        static async Task Main(
            string input="", 
            string output=null,
            string @namespace="DefaultNameSpace",
            bool force = false,
            bool verbose = false)
        {
            var watch = Stopwatch.StartNew();

            Write("***************** Starting C# Generation from JSON *****************", true);
            
            input = VerifyInputFile(input, verbose);
            string file = await GenerateFileContentFromSchema(input, @namespace);

            output = VerifyOutputFile(output, force, verbose, input);
            WriteContentToFile(output, file);

            watch.Stop();

            PrintCompletion(output, watch);
        }

        private static void PrintCompletion(string output, Stopwatch watch)
        {
            decimal elapsedMs = watch.ElapsedMilliseconds;
            decimal roundedTime = decimal.Round(elapsedMs / 1000, 3);

            Success($"Generation Completed!\nCreated: {Path.GetFullPath(output)} \nTotal Time (seconds):{roundedTime}");
        }

        private static void WriteContentToFile(string output, string file)
        {
            File.WriteAllText(output, file);
        }

        private static async Task<string> GenerateFileContentFromSchema(string input, string @namespace)
        {
            var schema = await JsonSchema.FromFileAsync(input);
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings
            {
                Namespace = @namespace,
                GenerateDataAnnotations = false
            });
            var file = generator.GenerateFile();
            return file;
        }

        private static string VerifyOutputFile(string output, bool force, bool verbose, string input)
        {
            var inputFileName = Path.GetFileNameWithoutExtension(input);
            output = output ?? $"{inputFileName}.cs";
            output = Path.GetFullPath(output);
            Info($"Output file {output}", verbose);
            if (File.Exists(output) && !force)
            {
                Error($"ERROR: {output} EXISTS!");
                Console.Write("Do you want to override? [y/n] ");
                var response = Console.ReadKey(false).Key;
                if (response == ConsoleKey.N)
                    Environment.Exit(-1);
            }

            return output;
        }

        private static string VerifyInputFile(string input, bool verbose)
        {
            Info($"Input file {input}", verbose);
            if (input == null)
            {
                Error("Input file missing! Please pass an input json file via --input");
                Environment.Exit(-1);
            }
            return input;
        }
    }
}
