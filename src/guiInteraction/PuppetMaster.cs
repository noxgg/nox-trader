using System;
using System.Threading;
using System.Collections.Generic;
using noxiousET.src.etevent;
using noxiousET.src.data;
using noxiousET.src.data.characters;
using noxiousET.src.guiInteraction.login;
using noxiousET.src.guiInteraction.orders.autoadjuster;
using noxiousET.src.guiInteraction.orders.autolister;
using noxiousET.src.guiInteraction.orders.autoinvester;

namespace noxiousET.src.guiInteraction
{
    class PuppetMaster
    {
        DataManager dataManager;
        CharacterManager characterManager;
        public LoginBot loginBot {set; get;}
        AutoAdjuster autoAdjuster;
        AutoLister autoLister;
        EventDispatcher eventDispatcher;
        AutoInvestor autoInvestor;

        public PuppetMaster(DataManager dataManager)
        {
            this.dataManager = dataManager;
            this.characterManager = dataManager.characterManager;
            this.loginBot = new LoginBot(dataManager.clientConfig, dataManager.uiElements, dataManager.paths, null, dataManager.eventDispatcher);
            this.autoLister = new AutoLister(dataManager.clientConfig, dataManager.uiElements, dataManager.paths, null, dataManager.modules, dataManager.eventDispatcher);
            this.autoAdjuster = new AutoAdjuster(dataManager.clientConfig, dataManager.uiElements, dataManager.paths, null, dataManager.modules, dataManager.eventDispatcher);
            this.autoInvestor = new AutoInvestor(dataManager.clientConfig, dataManager.uiElements, dataManager.paths, null, dataManager.modules, dataManager.eventDispatcher);
            this.eventDispatcher = dataManager.eventDispatcher;
        }
        public int automate(int iterations)
        {
            int result = 0;
            Character character;
            Queue<String> queue;
            Random random = new Random((int) DateTime.Now.Ticks & 0x0000FFFF);

            if (characterManager.selected == null && characterManager.active.Count > 0)
                characterManager.selected = characterManager.active[0];
            //else exception

            queue = new Queue<String>();
            int characterCount = characterManager.active.Count;
            int selectedIndex = characterManager.active.IndexOf(characterManager.selected);
            for (int i = 0; i < characterCount; i++)
                queue.Enqueue(characterManager.active[(i + selectedIndex) % characterCount]);

            iterations *= queue.Count;
            for (int i = 0; i < iterations; i++)
            {
                if (i != 0 && i % characterCount == 0)
                {
                    if (random.Next(0, 10) % 3 == 0)
                        Thread.Sleep(random.Next(0, 3600000));
                    eventDispatcher.log("Starting run #" + (i / characterCount) + 1);
                }
                character = characterManager.getCharacter(queue.Dequeue());
                characterManager.selected = character.name;
                result = loginBot.login(character);
                if (result == 0)
                {
                    autoAdjuster.execute(character, true);
                    if (autoAdjuster.getNumberOfFreeOrders() > 5)
                        autoLister.execute(character);
                    if (autoLister.getNumberOfFreeOrders() > 5)
                        autoInvestor.execute(character);
                    characterManager.save(character.name);
                }
                queue.Enqueue(character.name);
                if (queue.Count > 1)
                    autoAdjuster.killClient();
            }
            return 0;
        }
    }
}
