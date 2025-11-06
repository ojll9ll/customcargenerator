using System;
using System.IO;
using System.Text;
using CustomCarExporter.Core;

namespace Generator
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: Generator.exe <input.json> <output.customcar>");
                return 1;
            }

            try
            {
                var jsonPath = args[0];
                var outputPath = args[1];

                if (!File.Exists(jsonPath))
                {
                    Console.Error.WriteLine($"Input file not found: {jsonPath}");
                    return 1;
                }

                var json = File.ReadAllText(jsonPath, Encoding.UTF8);
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var input = serializer.Deserialize<ExportModelParams>(json);

                CustomCarExporter.GenerateToFile(input, outputPath);
                Console.WriteLine($"Generated: {outputPath}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }
    }
}

