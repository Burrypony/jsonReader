using System;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ExtractTextWithFontSize
{
    class Program
    {
        static void Main(string[] args)
        {
            string pdfPath = "/Users/admin/Downloads/5ECA7D7D89C05126A1191A3AF0B2D417.pdf"; // Replace with the path to your PDF file
            var fontSizes = new HashSet<float>();

            using (PdfReader reader = new PdfReader(pdfPath))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    ITextExtractionStrategy strategy = new MyLocationTextExtractionStrategy(fontSizes);
                    string pageText = PdfTextExtractor.GetTextFromPage(reader, i, strategy);
                    Console.WriteLine($"Page {i}: \n{pageText}");
                }
            }

            Console.WriteLine("Unique Font Sizes:");
            foreach (var size in fontSizes)
            {
                Console.WriteLine(size);
            }
        }
    }

    public class MyLocationTextExtractionStrategy : LocationTextExtractionStrategy
    {
        private readonly HashSet<float> _fontSizes;

        public MyLocationTextExtractionStrategy(HashSet<float> fontSizes)
        {
            _fontSizes = fontSizes;
        }
        public override void RenderText(TextRenderInfo renderInfo)
        {
            base.RenderText(renderInfo);

            string currentText = renderInfo.GetText(); // Get the text
            Vector start = renderInfo.GetBaseline().GetStartPoint();
            Vector end = renderInfo.GetBaseline().GetEndPoint();
            float fontSize = renderInfo.GetAscentLine().GetStartPoint()[Vector.I2] - renderInfo.GetDescentLine().GetStartPoint()[Vector.I2];
            Vector bottomLeft = renderInfo.GetDescentLine().GetStartPoint();
            float x = bottomLeft[Vector.I1];
            float y = bottomLeft[Vector.I2];

            _fontSizes.Add(fontSize);

            Console.WriteLine($"Text: {currentText}, Font Size: {fontSize}, Start: {start}, End: {end}, X: {x}, Y: {y}");
        }
    }
}
