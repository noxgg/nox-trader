using System.IO;
using System.Linq;

namespace noxiousET.src.data.io
{
    internal class DirectoryEraser
    {
        public static void Nuke(string directory)
        {
            var directoryInfo = new DirectoryInfo(directory);

            while (Directory.GetFiles(directory).Length > 0)
            {
                TryLabel:
                try
                {
                    FileInfo newestFileName = directoryInfo.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
                    var file = new StreamReader(directory + newestFileName);
                    file.Close();
                    File.Delete(directory + newestFileName);
                }
                catch
                {
                    goto TryLabel;
                }
            }
        }
    }
}