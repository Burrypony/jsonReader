using System;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;




namespace ExtractTextWithFontSize
{
    public class CustomTextChunk
    {
        public string Text { get; set; }
        public int FontSize { get; set; }
        public Vector StartLocation { get; set; }
        public Vector EndLocation { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class HierarchicalTextChunk
    {
        public CustomTextChunk TextChunk { get; set; }
        public HierarchicalTextChunk Parent { get; set; } // Reference to the parent
        public List<HierarchicalTextChunk> Children { get; set; }
        public static HierarchicalTextChunk CurrentRoot { get; set; }

        public HierarchicalTextChunk(CustomTextChunk textChunk, HierarchicalTextChunk parent = null)
        {
            TextChunk = textChunk;
            Parent = parent;
            Children = new List<HierarchicalTextChunk>();
            if (CurrentRoot == null) CurrentRoot = this; // Initialize the CurrentRoot if it's the first instance
        }

        public void AddChild(CustomTextChunk childChunk)
        {
            var childNode = new HierarchicalTextChunk(childChunk, this); // 'this' sets the current node as the parent
            this.Children.Add(childNode);
            CurrentRoot = childNode; // Update CurrentRoot to the newly added child
        }
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
                        // Console.WriteLine($"currentTextChunk.EndLocation[Vector.I1] - processedTextChunks[wordPositionIndex].StartLocation[Vector.I1]: {currentTextChunk.StartLocation[Vector.I1] - processedTextChunks[wordPositionIndex].EndLocation[Vector.I1]}");

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
                    else if (processedTextChunks[wordPositionIndex].FontSize == currentTextChunk.FontSize)
                    {
                        var joinText = new CustomTextChunk
                        {
                            Text = processedTextChunks[wordPositionIndex].Text + ' ' + currentTextChunk.Text,
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
                    processedTextChunks.Add(currentTextChunk);

                }

            }

            // foreach (var customTextChunk in processedTextChunks)
            // {
            //     Console.WriteLine($"Text: {customTextChunk.Text}, Font Size: {customTextChunk.FontSize}, Start Location: {customTextChunk.StartLocation}, End Location: {customTextChunk.EndLocation}");
            // }

            HierarchicalTextChunk treeRoot = BuildTree(processedTextChunks);

            PrintTree(treeRoot);

            static HierarchicalTextChunk BuildTree(List<CustomTextChunk> processedTextChunks)
            {
                var rootChunk = new CustomTextChunk
                {
                    Text = "Document Root", // Or any placeholder text
                    FontSize = int.MaxValue, // You might use a special value for the root
                    StartLocation = new Vector(0, 0, 0),
                    EndLocation = new Vector(0, 0, 0),
                    X = 0,
                    Y = 0
                }; // Placeholder root chunk
                HierarchicalTextChunk root = new HierarchicalTextChunk(rootChunk);
                
                Console.WriteLine($"CurrentRoot:{HierarchicalTextChunk.CurrentRoot.TextChunk.Text}");

                foreach (var chunk in processedTextChunks)
                {
                    InsertChunkRelativeToParent(HierarchicalTextChunk.CurrentRoot, chunk);

                }

                return root;
            }

            static void InsertChunkRelativeToParent(HierarchicalTextChunk node, CustomTextChunk newChunk)
            {
                // Check if the new chunk should be a child of the given node based on font size
                if (node.TextChunk.FontSize - newChunk.FontSize >= 2)
                {
                    // Console.WriteLine($"Text:{node.TextChunk.Text},FontSize:{node.TextChunk.FontSize},newChunk.FontSize:{newChunk.FontSize}");
                
                    // The new chunk is a child of the current node
                    node.AddChild(newChunk);
                }
                else
                {
                    // Move up towards the root to find a suitable parent
                    if (node.Parent != null)
                    {
                        // Console.WriteLine($"node.ParentText:{node.TextChunk.Text},FontSize:{node.TextChunk.FontSize},newChunk.FontSize:{newChunk.FontSize}");
                
                        // There is a parent, so check if the new chunk should be inserted relative to the parent
                        InsertChunkRelativeToParent(node.Parent, newChunk);
                    }
                    else
                    {
                        // No suitable parent found (node is the root or no parent with a larger font size), so add it as a child of the root
                        // This assumes that at the top level, all nodes can be siblings
                        node.AddChild(newChunk);
                    }
                }
            }

            static void PrintTree(HierarchicalTextChunk node, int depth = 0)
            {
                // Print the current node with indentation based on depth
                Console.WriteLine($"{new string(' ', depth * 2)}Text: {node.TextChunk.Text}, Font Size: {node.TextChunk.FontSize}");

                // Recursively print each child, increasing the depth
                foreach (var child in node.Children)
                {
                    // Console.WriteLine($"PrintTreeChilder{new string(' ', depth * 2)}Text: {child.TextChunk.Text}, Font Size: {child.TextChunk.FontSize}");

                    PrintTree(child, depth + 1);
                }
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
                FontSize = (int)renderInfo.GetAscentLine().GetStartPoint()[Vector.I2] - (int)renderInfo.GetDescentLine().GetStartPoint()[Vector.I2],
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
