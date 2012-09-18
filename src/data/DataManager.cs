﻿using System;
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
using noxiousET.src.orders;

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


        private string configFileName;
        private const String ROOT_CONFIG_FILENAME = "last.ini";
        private const String ROOT_CONFIG_FILENAME_ALT = "lastmba.ini";
        private const String FITTABLE_MODULE_TYPE_IDS_FILENAME = "fittableModuleTypeIDs.ini";
        private const String LONG_NAME_TYPE_IDS_FILENAME = "longNameTypeIDs.ini";
        private const String TYPE_NAMES_FILENAME = "typeNames.ini";

        public DataManager()
        {
            paths = new Paths();
            clientConfig = new ClientConfig();
            this.eventDispatcher = EventDispatcher.Instance;
            this.eventDispatcher.clientSettingUpdatedHandler += new EventDispatcher.ClientSettingUpdatedHandler(clientSettingUpdatedListener);
            this.eventDispatcher.saveAllSettingsRequestHandler += new EventDispatcher.SaveAllSettingsRequestHandler(saveAllSettingsRequestListener);
            this.eventDispatcher.getTypesFromFileRequestHandler += new EventDispatcher.GetTypesFromFileRequestHandler(getTypeForCharacterFromNewestLogFile);

            accountManager = new AccountManager();
            characterManager = new CharacterManager(paths, accountManager);
            modules = new Modules();
            uiElements = new UiElements();

        
                configFileName = ROOT_CONFIG_FILENAME;
                initialize();

            marketOrderio = new MarketOrderio();
        }

        private void initialize()
        {

            textFileio = new TextFileio("", configFileName);
            List<String> config = textFileio.read();
            Dictionary<String, String> configLines = new Dictionary<String, String>();
            List<String> characterNames = new List<String>();

            String[] parts;
            foreach (String line in config) {
                parts = line.Split('=');
                if (parts[0].Equals(EtConstants.CHARACTER_KEY))
                    characterNames.Add(parts[1]);
                else
                    configLines.Add(parts[0], parts[1]);

            }


            paths.logPath = configLines[EtConstants.LOG_PATH_KEY];
            paths.clientPath = configLines[EtConstants.CLIENT_PATH_KEY];
            paths.configPath = configLines[EtConstants.CONFIG_PATH_KEY];
            paths.eveSettingsPath = configLines[EtConstants.EVE_SETTINGS_PATH_KEY];
            clientConfig.timingMultiplier = Convert.ToInt32(configLines[EtConstants.TIMING_MULTIPLIER_KEY]);
            clientConfig.iterations = Convert.ToInt32(configLines[EtConstants.ITERATIONS_KEY]);
            clientConfig.xResolution = Convert.ToInt32(configLines[EtConstants.X_RESOLUTION_KEY]);
            clientConfig.yResolution = Convert.ToInt32(configLines[EtConstants.Y_RESOLUTION_KEY]);

            characterManager.load(characterNames);

            String fileName = Convert.ToString(clientConfig.xResolution) + "x" + Convert.ToString(clientConfig.yResolution) + ".ini";
            uiElementsio = new UiElementsio(paths.configPath, fileName, uiElements);

            textFileToDictionaryLoader = new TextFileToDictionaryLoader(paths.configPath, FITTABLE_MODULE_TYPE_IDS_FILENAME);
            modules.fittableModuleTypeIDs = textFileToDictionaryLoader.loadIntKeyEqualsIntValueEqualsOneLine();
            modules.longNameTypeIDs = textFileToDictionaryLoader.loadIntKeyEqualsIntValueEqualsOneLine(paths.configPath, LONG_NAME_TYPE_IDS_FILENAME);
            modules.typeNames = textFileToDictionaryLoader.loadIntKeyStringValue(paths.configPath, TYPE_NAMES_FILENAME);
        }

        public void savePathAndClientSettings()
        {
            List<Object> settings = new List<Object>();

            settings.Add(EtConstants.LOG_PATH_KEY + "=" + paths.logPath);
            settings.Add(EtConstants.CLIENT_PATH_KEY + "=" + paths.clientPath);
            settings.Add(EtConstants.CONFIG_PATH_KEY + "=" + paths.configPath);
            settings.Add(EtConstants.EVE_SETTINGS_PATH_KEY + "=" + paths.eveSettingsPath);
            settings.Add(EtConstants.TIMING_MULTIPLIER_KEY + "=" + clientConfig.timingMultiplier);
            settings.Add(EtConstants.ITERATIONS_KEY + "=" + clientConfig.iterations);
            settings.Add(EtConstants.X_RESOLUTION_KEY + "=" + clientConfig.xResolution);
            settings.Add(EtConstants.Y_RESOLUTION_KEY + "=" + clientConfig.yResolution);

            foreach (String s in characterManager.getAllCharacterNames())
            {
                settings.Add(EtConstants.CHARACTER_KEY + "=" + s);
            }

            textFileio.save(settings, "", configFileName);
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
                    case EtConstants.EVE_SETTINGS_PATH_KEY:
                        paths.eveSettingsPath = value;
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
        //TODO DEPRECIATE
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
