using System;
using System.IO;

namespace ImageResize
{
    class Program
    {
        const string imagesPath = @"E:\Projects\Test\ImageResize\ImageResize\ImageResize\Images";
        static void Main(string[] args)
        {
            int width = 500;
            int height = 500;
            double ratio = 1;

            DirectoryInfo di = new DirectoryInfo(imagesPath);
            var files = di.GetFiles();

            ImageUtility imageUtility = new ImageUtility(400, 400);
            foreach (var item in files)
            {
                if (!item.Name.StartsWith("Resize-"))
                {
                    try
                    {
                        var fname = Path.GetFileNameWithoutExtension(item.FullName);
                        SaveImage(imageUtility.ResizeWithSkiaSharp(item.OpenRead(), width, height, ratio), $"Resize-SkiaSharp-{fname}.png");

                        SaveImage(imageUtility.ResizeWithSystemDrawing(item.OpenRead(), width, height, ratio), $"Resize-SystemDrawing-{fname}.png");

                        SaveImage(imageUtility.ResizeWithSystemDrawingThumbnailImage(item.OpenRead(), width, height, ratio), $"Resize-SystemDrawingThumbnailImage-{fname}.png");

                        SaveImage(imageUtility.ResizeWithMagickImage(item.OpenRead(), width, height, ratio), $"Resize-MagickImage-{fname}.png");

                        SaveImage(imageUtility.ResizeWithMagicScaler(item.OpenRead(), width, height, ratio), $"Resize-MagickScaler-{fname}.png");
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            Console.WriteLine("Finish...");
            Console.ReadLine();
        }

        private static void SaveImage(byte[] resize, string fileName)
        {
            var savePath = Path.Combine(imagesPath, "outputs", fileName);
            using (var fs = File.Open(savePath, FileMode.Create))
                fs.Write(resize);
        }
    }
}
