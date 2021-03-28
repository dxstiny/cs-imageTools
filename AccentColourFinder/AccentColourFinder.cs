using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace AccentColour
{
    public class Finder
    {
        private readonly PictureAnalyser pictureAnalyser = new PictureAnalyser();
        public TimeSpan analysationTime;

        public int pixelsAnalysed { get { return pictureAnalyser.pixelsAnalysed; } }

        public async Task<List<Color>> analyse(Image image, int depth = 1)
        {
            return await analyse(new Bitmap(image), depth);
        }

        public async Task<List<Color>> analyse(string file, int depth = 1)
        {
            return await analyse(new Bitmap(file), depth);
        }

        public async Task<List<Color>> analyse(Bitmap bitmap, int depth = 1)
        {
            Stopwatch sw = new Stopwatch();
            Dictionary<string, TimeSpan> times = new Dictionary<string, TimeSpan>();
            sw.Start();

            List<Color> ret = new List<Color>();

            times.Add("analyse()", sw.Elapsed);
            await pictureAnalyser.GetMostUsedColour(bitmap, depth);
            times.Add("analysed", sw.Elapsed);
            List<Color> mColours = pictureAnalyser.TenMostUsedColors;
            List<int> aColours = pictureAnalyser.TenMostUsedColorIncidences;

            times.Add("sortList()", sw.Elapsed);
            List<int> indices = sortList(ref mColours, ref aColours);
            times.Add("sorted List", sw.Elapsed);

            foreach (int indice in indices)
            {
                ret.Add(mColours[indice]);
            }

            times.Add("return", sw.Elapsed);
            sw.Stop();
            analysationTime = sw.Elapsed;
            return ret;
        }

        public static Bitmap GetColourAsShadowBitmap(Color colour)
        {
            int w = 170, h = 100;
            Bitmap pic = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int i = 0; i < w / 2; i++)
            {
                colour = Color.FromArgb(GetAlpha(w, i), colour);

                for (int j = 0; j < h; j++)
                {
                    pic.SetPixel(i, j, colour);
                    pic.SetPixel(w - i - 1, j, colour);
                }
            }

            return pic;
        }

        private static int GetAlpha(int w, int i)
        {
            w /= 2;
            i /= 2;

            int value;

            if (i < w / 2)
            {
                value = i;
            }
            else
            {
                value = w - i;
            }

            int ret = Convert.ToInt32(Math.Round(value * 250.0 / w));
            return ret;
        }

        public static Bitmap GetColourAsBitmap(Color colour, int w = 100, int h = 100)
        {
            Bitmap pic = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    pic.SetPixel(i, j, colour);
                }
            }

            return pic;
        }

        private readonly List<int> vals = new List<int>();
        private List<Color> cols;

        private List<int> sortList(ref List<Color> colours, ref List<int> appearances)
        {
            int colourWeight = 5;
            int appearanceWeight = 1;
            int maxScoreEach = 510;
            int weight = colourWeight + appearanceWeight;
            int maxScore = maxScoreEach * weight;

            cols = colours;

            double appearanceMultiplier = 510.0 / appearances[0];

            for (int i = 0; i < colours.Count; i++)
            {
                int colourScore = getDif(Convert.ToInt32(colours[i].R), Convert.ToInt32(colours[i].G)) + getDif(Convert.ToInt32(colours[i].R), Convert.ToInt32(colours[i].B));
                colourScore *= colourWeight;
                double appearanceScore = appearances[i] * appearanceMultiplier;
                appearanceScore *= appearanceWeight;
                int score = maxScore - (colourScore + Convert.ToInt32(appearanceScore));

                vals.Add(score);
            }

            List<int> copy = new List<int>(vals);
            copy.Sort();

            List<int> indices = new List<int>();

            for (int j = 0; j < 10 && j < copy.Count; j++)
            {
                indices.Add(vals.IndexOf(copy[j]));
            }

            return indices;
        }

        private int getDif(int v1, int v2)
        {
            if (v1 > v2)
            {
                return (v1 - v2);
            }
            else
            {
                return (v2 - v1);
            }
        }

        private class PictureAnalyser
        {
            public int pixelsAnalysed = 0;

            public List<Color> TenMostUsedColors { get; private set; }
            public List<int> TenMostUsedColorIncidences { get; private set; }

            public Color MostUsedColor { get; private set; }
            public int MostUsedColorIncidence { get; private set; }

            private int pixelColor;

            private Dictionary<int, int> dctColorIncidence;

            // higher gap -> worse result, but faster

            public async Task GetMostUsedColour(string filename, int gap = 1)
            {
                Bitmap bitmap = new Bitmap(filename);
                GetMostUsedColour(bitmap, gap);
            }

            public async Task GetMostUsedColour(Bitmap theBitMap, int gap = 1)
            {
                Stopwatch sw = new Stopwatch();
                Dictionary<string, TimeSpan> times = new Dictionary<string, TimeSpan>();
                sw.Start();

                TenMostUsedColors = new List<Color>();
                TenMostUsedColorIncidences = new List<int>();

                MostUsedColor = Color.Empty;
                MostUsedColorIncidence = 0;

                dctColorIncidence = new Dictionary<int, int>();

                bool similar;

                times.Add("initialised", sw.Elapsed);

                int sizeMultiplier = (theBitMap.Size.Width / 1000);
                sizeMultiplier = sizeMultiplier >= 1 ? sizeMultiplier : 1;

                gap *= sizeMultiplier;

                gap = gap > theBitMap.Size.Height ? theBitMap.Size.Height - 1 : gap;

                if (gap > theBitMap.Size.Height || gap > theBitMap.Size.Width)
                {
                    throw new Exception($"gap too high: (sizeMultiplier: {sizeMultiplier}, multiplied gap: {gap}, bitmap size: {theBitMap.Size.Width}x{theBitMap.Size.Height})");
                }

                times.Add("big loop", sw.Elapsed);
                for (int row = 0; row < theBitMap.Size.Width - (gap - 1); row += gap)
                {
                    for (int col = 0; col < theBitMap.Size.Height - (gap - 1); col += gap)
                    {
                        pixelsAnalysed++;
                        similar = false;

                        pixelColor = theBitMap.GetPixel(row, col).ToArgb();

                        if (dctColorIncidence.Keys.Contains(pixelColor))
                        {
                            dctColorIncidence[pixelColor]++;
                        }
                        else
                        {
                            foreach (KeyValuePair<int, int> pair in dctColorIncidence)
                            {
                                if (getColourDif(pair.Key, pixelColor) < 100000)
                                {
                                    dctColorIncidence[pair.Key]++;
                                    similar = true;
                                    break;
                                }
                            }

                            if (!similar)
                            {
                                dctColorIncidence.Add(pixelColor, 1);
                            }
                        }
                    }
                }

                times.Add("big loop ended", sw.Elapsed);

                Dictionary<int, int> dctSortedByValueHighToLow = dctColorIncidence.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                foreach (KeyValuePair<int, int> kvp in dctSortedByValueHighToLow)
                {
                    TenMostUsedColors.Add(Color.FromArgb(kvp.Key));
                    TenMostUsedColorIncidences.Add(kvp.Value);
                }

                MostUsedColor = Color.FromArgb(dctSortedByValueHighToLow.First().Key);
                MostUsedColorIncidence = dctSortedByValueHighToLow.First().Value;

                times.Add("end", sw.Elapsed);
            }

            private double getColourDif(Color c1, Color c2)
            {
                double grC1 = .11 * c1.B + .59 * c1.G + .30 * c1.R;
                double grC2 = .11 * c2.B + .59 * c2.G + .30 * c2.R;

                double difference = (grC1 - grC2) * 100.0 / 255.0;

                return difference;
            }

            private int getColourDif(int c1, int c2)
            {
                int difference = c1 - c2;

                if (difference < 0)
                {
                    difference *= -1;
                }

                return difference;
            }
        }
    }
}
