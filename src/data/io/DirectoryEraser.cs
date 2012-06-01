using System.IO;
using System.Linq;

namespace noxiousET.src.data.io
{
    class DirectoryEraser
    {
        public static void nuke(string directory)
        {
            var directoryVar = new DirectoryInfo(directory);
            StreamReader file;

            while (Directory.GetFiles(directory).Length > 0)
            {
            TryLabel:
                try
                {
                    var fileTemp = directoryVar.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
                    file = new System.IO.StreamReader(directory.ToString() + fileTemp.ToString());
                    file.Close();
                    File.Delete(directory.ToString() + fileTemp.ToString());
                }
                catch
                {
                    goto TryLabel;
                }
            }
        }
    }
}
