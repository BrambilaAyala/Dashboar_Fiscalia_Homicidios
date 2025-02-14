using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ProyectoFisca.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LectorPdfOcrController : ControllerBase
    {
        [HttpGet]
        public IActionResult ReadPdf()
        {
            string pdfPath = "homicidios.pdf";
            List<int> numbers = ExtractNumbersFromPdf(pdfPath);
            return Ok(numbers); // Devuelve la lista de números como JSON
        }

        static List<int> ExtractNumbersFromPdf(string pdfPath)
        {
            List<int> result = new List<int>();
            int maxCharsPerLine = 2; // Máximo de caracteres por línea

            using (PdfReader reader = new PdfReader(pdfPath))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {
                for (int page = 1; page <= 1; page++)
                {
                    string textFromPage = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page));
                    string[] lines = textFromPage.Split('\n');

                    for (int i = 4; i < lines.Length - 3; i++)  // Empieza desde la cuarta línea
                    {
                        string numbers = ExtractNumbers(lines[i]);
                        string processedLine = ProcessLine(numbers, maxCharsPerLine);
                        result.AddRange(ProcessNumbers(processedLine));
                    }
                }
            }

            return result;
        }

        static string ProcessLine(string line, int maxChars)
        {
            if (line.Length <= maxChars)
            {
                return line + Environment.NewLine;
            }

            StringBuilder result = new StringBuilder();
            int start = 0;

            while (start < line.Length)
            {
                int length = Math.Min(maxChars, line.Length - start);
                result.AppendLine(line.Substring(start, length));
                start += length;
            }

            return result.ToString(); // No se elimina la nueva línea final
        }

        static List<int> ProcessNumbers(string processedLine)
        {
            List<int> numbers = new List<int>();
            var lines = processedLine.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                var matches = Regex.Matches(line, @"\d+");
                foreach (Match match in matches)
                {
                    if (int.TryParse(match.Value, out int number))
                    {
                        numbers.Add(number);
                    }
                }
            }

            return numbers;
        }

        static string ExtractNumbers(string line)
        {
            // Usa una expresión regular para extraer todos los números de la línea
            var matches = Regex.Matches(line, @"\d+");
            return string.Join(" ", matches);
        }
    }
}
