using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using noxiousET.src.data.characters;
using noxiousET.src.data.modules;

namespace noxiousET.src.control
{
    class CharacterInfoProvider
    {
        CharacterManager characterManager;
        Modules modules;

        public CharacterInfoProvider(CharacterManager characterManager, Modules modules)
        {
            this.characterManager = characterManager;
            this.modules = modules;
        }

        public List<String> getCharacterList()
        {
            return characterManager.getAllCharacterNames();
        }

        public String getSelectedCharacter()
        {
            return characterManager.selected;
        }

        public Dictionary<string, string> getCharacterInfo(String character)
        {
            return characterManager.convertCharacterToDictionary(characterManager.getCharacter(character));
        }

        public List<String> getCharacterKnownItems(String character)
        {
            return modules.getAlphabetizedItemNames(characterManager.getCharacter(character).tradeHistory.Values);
        }

        public void setSelectedCharacter(String character)
        {
            characterManager.selected = character;
        }

        public void saveCharacter(String name)
        {
            characterManager.save(name);
        }
    }
}
