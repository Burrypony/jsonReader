using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using iTextSharp.text;




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
        public int PageNumber { get; set; }
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



            string pdfPath = "";
            string newPdfPath = "";

            while (string.IsNullOrEmpty(pdfPath))
            {
                Console.WriteLine("Please write path and pdf name:");
                pdfPath = Console.ReadLine();

                if (string.IsNullOrEmpty(pdfPath))
                {
                    Console.WriteLine("The input cannot be empty. Please try again.");
                }
            }


            // Replace with the path to your PDF file



            while (string.IsNullOrEmpty(newPdfPath))
            {
                Console.WriteLine("Please write path and pdf name for a file with bookmars");
                newPdfPath = Console.ReadLine();
                if (string.IsNullOrEmpty(newPdfPath))
                {
                    Console.WriteLine("The input cannot be empty. Please try again.");
                }
            }


            List<CustomTextChunk> processedTextChunks = new List<CustomTextChunk>();

            CreatePdfCopyWithBookmarks(pdfPath, newPdfPath);


            using (PdfReader reader = new PdfReader(newPdfPath))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    var fontSizes = new HashSet<float>();
                    var strategy = new MyLocationTextExtractionStrategy(fontSizes, i);
                    PdfTextExtractor.GetTextFromPage(reader, i, strategy);
                    processedTextChunks.AddRange(strategy.TextChunks);
                }

            };

            int wordPositionIndex = 0;

            for (int i = 1; i < processedTextChunks.Count; i++)
            {
                // Console.WriteLine(processedTextChunks[i].Text);
                var currentTextChunk = processedTextChunks[i];
                // Console.WriteLine($"i: {i}, wordPositionIndex:{wordPositionIndex}");
                if (currentTextChunk.StartLocation[Vector.I1] - processedTextChunks[wordPositionIndex].EndLocation[Vector.I1] < 1 && processedTextChunks[wordPositionIndex].Y == currentTextChunk.Y)
                {
                    // Console.WriteLine($"currentTextChunk.EndLocation[Vector.I1] - processedTextChunks[wordPositionIndex].StartLocation[Vector.I1]: {currentTextChunk.StartLocation[Vector.I1] - processedTextChunks[wordPositionIndex].EndLocation[Vector.I1]}");
                    // Console.WriteLine($"processedTextChunks[wordPositionIndex].Text: {processedTextChunks[wordPositionIndex].Text}, currentTextChunk.Text: {currentTextChunk.Text}");
                    var joinText = new CustomTextChunk
                    {
                        Text = processedTextChunks[wordPositionIndex].Text + currentTextChunk.Text,
                        FontSize = currentTextChunk.FontSize,
                        StartLocation = processedTextChunks[wordPositionIndex].StartLocation,
                        EndLocation = currentTextChunk.EndLocation,
                        X = processedTextChunks[wordPositionIndex].X,
                        Y = processedTextChunks[wordPositionIndex].Y,
                        PageNumber = currentTextChunk.PageNumber
                    };
                    processedTextChunks[wordPositionIndex] = joinText;
                    // Console.WriteLine($"currentTextChunk.Text: {currentTextChunk.Text}");
                    processedTextChunks.RemoveAt(i);
                    i--;
                    // Console.WriteLine($"currentTextChunk.Text After Deliting: {processedTextChunks[i].Text}");
                    // Console.WriteLine($"processedTextChunks[wordPositionIndex].Text AFTER: {processedTextChunks[wordPositionIndex].Text}");

                }
                else if (processedTextChunks[wordPositionIndex].FontSize == currentTextChunk.FontSize)
                {
                    // Console.WriteLine($"processedTextChunks[wordPositionIndex].Text: {processedTextChunks[wordPositionIndex].Text}, currentTextChunk.Text: {currentTextChunk.Text}");

                    var joinText = new CustomTextChunk
                    {
                        Text = processedTextChunks[wordPositionIndex].Text + ' ' + currentTextChunk.Text,
                        FontSize = currentTextChunk.FontSize,
                        StartLocation = processedTextChunks[wordPositionIndex].StartLocation,
                        EndLocation = currentTextChunk.EndLocation,
                        X = processedTextChunks[wordPositionIndex].X,
                        Y = processedTextChunks[wordPositionIndex].Y,
                        PageNumber = currentTextChunk.PageNumber
                    };
                    processedTextChunks[wordPositionIndex] = joinText;
                    // Console.WriteLine($"processedTextChunks[wordPositionIndex].Text AFTER: {processedTextChunks[wordPositionIndex].Text}");
                    // Console.WriteLine($"currentTextChunk.Text: {currentTextChunk.Text}");
                    processedTextChunks.RemoveAt(i);
                    i--;
                    // Console.WriteLine($"currentTextChunk.Text After Deliting: {processedTextChunks[i].Text}");
                }
                else
                {
                    // Console.WriteLine($"processedTextChunks[wordPositionIndex].Text: {processedTextChunks[wordPositionIndex].Text}, currentTextChunk.Text: {currentTextChunk.Text}");
                    // Console.WriteLine($"currentTextChunk.EndLocation[Vector.I1] - processedTextChunks[wordPositionIndex].StartLocation[Vector.I1]: {currentTextChunk.StartLocation[Vector.I1] - processedTextChunks[wordPositionIndex].EndLocation[Vector.I1]}");
                    wordPositionIndex++;
                }


            }


            // Console.WriteLine($"wordPositionIndex:{wordPositionIndex}, processedTextChunks.Count:{processedTextChunks.Count}");

            // foreach (CustomTextChunk chunk in processedTextChunks)
            // {
            //     Console.WriteLine($"Text: {chunk.Text}, Font Size: {chunk.FontSize}, Start Location: {chunk.StartLocation}, End Location: {chunk.EndLocation}");
            // }

            HierarchicalTextChunk treeRoot = BuildTree(processedTextChunks);
            HashSet<int> uniqueFontSizes = GetUniqueFontSizes(treeRoot);
            List<int> sortedFontSizes = uniqueFontSizes.ToList();
            sortedFontSizes.Sort();
            sortedFontSizes.Reverse();

            Console.WriteLine("Sorted unique font sizes:");
            foreach (int fontSize in sortedFontSizes)
            {
                Console.WriteLine(fontSize);
            }

            Program programInstance = new Program();
            programInstance.AddBookmarksToExistingPdf(newPdfPath, treeRoot); // Assuming treeRoot is defined

            // PrintTree(treeRoot);

            static HierarchicalTextChunk BuildTree(List<CustomTextChunk> processedTextChunks)
            {
                var rootChunk = new CustomTextChunk
                {
                    Text = "Document Root", // Or any placeholder text
                    FontSize = int.MaxValue, // You might use a special value for the root
                    StartLocation = new Vector(0, 0, 0),
                    EndLocation = new Vector(0, 0, 0),
                    X = 0,
                    Y = 0,
                    PageNumber = 0
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

        public static HashSet<int> GetUniqueFontSizes(HierarchicalTextChunk root)
        {
            HashSet<int> uniqueFontSizes = new HashSet<int>();

            void Traverse(HierarchicalTextChunk node)
            {
                // Add the current node's font size to the HashSet
                uniqueFontSizes.Add(node.TextChunk.FontSize);

                // Recursively traverse the children
                foreach (var child in node.Children)
                {
                    Traverse(child);
                }
            }

            // Start the traversal from the root
            Traverse(root);

            return uniqueFontSizes;
        }

        static void CreatePdfCopyWithBookmarks(string sourcePdfPath, string destinationPdfPath)
        {
            using (PdfReader reader = new PdfReader(sourcePdfPath))
            {
                using (FileStream fs = new FileStream(destinationPdfPath, FileMode.Create, FileAccess.Write))
                {
                    using (Document document = new Document(reader.GetPageSizeWithRotation(1)))
                    {
                        PdfCopy copy = new PdfCopy(document, fs);
                        document.Open();

                        // Copy each page from the source PDF to the new PDF
                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        {
                            copy.AddPage(copy.GetImportedPage(reader, i));
                        }

                        document.Close();

                        // Now, add bookmarks based on the hierarchical tree
                        // This step must be done after the document is closed
                    }
                }
            }
        }


        public void AddBookmarksToExistingPdf(string pdfPath, HierarchicalTextChunk rootNode)
        {
            string tempFilePath = pdfPath + ".tmp";
            using (PdfReader reader = new PdfReader(pdfPath))
            {
                // Create a temporary file stream for the output to avoid issues with reading and writing to the same file simultaneously

                using (FileStream fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    using (PdfStamper stamper = new PdfStamper(reader, fs))
                    {
                        IList<Dictionary<string, object>> bookmarks = new List<Dictionary<string, object>>();
                        AddBookmarksFromNode(rootNode, bookmarks);

                        // Apply the bookmarks to the stamper
                        stamper.Outlines = bookmarks;
                    }
                }

                // Close the reader to release the original file
                reader.Close();
            }

            // Replace the original file with the modified temporary file
            File.Delete(pdfPath);
            File.Move(tempFilePath, pdfPath);
        }

        private void AddBookmarksFromNode(HierarchicalTextChunk node, IList<Dictionary<string, object>> bookmarksList)
        {
            Dictionary<string, object> bookmark = new Dictionary<string, object>
    {
        { "Title", node.TextChunk.Text },
        { "Action", "GoTo" },
        // This assumes the X, Y coordinates and page number are set correctly in your CustomTextChunk
        { "Page", $"{node.TextChunk.PageNumber} XYZ {node.TextChunk.X} {node.TextChunk.Y} null" }
    };
            Console.WriteLine($"Title: {node.TextChunk.Text}");
            bookmarksList.Add(bookmark);

            if (node.Children.Count > 0 && node.TextChunk.FontSize >= 9)
            {
                List<Dictionary<string, object>> kids = new List<Dictionary<string, object>>();
                foreach (HierarchicalTextChunk child in node.Children)
                {
                    AddBookmarksFromNode(child, kids);
                }
                bookmark.Add("Kids", kids);
            }
        }


    }


    public class MyLocationTextExtractionStrategy : LocationTextExtractionStrategy
    {

        public List<CustomTextChunk> TextChunks { get; } = new List<CustomTextChunk>();
        private readonly HashSet<float> _fontSizes;
        private int _pageNumber; // Store the current page number

        public MyLocationTextExtractionStrategy(HashSet<float> fontSizes, int pageNumber)
        {
            _fontSizes = fontSizes;
            _pageNumber = pageNumber;
        }
        public override void RenderText(TextRenderInfo renderInfo)
        {
            base.RenderText(renderInfo);

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
                Y = bottomLeft[Vector.I2],
                PageNumber = _pageNumber
            };

            TextChunks.Add(textChunk);

            // Console.WriteLine($"Text: {currentText}, Font Size: {fontSize}, Start: {start}, End: {end}, X: {x}, Y: {y}");
        }
    }
}
