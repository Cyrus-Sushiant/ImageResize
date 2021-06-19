using ImageMagick;
using PhotoSauce.MagicScaler;
using SkiaSharp;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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

        public byte[] ResizeWithSkiaSharp(Stream sourceFile, int width, int height, double resizeRatio, int quality = 100)
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

        public byte[] ResizeWithSystemDrawing(Stream sourceFile, int width, int height, double resizeRatio)
        {
            using (var srcBitmap = new Bitmap(sourceFile))
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

                var resized = new Bitmap(finalWidth, finalHeight);
                using (var graphics = Graphics.FromImage(resized))
                {
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.High;
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImage(srcBitmap, 0, 0, finalWidth, finalHeight);

                    if (finalWidth == width && finalHeight == height)
                    {
                        using (var ms = new MemoryStream())
                        {
                            resized.Save(ms, ImageFormat.Png);

                            return ms.ToArray();
                        }
                    }
                    else
                    {
                        int offset = (width - finalWidth) / 2;
                        int offsetTop = (height - finalHeight) / 2;

                        var resizedTrans = new Bitmap(width, height);
                        using (var graphicsTrans = Graphics.FromImage(resizedTrans))
                        {
                            graphicsTrans.Clear(Color.Transparent);
                            graphicsTrans.CompositingQuality = CompositingQuality.HighQuality;
                            graphicsTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphicsTrans.CompositingMode = CompositingMode.SourceOver;

                            graphicsTrans.DrawImage(resized, offset, offsetTop, resized.Width, resized.Height);
                        }

                        using (var ms = new MemoryStream())
                        {
                            resizedTrans.Save(ms, ImageFormat.Png);

                            return ms.ToArray();
                        }
                    }
                }
            }
        }

        public byte[] ResizeWithSystemDrawingThumbnailImage(Stream sourceFile, int width, int height, double resizeRatio)
        {
            using (var srcBitmap = new Bitmap(sourceFile))
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

                Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
                var resized = srcBitmap.GetThumbnailImage(finalWidth, finalHeight, myCallback, IntPtr.Zero);

                if (finalWidth == width && finalHeight == height)
                {
                    using (var ms = new MemoryStream())
                    {
                        resized.Save(ms, ImageFormat.Png);

                        return ms.ToArray();
                    }
                }
                else
                {
                    int offset = (width - finalWidth) / 2;
                    int offsetTop = (height - finalHeight) / 2;

                    var resizedTrans = new Bitmap(width, height);
                    using (var graphicsTrans = Graphics.FromImage(resizedTrans))
                    {
                        graphicsTrans.Clear(Color.Transparent);
                        graphicsTrans.CompositingQuality = CompositingQuality.HighQuality;
                        graphicsTrans.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphicsTrans.CompositingMode = CompositingMode.SourceOver;

                        graphicsTrans.DrawImage(resized, offset, offsetTop, resized.Width, resized.Height);
                    }

                    using (var ms = new MemoryStream())
                    {
                        resizedTrans.Save(ms, ImageFormat.Png);

                        return ms.ToArray();
                    }
                }
            }
        }

        public byte[] ResizeWithMagickImage(Stream sourceFile, int width, int height, double resizeRatio, int quality = 100)
        {
            using (var srcBitmap = new MagickImage(sourceFile))
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

                srcBitmap.Resize(finalWidth, finalHeight);
                srcBitmap.Strip();
                srcBitmap.Quality = quality;

                if (finalWidth == width && finalHeight == height)
                {
                    using (var ms = new MemoryStream())
                    {
                        srcBitmap.Write(ms, MagickFormat.Png);
                        return ms.ToArray();
                    }
                }
                else
                {
                    MagickImage backgroundImg = new MagickImage(MagickColors.Transparent, width, height);
                    backgroundImg.Settings.FillColor = MagickColors.Transparent;
                    backgroundImg.Settings.StrokeColor = MagickColors.Transparent;
                    backgroundImg.Composite(srcBitmap, Gravity.Center, CompositeOperator.SrcOver);
                    using (var ms = new MemoryStream())
                    {
                        backgroundImg.Write(ms, MagickFormat.Png);
                        return ms.ToArray();
                    }
                }
            }
        }

        public byte[] ResizeWithMagicScaler(Stream sourceFile, int width, int height, double resizeRatio)
        {
            using (var srcBitmap = new MagickImage(sourceFile))
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

                var settings = new ProcessImageSettings()
                {
                    Width = finalWidth,
                    Height = finalHeight,
                    ResizeMode = CropScaleMode.Max,
                    SaveFormat = FileFormat.Png
                };


                sourceFile.Seek(0, SeekOrigin.Begin);
                if (finalWidth == width && finalHeight == height)
                {
                    using (var ms = new MemoryStream())
                    {
                        MagicImageProcessor.ProcessImage(sourceFile, ms, settings);
                        return ms.ToArray();
                    }
                }
                else
                {
                    using (var ms = new MemoryStream())
                    {
                        MagicImageProcessor.ProcessImage(sourceFile, ms, settings);
                        return ms.ToArray();
                    }
                }
            }
        }

        private bool ThumbnailCallback()
        {
            return false;
        }
    }
}
