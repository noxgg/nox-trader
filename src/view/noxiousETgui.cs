using System;
using System.Collections.Generic;
using System.Windows.Forms;
using noxiousET.src;
using noxiousET.src.control;
using noxiousET.src.etevent;


//TODO: Margin trading scam? Order creation flags. Max order setting. Buying an item with no sell orders shifts buy order quantity box up.
namespace noxiousET
{
    partial class noxiousETgui : Form
    {
        CharacterInfoProvider characterInfoProvider;
        ClientConfigInfoProvider clientConfigInfoProvider;
        AutomationRequester manualExecution;
        EventDispatcher eventDispatcher;
        delegate void EventListenerCallback(object o, string s);
        delegate void ErrorEventListenerCallback(object o, string s);
        delegate void AutoAdjusterListenerCallback(object o, string s);
        delegate void AutoListerListenerCallback(object o, string s);

        

        public noxiousETgui(CharacterInfoProvider characterInfoProvider, ClientConfigInfoProvider clientConfigInfoProvider, AutomationRequester manualExecution, EventDispatcher eventDispatcher)
        {
            InitializeComponent();
            this.characterInfoProvider = characterInfoProvider;
            this.clientConfigInfoProvider = clientConfigInfoProvider;
            this.manualExecution = manualExecution;
            this.eventDispatcher = eventDispatcher;
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
            String [] paths = clientConfigInfoProvider.getPaths();
            logPathTB.Text = paths[0];
            clientPathTB.Text = paths[1];
            configPathTB.Text = paths[2];

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
                displayCharacterInfo(characterInfoProvider.getCharacterInfo((String)charactersLB.SelectedItem));
            }


        }

        private void displayCharacterInfo(String[] characterInfo)
        {
            int i = 0;
            nameTB.Text = (String)charactersLB.SelectedItem;
            loginTB.Text = characterInfo[i++];
            passwordTB.Text = characterInfo[i++];
            autoAdjustsPerAutoListTB.Text = characterInfo[i++];
            stationidTB.Text = characterInfo[i++];
            fileNameTrimLengthTB.Text = characterInfo[i++];
            itemsCB.Checked = Convert.ToBoolean(characterInfo[i++]);
            shipsCB.Checked = Convert.ToBoolean(characterInfo[i++]);
            aabuyCB.Checked = Convert.ToBoolean(characterInfo[i++]);
            aasellCB.Checked = Convert.ToBoolean(characterInfo[i++]);
            maximumOrdersTB.Text = characterInfo[i++];
            loginColorTB.Text = characterInfo[i++];
        }

        private void noxiousET_load(object sender, EventArgs e)
        {
            
        }

        private void charactersLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            displayCharacterInfo(characterInfoProvider.getCharacterInfo((String)charactersLB.SelectedItem));
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
            characterUpdate((String)charactersLB.SelectedItem, EtConstants.LOGIN_KEY, loginTB.Text);
        }

        private void passwordTB_KeyUp(object sender, System.EventArgs e)
        {
            characterUpdate((String)charactersLB.SelectedItem, EtConstants.PASSWORD_KEY, passwordTB.Text);
        }

        private void loginColorTB_KeyUp(object sender, System.EventArgs e)
        {
            characterUpdateIntegerValue((String)charactersLB.SelectedItem, EtConstants.LOGIN_COLOR_KEY, loginColorTB.Text);
        }

        private void stationidTB_KeyUp(object sender, System.EventArgs e)
        {
            characterUpdateIntegerValue((String)charactersLB.SelectedItem, EtConstants.STATION_ID_KEY, stationidTB.Text);
        }

        private void fileNameTrimLengthTB_KeyUp(object sender, System.EventArgs e)
        {
            characterUpdateIntegerValue((String)charactersLB.SelectedItem, EtConstants.FILE_NAME_TRIM_LENGTH_KEY, fileNameTrimLengthTB.Text);
        }

        private void maximumOrdersTB_KeyUp(object sender, System.EventArgs e)
        {
            characterUpdateIntegerValue((String)charactersLB.SelectedItem, EtConstants.MAXIMUM_ORDERS_KEY, maximumOrdersTB.Text);
        }

        private void autoAdjustsPerAutoListTB_TextChanged(object sender, System.EventArgs e)
        {
            characterUpdateIntegerValue((String)charactersLB.SelectedItem, EtConstants.AUTO_ADJUSTS_PER_AUTO_LIST_KEY, autoAdjustsPerAutoListTB.Text);
        }

        private void shipsCB_Click(object sender, System.EventArgs e)
        {
            characterUpdate((String)charactersLB.SelectedItem, EtConstants.TRADE_SHIPS_KEY, Convert.ToString(shipsCB.Checked));
        }

        private void itemsCB_Click(object sender, System.EventArgs e)
        {
            characterUpdate((String)charactersLB.SelectedItem, EtConstants.TRADE_ITEMS_KEY, Convert.ToString(itemsCB.Checked));
        }

        private void aabuyCB_Click(object sender, System.EventArgs e)
        {
            characterUpdate((String)charactersLB.SelectedItem, EtConstants.ADJUST_SELLS_KEY, Convert.ToString(aabuyCB.Checked));
        }

        private void aasellCB_Click(object sender, System.EventArgs e)
        {
            characterUpdate((String)charactersLB.SelectedItem, EtConstants.ADJUST_BUYS_KEY, Convert.ToString(aasellCB.Checked));
        }

        private void albuyCB_Click(object sender, System.EventArgs e)
        {
            //characterUpdate((String)charactersLB.SelectedItem, "autoListBuys", Convert.ToString(albuyCB.Checked));
        }

        private void alsellCB_Click(object sender, System.EventArgs e)
        {
            //characterUpdate((String)charactersLB.SelectedItem, "autoListSells", Convert.ToString(alsellCB.Checked));
        }

        private void configPathTB_LostFocus(object sender, System.EventArgs e)
        {
            eventDispatcher.clientSettingUpdated((String)charactersLB.SelectedItem, EtConstants.CONFIG_PATH_KEY, configPathTB.Text);
        }

        private void clientPathTB_LostFocus(object sender, System.EventArgs e)
        {
            eventDispatcher.clientSettingUpdated((String)charactersLB.SelectedItem, EtConstants.CLIENT_PATH_KEY, clientPathTB.Text);
        }

        private void logPathTB_LostFocus(object sender, System.EventArgs e)
        {
            eventDispatcher.clientSettingUpdated((String)charactersLB.SelectedItem, EtConstants.LOG_PATH_KEY, logPathTB.Text);
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
    }
}