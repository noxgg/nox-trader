using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using noxiousET.src.data.io;
using noxiousET.src.data.paths;
using noxiousET.src.data.accounts;

namespace noxiousET.src.data.characters
{
    class CharacterManager
    {
        private Dictionary<String, Character> characters;
        private TextFileio textFileio;
        private AccountManager accountManager;
        private Paths paths;
        public List<String> active { set; get; }
        public List<String> inactive { set; get; }
        public String selected { set; get; }

        public CharacterManager(Paths paths, AccountManager accountManager)
        {
            this.paths = paths;
            this.accountManager = accountManager;
            characters = new Dictionary<String, Character>();
            active = new List<String>();
            inactive = new List<String>();
            selected = null;
            textFileio = new TextFileio(paths.configPath, null);
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
            addCharacter(loadCharacter(name));
            
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

        public void loadCharacters(String[] names)
        {
            foreach (String n in names)
                loadCharacter(n);
        }

        private Character loadCharacter(string name)
        {
            textFileio.fileName = name + ".ini";
            List<String> characterData = textFileio.load();
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
            character.loginColors = Convert.ToInt32(characterData[line++]);

            if (Convert.ToBoolean(characterData[line++]))
                active.Add(name);
            else
                inactive.Add(name);

            while (line < characterData.Count)
            {
                character.quantityThreshHolds.Add(new int[] { Convert.ToInt32(characterData[line++]), Convert.ToInt32(characterData[line++]) });
            }
            return character;
        }
    }
}
