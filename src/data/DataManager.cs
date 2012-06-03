using System;
using System.Collections.Generic;
using noxiousET.src.data.accounts;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.io;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;
using noxiousET.src.etevent;
using System.IO;

namespace noxiousET.src.data
{
    class DataManager
    {
        public Modules modules { set; get; }
        public Paths paths { set; get; }
        public UiElements uiElements { set; get; }
        public CharacterManager characterManager { set; get; }
        public AccountManager accountManager { set; get; }
        public ClientConfig clientConfig { set; get; }
        public EventDispatcher eventDispatcher { set; get; }
        private TextFileio textFileio;
        private TextFileToDictionaryLoader textFileToDictionaryLoader;
        private MarketOrderio marketOrderio;
        private UiElementsio uiElementsio;

        private const String ROOT_CONFIG_FILENAME = "last.ini";
        private const String FITTABLE_MODULE_TYPE_IDS_FILENAME = "fittableModuleTypeIDs.dat";
        private const String LONG_NAME_TYPE_IDS_FILENAME = "longNameTypeIDs.dat";
        private const String IGNORE_ERROR_CHECK_TYPE_IDS_FILENAME = "ignoreErrorCheckTypeIDs.dat";
        private const String TYPE_NAMES_FILENAME = "typeNames.dat";

        public DataManager()
        {
            paths = new Paths();
            clientConfig = new ClientConfig();
            this.eventDispatcher = new EventDispatcher();
            this.eventDispatcher.clientSettingUpdatedHandler += new EventDispatcher.ClientSettingUpdatedHandler(clientSettingUpdatedListener);
            this.eventDispatcher.saveAllSettingsRequestHandler += new EventDispatcher.SaveAllSettingsRequestHandler(saveAllSettingsRequestListener);
            this.eventDispatcher.getTypesFromFileRequestHandler += new EventDispatcher.GetTypesFromFileRequestHandler(getTypeForCharacterFromNewestLogFile);

            textFileio = new TextFileio("", ROOT_CONFIG_FILENAME);

            List<String> config = textFileio.read();

            int line = 0;
            paths.logPath = config[line++];
            paths.clientPath = config[line++];
            paths.configPath = config[line++];
            clientConfig.timingMultiplier = Convert.ToInt32(config[line++]);
            clientConfig.iterations = Convert.ToInt32(config[line++]);
            clientConfig.xResolution = Convert.ToInt32(config[line++]);
            clientConfig.yResolution = Convert.ToInt32(config[line++]);

            accountManager = new AccountManager();
            characterManager = new CharacterManager(paths, accountManager, eventDispatcher);

            int length = config.Count - line;
            String[] characters = new String[length];
            for (int i = 0; i < length; i++)
            {
                characters[i] = config[line++];
            }
            characterManager.load(characters);

            modules = new Modules();

            String fileName = Convert.ToString(clientConfig.xResolution) + "x" + Convert.ToString(clientConfig.yResolution) + ".ini";
            uiElements = new UiElements();
            uiElementsio = new UiElementsio(paths.configPath, fileName, uiElements);

            textFileToDictionaryLoader = new TextFileToDictionaryLoader(paths.configPath, FITTABLE_MODULE_TYPE_IDS_FILENAME);
            modules.fittableModuleTypeIDs = textFileToDictionaryLoader.loadIntKeyEqualsIntValueEqualsOneLine();
            modules.longNameTypeIDs = textFileToDictionaryLoader.loadIntKeyEqualsIntValueEqualsOneLine(paths.configPath, LONG_NAME_TYPE_IDS_FILENAME);
            modules.typeNames = textFileToDictionaryLoader.loadIntKeyStringValue(paths.configPath, TYPE_NAMES_FILENAME);

            marketOrderio = new MarketOrderio();
        }

        public void savePathAndClientSettings()
        {
            List<Object> settings = new List<Object>();

            settings.Add(paths.logPath);
            settings.Add(paths.clientPath);
            settings.Add(paths.configPath);
            settings.Add(clientConfig.timingMultiplier);
            settings.Add(clientConfig.iterations);
            settings.Add(clientConfig.xResolution);
            settings.Add(clientConfig.yResolution);

            foreach (String s in characterManager.getAllCharacterNames())
            {
                settings.Add(s);
            }

            textFileio.save(settings, "", ROOT_CONFIG_FILENAME);
        }

        private void saveAllSettingsRequestListener(object o)
        {
            savePathAndClientSettings();
            characterManager.saveAll();
        }

        private void clientSettingUpdatedListener(object o, string name, string key, string value)
        {
            try
            {
                switch (key)
                {
                    case EtConstants.TIMING_MULTIPLIER_KEY:
                        clientConfig.timingMultiplier = parseValue(value, false);
                        break;
                    case EtConstants.ITERATIONS_KEY:
                        clientConfig.iterations = parseValue(value, true);
                        break;
                    case EtConstants.X_RESOLUTION_KEY:
                        clientConfig.xResolution = parseValue(value, false);
                        break;
                    case EtConstants.Y_RESOLUTION_KEY:
                        clientConfig.yResolution = parseValue(value, false);
                        break;
                    case EtConstants.LOG_PATH_KEY:
                        paths.logPath = value;
                        break;
                    case EtConstants.CONFIG_PATH_KEY:
                        paths.configPath = value;
                        break;
                    case EtConstants.CLIENT_PATH_KEY:
                        paths.clientPath = value;
                        break;
                    default:
                        eventDispatcher.logError("Attempt to save value for non-existant client data key.");
                        break;
                }
            }
            catch
            {
                eventDispatcher.logError("Attempt to save value of incorrect type for the given character data key.");
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
        private void getTypeForCharacterFromNewestLogFile(object o, String name)
        {
            Character character = characterManager.getCharacter(name);
            String fileName;
            while (Directory.GetFiles(paths.logPath).Length > 0)
            {
                fileName = marketOrderio.getNewestFileNameInDirectory(paths.logPath);
                String[] result = marketOrderio.readFirstEntry(paths.logPath, fileName);
                int typeid = Convert.ToInt32(result[2]);
                if (modules.typeNames.ContainsKey(typeid))
                {
                    if (!character.tradeQueue.Contains(typeid))
                    {
                        character.tradeQueue.Enqueue(typeid);
                        eventDispatcher.log("Added " + modules.typeNames[typeid] + " to queue for " + character.name);
                    }
                }
            }
            characterManager.save(name);
        }
    }
}
