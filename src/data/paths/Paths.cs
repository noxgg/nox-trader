namespace noxiousET.src.data.paths
{
    class Paths
    {
        public string logPath { set; get; }
        public string clientPath { set; get; }
        public string configPath { set; get; }
        public string eveSettingsPath { set; get; }
        public static readonly string characterDataSubDir = @"CharacterData\";
        public static readonly string clientSettingsSubDir = @"ClientSettings\";

        public Paths()
        {
            logPath = @"A:\Users\nox\Documents\EVE\logs\Marketlogs\";
            clientPath = @"G:\EVE\eve.exe";
            configPath = @"D:\Dropbox\Dropbox\Apps\noxiousETConfig\";
            eveSettingsPath = @"C:\Users\nox\AppData\Local\CCP\EVE\c_games_eve_tranquility\settings\";

        }
    }
}
