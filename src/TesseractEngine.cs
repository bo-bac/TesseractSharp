﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TesseractSharp.Core;

namespace TesseractSharp
{
    internal class TesseractEngine
    {
        private const string TesseractFolder = "tesseract";
        private const string TesseractExe = "tesseract.exe";
        private const string TesseractData = "tessdata";

        public ProcessHelper.ProcessResult Result { get; }

        public TesseractEngine(
         string inputFilePath,
         string outputBasenameFilePath = null,
         long? dotPerInch = null,
         PageSegMode? psm = null,
         OcrEngineMode? oem = null,
         IEnumerable<Language> languages = null,
         IEnumerable<ConfigFile> configFiles = null,
         IEnumerable<KeyValuePair<string, string>> configVars = null)
        {
            if (!File.Exists(inputFilePath))
                throw new ArgumentException($"Input file '{inputFilePath}' does not exit.");


            if (!LibraryHelper.Instance.TryGetDirectory(TesseractData, TesseractFolder, out var tesseractData))
                throw new InvalidOperationException($@"'{TesseractFolder}\{TesseractData}' directory not found.");

            var environmentVariables = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("TESSDATA_PREFIX", tesseractData)
            };

            var args = BuildArgs(inputFilePath, outputBasenameFilePath,
                dotPerInch, psm, oem, languages,
                configFiles, configVars);

            if (!LibraryHelper.Instance.TryGetBinary(TesseractExe, TesseractFolder, out var tesseractCmd))
                throw new InvalidOperationException($@"'{TesseractFolder}\{TesseractExe}' command not found.");

            Debug.WriteLine($"Call '{tesseractCmd} {string.Join(" ", args)}'");

            var result = ProcessHelper.RunProcess(tesseractCmd, args, environmentVariables:  environmentVariables);

            if (result.ExitCode != 0) // POSIX
                Debug.WriteLine(result.Error);

            Debug.WriteLine(result.Output);

            Result = result;
        }

        private static List<string> BuildArgs(
            string inputFilePath,
            string outputBasenameFilePath = null,
            long? dotPerInch = null,
            PageSegMode? psm = null,
            OcrEngineMode? oem = null,
            IEnumerable<Language> languages = null,
            IEnumerable<ConfigFile> configFiles = null,
            IEnumerable<KeyValuePair<string, string>> configVars = null)
        {
            var args = new List<string> { $"\"{inputFilePath}\"" };

            if (outputBasenameFilePath != null)
                args.Add(outputBasenameFilePath);

            if (dotPerInch != null)
                args.Add($"--dpi {dotPerInch}");

            if (psm != null)
                args.Add($"--psm {(int)psm}");

            if (oem != null)
                args.Add($"--oem {(int)oem}");

            if (languages != null)
                args.Add("-l " + string.Join("+", languages.Select(l => l.ToName())));

            if (configVars != null)
                args.Add(string.Join(" ", configVars.Select(kv => $"-c {kv.Key}={kv.Value}")));

            if (configFiles != null)
                args.Add(string.Join(" ", configFiles.Select(cf => cf.ToName())));

            return args;
        }
    }
}
