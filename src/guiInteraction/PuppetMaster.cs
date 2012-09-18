﻿using System;
using System.Threading;
using System.Collections.Generic;
using noxiousET.src.etevent;
using noxiousET.src.data;
using noxiousET.src.data.characters;
using noxiousET.src.guiInteraction.login;
using noxiousET.src.guiInteraction.orders.autoadjuster;
using noxiousET.src.guiInteraction.orders.autolister;
using noxiousET.src.guiInteraction.orders.autoinvester;
using noxiousET.src.orders;

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
        private OrderAnalyzer orderAnalyzer;

        public PuppetMaster(DataManager dataManager)
        {
            this.dataManager = dataManager;
            characterManager = dataManager.characterManager;
            orderAnalyzer = new OrderAnalyzer();
            loginBot = new LoginBot(dataManager.clientConfig, dataManager.uiElements, dataManager.paths, null, orderAnalyzer);
            autoLister = new AutoLister(dataManager.clientConfig, dataManager.uiElements, dataManager.paths, null, dataManager.modules, orderAnalyzer);
            autoAdjuster = new AutoAdjuster(dataManager.clientConfig, dataManager.uiElements, dataManager.paths, null, dataManager.modules, orderAnalyzer);
            autoInvestor = new AutoInvestor(dataManager.clientConfig, dataManager.uiElements, dataManager.paths, null, dataManager.modules, orderAnalyzer);
            eventDispatcher = dataManager.eventDispatcher;
            eventDispatcher.getTypesFromQuickbarRequestHandler += new EventDispatcher.GetTypesFromQuickbarRequestHandler(getTypeForCharacterFromQuickbar);       
        }


        private void getTypeForCharacterFromQuickbar(object o, String name, String firstItemId, String lastItemId)
        {
            autoInvestor.getTypeForCharacterFromQuickbar(characterManager.getCharacter(name), firstItemId, lastItemId);
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
                //Take occassional breaks to simulate being a human.
                if (i != 0 && i % characterCount == 0)
                {
                    if (random.Next(0, 10) % 3 == 0)
                        Thread.Sleep(random.Next(0, 3600000));
                    eventDispatcher.log("Starting run #" + ((i / characterCount) + 1));
                }

                character = characterManager.getCharacter(queue.Dequeue());
                characterManager.selected = character.name;

                automate(character);

                queue.Enqueue(character.name);

                if (queue.Count > 1)
                    autoAdjuster.killClient();
            }
            return 0;
        }

        private void automate(Character character)
        {
            if (loginBot.login(character) != 0)
                return;

            autoAdjuster.execute(character);
            characterManager.save(character.name);

            if (autoAdjuster.getNumberOfFreeOrders() < 5)
                return;

            autoLister.execute(character);
            characterManager.save(character.name);

            if (autoLister.getNumberOfFreeOrders() < 5)
                return;

            autoInvestor.execute(character);
            characterManager.save(character.name);
        }
    }
}
