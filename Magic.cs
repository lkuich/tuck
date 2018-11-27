using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using Dfs;

namespace Tuck
{
    public struct Range {
        public int Min, Max;
    }

    public class Magic
    {
        public static void DefaultDistort(string file, Guid session, int distortions) {
            if (!file.EndsWith(".jpg")) {
                Console.WriteLine(file + " is not a JPEG");
                return;
            }
            Console.WriteLine($"Distorting {file}");
            var bytes = DIO.File.ReadAllBytes(file);
            
            for (int i = 1; i <= distortions; i++) {
                string newFilename = $"{Path.GetFileNameWithoutExtension(file)}_{session.ToString()}_distort{i}.jpg";
                string newFilepath = $"{Path.GetDirectoryName(file)}/{newFilename}".Replace("\\", "/");
                
                Distort(bytes, newFilepath,
                    brightnessRange: new Range() { Min = -20, Max = 20 },
                    rotationRange: new Range() { Min = -180, Max = 180 },
                    zoomRangeX: new Range() { Min = -1000, Max = 1000 },
                    zoomRangeY: new Range() { Min = -1000, Max = 1000 }
                );
            }
        }

        private static void Distort(byte[] file, string path, Range brightnessRange, Range rotationRange, Range zoomRangeX, Range zoomRangeY) {
            var random = new Random();

            int contrast = random.Next(brightnessRange.Min, brightnessRange.Max);
            int brightness = random.Next(brightnessRange.Min, brightnessRange.Max);
            int rotation = random.Next(rotationRange.Min, rotationRange.Max);
            
            int zoomX = random.Next(zoomRangeX.Min, zoomRangeX.Max);
            int zoomY = random.Next(zoomRangeY.Min, zoomRangeY.Max);

            using (var image = new MagickImage(file))
            {
                image.Crop(zoomX, zoomY, image.Width, image.Height);
                image.BrightnessContrast(new Percentage(brightness), new Percentage(contrast));
                image.Rotate(rotation);

                var b = image.ToByteArray();
                Console.WriteLine($"Writing {path}");
                DIO.File.WriteAllBytes(path, b);
            }
        }
    }
}