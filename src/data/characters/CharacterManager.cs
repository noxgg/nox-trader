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
        private static readonly string tradeQueueFileName = "Queue.ini";
        private static readonly string tradeHistoryFileName = "History.ini";

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

        public void load(List<String> names)
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

            characterData.Add(EtConstants.ACCOUNT_LOGIN_KEY + "=" + character.account.l);
            characterData.Add(EtConstants.ACCOUNT_PASSWORD_KEY + "=" + character.account.p);
            characterData.Add(EtConstants.ACCOUNT_ID_KEY + "=" + character.account.id);
            characterData.Add(EtConstants.CHARACTER_ID_KEY + "=" + character.id);
            characterData.Add(EtConstants.CHARACTER_STATION_ID_KEY + "=" + character.stationid);
            characterData.Add(EtConstants.CHARACTER_TRADE_SHIPS_KEY + "=" + character.tradeItems);
            characterData.Add(EtConstants.CHARACTER_TRADE_ITEMS_KEY + "=" + character.tradeShips);
            characterData.Add(EtConstants.CHARACTER_ADJUST_SELLS_KEY + "=" + character.adjustSells);
            characterData.Add(EtConstants.CHARACTER_ADJUST_BUYS_KEY + "=" + character.adjustBuys);
            characterData.Add(EtConstants.CHARACTER_MAXIMUM_ORDERS_KEY + "=" + character.maximumOrders);
            characterData.Add(EtConstants.CHARACTER_LOGIN_COLOR_KEY + "=" + character.loginColor);

            if (this.active.Contains(character.name))
                characterData.Add(EtConstants.CHARACTER_ACTIVE_STATE + "=" + "true");
            else
                characterData.Add(EtConstants.CHARACTER_ACTIVE_STATE + "=" + "false");

            foreach (int[] i in character.quantityThreshHolds)
            {
                characterData.Add(EtConstants.CHARACTER_THRESHHOLD_PRICE + "=" + i[0]);
                characterData.Add(EtConstants.CHARACTER_THRESHHOLD_QUANTITY + "=" + i[1]);
            }

            textFileio.save(characterData, paths.configPath, name + ".ini");

            saveTradeHistory(character.name, character.tradeHistory.Keys);
            saveTradeQueue(character.name, character.tradeQueue.ToArray());
        }

        private Character load(string name)
        {
            List<String> characterData = textFileio.read(paths.configPath, name + ".ini");
            Character character = new Character(name);

            Dictionary<string, string> characterSettings = new Dictionary<string,string>();
            List<int> threshholdPrices = new List<int>();
            List<int> threshholdQuantities = new List<int>();

            String[] split;

            foreach (string line in characterData)
            {
                split = line.Split('=');

                if (split[0].Equals(EtConstants.CHARACTER_THRESHHOLD_PRICE))
                {
                    threshholdPrices.Add(int.Parse(split[1]));
                }
                else if (split[0].Equals(EtConstants.CHARACTER_THRESHHOLD_QUANTITY))
                {
                    threshholdQuantities.Add(int.Parse(split[1]));
                }
                else
                {
                    characterSettings.Add(split[0], split[1]);
                }
            }

            Account account = new Account(characterSettings[EtConstants.ACCOUNT_LOGIN_KEY], characterSettings[EtConstants.ACCOUNT_PASSWORD_KEY], characterSettings[EtConstants.ACCOUNT_ID_KEY]);
            accountManager.addAccount(account);
            character.account = account;
            character.id = characterSettings[EtConstants.CHARACTER_ID_KEY];
            character.stationid = Convert.ToInt32(characterSettings[EtConstants.CHARACTER_STATION_ID_KEY]);
            character.tradeItems = Convert.ToBoolean(characterSettings[EtConstants.CHARACTER_TRADE_ITEMS_KEY]);
            character.tradeShips = Convert.ToBoolean(characterSettings[EtConstants.CHARACTER_TRADE_SHIPS_KEY]);
            character.adjustSells = Convert.ToBoolean(characterSettings[EtConstants.CHARACTER_ADJUST_SELLS_KEY]);
            character.adjustBuys = Convert.ToBoolean(characterSettings[EtConstants.CHARACTER_ADJUST_BUYS_KEY]);
            character.maximumOrders = Convert.ToInt32(characterSettings[EtConstants.CHARACTER_MAXIMUM_ORDERS_KEY]);
            character.loginColor = Convert.ToInt32(characterSettings[EtConstants.CHARACTER_LOGIN_COLOR_KEY]);

            if (Convert.ToBoolean(characterSettings[EtConstants.CHARACTER_ACTIVE_STATE]))
                active.Add(name);
            else
                inactive.Add(name);

            for (int i = 0; i < threshholdPrices.Count; i++ )
            {
                character.quantityThreshHolds.Add(new int[] { threshholdPrices[i], threshholdQuantities[i] });
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

        public Dictionary<String, String> convertCharacterToDictionary(Character character)
        {
            Dictionary<String, String> result = new Dictionary<String, String>();

            result.Add(EtConstants.ACCOUNT_LOGIN_KEY, character.account.l);
            result.Add(EtConstants.ACCOUNT_PASSWORD_KEY, character.account.p);
            result.Add(EtConstants.ACCOUNT_ID_KEY, character.account.id);
            result.Add(EtConstants.CHARACTER_ID_KEY, character.id);
            result.Add(EtConstants.CHARACTER_STATION_ID_KEY, Convert.ToString(character.stationid));
            result.Add(EtConstants.CHARACTER_TRADE_ITEMS_KEY, Convert.ToString(character.tradeItems));
            result.Add(EtConstants.CHARACTER_TRADE_SHIPS_KEY, Convert.ToString(character.tradeShips));
            result.Add(EtConstants.CHARACTER_ADJUST_BUYS_KEY, Convert.ToString(character.adjustBuys));
            result.Add(EtConstants.CHARACTER_ADJUST_SELLS_KEY, Convert.ToString(character.adjustSells));
            result.Add(EtConstants.CHARACTER_MAXIMUM_ORDERS_KEY, Convert.ToString(character.maximumOrders));
            result.Add(EtConstants.CHARACTER_LOGIN_COLOR_KEY, Convert.ToString(character.loginColor));
            result.Add(EtConstants.CHARACTER_ACTIVE_STATE, Convert.ToString(active.Contains(character.name)));

            return result;

        }

        private void characterSettingUpdatedListener(object o, string name, string key, string value)
        {
            try
            {
                switch (key)
                {
                    case EtConstants.ACCOUNT_LOGIN_KEY:
                        characters[name].account.l = value;
                        break;
                    case EtConstants.ACCOUNT_PASSWORD_KEY:
                        characters[name].account.p = value;
                        break;
                    case EtConstants.CHARACTER_TRADE_ITEMS_KEY:
                        characters[name].tradeItems = Convert.ToBoolean(value);
                        break;
                    case EtConstants.CHARACTER_TRADE_SHIPS_KEY:
                        characters[name].tradeShips = Convert.ToBoolean(value);
                        break;
                    case EtConstants.CHARACTER_ADJUST_SELLS_KEY:
                        characters[name].adjustSells = Convert.ToBoolean(value);
                        break;
                    case EtConstants.CHARACTER_ADJUST_BUYS_KEY:
                        characters[name].adjustBuys = Convert.ToBoolean(value);
                        break;
                    case EtConstants.CHARACTER_MAXIMUM_ORDERS_KEY:
                        characters[name].maximumOrders = Convert.ToInt32(value);
                        break;
                    case EtConstants.CHARACTER_STATION_ID_KEY:
                        characters[name].stationid = Convert.ToInt32(value);
                        break;
                    case EtConstants.ACCOUNT_ID_KEY:
                        characters[name].account.id = value;
                        break;
                    case EtConstants.CHARACTER_ID_KEY:
                        characters[name].id = value;
                        break;
                    case EtConstants.CHARACTER_LOGIN_COLOR_KEY:
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
