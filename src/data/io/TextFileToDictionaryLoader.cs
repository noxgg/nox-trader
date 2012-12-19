using System;
using System.Collections.Generic;

namespace noxiousET.src.data.io
{
    internal class TextFileToDictionaryLoader : TextFileio
    {
        public TextFileToDictionaryLoader(String path, String fileName) : base(path, fileName)
        {
        }

        public Dictionary<int, int> LoadIntKeyEqualsIntValueEqualsOneLine()
        {
            var result = new Dictionary<int, int>();
            ReadOpen();
            if (TextReader == null)
                return result;
            while (TextReader.Peek() >= 0)
            {
                int line = Convert.ToInt32(TextReader.ReadLine());
                result.Add(line, line);
            }
            ReadClose();
            return result;
        }

        public Dictionary<int, int> LoadIntKeyEqualsIntValueEqualsOneLine(String path, String fileName)
        {
            Path = path;
            FileName = fileName;
            return LoadIntKeyEqualsIntValueEqualsOneLine();
        }

        public Dictionary<int, String> LoadIntKeyStringValue(String path, String fileName)
        {
            Path = path;
            FileName = fileName;
            return LoadIntKeyStringValue();
        }

        public Dictionary<int, String> LoadIntKeyStringValue()
        {
            var result = new Dictionary<int, String>();
            ReadOpen();
            if (TextReader == null)
                return result;
            while (TextReader.Peek() >= 0)
            {
                String[] line = TextReader.ReadLine().Split('=');
                result.Add(Convert.ToInt32(line[0]), line[1]);
            }
            ReadClose();
            return result;
        }
    }
}