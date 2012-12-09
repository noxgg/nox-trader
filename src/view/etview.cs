using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using noxiousET.src;
using noxiousET.src.control;
using noxiousET.src.etevent;
using noxiousET.src.guiInteraction;


//TODO: Margin trading scam? Order creation flags. Max order setting. Buying an item with no sell orders shifts buy order quantity box up.
namespace noxiousET
{
    partial class etview : Form
    {
        OrderReviewInfoProvider orderReviewInfoProvider;
        CharacterInfoProvider characterInfoProvider;
        ClientConfigInfoProvider clientConfigInfoProvider;
        AutomationRequester manualExecution;
        EventDispatcher eventDispatcher;
        delegate void EventListenerCallback(object o, string s);
        delegate void ErrorEventListenerCallback(object o, string s);
        delegate void AutoAdjusterListenerCallback(object o, string s);
        delegate void AutoListerListenerCallback(object o, string s);

        public etview(CharacterInfoProvider characterInfoProvider, ClientConfigInfoProvider clientConfigInfoProvider, OrderReviewInfoProvider orderReviewInfoProvider, AutomationRequester manualExecution)
        {
            InitializeComponent();
            this.characterInfoProvider = characterInfoProvider;
            this.clientConfigInfoProvider = clientConfigInfoProvider;
            this.orderReviewInfoProvider = orderReviewInfoProvider;
            this.manualExecution = manualExecution;
            this.eventDispatcher = EventDispatcher.Instance;
            this.eventDispatcher.genericEvent += new EventDispatcher.GenericEventHandler(genericEventListener);
            this.eventDispatcher.genericErrorEvent += new EventDispatcher.GenericErrorEventHandler(genericErrorEventListener);
            this.eventDispatcher.autoAdjusterEvent += new EventDispatcher.AutoAdjusterEventHandler(autoAdjusterListener);
            this.eventDispatcher.autoListerEvent += new EventDispatcher.AutoListerEventHandler(autoListerListener);
            initializeCharacterView();
            initializeConfigView();
        }

        private void genericEventListener(object o, string s)
        {
            if (this.InvokeRequired)
            {
                EventListenerCallback els = new EventListenerCallback(genericEventListener);
                this.Invoke(els, new object[] { o, s });
            }
            else
            {
                consoleLB.Items.Add(s);
                consoleLB.SelectedIndex = consoleLB.Items.Count - 1;
            }
        }

        private void genericErrorEventListener(object o, string s)
        {
            if (this.InvokeRequired)
            {
                ErrorEventListenerCallback els = new ErrorEventListenerCallback(genericErrorEventListener);
                this.Invoke(els, new object[] { o, s });
            }
            else
            {
                consoleLB.Items.Add(s);
            }
        }

        private void autoAdjusterListener(object o, string s)
        {
            if (this.InvokeRequired)
            {
                AutoAdjusterListenerCallback els = new AutoAdjusterListenerCallback(autoAdjusterListener);
                this.Invoke(els, new object[] { o, s });
            }
            else
            {
                autoAdjusterLB.Items.Add(s);
            }
        }

        private void autoListerListener(object o, string s)
        {
            if (this.InvokeRequired)
            {
                AutoListerListenerCallback els = new AutoListerListenerCallback(autoListerListener);
                this.Invoke(els, new object[] { o, s });
            }
            else
            {
                autoListerLB.Items.Add(s);
            }
        }

        private void initializeConfigView()
        {
            String[] paths = clientConfigInfoProvider.getPaths();
            logPathTB.Text = paths[0];
            clientPathTB.Text = paths[1];
            configPathTB.Text = paths[2];
            eveSettingsTB.Text = paths[3];

            String[] config = clientConfigInfoProvider.getConfig();
            timingTB.Text = config[0];
            iterationsTB.Text = config[1];
            xResolutionTB.Text = config[2];
            yResolutionTB.Text = config[3];
        }

        private void initializeCharacterView()
        {
            List<String> characters = characterInfoProvider.getCharacterList();
            foreach (String c in characters)
                this.charactersLB.Items.Add(c);

            if (charactersLB.Items.Count > 0)
            {
                String selected = characterInfoProvider.getSelectedCharacter();
                if (selected == null)
                    charactersLB.SelectedIndex = 0;
                else
                    charactersLB.SelectedIndex = charactersLB.Items.IndexOf(selected);
                displayCharacterInfo(characterInfoProvider.getCharacterInfo((String)charactersLB.SelectedItem),
                    characterInfoProvider.getCharacterKnownItems((String)charactersLB.SelectedItem));
            }


        }

        private void displayCharacterInfo(Dictionary<string, string> characterInfo, List<String> items)
        {
            nameTB.Text = (String)charactersLB.SelectedItem;
            loginTB.Text = characterInfo[EtConstants.ACCOUNT_LOGIN_KEY];
            passwordTB.Text = characterInfo[EtConstants.ACCOUNT_PASSWORD_KEY];
            characterIdTB.Text = characterInfo[EtConstants.CHARACTER_ID_KEY];
            stationidTB.Text = characterInfo[EtConstants.CHARACTER_STATION_ID_KEY];
            accountIdTB.Text = characterInfo[EtConstants.ACCOUNT_ID_KEY];
            itemsCB.Checked = Convert.ToBoolean(characterInfo[EtConstants.CHARACTER_TRADE_ITEMS_KEY]);
            shipsCB.Checked = Convert.ToBoolean(characterInfo[EtConstants.CHARACTER_TRADE_SHIPS_KEY]);
            aasellCB.Checked = Convert.ToBoolean(characterInfo[EtConstants.CHARACTER_ADJUST_SELLS_KEY]);
            aabuyCB.Checked = Convert.ToBoolean(characterInfo[EtConstants.CHARACTER_ADJUST_BUYS_KEY]);
            maximumOrdersTB.Text = characterInfo[EtConstants.CHARACTER_MAXIMUM_ORDERS_KEY];
            loginColorTB.Text = characterInfo[EtConstants.CHARACTER_LOGIN_COLOR_KEY];

            knownItemsListBox.Items.Clear();
            foreach (String s in items)
            {
                knownItemsListBox.Items.Add(s);
            }
        }

        private void noxiousET_load(object sender, EventArgs e)
        {

        }

        private void charactersLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            displayCharacterInfo(characterInfoProvider.getCharacterInfo((String)charactersLB.SelectedItem),
                characterInfoProvider.getCharacterKnownItems((String)charactersLB.SelectedItem));
            characterInfoProvider.setSelectedCharacter((String)charactersLB.SelectedItem);
        }

        private void automateB_Click(object sender, EventArgs e)
        {
            manualExecution.automate();
        }

        private void saveSelectedCharacterB_Click(object sender, System.EventArgs e)
        {
            characterInfoProvider.saveCharacter((String)charactersLB.SelectedItem);
        }

        private void characterUpdateIntegerValue(string character, string key, string value)
        {
            int result;
            try { result = int.Parse(value); }
            catch { return; }
            characterUpdate(character, key, value);
        }

        private void characterUpdate(string character, string key, string value)
        {
            if (value.Length > 0)
                eventDispatcher.characterSettingUpdated(character, key, value);

        }

        private void loginTB_KeyUp(object sender, System.EventArgs e)
        {
            characterUpdate((String)charactersLB.SelectedItem, EtConstants.ACCOUNT_LOGIN_KEY, loginTB.Text);
        }

        private void passwordTB_KeyUp(object sender, System.EventArgs e)
        {
            characterUpdate((String)charactersLB.SelectedItem, EtConstants.ACCOUNT_PASSWORD_KEY, passwordTB.Text);
        }

        private void loginColorTB_KeyUp(object sender, System.EventArgs e)
        {
            characterUpdateIntegerValue((String)charactersLB.SelectedItem, EtConstants.CHARACTER_LOGIN_COLOR_KEY, loginColorTB.Text);
        }

        private void stationidTB_KeyUp(object sender, System.EventArgs e)
        {
            characterUpdateIntegerValue((String)charactersLB.SelectedItem, EtConstants.CHARACTER_STATION_ID_KEY, stationidTB.Text);
        }

        private void accountIdTB_KeyUp(object sender, System.EventArgs e)
        {
            characterUpdateIntegerValue((String)charactersLB.SelectedItem, EtConstants.ACCOUNT_ID_KEY, accountIdTB.Text);
        }

        private void maximumOrdersTB_KeyUp(object sender, System.EventArgs e)
        {
            characterUpdateIntegerValue((String)charactersLB.SelectedItem, EtConstants.CHARACTER_MAXIMUM_ORDERS_KEY, maximumOrdersTB.Text);
        }

        private void characterIdTB_TextChanged(object sender, System.EventArgs e)
        {
            characterUpdateIntegerValue((String)charactersLB.SelectedItem, EtConstants.CHARACTER_ID_KEY, characterIdTB.Text);
        }

        private void shipsCB_Click(object sender, System.EventArgs e)
        {
            characterUpdate((String)charactersLB.SelectedItem, EtConstants.CHARACTER_TRADE_SHIPS_KEY, Convert.ToString(shipsCB.Checked));
        }

        private void itemsCB_Click(object sender, System.EventArgs e)
        {
            characterUpdate((String)charactersLB.SelectedItem, EtConstants.CHARACTER_TRADE_ITEMS_KEY, Convert.ToString(itemsCB.Checked));
        }

        private void aabuyCB_Click(object sender, System.EventArgs e)
        {
            characterUpdate((String)charactersLB.SelectedItem, EtConstants.CHARACTER_ADJUST_BUYS_KEY, Convert.ToString(aabuyCB.Checked));
        }

        private void aasellCB_Click(object sender, System.EventArgs e)
        {
            characterUpdate((String)charactersLB.SelectedItem, EtConstants.CHARACTER_ADJUST_SELLS_KEY, Convert.ToString(aasellCB.Checked));
        }

        private void albuyCB_Click(object sender, System.EventArgs e)
        {
            //characterUpdate((String)charactersLB.SelectedItem, "autoListBuys", Convert.ToString(albuyCB.Checked));
        }

        private void alsellCB_Click(object sender, System.EventArgs e)
        {
            //characterUpdate((String)charactersLB.SelectedItem, "autoListSells", Convert.ToString(alsellCB.Checked));
        }

        private void configPathTB_TextChanged(object sender, System.EventArgs e)
        {
            if (configPathTB.Text.Length > 0)
                eventDispatcher.clientSettingUpdated((String)charactersLB.SelectedItem, EtConstants.CONFIG_PATH_KEY, configPathTB.Text);
        }

        private void clientPathTB_TextChanged(object sender, System.EventArgs e)
        {
            if (clientPathTB.Text.Length > 0)
                eventDispatcher.clientSettingUpdated((String)charactersLB.SelectedItem, EtConstants.CLIENT_PATH_KEY, clientPathTB.Text);
        }

        private void logPathTB_TextChanged(object sender, System.EventArgs e)
        {
            if (logPathTB.Text.Length > 0)
                eventDispatcher.clientSettingUpdated((String)charactersLB.SelectedItem, EtConstants.LOG_PATH_KEY, logPathTB.Text);
        }

        private void eveSettingsTB_TextChanged(object sender, System.EventArgs e)
        {
            if (eveSettingsTB.Text.Length > 0)
                eventDispatcher.clientSettingUpdated((String)charactersLB.SelectedItem, EtConstants.EVE_SETTINGS_PATH_KEY, eveSettingsTB.Text);
        }

        private void yResolutionTB_TextChanged(object sender, System.EventArgs e)
        {
            eventDispatcher.clientSettingUpdated((String)charactersLB.SelectedItem, EtConstants.Y_RESOLUTION_KEY, yResolutionTB.Text);
        }

        private void xResolutionTB_TextChanged(object sender, System.EventArgs e)
        {
            eventDispatcher.clientSettingUpdated((String)charactersLB.SelectedItem, EtConstants.X_RESOLUTION_KEY, xResolutionTB.Text);
        }

        private void timingTB_TextChanged(object sender, System.EventArgs e)
        {
            eventDispatcher.clientSettingUpdated((String)charactersLB.SelectedItem, EtConstants.TIMING_MULTIPLIER_KEY, timingTB.Text);
        }

        private void iterationsTB_TextChanged(object sender, System.EventArgs e)
        {
            eventDispatcher.clientSettingUpdated((String)charactersLB.SelectedItem, EtConstants.ITERATIONS_KEY, iterationsTB.Text);
        }

        private void saveAllSettingsB_Click(object sender, System.EventArgs e)
        {
            eventDispatcher.saveAllSettingsRequest();
        }

        private void getTypeB_Click(object sender, EventArgs e)
        {
            eventDispatcher.getTypesFromFileRequest((String)charactersLB.SelectedItem);
        }

        private void unpauseB_Click(object sender, EventArgs e)
        {
        }

        private void knownItemsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (knownItemsListBox.SelectedItem != null)
                noxiousET.src.guiInteraction.Clipboard.setClip(knownItemsListBox.SelectedItem.ToString());
        }

        private void fetchItemsFromQuickbarButton_Click(object sender, EventArgs e)
        {
            eventDispatcher.getTypesFromQuickbarRequest((String)charactersLB.SelectedItem, firstItemTextbox.Text, lastItemTextbox.Text);
        }

        private void itemsToReviewList_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (itemsToReviewList.SelectedIndices.Count <= 0)
                return;
            String pressedKey = e.KeyChar.ToString();
            int selectedIndex = itemsToReviewList.SelectedIndices[0];
            string currentItemAction = itemsToReviewList.Items[selectedIndex].SubItems[1].Text;
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
            if (buyOrSell.Equals("B"))
                buyOrSell = EtConstants.BUY.ToString();
            else
                buyOrSell = EtConstants.SELL.ToString();
            eventDispatcher.updateActionToTakeRequest(character, typeId, buyOrSell, newItemAction);
        }

        private void itemsToReviewList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (itemsToReviewList.SelectedIndices.Count <= 0)
                return;

            int selectedIndex = itemsToReviewList.SelectedIndices[0];
            String character = itemsToReviewList.Items[selectedIndex].SubItems[2].Text;
            String typeId = itemsToReviewList.Items[selectedIndex].SubItems[4].Text;
            String buyOrSell = itemsToReviewList.Items[selectedIndex].SubItems[0].Text;
            displayOrderDetails(orderReviewInfoProvider.getItemDetails(character, typeId, buyOrSell));
        }

        private void displayOrderDetails(List<string[]> orderDetails)
        {
            int size = reviewDetailsListView.Items.Count;
            for (int i = 0; i < size; i++)
            {
                reviewDetailsListView.Items.RemoveAt(0);
            }
            size = orderDetails.Count;
            for (int i = 1; i < size; i++)
            {
                ListViewItem item = new ListViewItem(orderDetails[i][0]);
                item.SubItems.Add(String.Format("{0:0,0.00}", Double.Parse(orderDetails[i][1])));
                item.SubItems.Add(orderDetails[i][2]);
                item.SubItems.Add(orderDetails[i][3]);
                reviewDetailsListView.Items.Add(item);
            }
        }

        private void clientTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (clientTabs.SelectedIndex.Equals(5))
            {
                List<String[]> ordersRequiringReview = orderReviewInfoProvider.getOrdersRequiringReview();
                int size = itemsToReviewList.Items.Count;
                for (int i = 0; i < size; i++)
                {
                    itemsToReviewList.Items.RemoveAt(0);
                }
                foreach (string[] entry in ordersRequiringReview)
                {
                    ListViewItem item = new ListViewItem(buyOrSellToString(entry[0]));
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
            if (buyOrSell.Equals(EtConstants.BUY.ToString()))
                return "B";
            else
                return "S";
        }
    }
}