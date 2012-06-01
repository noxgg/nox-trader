using System;
using System.Collections.Generic;
using System.IO;

namespace noxiousET.src.model.data.io
{
    public class TextFileio
    {
        public String path { set; get; }
        public String fileName { set; get; }
        protected TextReader textReader;
        protected TextWriter textWriter;

        public TextFileio(String path, String fileName)
        {
            this.path = path;
            this.fileName = fileName;
        }

        protected int readOpen()
        {
            try
            {
                textReader = new StreamReader(path + fileName);
            }
            catch
            {
                //TODO: No previous settings found exception
                return 1;
            }
            return 0;
        }

        protected int readOpen(String path, String fileName)
        {
            this.path = path;
            this.fileName = fileName;
            return readOpen();
        }

        protected int readClose()
        {
            textReader.Close();
            return 0;
        }

        protected int writeOpen()
        { 
            try
            {
                if (path.Length > 0 && !Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (!File.Exists(path + fileName))
                    File.Create(path + fileName).Close();
                textWriter = new StreamWriter(path + fileName);
            }
            catch
            {
                //TODO: No previous settings found exception
                return 1;
            }
            return 0;
        }

        protected int writeOpen(String path, String fileName)
        {
            this.path = path;
            this.fileName = fileName;
            return writeOpen();
        }

        protected int writeClose()
        {
            textWriter.Close();
            return 0;
        }

        protected int readLineAsInt()
        {
            return Convert.ToInt32(textReader.ReadLine());
        }

        protected String readLine()
        {
            return textReader.ReadLine();
        }

        protected List<String> readFile()
        {
            List<String> settings = new List<String>();
            while (textReader.Peek() >= 0)
                settings.Add(textReader.ReadLine());
            return settings;
        }

        protected int writeLine(Object line)
        {
            textWriter.WriteLine(line);
            return 0;
        }

        public int save(List<Object> settings)
        {
            if (writeOpen() != 0)
            {
                //Exception
                return 1;
            }

            foreach (Object o in settings)
            {
                writeLine(o);
            }

            writeClose();

            return 0;
        }

        public int save(List<Object> settings, String path, String file)
        {
            this.path = path;
            this.fileName = file;
            return save(settings);
        }

        public List<String> read()
        {
            try
            {
                readOpen();
                List<String> result = readFile();
                readClose();
                return result;
            }
            catch
            {
                return new List<String>();
            }
        }

        public List<String> read(String path, String file)
        {
            this.path = path;
            this.fileName = file;
            return read();
        }
    }
}
