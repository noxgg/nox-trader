using System;
using System.Collections.Generic;
using noxiousET.src.data;
using noxiousET.src.data.characters;
using noxiousET.src.guiInteraction.login;
using noxiousET.src.guiInteraction.orders.autoadjuster;
using noxiousET.src.guiInteraction.orders.autolister;

namespace noxiousET.src.guiInteraction
{
    class PuppetMaster
    {
        DataManager dataManager;
        CharacterManager characterManager;
        LoginBot loginBot;
        AutoAdjuster autoAdjuster;
        AutoLister autoLister;
        private static readonly int stopAllActivity = -69;

        public PuppetMaster(DataManager dataManager)
        {
            this.dataManager = dataManager;
            this.characterManager = dataManager.characterManager;
            this.loginBot = new LoginBot(dataManager.clientConfig, dataManager.uiElements, dataManager.paths, null);
            this.autoLister = new AutoLister(dataManager.clientConfig, dataManager.uiElements, dataManager.paths, null, dataManager.modules);
            this.autoAdjuster = new AutoAdjuster(dataManager.clientConfig, dataManager.uiElements, dataManager.paths, null, dataManager.modules);
        }
        private int fullAutoManager()
        {
            int result = 0;
            int tries = 0;

            Character character;

            if (characterManager.selected == null && characterManager.active.Count > 0)
            {
                characterManager.selected = characterManager.active[0];
            }
            else
            {
                //exception
            }

            //Reorder the list so that the selected character is first.
            int index = characterManager.active.IndexOf(characterManager.selected);
            if (index != 0)
            {
                List<String> front = new List<String>();
                List<String> back = new List<String>();
                for (int i = 0; i < index; i++)
                    back.Add(characterManager.active[i]);
                for (int i = index; i < characterManager.active.Count; i++)
                    front.Add(characterManager.active[i]);
                front.AddRange(back);
                characterManager.active = front;
            }

            for (int i = 0; i < 50; i++)
            {
                foreach (String c in characterManager.active)
                {
                    character = characterManager.getCharacter(c);
                    loginBot.character = character;
                    //excepListBox.Items.Add("Starting run #" + i + 1 + ". i % autoSellEveryNRuns = " + i % autoSellEveryNRuns);
                    tries = 0;
                    while ((result = loginBot.launchEVEPrep()) != 0 && tries < 5)
                    {
                        if (result == stopAllActivity)
                        {
                            //if (exceptionsListbox.Items.Count == 0)
                                //  displayExceptions();
                            return 1;
                        }
                        else
                        {
                            tries++;
                            //What to do when we fail too many times?
                        }
                    }
                    if (character.autoAdjustsPerAutoList > 0 && (i % character.autoAdjustsPerAutoList) == 0)
                    {
                        autoLister.character = character;
                        if (autoLister.execute() == stopAllActivity)
                        {
                            //if (exceptionsListbox.Items.Count == 0)
                                //  displayExceptions();
                            return 1;
                        }
                    }
                    autoAdjuster.character = character;
                    if (autoAdjuster.execute(true) == stopAllActivity)
                    {
                        //if (exceptionsListbox.Items.Count == 0)
                            //displayExceptions();
                        return 1;
                    }
                    if (characterManager.active.Count > 1)
                    {
                        autoAdjuster.killClient();
                    }
                    //displayExceptions();
                }
            }
            return 0;
        }
    }
}
