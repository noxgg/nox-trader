using System;
using System.Collections.Generic;

namespace noxiousET.src.data.io
{
    internal class MarketOrderio : TextFileio
    {
        public new List<String[]> Read(String path, String fileName)
        {
            Path = path;
            FileName = fileName;
            return Read();
        }

        public new List<String[]> Read()
        {
            var result = new List<String[]>();

            ReadOpen();
            string line = ReadLine();
            line = ReadLine(); //Skip past header
            while (line != null)
            {
                result.Add(line.Split(','));
                line = ReadLine();
            }
            ReadClose();
            return result;
        }

        public String[] ReadFirstEntry(String path, String fileName)
        {
            String[] line = ReadFirst(ref path, ref fileName);
            Delete();
            return line;
        }

        private String[] ReadFirst(ref String path, ref String fileName)
        {
            Path = path;
            FileName = fileName;
            ReadOpen();
            ReadLine();
            string line = ReadLine(); 
            ReadClose();
            if (line != null)
            {
                return line.Split(',');
            }
            else
            {
                return null;
            }
        }

        public String[] ReadFirstEntryNoDelete(String path, String fileName)
        {
            return ReadFirst(ref path, ref fileName);
        }
    }
}