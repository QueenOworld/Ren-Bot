using CommandLine;
using System.Collections.Generic;
using System.IO;

namespace StutterMosher.ConsoleApp
{
    class Program
    {
        // Command-line arguments
        public class Options
        {
            [Option('i', "inputfile", Required = true, HelpText = "Specifies the AVI video file to mosh.")]
            public string InputFile { get; set; }
            [Option('o', "outputfile", Required = true, HelpText = "Specifies the destination AVI file for the moshed data.")]
            public string OutputFile { get; set; }
            [Option('m', "mosh", Required = false, HelpText = "Set number of times to write each p-frame to the output file. Larger values make the effect more intense.")]
            public int Mosh { get; set; } = 3;
        }

        // Byte signature for i-frames (full image frames)
        static readonly List<byte> iFrameSig = new List<byte> { 0x00, 0x01, 0xB0 };
        // Byte signature for p-frames (delta frames)
        static readonly List<byte> pFrameSig = new List<byte> { 0x00, 0x01, 0xB6 };
        // Byte signature that signals the end of frame data
        static readonly List<byte> EndOfFrame = new List<byte> { 0x30, 0x30, 0x64, 0x63 };

        static void Main(string[] args)
        {
            // Parse arguments and go!
            Parser.Default
                .ParseArguments<Options>(args)
                .WithParsed(Run);
        }

        static void Run(Options options)
        {
            // Open files
            using (FileStream inputFile = File.OpenRead(options.InputFile))
            using (FileStream outputFile = File.Create(options.OutputFile))
            {
                Mosher mosher = new Mosher(inputFile, outputFile);
                mosher.Mosh(options.Mosh);
            }
        }
    }
}
