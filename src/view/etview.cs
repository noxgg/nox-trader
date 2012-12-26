using System;
using System.Collections.Generic;
using System.Windows.Forms;
using noxiousET.src;
using noxiousET.src.control;
using noxiousET.src.etevent;
using Clipboard = noxiousET.src.guiInteraction.Clipboard;

//TODO: Margin trading scam? Order creation flags. Max order setting. Buying an item with no sell orders shifts buy order quantity box up.

namespace noxiousET
{
    internal partial class etview : Form
    {
        private readonly CharacterInfoProvider _characterInfoProvider;
        private readonly ClientConfigInfoProvider _clientConfigInfoProvider;
        private readonly EventDispatcher _eventDispatcher;
        private readonly AutomationRequester _manualExecution;
        private readonly OrderReviewInfoProvider _orderReviewInfoProvider;

        public etview(CharacterInfoProvider characterInfoProvider, ClientConfigInfoProvider clientConfigInfoProvider,
                      OrderReviewInfoProvider orderReviewInfoProvider, AutomationRequester manualExecution)
        {
            InitializeComponent();
            _characterInfoProvider = characterInfoProvider;
            _clientConfigInfoProvider = clientConfigInfoProvider;
            _orderReviewInfoProvider = orderReviewInfoProvider;
            _manualExecution = manualExecution;
            _eventDispatcher = EventDispatcher.Instance;
            _eventDispatcher.GenericEvent += GenericEventListener;
            _eventDispatcher.GenericErrorEvent += GenericErrorEventListener;
            _eventDispatcher.AutoAdjusterEvent += AutoAdjusterListener;
            _eventDispatcher.AutoListerEvent += AutoListerListener;
            InitializeCharacterView();
            InitializeConfigView();
        }

        private void GenericEventListener(object o, string s)
        {
            if (InvokeRequired)
            {
                var els = new EventListenerCallback(GenericEventListener);
                Invoke(els, new[] {o, s});
            }
            else
            {
                consoleLB.Items.Add(s);
                consoleLB.SelectedIndex = consoleLB.Items.Count - 1;
            }
        }

        private void GenericErrorEventListener(object o, string s)
        {
            if (InvokeRequired)
            {
                var els = new ErrorEventListenerCallback(GenericErrorEventListener);
                Invoke(els, new[] {o, s});
            }
            else
            {
                consoleLB.Items.Add(s);
            }
        }

        private void AutoAdjusterListener(object o, string s)
        {
            if (InvokeRequired)
            {
                var els = new AutoAdjusterListenerCallback(AutoAdjusterListener);
                Invoke(els, new[] {o, s});
            }
            else
            {
                autoAdjusterLB.Items.Add(s);
            }
        }

        private void AutoListerListener(object o, string s)
        {
            if (InvokeRequired)
            {
                var els = new AutoListerListenerCallback(AutoListerListener);
                Invoke(els, new[] {o, s});
            }
            else
            {
                autoListerLB.Items.Add(s);
            }
        }

        private void InitializeConfigView()
        {
            String[] paths = _clientConfigInfoProvider.GetPaths();
            logPathTB.Text = paths[0];
            clientPathTB.Text = paths[1];
            configPathTB.Text = paths[2];
            eveSettingsTB.Text = paths[3];

            String[] config = _clientConfigInfoProvider.GetConfig();
            timingTB.Text = config[0];
            iterationsTB.Text = config[1];
            xResolutionTB.Text = config[2];
            yResolutionTB.Text = config[3];
        }

        private void InitializeCharacterView()
        {
            List<String> characters = _characterInfoProvider.GetCharacterList();
            foreach (String c in characters)
                charactersLB.Items.Add(c);

            if (charactersLB.Items.Count > 0)
            {
                String selected = _characterInfoProvider.GetSelectedCharacter();
                if (selected == null)
                    charactersLB.SelectedIndex = 0;
                else
                    charactersLB.SelectedIndex = charactersLB.Items.IndexOf(selected);
                DisplayCharacterInfo(_characterInfoProvider.GetCharacterInfo((String) charactersLB.SelectedItem),
                                     _characterInfoProvider.GetCharacterKnownItems((String) charactersLB.SelectedItem));
            }
        }

        private void DisplayCharacterInfo(Dictionary<string, string> characterInfo, List<String> items)
        {
            nameTB.Text = (String) charactersLB.SelectedItem;
            loginTB.Text = characterInfo[EtConstants.AccountLoginKey];
            passwordTB.Text = characterInfo[EtConstants.AccountPasswordKey];
            characterIdTB.Text = characterInfo[EtConstants.CharacterIdKey];
            stationidTB.Text = characterInfo[EtConstants.CharacterStationIdKey];
            accountIdTB.Text = characterInfo[EtConstants.AccountIdKey];
            itemsCB.Checked = Convert.ToBoolean(characterInfo[EtConstants.CharacterTradeItemsKey]);
            shipsCB.Checked = Convert.ToBoolean(characterInfo[EtConstants.CharacterTradeShipsKey]);
            aasellCB.Checked = Convert.ToBoolean(characterInfo[EtConstants.CharacterAdjustSellsKey]);
            aabuyCB.Checked = Convert.ToBoolean(characterInfo[EtConstants.CharacterAdjustBuysKey]);
            maximumOrdersTB.Text = characterInfo[EtConstants.CharacterMaximumOrdersKey];
            loginColorTB.Text = characterInfo[EtConstants.CharacterLoginColorKey];

            knownItemsListBox.Items.Clear();
            foreach (String s in items)
            {
                knownItemsListBox.Items.Add(s);
            }
        }

        private void NoxiousEtLoad(object sender, EventArgs e)
        {
        }

        private void CharactersLbSelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayCharacterInfo(_characterInfoProvider.GetCharacterInfo((String) charactersLB.SelectedItem),
                                 _characterInfoProvider.GetCharacterKnownItems((String) charactersLB.SelectedItem));
            _characterInfoProvider.SetSelectedCharacter((String) charactersLB.SelectedItem);
        }

        private void AutomateBClick(object sender, EventArgs e)
        {
            _manualExecution.Automate();
        }

        private void SaveSelectedCharacterBClick(object sender, EventArgs e)
        {
            _characterInfoProvider.SaveCharacter((String) charactersLB.SelectedItem);
        }

        private void CharacterUpdateIntegerValue(string character, string key, string value)
        {
            int result;
            try
            {
                result = int.Parse(value);
            }
            catch
            {
                return;
            }
            CharacterUpdate(character, key, value);
        }

        private void CharacterUpdate(string character, string key, string value)
        {
            if (value.Length > 0)
                _eventDispatcher.CharacterSettingUpdated(character, key, value);
        }

        private void LoginTbKeyUp(object sender, EventArgs e)
        {
            CharacterUpdate((String) charactersLB.SelectedItem, EtConstants.AccountLoginKey, loginTB.Text);
        }

        private void PasswordTbKeyUp(object sender, EventArgs e)
        {
            CharacterUpdate((String) charactersLB.SelectedItem, EtConstants.AccountPasswordKey, passwordTB.Text);
        }

        private void LoginColorTbKeyUp(object sender, EventArgs e)
        {
            CharacterUpdateIntegerValue((String) charactersLB.SelectedItem, EtConstants.CharacterLoginColorKey,
                                        loginColorTB.Text);
        }

        private void StationidTbKeyUp(object sender, EventArgs e)
        {
            CharacterUpdateIntegerValue((String) charactersLB.SelectedItem, EtConstants.CharacterStationIdKey,
                                        stationidTB.Text);
        }

        private void AccountIdTbKeyUp(object sender, EventArgs e)
        {
            CharacterUpdateIntegerValue((String) charactersLB.SelectedItem, EtConstants.AccountIdKey, accountIdTB.Text);
        }

        private void MaximumOrdersTbKeyUp(object sender, EventArgs e)
        {
            CharacterUpdateIntegerValue((String) charactersLB.SelectedItem, EtConstants.CharacterMaximumOrdersKey,
                                        maximumOrdersTB.Text);
        }

        private void CharacterIdTbTextChanged(object sender, EventArgs e)
        {
            CharacterUpdateIntegerValue((String) charactersLB.SelectedItem, EtConstants.CharacterIdKey,
                                        characterIdTB.Text);
        }

        private void ShipsCbClick(object sender, EventArgs e)
        {
            CharacterUpdate((String) charactersLB.SelectedItem, EtConstants.CharacterTradeShipsKey,
                            Convert.ToString(shipsCB.Checked));
        }

        private void ItemsCbClick(object sender, EventArgs e)
        {
            CharacterUpdate((String) charactersLB.SelectedItem, EtConstants.CharacterTradeItemsKey,
                            Convert.ToString(itemsCB.Checked));
        }

        private void AabuyCbClick(object sender, EventArgs e)
        {
            CharacterUpdate((String) charactersLB.SelectedItem, EtConstants.CharacterAdjustBuysKey,
                            Convert.ToString(aabuyCB.Checked));
        }

        private void AasellCbClick(object sender, EventArgs e)
        {
            CharacterUpdate((String) charactersLB.SelectedItem, EtConstants.CharacterAdjustSellsKey,
                            Convert.ToString(aasellCB.Checked));
        }

        private void AlbuyCbClick(object sender, EventArgs e)
        {
            //characterUpdate((String)charactersLB.SelectedItem, "autoListBuys", Convert.ToString(albuyCB.Checked));
        }

        private void AlsellCbClick(object sender, EventArgs e)
        {
            //characterUpdate((String)charactersLB.SelectedItem, "autoListSells", Convert.ToString(alsellCB.Checked));
        }

        private void ConfigPathTbTextChanged(object sender, EventArgs e)
        {
            if (configPathTB.Text.Length > 0)
                _eventDispatcher.ClientSettingUpdated((String) charactersLB.SelectedItem, EtConstants.ConfigPathKey,
                                                      configPathTB.Text);
        }

        private void ClientPathTbTextChanged(object sender, EventArgs e)
        {
            if (clientPathTB.Text.Length > 0)
                _eventDispatcher.ClientSettingUpdated((String) charactersLB.SelectedItem, EtConstants.ClientPathKey,
                                                      clientPathTB.Text);
        }

        private void LogPathTbTextChanged(object sender, EventArgs e)
        {
            if (logPathTB.Text.Length > 0)
                _eventDispatcher.ClientSettingUpdated((String) charactersLB.SelectedItem, EtConstants.LogPathKey,
                                                      logPathTB.Text);
        }

        private void EveSettingsTbTextChanged(object sender, EventArgs e)
        {
            if (eveSettingsTB.Text.Length > 0)
                _eventDispatcher.ClientSettingUpdated((String) charactersLB.SelectedItem, EtConstants.EveSettingsPathKey,
                                                      eveSettingsTB.Text);
        }

        private void YResolutionTbTextChanged(object sender, EventArgs e)
        {
            _eventDispatcher.ClientSettingUpdated((String) charactersLB.SelectedItem, EtConstants.YResolutionKey,
                                                  yResolutionTB.Text);
        }

        private void XResolutionTbTextChanged(object sender, EventArgs e)
        {
            _eventDispatcher.ClientSettingUpdated((String) charactersLB.SelectedItem, EtConstants.XResolutionKey,
                                                  xResolutionTB.Text);
        }

        private void TimingTbTextChanged(object sender, EventArgs e)
        {
            _eventDispatcher.ClientSettingUpdated((String) charactersLB.SelectedItem, EtConstants.TimingMultiplierKey,
                                                  timingTB.Text);
        }

        private void IterationsTbTextChanged(object sender, EventArgs e)
        {
            _eventDispatcher.ClientSettingUpdated((String) charactersLB.SelectedItem, EtConstants.IterationsKey,
                                                  iterationsTB.Text);
        }

        private void SaveAllSettingsBClick(object sender, EventArgs e)
        {
            _eventDispatcher.SaveAllSettingsRequest();
        }

        private void GetTypeBClick(object sender, EventArgs e)
        {
            _eventDispatcher.GetTypesFromFileRequest((String) charactersLB.SelectedItem);
        }

        private void UnpauseBClick(object sender, EventArgs e)
        {
        }

        private void KnownItemsListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (knownItemsListBox.SelectedItem != null)
                Clipboard.SetClip(knownItemsListBox.SelectedItem.ToString());
        }

        private void FetchItemsFromQuickbarButtonClick(object sender, EventArgs e)
        {
            _eventDispatcher.GetTypesFromQuickbarRequest((String) charactersLB.SelectedItem, firstItemTextbox.Text,
                                                         lastItemTextbox.Text);
        }

        private void ItemsToReviewListKeyPress(object sender, KeyPressEventArgs e)
        {
            if (itemsToReviewList.SelectedIndices.Count <= 0)
                return;
            String pressedKey = e.KeyChar.ToString();
            int selectedIndex = itemsToReviewList.SelectedIndices[0];

            string newItemAction;
            if (pressedKey.ToLower().Equals("x"))
                newItemAction = "Update";
            else if (pressedKey.ToLower().Equals("c"))
                newItemAction = "Cancel";
            else
                newItemAction = "Ignore";
            itemsToReviewList.Items[selectedIndex].SubItems[1].Text = newItemAction;

            string character = itemsToReviewList.Items[selectedIndex].SubItems[2].Text;
            string typeId = itemsToReviewList.Items[selectedIndex].SubItems[4].Text;
            string buyOrSell = itemsToReviewList.Items[selectedIndex].SubItems[0].Text;

            _eventDispatcher.UpdateActionToTakeRequest(character, typeId, buyOrSell.Equals("B"), newItemAction);
        }

        private void ItemsToReviewListSelectedIndexChanged(object sender, EventArgs e)
        {
            if (itemsToReviewList.SelectedIndices.Count <= 0)
                return;

            int selectedIndex = itemsToReviewList.SelectedIndices[0];
            String character = itemsToReviewList.Items[selectedIndex].SubItems[2].Text;
            String typeId = itemsToReviewList.Items[selectedIndex].SubItems[4].Text;
            String buyOrSell = itemsToReviewList.Items[selectedIndex].SubItems[0].Text;
            DisplayOrderDetails(_orderReviewInfoProvider.GetItemDetails(character, typeId, buyOrSell));
        }

        private void DisplayOrderDetails(List<string[]> orderDetails)
        {
            int size = reviewDetailsListView.Items.Count;
            for (int i = 0; i < size; i++)
            {
                reviewDetailsListView.Items.RemoveAt(0);
            }
            size = orderDetails.Count;
            for (int i = 1; i < size; i++)
            {
                var item = new ListViewItem(orderDetails[i][0]);
                item.SubItems.Add(String.Format("{0:0,0.00}", Double.Parse(orderDetails[i][1])));
                item.SubItems.Add(orderDetails[i][2]);
                item.SubItems.Add(orderDetails[i][3]);
                reviewDetailsListView.Items.Add(item);
            }
        }

        private void ClientTabsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (clientTabs.SelectedIndex.Equals(5))
            {
                List<String[]> ordersRequiringReview = _orderReviewInfoProvider.GetOrdersRequiringReview();
                int size = itemsToReviewList.Items.Count;
                for (int i = 0; i < size; i++)
                {
                    itemsToReviewList.Items.RemoveAt(0);
                }
                foreach (var entry in ordersRequiringReview)
                {
                    var item = new ListViewItem(buyOrSellToString(entry[0]));
                    item.SubItems.Add(entry[1]);
                    item.SubItems.Add(entry[2]);
                    item.SubItems.Add(entry[3]);
                    item.SubItems.Add(entry[4]);
                    itemsToReviewList.Items.Add(item);
                }
            }
        }

        private string buyOrSellToString(string buyOrSell)
        {
            return buyOrSell.Equals(EtConstants.Buy.ToString()) ? "B" : "S";
        }

        #region Nested type: AutoAdjusterListenerCallback

        private delegate void AutoAdjusterListenerCallback(object o, string s);

        #endregion

        #region Nested type: AutoListerListenerCallback

        private delegate void AutoListerListenerCallback(object o, string s);

        #endregion

        #region Nested type: ErrorEventListenerCallback

        private delegate void ErrorEventListenerCallback(object o, string s);

        #endregion

        #region Nested type: EventListenerCallback

        private delegate void EventListenerCallback(object o, string s);

        #endregion
    }
}