﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using Shape = DocumentFormat.OpenXml.Presentation.Shape;

namespace PPT_To_Latex
{
    class Program
    {
        static void Main(string[] args)
        {
            // http://stackoverflow.com/questions/7070074/how-can-i-retrieve-images-from-a-pptx-file-using-ms-open-xml-sdk

            // http://msdn.microsoft.com/en-us/library/bb448854.aspx
            bool includeHidden = false;


            using (PresentationDocument presentationDocument = PresentationDocument.Open("test.pptx", false))
            {
                PresentationPart presentationPart = presentationDocument.PresentationPart;

                //Count slides
                int slidesCount = 0;
                if (includeHidden)
                {
                    slidesCount = presentationPart.SlideParts.Count();
                }
                else
                {
                    var slides = presentationPart.SlideParts.Where((s) => (s.Slide != null) && ((s.Slide.Show == null) || (s.Slide.Show.HasValue && s.Slide.Show.Value)));
                    slidesCount = slides.Count();
                }
                Console.WriteLine("Slides counts={0}", slidesCount);

                Presentation presentation = presentationPart.Presentation;









                foreach (SlideId slideId in presentation.SlideIdList)

                // foreach (var slide in presentationPart.SlideParts)
                {

                    String relId = slideId.RelationshipId.Value;

                    SlidePart slide = (SlidePart)presentation.PresentationPart.GetPartById(relId);
                    // Perform actions on SlidePart.

                
                    Console.WriteLine("\n\n\n********************************");
                    //Get title
                    var shapes = from shape in slide.Slide.Descendants<Shape>()
                                 where IsTitleShape(shape)
                                 select shape;
                    StringBuilder paragraphTexttit = new StringBuilder();
                    string paragraphSeparator = null;
                    foreach (var shape in shapes)
                    {
                        // Get the text in each paragraph in this shape.
                        foreach (var paragraph in shape.TextBody.Descendants<DocumentFormat.OpenXml.Drawing.Paragraph>())
                        {
                            // Add a line break.
                            paragraphTexttit.Append(paragraphSeparator);

                            foreach (var text in paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
                            {
                                paragraphTexttit.Append(text.Text);
                            }

                            paragraphSeparator = "\n";
                        }
                    }
                    Console.WriteLine("\t\t" + paragraphTexttit.ToString());
                    Console.WriteLine("----------------------");

                    //GEt all text

                    foreach (var paragraph in slide.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Paragraph>())
                    {
                        // Create a new string builder.                    
                        StringBuilder paragraphText = new StringBuilder();

                        // Iterate through the lines of the paragraph.
                        foreach (var text in paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
                        {
                            // Append each line to the previous lines.
                            paragraphText.Append(text.Text);
                        }

                        if (paragraphText.Length > 0)
                        {
                            // Add each paragraph to the linked list.
                            Console.WriteLine(paragraphText.ToString());
                        }
                    }

                    //Get all images
                    foreach (var pic in slide.Slide.Descendants<Picture>())
                    {
                        // First, get relationship id of image
                        string rId = pic.BlipFill.Blip.Embed.Value;

                        ImagePart imagePart = (ImagePart)slide.GetPartById(rId);

                        // Get the original file name.
                        Console.Out.WriteLine("$$Image:" + imagePart.Uri.OriginalString);
                        // Get the content type (e.g. image/jpeg).
                        // Console.Out.WriteLine("content-type: {0}", imagePart.ContentType);

                        // GetStream() returns the image data
                        // System.Drawing.Image img = System.Drawing.Image.FromStream(imagePart.GetStream());

                        // You could save the image to disk using the System.Drawing.Image class
                        //  img.Save(@"c:\temp\temp.jpg"); 
                    }

                }
            }
        }
        // Determines whether the shape is a title shape.
        private static bool IsTitleShape(Shape shape)
        {
            var placeholderShape = shape.NonVisualShapeProperties.ApplicationNonVisualDrawingProperties.GetFirstChild<PlaceholderShape>();
            if (placeholderShape != null && placeholderShape.Type != null && placeholderShape.Type.HasValue)
            {
                switch ((PlaceholderValues)placeholderShape.Type)
                {
                    // Any title shape.
                    case PlaceholderValues.Title:

                    // A centered title.
                    case PlaceholderValues.CenteredTitle:
                        return true;

                    default:
                        return false;
                }
            }
            return false;
        }
    }
}
