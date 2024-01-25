using System;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;




namespace ExtractTextWithFontSize
{
    public class CustomTextChunk
    {
        public string Text { get; set; }
        public float FontSize { get; set; }
        public Vector StartLocation { get; set; }
        public Vector EndLocation { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {

            string pdfPath = "/Users/admin/Downloads/5ECA7D7D89C05126A1191A3AF0B2D417.pdf"; // Replace with the path to your PDF file
            var fontSizes = new HashSet<float>();
            var strategy = new MyLocationTextExtractionStrategy(fontSizes);

            List<CustomTextChunk> processedTextChunks = new List<CustomTextChunk>();


            using (PdfReader reader = new PdfReader(pdfPath))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    PdfTextExtractor.GetTextFromPage(reader, i, strategy);
                }

            };

            int wordPositionIndex = 0;

            for (int i = 0; i < strategy.TextChunks.Count; i++)
            {
                var currentTextChunk = strategy.TextChunks[i];
                if (i != 0)
                {
                    // Console.WriteLine($"i: {i}, wordPositionIndex:{wordPositionIndex}");
                    if (currentTextChunk.StartLocation[Vector.I1] - processedTextChunks[wordPositionIndex].EndLocation[Vector.I1] < 1 && processedTextChunks[wordPositionIndex].FontSize == currentTextChunk.FontSize)
                    {
                        Console.WriteLine($"currentTextChunk.EndLocation[Vector.I1] - processedTextChunks[wordPositionIndex].StartLocation[Vector.I1]: {currentTextChunk.StartLocation[Vector.I1] - processedTextChunks[wordPositionIndex].EndLocation[Vector.I1]}");

                        var joinText = new CustomTextChunk
                        {
                            Text = processedTextChunks[wordPositionIndex].Text + currentTextChunk.Text,
                            FontSize = currentTextChunk.FontSize,
                            StartLocation = processedTextChunks[wordPositionIndex].StartLocation,
                            EndLocation = currentTextChunk.EndLocation,
                            X = processedTextChunks[wordPositionIndex].X,
                            Y = processedTextChunks[wordPositionIndex].Y
                        };
                        processedTextChunks[wordPositionIndex] = joinText;
                    }
                    else
                    {

                        wordPositionIndex++;
                        processedTextChunks.Add(currentTextChunk);
                    }
                }
                else
                {
                    // Console.WriteLine($"i: {i}, wordPositionIndex:{wordPositionIndex}");
                    wordPositionIndex = i;
                    processedTextChunks.Add(currentTextChunk);

                }

            }

            foreach (var customTextChunk in processedTextChunks)
            {
                Console.WriteLine($"Text: {customTextChunk.Text}, Font Size: {customTextChunk.FontSize}, Start Location: {customTextChunk.StartLocation}, End Location: {customTextChunk.EndLocation}");
            }
        }
    }

    public class MyLocationTextExtractionStrategy : LocationTextExtractionStrategy
    {

        public List<CustomTextChunk> TextChunks { get; } = new List<CustomTextChunk>();
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

            var textChunk = new CustomTextChunk
            {
                Text = renderInfo.GetText(),
                FontSize = renderInfo.GetAscentLine().GetStartPoint()[Vector.I2] - renderInfo.GetDescentLine().GetStartPoint()[Vector.I2],
                StartLocation = renderInfo.GetBaseline().GetStartPoint(),
                EndLocation = renderInfo.GetBaseline().GetEndPoint(),
                X = bottomLeft[Vector.I1],
                Y = bottomLeft[Vector.I2]
            };

            TextChunks.Add(textChunk);

            // Console.WriteLine($"Text: {currentText}, Font Size: {fontSize}, Start: {start}, End: {end}, X: {x}, Y: {y}");
        }
    }
}
