using System;
using System.Collections.Generic;
using System.Threading;
using noxiousET.src.data;
using noxiousET.src.data.characters;
using noxiousET.src.etevent;
using noxiousET.src.guiInteraction.login;
using noxiousET.src.guiInteraction.orders.autoadjuster;
using noxiousET.src.guiInteraction.orders.autoinvester;
using noxiousET.src.guiInteraction.orders.autolister;
using noxiousET.src.helpers;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction
{
    internal class PuppetMaster
    {
        private readonly AutoAdjuster _autoAdjuster;
        private readonly AutoInvestor _autoInvestor;
        private readonly AutoLister _autoLister;
        private readonly CharacterManager _characterManager;
        private readonly EventDispatcher _eventDispatcher;
        private readonly OrderAnalyzer _orderAnalyzer;

        public PuppetMaster(DataManager dataManager)
        {
            _characterManager = dataManager.CharacterManager;
            _orderAnalyzer = new OrderAnalyzer();
            OrderReviewer = new OrderReviewer(dataManager.EventDispatcher);
            LoginBot = new LoginBot(dataManager.ClientConfig, dataManager.UiElements, dataManager.Paths, null,
                                    _orderAnalyzer);
            _autoLister = new AutoLister(dataManager.ClientConfig, dataManager.UiElements, dataManager.Paths, null,
                                         dataManager.Modules, _orderAnalyzer);
            _autoAdjuster = new AutoAdjuster(dataManager.ClientConfig, dataManager.UiElements, dataManager.Paths, null,
                                             dataManager.Modules, _orderAnalyzer, OrderReviewer);
            _autoInvestor = new AutoInvestor(dataManager.ClientConfig, dataManager.UiElements, dataManager.Paths, null,
                                             dataManager.Modules, _orderAnalyzer);
            _eventDispatcher = dataManager.EventDispatcher;
            _eventDispatcher.getTypesFromQuickbarRequestHandler += GetTypeForCharacterFromQuickbar;
        }

        public LoginBot LoginBot { set; get; }
        public OrderReviewer OrderReviewer { set; get; }


        private void GetTypeForCharacterFromQuickbar(object o, String name, String firstItemId, String lastItemId)
        {
            _autoInvestor.GetTypeForCharacterFromQuickbar(_characterManager.GetCharacter(name), firstItemId, lastItemId);
        }

        public int Automate(int iterations)
        {
            var random = new Random((int) DateTime.Now.Ticks & 0x0000FFFF);

            if (_characterManager.SelectedCharacter == null && _characterManager.ActiveCharacters.Count > 0)
                _characterManager.SelectedCharacter = _characterManager.ActiveCharacters[0];
            //else exception

            var queue = new Queue<String>();
            int characterCount = _characterManager.ActiveCharacters.Count;
            int selectedIndex = _characterManager.ActiveCharacters.IndexOf(_characterManager.SelectedCharacter);
            for (int i = 0; i < characterCount; i++)
                queue.Enqueue(_characterManager.ActiveCharacters[(i + selectedIndex)%characterCount]);

            iterations *= queue.Count;
            for (int i = 0; i < iterations; i++)
            {
                //Take occassional breaks to simulate being a human.
                if (i != 0 && i%characterCount == 0)
                {
                    if (random.Next(0, 10)%3 == 0)
                        Thread.Sleep(random.Next(0, 3600000));
                    _eventDispatcher.Log("Starting run #" + ((i/characterCount) + 1));
                }

                Character character = _characterManager.GetCharacter(queue.Dequeue());
                _characterManager.SelectedCharacter = character.Name;

                try
                {
                    Automate(character);
                }
                catch (Exception)
                {
                    ProcessKiller.killProcess("ExeFile");
                }

                queue.Enqueue(character.Name);

                if (queue.Count > 1)
                    _autoAdjuster.KillClient();
            }
            return 0;
        }

        private void Automate(Character character)
        {
            if (LoginBot.Login(character) != 0)
                return;

            _autoAdjuster.Execute(character);
            _characterManager.Save(character.Name);

            if (_autoAdjuster.GetNumberOfFreeOrders() < 5)
                return;

            _autoLister.Execute(character);
            _characterManager.Save(character.Name);

            if (_autoLister.FreeOrders < 5)
                return;

            _autoInvestor.Execute(character);
            _characterManager.Save(character.Name);
        }
    }
}