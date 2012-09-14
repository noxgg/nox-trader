using System;
using System.Collections;
using System.Collections.Generic;
using noxiousET.src.data.accounts;
using noxiousET.src.data.io;
using noxiousET.src.data.paths;
using noxiousET.src.etevent;

namespace noxiousET.src.data.characters
{
    class CharacterManager
    {
        public List<String> active { set; get; }
        public List<String> inactive { set; get; }
        public String selected { set; get; }
        private Dictionary<String, Character> characters;
        private TextFileio textFileio;
        private AccountManager accountManager;
        private Paths paths;
        private EventDispatcher eventDispatcher;
        private static readonly string tradeQueueFileName = "Queue.dat";
        private static readonly string tradeHistoryFileName = "History.dat";

        public CharacterManager(Paths paths, AccountManager accountManager)
        {
            this.paths = paths;
            this.accountManager = accountManager;
            characters = new Dictionary<String, Character>();
            active = new List<String>();
            inactive = new List<String>();
            selected = null;
            textFileio = new TextFileio(paths.configPath, null);
            this.eventDispatcher = EventDispatcher.Instance;
            this.eventDispatcher.characterSettingUpdatedHandler += new EventDispatcher.CharacterSettingUpdatedHandler(characterSettingUpdatedListener);
        }

        public void addCharacter(Character character)
        {
            if (characters.ContainsKey(character.name))
                characters.Remove(character.name);
            characters.Add(character.name, character);
        }

        public Character getCharacter(string name)
        {
            if (characters.ContainsKey(name))
            {
                return characters[name];
            }
            addCharacter(load(name));
            
            return getCharacter(name);
        }

        public List<String> getAllCharacterNames()
        {
            List<String> result = new List<String>();
            foreach (String s in active)
                result.Add(s);
            foreach (String s in inactive)
                result.Add(s);
            return result;
        }

        public void load(String[] names)
        {
            foreach (String n in names)
                load(n);
        }

        public void save(String[] names)
        {
            foreach (String n in names)
                save(n);
        }

        public void saveAll()
        {
            foreach (String s in getAllCharacterNames())
                save(s);
        }

        public void save(string name)
        {
            List<Object> characterData = new List<Object>();
            Character character = characters[name];

            characterData.Add(character.account.l);
            characterData.Add(character.account.p);
            characterData.Add(character.autoAdjustsPerAutoList);
            characterData.Add(character.stationid);
            characterData.Add(character.fileNameTrimLength);
            characterData.Add(character.tradeItems);
            characterData.Add(character.tradeShips);
            characterData.Add(character.adjustSells);
            characterData.Add(character.adjustBuys);
            characterData.Add(character.maximumOrders);
            characterData.Add(character.loginColor);

            if (this.active.Contains(character.name))
                characterData.Add(true);
            else
                characterData.Add(false);

            foreach (int[] i in character.quantityThreshHolds)
            {
                characterData.Add(i[0]);
                characterData.Add(i[1]);
            }

            textFileio.save(characterData, paths.configPath, name + ".ini");

            saveTradeHistory(character.name, character.tradeHistory.Keys);
            saveTradeQueue(character.name, character.tradeQueue.ToArray());
        }

        private Character load(string name)
        {
            List<String> characterData = textFileio.read(paths.configPath, name + ".ini");
            Character character = new Character(name);

            int line = 0;
            Account account = new Account(characterData[line++], characterData[line++]);
            accountManager.addAccount(account);
            character.account = account;
            character.autoAdjustsPerAutoList = Convert.ToInt32(characterData[line++]);
            character.stationid = Convert.ToInt32(characterData[line++]);
            character.fileNameTrimLength = Convert.ToInt32(characterData[line++]);
            character.tradeItems = Convert.ToBoolean(characterData[line++]);
            character.tradeShips = Convert.ToBoolean(characterData[line++]);
            character.adjustSells = Convert.ToBoolean(characterData[line++]);
            character.adjustBuys = Convert.ToBoolean(characterData[line++]);
            character.maximumOrders = Convert.ToInt32(characterData[line++]);
            character.loginColor = Convert.ToInt32(characterData[line++]);

            if (Convert.ToBoolean(characterData[line++]))
                active.Add(name);
            else
                inactive.Add(name);

            while (line < characterData.Count)
            {
                character.quantityThreshHolds.Add(new int[] { Convert.ToInt32(characterData[line++]), Convert.ToInt32(characterData[line++]) });
            }
            characters.Add(name, character);

            character.tradeQueue = loadTradeQueue(character.name);
            character.tradeHistory = loadTradeHistory(character.name);

            return character;
        }

        private Queue<int> loadTradeQueue(String name)
        {
            Queue<int> tradeQueue = new Queue<int>();
            List<String> fileData = textFileio.read(paths.configPath + Paths.characterDataSubDir, name + tradeQueueFileName);

            foreach (String s in fileData)
                tradeQueue.Enqueue(Convert.ToInt32(s));

            return tradeQueue;
        }

        private Dictionary<int, int> loadTradeHistory(String name)
        {
            Dictionary<int, int> tradeHistory = new Dictionary<int, int>();
            List<String> fileData = textFileio.read(paths.configPath + Paths.characterDataSubDir, name + tradeHistoryFileName);
            foreach (String s in fileData)
            {
                tradeHistory.Add(int.Parse(s), int.Parse(s));
            }

            return tradeHistory;
        }

        private void saveTradeQueue(String name, int[] tradeQueue)
        {
            List<Object> data = new List<Object>();
            foreach (int i in tradeQueue)
            {
                data.Add(i);
            }
            textFileio.save(data, paths.configPath + Paths.characterDataSubDir, name + tradeQueueFileName);
        }

        private void saveTradeHistory(String name, ICollection<int> tradeHistory)
        {
            List<Object> data = new List<Object>();
            foreach (int n in tradeHistory)
                data.Add(n);
            textFileio.save(data, paths.configPath + Paths.characterDataSubDir, name + tradeHistoryFileName);
        }

        public String[] convertCharacterToStringArray(Character character)
        {
            String[] result = new String[12];
            int i = 0;

            result[i++] = character.account.l;
            result[i++] = character.account.p;
            result[i++] = Convert.ToString(character.autoAdjustsPerAutoList);
            result[i++] = Convert.ToString(character.stationid);
            result[i++] = Convert.ToString(character.fileNameTrimLength);
            result[i++] = Convert.ToString(character.tradeItems);
            result[i++] = Convert.ToString(character.tradeShips);
            result[i++] = Convert.ToString(character.adjustBuys);
            result[i++] = Convert.ToString(character.adjustSells);
            result[i++] = Convert.ToString(character.maximumOrders);
            result[i++] = Convert.ToString(character.loginColor);
            result[i++] = Convert.ToString(active.Contains(character.name));

            return result;

        }

        private void characterSettingUpdatedListener(object o, string name, string key, string value)
        {
            try
            {
                switch (key)
                {
                    case EtConstants.LOGIN_KEY:
                        characters[name].account.l = value;
                        break;
                    case EtConstants.PASSWORD_KEY:
                        characters[name].account.p = value;
                        break;
                    case EtConstants.TRADE_ITEMS_KEY:
                        characters[name].tradeItems = Convert.ToBoolean(value);
                        break;
                    case EtConstants.TRADE_SHIPS_KEY:
                        characters[name].tradeShips = Convert.ToBoolean(value);
                        break;
                    case EtConstants.ADJUST_SELLS_KEY:
                        characters[name].adjustSells = Convert.ToBoolean(value);
                        break;
                    case EtConstants.ADJUST_BUYS_KEY:
                        characters[name].adjustBuys = Convert.ToBoolean(value);
                        break;
                    case EtConstants.MAXIMUM_ORDERS_KEY:
                        characters[name].maximumOrders = Convert.ToInt32(value);
                        break;
                    case EtConstants.STATION_ID_KEY:
                        characters[name].stationid = Convert.ToInt32(value);
                        break;
                    case EtConstants.FILE_NAME_TRIM_LENGTH_KEY:
                        characters[name].fileNameTrimLength = Convert.ToInt32(value);
                        break;
                    case EtConstants.AUTO_ADJUSTS_PER_AUTO_LIST_KEY:
                        characters[name].autoAdjustsPerAutoList = Convert.ToInt32(value);
                        break;
                    case EtConstants.LOGIN_COLOR_KEY:
                        characters[name].loginColor = Convert.ToInt32(value);
                        break;
                    default:
                        eventDispatcher.logError("Attempt to save value for non-existant character data key.");
                        break;
                }
            }
            catch
            {
                eventDispatcher.logError("Attempt to save value of incorrect type for the given character data key.");
            }
        }
    }
}
