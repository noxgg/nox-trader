using System;
using System.Collections.Generic;

namespace noxiousET.src.data.io
{
    class TextFileToDictionaryLoader : TextFileio
    {
        public TextFileToDictionaryLoader(String path, String fileName) : base(path, fileName)
        {
        }

        public Dictionary<int, int> loadIntKeyEqualsIntValueEqualsOneLine()
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            int line;
            readOpen();
            if (textReader == null)
                return result;
            while (textReader.Peek() >= 0)
            {
                line = Convert.ToInt32(textReader.ReadLine());
                result.Add(line, line);
            }
            readClose();
            return result;
        }

        public Dictionary<int, int> loadIntKeyEqualsIntValueEqualsOneLine(String path, String fileName)
        {
            this.path = path;
            this.fileName = fileName;
            return loadIntKeyEqualsIntValueEqualsOneLine();
        }

        public Dictionary<int, String> loadIntKeyStringValue(String path, String fileName)
        {
            this.path = path;
            this.fileName = fileName;
            return loadIntKeyStringValue();
        }

        public Dictionary<int, String> loadIntKeyStringValue()
        {
            Dictionary<int, String> result = new Dictionary<int, String>();
            String [] line;
            readOpen();
            if (textReader == null)
                return result;
            while (textReader.Peek() >= 0)
            {
                line = textReader.ReadLine().Split('=');
                result.Add(Convert.ToInt32(line[0]), line[1]);
            }
            readClose();
            return result;
        }
    }
}
