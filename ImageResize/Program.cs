using System;
using System.IO;

namespace ImageResize
{
    class Program
    {
        const string imagesPath = @"E:\Projects\Test\ImageResize\ImageResize\ImageResize\Images";
        static void Main(string[] args)
        {
            DirectoryInfo di = new DirectoryInfo(imagesPath);
            var files = di.GetFiles();

            ImageUtility imageUtility = new ImageUtility(400, 400);
            foreach (var item in files)
            {
                if (!item.Name.StartsWith("Resize-"))
                {
                    try
                    {
                        var resize = imageUtility.Resize(item.OpenRead(), 400, 400, 1);
                        var savePath = Path.Combine(imagesPath, $"Resize-{item.Name}");
                        using (var fs = File.Open(savePath, FileMode.Create))
                            fs.Write(resize);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            Console.WriteLine("Finish...");
            Console.ReadLine();
        }
    }
}
