namespace noxiousET.src.data.paths
{
    internal class Paths
    {
        public const string CharacterDataSubDir = @"CharacterData\";
        public const string ClientSettingsSubDir = @"ClientSettings\";

        public Paths()
        {
            LogPath = @"A:\Users\nox\Documents\EVE\logs\Marketlogs\";
            ClientPath = @"G:\EVE\eve.exe";
            ConfigPath = @"D:\Dropbox\Dropbox\Apps\noxiousETConfig\";
            EveSettingsPath = @"C:\Users\nox\AppData\Local\CCP\EVE\c_games_eve_tranquility\settings\";
        }

        public string LogPath { set; get; }
        public string ClientPath { set; get; }
        public string ConfigPath { set; get; }
        public string EveSettingsPath { set; get; }
        public string LocalDropboxPath { set; get; }
        public string WebDropboxPath { set; get; }
    }
}