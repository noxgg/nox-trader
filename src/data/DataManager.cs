using System;
using System.Collections.Generic;
using noxiousET.src.data.accounts;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.io;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;

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
        private TextFileio textFileio;
        private TextFileToDictionaryLoader textFileToDictionaryLoader;
        private UiElementsio uiElementsio;

        private static readonly String ROOT_CONFIG_FILENAME = "last.ini";
        private static readonly String FITTABLE_MODULE_TYPE_IDS_FILENAME = "fittableModuleTypeIDs.dat";
        private static readonly String LONG_NAME_TYPE_IDS_FILENAME = "longNameTypeIDs.dat";
        private static readonly String IGNORE_ERROR_CHECK_TYPE_IDS_FILENAME = "ignoreErrorCheckTypeIDs.dat";

        public DataManager()
        {
            paths = new Paths();
            clientConfig = new ClientConfig();

            textFileio = new TextFileio("", ROOT_CONFIG_FILENAME);

            List<String> config = textFileio.load();

            int line = 0;
            paths.logPath = config[line++];
            paths.EVEPath = config[line++];
            paths.configPath = config[line++];
            clientConfig.timingMultiplier = Convert.ToInt32(config[line++]);
            clientConfig.iterations = Convert.ToInt32(config[line++]);
            clientConfig.xResolution = Convert.ToInt32(config[line++]);
            clientConfig.yResolution = Convert.ToInt32(config[line++]);

            accountManager = new AccountManager();
            characterManager = new CharacterManager(paths, accountManager);

            int length = config.Count - line;
            String[] characters = new String[length];
            for (int i = 0; i < length; i++)
            {
                characters[i] = config[line++];
            }
            characterManager.loadCharacters(characters);

            modules = new Modules();

            String fileName = Convert.ToString(clientConfig.xResolution) + "x" + Convert.ToString(clientConfig.yResolution) + ".ini";
            uiElements = new UiElements();
            uiElementsio = new UiElementsio(paths.configPath, fileName, uiElements);

            textFileToDictionaryLoader = new TextFileToDictionaryLoader(paths.configPath, "");
            modules.fittableModuleTypeIDs = loadModuleData(FITTABLE_MODULE_TYPE_IDS_FILENAME);
            modules.ignoreErrorCheckTypeIDs = loadModuleData(IGNORE_ERROR_CHECK_TYPE_IDS_FILENAME);
            modules.longNameTypeIDs = loadModuleData(LONG_NAME_TYPE_IDS_FILENAME);
        }

        private Dictionary<int, int> loadModuleData(String fileName)
        {
            textFileToDictionaryLoader.fileName = fileName;
            return textFileToDictionaryLoader.loadIntKeyEqualsIntValueEqualsOneLine();
        }

        public void saveSettings()
        {
            List<Object> settings = new List<Object>();

            settings.Add(paths.logPath);
            settings.Add(paths.EVEPath);
            settings.Add(paths.configPath);
            settings.Add(clientConfig.timingMultiplier);
            settings.Add(clientConfig.iterations);
            settings.Add(clientConfig.xResolution);
            settings.Add(clientConfig.yResolution);

            foreach (String s in characterManager.getAllCharacterNames())
            {
                settings.Add(s);
            }

            textFileio.save(settings);
        }
    }
}
