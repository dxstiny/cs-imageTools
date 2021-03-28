using System;
using System.Collections.Generic;
using System.Drawing;

namespace AccentColourFinder
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Console.WriteLine("loading image...");

            // take screenshot
            ScreenCapture sc = new ScreenCapture();
            Image img = sc.CaptureScreen();
            
            Console.WriteLine($"analysing... ({sw.Elapsed})");
            
            // get the accent colour of the screenshot
            AccentColour.Finder finder = new AccentColour.Finder();
            List<Color> colours = finder.analyse(img, 1).Result;
            
            Console.WriteLine($"analysed... ({sw.Elapsed})");
            Console.WriteLine($"analysed {finder.pixelsAnalysed} pixels in {finder.analysationTime.TotalMilliseconds} ms");

            // save all detected accent colours
            for (int i = 0; i < colours.Count; i++)
            {
                AccentColour.Finder.GetColourAsBitmap(colours[i]).Save($"output/accents/colour-{i}.jpg");
            }

            // save analysed image
            img.Save("output/img.jpg");

            Console.WriteLine($"finished... ({sw.Elapsed})");
        }
    }
}
