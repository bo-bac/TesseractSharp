using System;
using System.IO;
using System.Threading.Tasks;

namespace TesseractSharp.Xpdf
{
    public static class Pdf
    {
        public static async Task<DirectoryInfo> ToPngAsync(string inputFilePath, PdfToPngOptions options = null)
        {
            var engine = new PdfToPngEngine(options ?? new PdfToPngOptions
            {
                FirstPage = 1,
                LastPage = 1
            });

            try
            {
                return await engine.RunAsync(inputFilePath);
            }
            catch (Exception ex)
            {
                throw new TesseractException("Fail to call pdftopng", ex);
            }
        }
    }
}
