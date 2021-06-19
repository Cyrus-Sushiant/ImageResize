using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using System.IO;

namespace ImageResize.Tests
{
    [TestClass]
    public class ImageUtilityTests
    {
        [TestMethod]
        public void ResizeTest()
        {
            const string imagePath = @"E:\Projects\Test\ImageResize\ImageResize\ImageResize\Images\test (2).jpg";
            ImageUtility imageUtility = new ImageUtility(400, 400);
            var resize = imageUtility.ResizeWithSkiaSharp(File.Open(imagePath, FileMode.Open), 400, 400, 1);

            Assert.IsNotNull(resize);
            Assert.IsTrue(resize.Length > 0);

            using(var bitmap = SKBitmap.Decode(resize))
            {
                Assert.AreEqual(400, bitmap.Width);
                Assert.AreEqual(400, bitmap.Height);
            }
        }

        [TestMethod]
        public void OutOfSize()
        {
            const string imagePath = @"E:\Projects\Test\ImageResize\ImageResize\ImageResize\Images\test (1).png";
            ImageUtility imageUtility = new ImageUtility(400, 400);
            try
            {
                var resize = imageUtility.ResizeWithSkiaSharp(File.Open(imagePath, FileMode.Open), 400, 400, 1);
            }
            catch (System.ArgumentOutOfRangeException ex)
            {
                StringAssert.Contains(ex.Message, "Your image is smaller than the standard size.");
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}
