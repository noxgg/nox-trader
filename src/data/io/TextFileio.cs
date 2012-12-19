using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace noxiousET.src.data.io
{
    public class TextFileio
    {
        protected TextReader TextReader;
        protected TextWriter TextWriter;

        public TextFileio()
        {
            Path = Path;
            FileName = FileName;
        }

        public TextFileio(String path, String fileName)
        {
            Path = path;
            FileName = fileName;
        }

        public String Path { set; get; }
        public String FileName { set; get; }

        protected int ReadOpen()
        {
            try
            {
                TextReader = new StreamReader(Path + FileName);
            }
            catch
            {
                //TODO: No previous settings found exception
                return 1;
            }
            return 0;
        }

        protected int ReadOpen(String path, String fileName)
        {
            Path = path;
            FileName = fileName;
            return ReadOpen();
        }

        protected int ReadClose()
        {
            TextReader.Close();
            return 0;
        }

        protected int WriteOpen()
        {
            try
            {
                if (Path.Length > 0 && !Directory.Exists(Path))
                    Directory.CreateDirectory(Path);
                if (!File.Exists(Path + FileName))
                    File.Create(Path + FileName).Close();
                TextWriter = new StreamWriter(Path + FileName);
            }
            catch
            {
                //TODO: No previous settings found exception
                return 1;
            }
            return 0;
        }

        protected int WriteOpen(String path, String fileName)
        {
            Path = path;
            FileName = fileName;
            return WriteOpen();
        }

        protected int WriteClose()
        {
            TextWriter.Close();
            return 0;
        }

        protected int ReadLineAsInt()
        {
            return Convert.ToInt32(TextReader.ReadLine());
        }

        protected String ReadLine()
        {
            try
            {
                return TextReader.ReadLine();
            }
            catch
            {
                return null;
            }
        }

        protected List<String> ReadFile()
        {
            var settings = new List<String>();
            while (TextReader.Peek() >= 0)
                settings.Add(TextReader.ReadLine());
            return settings;
        }

        protected int WriteLine(Object line)
        {
            TextWriter.WriteLine(line);
            return 0;
        }

        public int Save(List<Object> settings)
        {
            if (WriteOpen() != 0)
            {
                //Exception
                return 1;
            }

            foreach (object o in settings)
            {
                WriteLine(o);
            }

            WriteClose();

            return 0;
        }

        public int Save(List<Object> settings, String path, String file)
        {
            Path = path;
            FileName = file;
            return Save(settings);
        }

        public List<String> Read()
        {
            try
            {
                ReadOpen();
                List<String> result = ReadFile();
                ReadClose();
                return result;
            }
            catch
            {
                return new List<String>();
            }
        }

        public List<String> Read(String path, String file)
        {
            Path = path;
            FileName = file;
            return Read();
        }

        public int GetNumberOfFilesInDirectory(String directory)
        {
            return Directory.GetFiles(directory).Length;
        }

        public String GetNewestFileNameInDirectory(String directory)
        {
            if (GetNumberOfFilesInDirectory(directory) > 0)
            {
                var directoryVar = new DirectoryInfo(directory);
                return directoryVar.GetFiles().OrderByDescending(f => f.LastWriteTime).First().ToString();
            }
            return null;
        }

        protected void Delete()
        {
            File.Delete(Path + FileName);
        }
    }
}