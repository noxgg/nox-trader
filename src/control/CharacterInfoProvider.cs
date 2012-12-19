using System;
using System.Collections.Generic;
using noxiousET.src.data.characters;
using noxiousET.src.data.modules;

namespace noxiousET.src.control
{
    internal class CharacterInfoProvider
    {
        private readonly CharacterManager _characterManager;
        private readonly Modules _modules;

        public CharacterInfoProvider(CharacterManager characterManager, Modules modules)
        {
            _characterManager = characterManager;
            _modules = modules;
        }

        public List<String> GetCharacterList()
        {
            return _characterManager.GetAllCharacterNames();
        }

        public String GetSelectedCharacter()
        {
            return _characterManager.SelectedCharacter;
        }

        public Dictionary<string, string> GetCharacterInfo(String character)
        {
            return _characterManager.ConvertCharacterToDictionary(_characterManager.GetCharacter(character));
        }

        public List<String> GetCharacterKnownItems(String character)
        {
            return _modules.GetAlphabetizedItemNames(_characterManager.GetCharacter(character).TradeHistory.Values);
        }

        public void SetSelectedCharacter(String character)
        {
            _characterManager.SelectedCharacter = character;
        }

        public void SaveCharacter(String name)
        {
            _characterManager.Save(name);
        }
    }
}