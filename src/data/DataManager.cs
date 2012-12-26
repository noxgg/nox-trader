using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using noxiousET.src.data.accounts;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.io;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uidata;
using noxiousET.src.etevent;

namespace noxiousET.src.data
{
    internal class DataManager
    {
        private const String RootConfigFilename = "last.ini";
        private const String RootConfigFilenameAlt = "lastmba.ini";
        private const String FittableModuleTypeIdsFilename = "fittableModuleTypeIDs.ini";
        private const String LongNameTypeIdsFilename = "longNameTypeIDs.ini";
        private const String TypeNamesFilename = "typeNames.ini";
        private readonly string _configFileName;
        private readonly MarketOrderio _marketOrderio;
        private TextFileToDictionaryLoader _textFileToDictionaryLoader;
        private TextFileio _textFileio;
        private EveUiDataIo _eveUiDataIo;

        public DataManager()
        {
            Paths = new Paths();
            ClientConfig = new ClientConfig();
            EventDispatcher = EventDispatcher.Instance;
            EventDispatcher.clientSettingUpdatedHandler += ClientSettingUpdatedListener;
            EventDispatcher.saveAllSettingsRequestHandler += SaveAllSettingsRequestListener;
            EventDispatcher.getTypesFromFileRequestHandler += GetTypeForCharacterFromNewestLogFile;

            AccountManager = new AccountManager();
            CharacterManager = new CharacterManager(Paths, AccountManager);
            Modules = new Modules();
            Ui = new EveUi();

            try
            {
                _configFileName = RootConfigFilename;
                Initialize();
            }
            catch (Exception)
            {
                _configFileName = RootConfigFilenameAlt;
                Initialize();
            }

            _marketOrderio = new MarketOrderio();
        }

        public Modules Modules { set; get; }
        public Paths Paths { set; get; }
        public EveUi Ui { set; get; }
        public CharacterManager CharacterManager { set; get; }
        public AccountManager AccountManager { set; get; }
        public ClientConfig ClientConfig { set; get; }
        public EventDispatcher EventDispatcher { set; get; }

        private void Initialize()
        {
            _textFileio = new TextFileio("", _configFileName);
            List<String> config = _textFileio.Read();
            var configLines = new Dictionary<String, String>();
            var characterNames = new List<String>();

            foreach (String line in config)
            {
                String[] parts = line.Split('=');
                if (parts[0].Equals(EtConstants.CharacterKey))
                    characterNames.Add(parts[1]);
                else
                    configLines.Add(parts[0], parts[1]);
            }


            Paths.LogPath = configLines[EtConstants.LogPathKey];
            Paths.ClientPath = configLines[EtConstants.ClientPathKey];
            Paths.ConfigPath = configLines[EtConstants.ConfigPathKey];
            Paths.EveSettingsPath = configLines[EtConstants.EveSettingsPathKey];
            ClientConfig.TimingMultiplier = Convert.ToInt32(configLines[EtConstants.TimingMultiplierKey]);
            ClientConfig.Iterations = Convert.ToInt32(configLines[EtConstants.IterationsKey]);
            ClientConfig.XResolution = Convert.ToInt32(configLines[EtConstants.XResolutionKey]);
            ClientConfig.YResolution = Convert.ToInt32(configLines[EtConstants.YResolutionKey]);

            CharacterManager.Load(characterNames);

            String fileName = Convert.ToString(ClientConfig.XResolution) + "x" +
                              Convert.ToString(ClientConfig.YResolution) + ".ini";
            _eveUiDataIo = new EveUiDataIo(Paths.ConfigPath, fileName, Ui);

            _textFileToDictionaryLoader = new TextFileToDictionaryLoader(Paths.ConfigPath, FittableModuleTypeIdsFilename);
            Modules.FittableModuleTypeIDs = _textFileToDictionaryLoader.LoadIntKeyEqualsIntValueEqualsOneLine();
            Modules.LongNameTypeIDs = _textFileToDictionaryLoader.LoadIntKeyEqualsIntValueEqualsOneLine(
                Paths.ConfigPath, LongNameTypeIdsFilename);
            Modules.TypeNames = _textFileToDictionaryLoader.LoadIntKeyStringValue(Paths.ConfigPath, TypeNamesFilename);
        }

        public void SavePathAndClientSettings()
        {
            var settings = new List<Object>
                               {
                                   EtConstants.LogPathKey + "=" + Paths.LogPath,
                                   EtConstants.ClientPathKey + "=" + Paths.ClientPath,
                                   EtConstants.ConfigPathKey + "=" + Paths.ConfigPath,
                                   EtConstants.EveSettingsPathKey + "=" + Paths.EveSettingsPath,
                                   EtConstants.TimingMultiplierKey + "=" + ClientConfig.TimingMultiplier,
                                   EtConstants.IterationsKey + "=" + ClientConfig.Iterations,
                                   EtConstants.XResolutionKey + "=" + ClientConfig.XResolution,
                                   EtConstants.YResolutionKey + "=" + ClientConfig.YResolution
                               };

            settings.AddRange(CharacterManager.GetAllCharacterNames().Select(s => EtConstants.CharacterKey + "=" + s));
            _textFileio.Save(settings, "", _configFileName);
        }

        private void SaveAllSettingsRequestListener(object o)
        {
            SavePathAndClientSettings();
            CharacterManager.SaveAll();
        }

        private void ClientSettingUpdatedListener(object o, string name, string key, string value)
        {
            try
            {
                switch (key)
                {
                    case EtConstants.TimingMultiplierKey:
                        ClientConfig.TimingMultiplier = parseValue(value, false);
                        break;
                    case EtConstants.IterationsKey:
                        ClientConfig.Iterations = parseValue(value, true);
                        break;
                    case EtConstants.XResolutionKey:
                        ClientConfig.XResolution = parseValue(value, false);
                        break;
                    case EtConstants.YResolutionKey:
                        ClientConfig.YResolution = parseValue(value, false);
                        break;
                    case EtConstants.LogPathKey:
                        Paths.LogPath = value;
                        break;
                    case EtConstants.ConfigPathKey:
                        Paths.ConfigPath = value;
                        break;
                    case EtConstants.ClientPathKey:
                        Paths.ClientPath = value;
                        break;
                    case EtConstants.EveSettingsPathKey:
                        Paths.EveSettingsPath = value;
                        break;
                    default:
                        EventDispatcher.LogError("Attempt to save value for non-existant client data key.");
                        break;
                }
            }
            catch
            {
                EventDispatcher.LogError("Attempt to save value of incorrect type for the given character data key.");
            }
        }

        private int parseValue(String value, Boolean acceptZero)
        {
            int result = Convert.ToInt32(value);

            if ((acceptZero && result >= 0) || result > 0)
                return result;
            else
                throw new Exception("Invalid integer value for given character data key");
        }

        //TODO DEPRECIATE
        private void GetTypeForCharacterFromNewestLogFile(object o, String name)
        {
            Character character = CharacterManager.GetCharacter(name);
            while (Directory.GetFiles(Paths.LogPath).Length > 0)
            {
                String fileName = _marketOrderio.GetNewestFileNameInDirectory(Paths.LogPath);
                String[] result = _marketOrderio.ReadFirstEntry(Paths.LogPath, fileName);
                int typeid = Convert.ToInt32(result[2]);
                if (Modules.TypeNames.ContainsKey(typeid) && !character.TradeQueue.Contains(typeid))
                {
                    character.TradeQueue.Enqueue(typeid);
                    EventDispatcher.Log("Added " + Modules.TypeNames[typeid] + " to queue for " + character.Name);
                }
            }
            CharacterManager.Save(name);
        }
    }
}