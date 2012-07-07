using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace noxiousET.src.data.io
{
    class MarketOrderio : TextFileio
    {
        public MarketOrderio()
        {

        }

        public new List<String[]> read(String path, String fileName)
        {
            this.path = path;
            this.fileName = fileName;
            return read();
        }

        public new List<String[]> read()
        {
            string line;
            List<String[]> result = new List<String[]>();

            readOpen();
            line = readLine();
            line = readLine(); //Skip past header
            while (line != null)
            {
                result.Add(line.Split(','));
                line = readLine();
            }
            readClose();
            return result;
        }

        public String[] readFirstEntry(String path, String fileName)
        {
            String[] line = readFirst(ref path, ref fileName);
            delete();
            return line;
        }

        private String[] readFirst(ref String path, ref String fileName)
        {
            this.path = path;
            this.fileName = fileName;
            String[] line = new String[14];
            readOpen();
            readLine();
            line = readLine().Split(',');
            readClose();
            return line;
        }

        public String[] readFirstEntryNoDelete(String path, String fileName)
        {
            return readFirst(ref path, ref fileName);
        }
    }
}
