using System;
using Dfs;

namespace Tuck
{
    public static class IOConsole
    {
        public static string ParsePath(string s)
        {
            if (DIO.File.Exists(s))
                return s;
            else if (DIO.Directory.Exists(s))
                return s;

            const string err = "Path does not exist";
            Console.WriteLine(err);
            throw new Exception(err);
        }

        public static string ReadAndParsePath()
        {
            string s = Console.ReadLine();
            return ParsePath(s);
        }

        public static string ReadAndParseFile()
        {
            string s = Console.ReadLine();
            return ParseFile(s);
        }

        public static string ParseFile(string s)
        {
            if (!DIO.File.Exists(s))
            {
                string err = "File does not exist!";
                Console.WriteLine(err);
                throw new Exception(err);
            }

            return s;
        }

        public static string ReadAndParseDir()
        {
            string s = Console.ReadLine();
            return ParseDir(s);
        }

        public static string ParseDir(string s)
        {
            if (!DIO.Directory.Exists(s))
            {
                string err = "Directory does not exist!";
                Console.WriteLine(err);
                throw new Exception(err);
            }

            return s;
        }
    }
}