using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesseractSharp.Core;

namespace TesseractSharp.Xpdf
{
    internal class PdfToPngEngine
    {
        private const string SubFolderName = "xpdf";
        private const string ExeFileName = "pdftopng.exe";
        private PdfToPngOptions options;

        public ProcessHelper.ProcessResult Result { get; }

        public PdfToPngEngine(PdfToPngOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.options.Output = this.options.Output ?? LibraryHelper.Instance.GetTempPath("page");
        }

        public async Task<DirectoryInfo> RunAsync(string inputFilePath, PdfToPngOptions options = null) 
        {
            if (!File.Exists(inputFilePath))
                throw new ArgumentException($"Input file '{inputFilePath}' does not exit.");

            options = options ?? this.options;
            var args = BuildArgs(options, inputFilePath);

            if (!LibraryHelper.Instance.TryGetBinary(ExeFileName, SubFolderName, out var cmd))
                throw new InvalidOperationException($@"'{SubFolderName}\{ExeFileName}' command not found.");

            Debug.WriteLine($"Call '{cmd} {string.Join(" ", args)}'");            

            var result = await ProcessHelper.RunProcessAsync(cmd, args);

            if (result.ExitCode != 0) // POSIX
                Debug.WriteLine(result.Error);

            Debug.WriteLine(result.Output);

            return Directory.GetParent(options.Output);
        }

        private static List<string> BuildArgs(PdfToPngOptions options, string inputFilePath)
        {
            var args = new List<string>();            

            if (options.FirstPage.HasValue)
                args.Add($"-f {options.FirstPage}");

            if (options.LastPage.HasValue)
                args.Add($"-l {options.LastPage}");

            args.Add(inputFilePath);

            var output = options.Output ?? LibraryHelper.Instance.GetTempPath("page");
            args.Add(output);

            return args;
        }
    }
}
