namespace noxiousET.src.data.paths
{
    class Paths
    {
        public string logPath { set; get; }
        public string clientPath { set; get; }
        public string configPath { set; get; }
        public static readonly string characterDataSubDir = @"CharacterData\";

        public Paths()
        {
            logPath = @"A:\Users\nox\Documents\EVE\logs\Marketlogs\";
            clientPath = @"G:\EVE\eve.exe";
            configPath = @"D:\Dropbox\Dropbox\Apps\noxiousETConfig\";
        }
    }
}
