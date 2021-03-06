﻿using System.IO;
using System.Linq;
using System.Threading;

namespace noxiousET.src.helpers
{
    class FileHandler
    {
        private StreamReader file;
        private string path;
        private string fileName;

        public FileHandler(string path)
        {
            this.path = path;
        }

        public int openNewestFile(string path)
        {
            var directory = new DirectoryInfo(path);
            try
            {

                var fileTemp = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
                file = new System.IO.StreamReader(directory.ToString() + fileTemp.ToString());
                fileName = fileTemp.ToString();
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        public string readLine()
        {
            return file.ReadLine();
        }

        public int close()
        {
            int tryFailed = 0;
            var directory = new DirectoryInfo(path);
            file.Close();
            do
                try
                {
                    File.Delete(directory.ToString() + fileName);
                    return 0;
                }
                catch
                {
                    ++tryFailed;
                    Thread.Sleep(2000);

                }
            while (tryFailed < 3);
            return 1;
        }

        public string getFileName()
        {
            return fileName;
        }
    }
}