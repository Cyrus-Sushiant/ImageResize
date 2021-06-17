using SkiaSharp;
using System;
using System.IO;

namespace ImageResize
{
    public class ImageUtility
    {
        private readonly int _minWidth;
        private readonly int _minHeight;

        public ImageUtility(int minWidth, int minHeight)
        {
            _minWidth = minWidth;
            _minHeight = minHeight;
        }

        public byte[] Resize(Stream sourceFile, int width, int height, double resizeRatio, int quality = 100)
        {
            // SKBitmap resizes crisp.
            using (var srcBitmap = SKBitmap.Decode(sourceFile))
            {
                int sourceWidth = srcBitmap.Width;
                int sourceHeight = srcBitmap.Height;

                if (sourceWidth < _minWidth)
                {
                    throw new ArgumentOutOfRangeException("Width", sourceWidth, "Your image is smaller than the standard size.");
                }

                if (sourceHeight < _minHeight)
                {
                    throw new ArgumentOutOfRangeException("Height", sourceHeight, "Your image is smaller than the standard size.");
                }

                var nominalRatio = Math.Min((double)width / (double)sourceWidth, (double)height / (double)sourceHeight);

                var adjustRatio = nominalRatio * resizeRatio;

                var finalWidth = (int)(sourceWidth * adjustRatio);
                var finalHeight = (int)(sourceHeight * adjustRatio);

                using (var newBmp = srcBitmap.Resize(new SKImageInfo(finalWidth, finalHeight), SKFilterQuality.High))
                {
                    if (finalWidth == width && finalHeight == height)
                    {
                        using (var img = SKImage.FromBitmap(newBmp))
                        using (var data = img.Encode(SKEncodedImageFormat.Png, quality))
                            return data.ToArray();
                    }
                    else
                    {
                        using (var tempSurface = SKSurface.Create(new SKImageInfo(width, height)))
                        using (var canvas = tempSurface.Canvas)
                        {
                            //set background color
                            canvas.Clear(SKColors.Transparent);

                            int offset = (width - newBmp.Width) / 2;
                            int offsetTop = (height - newBmp.Height) / 2;

                            canvas.DrawBitmap(newBmp, SKRect.Create(offset, offsetTop, newBmp.Width, newBmp.Height));

                            using (var finalImage = tempSurface.Snapshot())
                            using (var data = finalImage.Encode(SKEncodedImageFormat.Png, quality))
                                return data.ToArray();
                        }
                    }
                }
            }
        }
    }
}
