using System;
using System.Collections.Generic;
using System.Linq;
using noxiousET.src.data.accounts;
using noxiousET.src.data.io;
using noxiousET.src.data.paths;
using noxiousET.src.etevent;

namespace noxiousET.src.data.characters
{
    internal class CharacterManager
    {
        private const string TradeQueueFileName = "Queue.ini";
        private const string TradeHistoryFileName = "History.ini";
        private readonly AccountManager _accountManager;
        private readonly Dictionary<String, Character> _characters;
        private readonly EventDispatcher _eventDispatcher;
        private readonly Paths _paths;
        private readonly TextFileio _textFileio;

        public CharacterManager(Paths paths, AccountManager accountManager)
        {
            _paths = paths;
            _accountManager = accountManager;
            _characters = new Dictionary<String, Character>();
            ActiveCharacters = new List<String>();
            InactiveCharacters = new List<String>();
            SelectedCharacter = null;
            _textFileio = new TextFileio(paths.ConfigPath, null);
            _eventDispatcher = EventDispatcher.Instance;
            _eventDispatcher.characterSettingUpdatedHandler += CharacterSettingUpdatedListener;
        }

        public List<String> ActiveCharacters { set; get; }
        public List<String> InactiveCharacters { set; get; }
        public String SelectedCharacter { set; get; }

        public void AddCharacter(Character character)
        {
            if (_characters.ContainsKey(character.Name))
            {
                _characters.Remove(character.Name);
            }
            _characters.Add(character.Name, character);
        }

        public Character GetCharacter(string name)
        {
            if (_characters.ContainsKey(name))
            {
                return _characters[name];
            }
            AddCharacter(Load(name));

            return GetCharacter(name);
        }

        public List<String> GetAllCharacterNames()
        {
            List<string> result = ActiveCharacters.ToList();
            result.AddRange(InactiveCharacters);
            return result;
        }

        public void Load(List<String> names)
        {
            foreach (string n in names)
                Load(n);
        }

        public void Save(String[] names)
        {
            foreach (string n in names)
                Save(n);
        }

        public void SaveAll()
        {
            foreach (string s in GetAllCharacterNames())
                Save(s);
        }

        public void Save(string name)
        {
            var characterData = new List<Object>();
            Character character = _characters[name];

            characterData.Add(EtConstants.AccountLoginKey + "=" + character.Account.UserName);
            characterData.Add(EtConstants.AccountPasswordKey + "=" + character.Account.Password);
            characterData.Add(EtConstants.AccountIdKey + "=" + character.Account.Id);
            characterData.Add(EtConstants.CharacterIdKey + "=" + character.Id);
            characterData.Add(EtConstants.CharacterStationIdKey + "=" + character.StationId);
            characterData.Add(EtConstants.CharacterTradeShipsKey + "=" + character.ShouldTradeItems);
            characterData.Add(EtConstants.CharacterTradeItemsKey + "=" + character.ShouldTradeShips);
            characterData.Add(EtConstants.CharacterAdjustSellsKey + "=" + character.ShouldAdjustSells);
            characterData.Add(EtConstants.CharacterAdjustBuysKey + "=" + character.ShouldAdjustBuys);
            characterData.Add(EtConstants.CharacterMaximumOrdersKey + "=" + character.MaximumOrders);
            characterData.Add(EtConstants.CharacterLoginColorKey + "=" + character.LoginColor);

            if (ActiveCharacters.Contains(character.Name))
                characterData.Add(EtConstants.CharacterActiveState + "=" + "true");
            else
                characterData.Add(EtConstants.CharacterActiveState + "=" + "false");

            foreach (var i in character.QuantityThreshHolds)
            {
                characterData.Add(EtConstants.CharacterThreshholdPrice + "=" + i[0]);
                characterData.Add(EtConstants.CharacterThreshholdQuantity + "=" + i[1]);
            }

            _textFileio.Save(characterData, _paths.ConfigPath, name + ".ini");

            SaveTradeHistory(character.Name, character.TradeHistory.Keys);
            SaveTradeQueue(character.Name, character.TradeQueue.ToArray());
        }

        private Character Load(string name)
        {
            List<String> characterData = _textFileio.Read(_paths.ConfigPath, name + ".ini");
            var character = new Character(name);

            var characterSettings = new Dictionary<string, string>();
            var threshholdPrices = new List<int>();
            var threshholdQuantities = new List<int>();

            foreach (string line in characterData)
            {
                string[] split = line.Split('=');

                if (split[0].Equals(EtConstants.CharacterThreshholdPrice))
                {
                    threshholdPrices.Add(int.Parse(split[1]));
                }
                else if (split[0].Equals(EtConstants.CharacterThreshholdQuantity))
                {
                    threshholdQuantities.Add(int.Parse(split[1]));
                }
                else
                {
                    characterSettings.Add(split[0], split[1]);
                }
            }

            var account = new Account(characterSettings[EtConstants.AccountLoginKey],
                                      characterSettings[EtConstants.AccountPasswordKey],
                                      characterSettings[EtConstants.AccountIdKey]);
            _accountManager.AddAccount(account);
            character.Account = account;
            character.Id = characterSettings[EtConstants.CharacterIdKey];
            character.StationId = Convert.ToInt32(characterSettings[EtConstants.CharacterStationIdKey]);
            character.ShouldTradeItems = Convert.ToBoolean(characterSettings[EtConstants.CharacterTradeItemsKey]);
            character.ShouldTradeShips = Convert.ToBoolean(characterSettings[EtConstants.CharacterTradeShipsKey]);
            character.ShouldAdjustSells = Convert.ToBoolean(characterSettings[EtConstants.CharacterAdjustSellsKey]);
            character.ShouldAdjustBuys = Convert.ToBoolean(characterSettings[EtConstants.CharacterAdjustBuysKey]);
            character.MaximumOrders = Convert.ToInt32(characterSettings[EtConstants.CharacterMaximumOrdersKey]);
            character.LoginColor = Convert.ToInt32(characterSettings[EtConstants.CharacterLoginColorKey]);

            if (Convert.ToBoolean(characterSettings[EtConstants.CharacterActiveState]))
                ActiveCharacters.Add(name);
            else
                InactiveCharacters.Add(name);

            for (int i = 0; i < threshholdPrices.Count; i++)
            {
                character.QuantityThreshHolds.Add(new[] {threshholdPrices[i], threshholdQuantities[i]});
            }
            _characters.Add(name, character);

            character.TradeQueue = LoadTradeQueue(character.Name);
            character.TradeHistory = LoadTradeHistory(character.Name);

            return character;
        }

        private Queue<int> LoadTradeQueue(String name)
        {
            List<String> fileData = _textFileio.Read(_paths.ConfigPath + Paths.CharacterDataSubDir,
                                                     name + TradeQueueFileName);
            var tradeQueue = new Queue<int>();

            foreach (String s in fileData)
            {
                tradeQueue.Enqueue(Convert.ToInt32(s));
            }
            return tradeQueue;
        }

        private Dictionary<int, int> LoadTradeHistory(String name)
        {
            List<String> fileData = _textFileio.Read(_paths.ConfigPath + Paths.CharacterDataSubDir,
                                                     name + TradeHistoryFileName);
            return fileData.ToDictionary(int.Parse, int.Parse);
        }

        private void SaveTradeQueue(String name, int[] tradeQueue)
        {
            List<Object> data = tradeQueue.Cast<object>().ToList();
            _textFileio.Save(data, _paths.ConfigPath + Paths.CharacterDataSubDir, name + TradeQueueFileName);
        }

        private void SaveTradeHistory(String name, IEnumerable<int> tradeHistory)
        {
            List<Object> data = tradeHistory.Cast<object>().ToList();
            _textFileio.Save(data, _paths.ConfigPath + Paths.CharacterDataSubDir, name + TradeHistoryFileName);
        }

        public Dictionary<String, String> ConvertCharacterToDictionary(Character character)
        {
            var result = new Dictionary<String, String>
                             {
                                 {EtConstants.AccountLoginKey, character.Account.UserName},
                                 {EtConstants.AccountPasswordKey, character.Account.Password},
                                 {EtConstants.AccountIdKey, character.Account.Id},
                                 {EtConstants.CharacterIdKey, character.Id},
                                 {EtConstants.CharacterStationIdKey, Convert.ToString(character.StationId)},
                                 {EtConstants.CharacterTradeItemsKey, Convert.ToString(character.ShouldTradeItems)},
                                 {EtConstants.CharacterTradeShipsKey, Convert.ToString(character.ShouldTradeShips)},
                                 {EtConstants.CharacterAdjustBuysKey, Convert.ToString(character.ShouldAdjustBuys)},
                                 {EtConstants.CharacterAdjustSellsKey, Convert.ToString(character.ShouldAdjustSells)},
                                 {EtConstants.CharacterMaximumOrdersKey, Convert.ToString(character.MaximumOrders)},
                                 {EtConstants.CharacterLoginColorKey, Convert.ToString(character.LoginColor)},
                                 {
                                     EtConstants.CharacterActiveState,
                                     Convert.ToString(ActiveCharacters.Contains(character.Name))
                                     }
                             };
            return result;
        }

        private void CharacterSettingUpdatedListener(object o, string name, string key, string value)
        {
            try
            {
                switch (key)
                {
                    case EtConstants.AccountLoginKey:
                        _characters[name].Account.UserName = value;
                        break;
                    case EtConstants.AccountPasswordKey:
                        _characters[name].Account.Password = value;
                        break;
                    case EtConstants.CharacterTradeItemsKey:
                        _characters[name].ShouldTradeItems = Convert.ToBoolean(value);
                        break;
                    case EtConstants.CharacterTradeShipsKey:
                        _characters[name].ShouldTradeShips = Convert.ToBoolean(value);
                        break;
                    case EtConstants.CharacterAdjustSellsKey:
                        _characters[name].ShouldAdjustSells = Convert.ToBoolean(value);
                        break;
                    case EtConstants.CharacterAdjustBuysKey:
                        _characters[name].ShouldAdjustBuys = Convert.ToBoolean(value);
                        break;
                    case EtConstants.CharacterMaximumOrdersKey:
                        _characters[name].MaximumOrders = Convert.ToInt32(value);
                        break;
                    case EtConstants.CharacterStationIdKey:
                        _characters[name].StationId = Convert.ToInt32(value);
                        break;
                    case EtConstants.AccountIdKey:
                        _characters[name].Account.Id = value;
                        break;
                    case EtConstants.CharacterIdKey:
                        _characters[name].Id = value;
                        break;
                    case EtConstants.CharacterLoginColorKey:
                        _characters[name].LoginColor = Convert.ToInt32(value);
                        break;
                    default:
                        _eventDispatcher.LogError("Attempt to save value for non-existant character data key.");
                        break;
                }
            }
            catch
            {
                _eventDispatcher.LogError("Attempt to save value of incorrect type for the given character data key.");
            }
        }
    }
}