using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using Dfs;

namespace Tuck
{
    public class Eval
    {
        public static void EvalFile(string img, string labelsFile, string model) {
            var tensor = ImageUtil.CreateTensorFromImageFile(img);
            var t = new ImageTensor(model, DIO.File.ReadAllLines(labelsFile));
            var matches = t.Parse(tensor);

            var bestMatch = matches[0];
            var secondBestMatch = matches[1];

            Console.WriteLine($"First match: {bestMatch.Name}: {bestMatch.Confidence}");
            Console.WriteLine($"Second match: {secondBestMatch.Name}: {secondBestMatch.Confidence}");
        }

        public static void EvalDir(string dir, string labelsFile, string model)
        {
            var t = new ImageTensor(model, DIO.File.ReadAllLines(labelsFile));
            var files = DIO.Directory.GetFiles(dir);
            string dirName = new System.IO.DirectoryInfo(dir).Name.Trim().ToLower();

            int evalSize = files.Length;
            
            Console.WriteLine($"Evaluating {evalSize} files in {dirName}...");
            for (int fi = 0; fi < evalSize; fi++) {
                var file = files[fi];
                var tensor = ImageUtil.CreateTensorFromImageFile(file);
                
                var matches = t.Parse(tensor);

                var bestMatch = matches[0];
                var secondBestMatch = matches[1];
                
                bool firstIsMatch = bestMatch.Name.Trim().ToLower() == dirName;
                bool secondIsMatch = secondBestMatch.Name.Trim().ToLower() == dirName;
                
                Console.WriteLine($"\nEvaluating {System.IO.Path.GetFileName(file)}");
                Console.WriteLine($"First match:  | TF name: {bestMatch.Name}, confidence: {bestMatch.Confidence}, match: {firstIsMatch}");
                Console.WriteLine($"Second match: | TF name: {secondBestMatch.Name}, confidence: {secondBestMatch.Confidence}, match: {secondIsMatch}");
            }
        }

        public static List<Report> ReportEvalDir(string dir, string labelsFile, string model) {
            var t = new ImageTensor(model, DIO.File.ReadAllLines(labelsFile));
            
            var finalReport = new List<Report>();
            foreach (var subDir in DIO.Directory.GetDirectories(dir)) {
                var files = DIO.Directory.GetFiles(subDir);
                string subDirName = new System.IO.DirectoryInfo(subDir).Name.Trim().ToLower();

                int evalSize = files.Length / 2;
                Report report = new Report() { Name = subDirName.ToLower(), Size = evalSize };
                
                Console.WriteLine($"Evaluating {evalSize} files in {subDirName}...");
                for (int fi = 0; fi < evalSize; fi++) {
                    var file = files[fi];
                    var tensor = ImageUtil.CreateTensorFromImageFile(file);
                    var match = t.Parse(tensor)[0];
                    
                    bool isMatch = match.Name.Trim().ToLower() == subDirName;
                    if (isMatch) {
                        report.Successes++;
                        report.AvgConfidence += match.Confidence;
                    }
                    Console.WriteLine($"{System.IO.Path.GetFileName(file)} | TF name: {match.Name}, confidence: {match.Confidence}, match: {isMatch}");
                }
                report.AvgConfidence /= evalSize;
                finalReport.Add(report);
            }

            return finalReport;
        }
    }
}