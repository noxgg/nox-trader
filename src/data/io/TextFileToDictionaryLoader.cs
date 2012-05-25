using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            while (textReader.Peek() >= 0)
            {
                line = Convert.ToInt32(textReader.ReadLine());
                result.Add(line, line);
            }
            readClose();
            return result;
        }
    }
}
