using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dfs;

namespace Tuck
{
    class TuckClient : DfsClient
    {
        public TuckClient() {
            DIO.Client = this;
        }

        public override void RemoteTask(string[] args)
        {
            Console.WriteLine("Recieved task");
            ParseArgs(args);
        }

        void DistortCmd(Guid session, string[] args)
        {
            string path = string.Empty;
            int distortions = 0;
            if (args.Length == 3)
            {
                path = args[1];
                distortions = int.Parse(args[2]);
            }
            else
            {
                Console.Write("Target directory or file: ");
                path = IOConsole.ReadAndParsePath();

                Console.Write("How many distortions per image?: ");
                distortions = int.Parse(Console.ReadLine());
            }

            if (DIO.Directory.Exists(path))
            {
                foreach (string file in DIO.Directory.GetFiles(path))
                {
                    Magic.DefaultDistort(file, session, distortions);
                }
            }
            else
            {
                Magic.DefaultDistort(path, session, distortions);
            }
        }

        void EvalCmd(string[] args)
        {
            bool isDir = false;

            string path = string.Empty;
            string labelsPath = string.Empty;
            string modelPath = string.Empty;

            if (args.Length == 3)
            {
                path = args[1];
                isDir = DIO.Directory.Exists(path);

                string trainedDir = IOConsole.ParseDir(args[2]);
                labelsPath = IOConsole.ParseFile($"{trainedDir}/labels.txt");
                modelPath = IOConsole.ParseFile($"{trainedDir}/mobile.bytes");
            }
            else if (args.Length == 4)
            {
                path = args[1];
                isDir = DIO.Directory.Exists(path);

                labelsPath = IOConsole.ParseFile(args[2]);
                modelPath = IOConsole.ParseFile(args[3]);
            }
            else
            {
                Console.Write("Target image: ");
                path = IOConsole.ReadAndParsePath();
                isDir = DIO.Directory.Exists(path);

                Console.Write("Trained dir: ");
                string trainedDir = IOConsole.ReadAndParseDir();
                labelsPath = IOConsole.ParseFile($"{trainedDir}/labels.txt");
                modelPath = IOConsole.ParseFile($"{trainedDir}/mobile.bytes");
            }

            if (isDir)
                Eval.EvalDir(path, labelsPath, modelPath);
            else
                Eval.EvalFile(path, labelsPath, modelPath);
        }

        void ReportCmd(Guid session, string[] args)
        {
            string path = string.Empty;
            string labelsPath = string.Empty;
            string modelPath = string.Empty;

            if (args.Length == 3)
            {
                path = IOConsole.ParseDir(args[1]);

                string trainedDir = IOConsole.ParseDir(args[2]);
                labelsPath = IOConsole.ParseFile($"{trainedDir}/labels.txt");
                modelPath = IOConsole.ParseFile($"{trainedDir}/mobile.bytes");
            }
            else if (args.Length == 4)
            {
                path = IOConsole.ParseDir(args[1]);
                labelsPath = IOConsole.ParseFile(args[2]);
                modelPath = IOConsole.ParseFile(args[3]);
            }
            else
            {
                Console.Write("Target image: ");
                path = IOConsole.ReadAndParseDir();

                Console.Write("Trained dir: ");
                string trainedDir = IOConsole.ReadAndParseDir();
                labelsPath = IOConsole.ParseFile($"{trainedDir}/labels.txt");
                modelPath = IOConsole.ParseFile($"{trainedDir}/mobile.bytes");
            }

            var sheets = new GSheets();
            string sheetName = new System.IO.DirectoryInfo(path).Name;
            sheets.AddExcelSheet(sheetName);

            var finalReport = Eval.ReportEvalDir(path, labelsPath, modelPath);

            Console.WriteLine("=========================");

            int excelIndex = 2;
            foreach (Report report in finalReport)
            {
                Console.WriteLine($"{report.Name}: {report.Successes}/{report.Size}, Avg Confidence: {report.AvgConfidence}");
                sheets.AddReportExcel(sheetName, report, excelIndex);
                excelIndex++;
            }
        }

        void TrainCmd(string[] args)
        {
            string trainingDir = string.Empty;
            bool distort = false;
            if (args.Length <= 3 && args.Length > 1)
            {
                trainingDir = IOConsole.ParseDir(args[1]);
            }
            if (args.Length == 3)
            {
                distort = args[2].ToLower() == "true";
            }
            // ...
        }
        
        public void ParseArgs(string[] args)
        {
            string cmd = args[0].ToLower();

            if (cmd == "distort")
                DistortCmd(DIO.Client.Session, args);
            else if (cmd == "eval")
                EvalCmd(args);
            else if (cmd == "report")
                ReportCmd(DIO.Client.Session, args);
            else if (cmd == "train")
                TrainCmd(args);
            else if (cmd == "node")
            {
                DIO.Client.RegisterRemote().Wait();
                while (true) { Console.ReadLine(); }
            }
            else if (cmd == "remote")
            {
                string sout = string.Empty;
                foreach (string s in args) {
                    sout += s + " ";
                }
                sout = sout.Replace("remote", "").Trim();

                Console.WriteLine("Registering this remote");
                DIO.Client.RegisterRemote().Wait();
                // Console.ReadLine();
                Console.WriteLine("Writing to Stream");                
                DIO.Client.CallRemote(sout).Wait();
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("distort [file/dir] [number_of_distortions]");
                Console.WriteLine("eval [img_path] [trained_dir]");
                Console.WriteLine("report [eval_dir] [trained_dir]");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var t = new TuckClient();
            t.ParseArgs(args);
        }
    }
}
