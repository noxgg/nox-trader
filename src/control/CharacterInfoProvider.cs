using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using noxiousET.src.data.characters;

namespace noxiousET.src.control
{
    class CharacterInfoProvider
    {
        CharacterManager characterManager;

        public CharacterInfoProvider(CharacterManager characterManager)
        {
            this.characterManager = characterManager;
        }

        public List<String> getCharacterList()
        {
            return characterManager.getAllCharacterNames();
        }

        public String getSelectedCharacter()
        {
            return characterManager.selected;
        }

        public String[] getCharacterInfo(String character)
        {
            return characterManager.convertCharacterToStringArray(characterManager.getCharacter(character));
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
