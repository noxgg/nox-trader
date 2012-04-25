namespace noxiousET
{
    partial class ETGUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.autoSellerTab = new System.Windows.Forms.TabPage();
            this.runsStatsListView = new System.Windows.Forms.ListView();
            this.runs = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.scanedOrders = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.modifiedOrders = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runDuration = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.autoSellerListBox = new System.Windows.Forms.ListBox();
            this.modificationLogTab = new System.Windows.Forms.TabPage();
            this.modifiedOrdersListView = new System.Windows.Forms.ListView();
            this.runHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.itemNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.bestSellOrder = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.bestBuyOrder = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.myOrderPrice = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.newOrderPrice = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mainTab = new System.Windows.Forms.TabPage();
            this.maxOrdersTextbox = new System.Windows.Forms.TextBox();
            this.maxOrdersLabel = new System.Windows.Forms.Label();
            this.removeActiveButton = new System.Windows.Forms.Button();
            this.addAcctiveButton = new System.Windows.Forms.Button();
            this.inactiveCharactersLabel = new System.Windows.Forms.Label();
            this.inactiveCharactersTextbox = new System.Windows.Forms.ListBox();
            this.autoSellEveryNRunsTextbox = new System.Windows.Forms.TextBox();
            this.autoSellEveryNRunsLabel = new System.Windows.Forms.Label();
            this.fullAutomationButton = new System.Windows.Forms.Button();
            this.configPathLabel = new System.Windows.Forms.Label();
            this.configPathTextbox = new System.Windows.Forms.TextBox();
            this.adjustBuysCheckbox = new System.Windows.Forms.CheckBox();
            this.adjustSellsCheckbox = new System.Windows.Forms.CheckBox();
            this.autoSellerButton = new System.Windows.Forms.Button();
            this.tradesShipsCheckbox = new System.Windows.Forms.CheckBox();
            this.tradesItemsCheckbox = new System.Windows.Forms.CheckBox();
            this.stationIDLabel = new System.Windows.Forms.Label();
            this.threshHoldsLabel = new System.Windows.Forms.Label();
            this.activeCharacterLabel = new System.Windows.Forms.Label();
            this.resolutionLabel = new System.Windows.Forms.Label();
            this.elementsListbox = new System.Windows.Forms.ListBox();
            this.EVEPathLabel = new System.Windows.Forms.Label();
            this.EVEPathTextbox = new System.Windows.Forms.TextBox();
            this.prototypeButton = new System.Windows.Forms.Button();
            this.stationsListBox = new System.Windows.Forms.ListBox();
            this.threshHoldTextbox5 = new System.Windows.Forms.TextBox();
            this.threshHoldTextbox4 = new System.Windows.Forms.TextBox();
            this.threshHoldTextbox3 = new System.Windows.Forms.TextBox();
            this.threshHoldTextbox2 = new System.Windows.Forms.TextBox();
            this.threshHoldTextbox1 = new System.Windows.Forms.TextBox();
            this.qtyTextbox5 = new System.Windows.Forms.TextBox();
            this.qtyTextbox4 = new System.Windows.Forms.TextBox();
            this.qtyTextbox3 = new System.Windows.Forms.TextBox();
            this.qtyTextbox2 = new System.Windows.Forms.TextBox();
            this.qtyTextbox1 = new System.Windows.Forms.TextBox();
            this.saveSettingsButton = new System.Windows.Forms.Button();
            this.pathLabel = new System.Windows.Forms.Label();
            this.pathTextBox = new System.Windows.Forms.TextBox();
            this.excepListBox = new System.Windows.Forms.ListBox();
            this.characterListBox = new System.Windows.Forms.ListBox();
            this.timingLabel = new System.Windows.Forms.Label();
            this.timingTextBox = new System.Windows.Forms.TextBox();
            this.nTimesTextBox = new System.Windows.Forms.TextBox();
            this.runButton = new System.Windows.Forms.Button();
            this.nTimesLabel = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.exceptionsTab = new System.Windows.Forms.TabPage();
            this.exceptionsListbox = new System.Windows.Forms.ListBox();
            this.autoSellerTab.SuspendLayout();
            this.modificationLogTab.SuspendLayout();
            this.mainTab.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.exceptionsTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // autoSellerTab
            // 
            this.autoSellerTab.BackColor = System.Drawing.Color.Gainsboro;
            this.autoSellerTab.Controls.Add(this.runsStatsListView);
            this.autoSellerTab.Controls.Add(this.autoSellerListBox);
            this.autoSellerTab.Location = new System.Drawing.Point(4, 22);
            this.autoSellerTab.Name = "autoSellerTab";
            this.autoSellerTab.Padding = new System.Windows.Forms.Padding(3);
            this.autoSellerTab.Size = new System.Drawing.Size(681, 671);
            this.autoSellerTab.TabIndex = 2;
            this.autoSellerTab.Text = "History";
            // 
            // runsStatsListView
            // 
            this.runsStatsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.runs,
            this.scanedOrders,
            this.modifiedOrders,
            this.runDuration});
            this.runsStatsListView.Location = new System.Drawing.Point(363, 107);
            this.runsStatsListView.Name = "runsStatsListView";
            this.runsStatsListView.Size = new System.Drawing.Size(261, 553);
            this.runsStatsListView.TabIndex = 22;
            this.runsStatsListView.UseCompatibleStateImageBehavior = false;
            this.runsStatsListView.View = System.Windows.Forms.View.Details;
            // 
            // runs
            // 
            this.runs.Text = "Run";
            this.runs.Width = 37;
            // 
            // scanedOrders
            // 
            this.scanedOrders.Text = "Scanned";
            this.scanedOrders.Width = 57;
            // 
            // modifiedOrders
            // 
            this.modifiedOrders.Text = "Modified";
            this.modifiedOrders.Width = 55;
            // 
            // runDuration
            // 
            this.runDuration.Text = "Duration";
            this.runDuration.Width = 104;
            // 
            // autoSellerListBox
            // 
            this.autoSellerListBox.FormattingEnabled = true;
            this.autoSellerListBox.Location = new System.Drawing.Point(15, 19);
            this.autoSellerListBox.Name = "autoSellerListBox";
            this.autoSellerListBox.Size = new System.Drawing.Size(294, 641);
            this.autoSellerListBox.TabIndex = 21;
            // 
            // modificationLogTab
            // 
            this.modificationLogTab.BackColor = System.Drawing.Color.Gainsboro;
            this.modificationLogTab.Controls.Add(this.modifiedOrdersListView);
            this.modificationLogTab.Location = new System.Drawing.Point(4, 22);
            this.modificationLogTab.Name = "modificationLogTab";
            this.modificationLogTab.Padding = new System.Windows.Forms.Padding(3);
            this.modificationLogTab.Size = new System.Drawing.Size(681, 671);
            this.modificationLogTab.TabIndex = 1;
            this.modificationLogTab.Text = "Detailed Modification Log";
            // 
            // modifiedOrdersListView
            // 
            this.modifiedOrdersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.runHeader,
            this.itemNameHeader,
            this.Type,
            this.bestSellOrder,
            this.bestBuyOrder,
            this.myOrderPrice,
            this.newOrderPrice});
            this.modifiedOrdersListView.Location = new System.Drawing.Point(-4, 0);
            this.modifiedOrdersListView.Name = "modifiedOrdersListView";
            this.modifiedOrdersListView.Size = new System.Drawing.Size(685, 680);
            this.modifiedOrdersListView.TabIndex = 11;
            this.modifiedOrdersListView.UseCompatibleStateImageBehavior = false;
            this.modifiedOrdersListView.View = System.Windows.Forms.View.Details;
            // 
            // runHeader
            // 
            this.runHeader.Text = "Run";
            this.runHeader.Width = 41;
            // 
            // itemNameHeader
            // 
            this.itemNameHeader.Text = "Item";
            this.itemNameHeader.Width = 128;
            // 
            // Type
            // 
            this.Type.Text = "Type";
            this.Type.Width = 47;
            // 
            // bestSellOrder
            // 
            this.bestSellOrder.Text = "Best Sell Price";
            this.bestSellOrder.Width = 115;
            // 
            // bestBuyOrder
            // 
            this.bestBuyOrder.Text = "Best Buy Price";
            this.bestBuyOrder.Width = 109;
            // 
            // myOrderPrice
            // 
            this.myOrderPrice.Text = "Original Order Price";
            this.myOrderPrice.Width = 113;
            // 
            // newOrderPrice
            // 
            this.newOrderPrice.Text = "New Price";
            this.newOrderPrice.Width = 105;
            // 
            // mainTab
            // 
            this.mainTab.BackColor = System.Drawing.Color.Gainsboro;
            this.mainTab.Controls.Add(this.maxOrdersTextbox);
            this.mainTab.Controls.Add(this.maxOrdersLabel);
            this.mainTab.Controls.Add(this.removeActiveButton);
            this.mainTab.Controls.Add(this.addAcctiveButton);
            this.mainTab.Controls.Add(this.inactiveCharactersLabel);
            this.mainTab.Controls.Add(this.inactiveCharactersTextbox);
            this.mainTab.Controls.Add(this.autoSellEveryNRunsTextbox);
            this.mainTab.Controls.Add(this.autoSellEveryNRunsLabel);
            this.mainTab.Controls.Add(this.fullAutomationButton);
            this.mainTab.Controls.Add(this.configPathLabel);
            this.mainTab.Controls.Add(this.configPathTextbox);
            this.mainTab.Controls.Add(this.adjustBuysCheckbox);
            this.mainTab.Controls.Add(this.adjustSellsCheckbox);
            this.mainTab.Controls.Add(this.autoSellerButton);
            this.mainTab.Controls.Add(this.tradesShipsCheckbox);
            this.mainTab.Controls.Add(this.tradesItemsCheckbox);
            this.mainTab.Controls.Add(this.stationIDLabel);
            this.mainTab.Controls.Add(this.threshHoldsLabel);
            this.mainTab.Controls.Add(this.activeCharacterLabel);
            this.mainTab.Controls.Add(this.resolutionLabel);
            this.mainTab.Controls.Add(this.elementsListbox);
            this.mainTab.Controls.Add(this.EVEPathLabel);
            this.mainTab.Controls.Add(this.EVEPathTextbox);
            this.mainTab.Controls.Add(this.prototypeButton);
            this.mainTab.Controls.Add(this.stationsListBox);
            this.mainTab.Controls.Add(this.threshHoldTextbox5);
            this.mainTab.Controls.Add(this.threshHoldTextbox4);
            this.mainTab.Controls.Add(this.threshHoldTextbox3);
            this.mainTab.Controls.Add(this.threshHoldTextbox2);
            this.mainTab.Controls.Add(this.threshHoldTextbox1);
            this.mainTab.Controls.Add(this.qtyTextbox5);
            this.mainTab.Controls.Add(this.qtyTextbox4);
            this.mainTab.Controls.Add(this.qtyTextbox3);
            this.mainTab.Controls.Add(this.qtyTextbox2);
            this.mainTab.Controls.Add(this.qtyTextbox1);
            this.mainTab.Controls.Add(this.saveSettingsButton);
            this.mainTab.Controls.Add(this.pathLabel);
            this.mainTab.Controls.Add(this.pathTextBox);
            this.mainTab.Controls.Add(this.excepListBox);
            this.mainTab.Controls.Add(this.characterListBox);
            this.mainTab.Controls.Add(this.timingLabel);
            this.mainTab.Controls.Add(this.timingTextBox);
            this.mainTab.Controls.Add(this.nTimesTextBox);
            this.mainTab.Controls.Add(this.runButton);
            this.mainTab.Controls.Add(this.nTimesLabel);
            this.mainTab.Location = new System.Drawing.Point(4, 22);
            this.mainTab.Name = "mainTab";
            this.mainTab.Padding = new System.Windows.Forms.Padding(3);
            this.mainTab.Size = new System.Drawing.Size(681, 671);
            this.mainTab.TabIndex = 0;
            this.mainTab.Text = "Main";
            // 
            // maxOrdersTextbox
            // 
            this.maxOrdersTextbox.Location = new System.Drawing.Point(195, 290);
            this.maxOrdersTextbox.Name = "maxOrdersTextbox";
            this.maxOrdersTextbox.Size = new System.Drawing.Size(50, 20);
            this.maxOrdersTextbox.TabIndex = 62;
            this.maxOrdersTextbox.Text = "109";
            this.maxOrdersTextbox.TextChanged += new System.EventHandler(this.maxOrdersTextbox_TextChanged);
            // 
            // maxOrdersLabel
            // 
            this.maxOrdersLabel.AutoSize = true;
            this.maxOrdersLabel.Location = new System.Drawing.Point(135, 294);
            this.maxOrdersLabel.Name = "maxOrdersLabel";
            this.maxOrdersLabel.Size = new System.Drawing.Size(61, 13);
            this.maxOrdersLabel.TabIndex = 63;
            this.maxOrdersLabel.Text = "Max Orders";
            // 
            // removeActiveButton
            // 
            this.removeActiveButton.Location = new System.Drawing.Point(152, 213);
            this.removeActiveButton.Name = "removeActiveButton";
            this.removeActiveButton.Size = new System.Drawing.Size(14, 20);
            this.removeActiveButton.TabIndex = 61;
            this.removeActiveButton.Text = ">";
            this.removeActiveButton.UseVisualStyleBackColor = true;
            this.removeActiveButton.Click += new System.EventHandler(this.removeActiveButton_Click);
            // 
            // addAcctiveButton
            // 
            this.addAcctiveButton.Location = new System.Drawing.Point(138, 213);
            this.addAcctiveButton.Name = "addAcctiveButton";
            this.addAcctiveButton.Size = new System.Drawing.Size(14, 20);
            this.addAcctiveButton.TabIndex = 60;
            this.addAcctiveButton.Text = "<";
            this.addAcctiveButton.UseVisualStyleBackColor = true;
            this.addAcctiveButton.Click += new System.EventHandler(this.addAcctiveButton_Click);
            // 
            // inactiveCharactersLabel
            // 
            this.inactiveCharactersLabel.AutoSize = true;
            this.inactiveCharactersLabel.Location = new System.Drawing.Point(169, 193);
            this.inactiveCharactersLabel.Name = "inactiveCharactersLabel";
            this.inactiveCharactersLabel.Size = new System.Drawing.Size(99, 13);
            this.inactiveCharactersLabel.TabIndex = 59;
            this.inactiveCharactersLabel.Text = "Inactive Characters";
            // 
            // inactiveCharactersTextbox
            // 
            this.inactiveCharactersTextbox.FormattingEnabled = true;
            this.inactiveCharactersTextbox.Items.AddRange(new object[] {
            "Admiral von DoucheyPants"});
            this.inactiveCharactersTextbox.Location = new System.Drawing.Point(173, 209);
            this.inactiveCharactersTextbox.Name = "inactiveCharactersTextbox";
            this.inactiveCharactersTextbox.Size = new System.Drawing.Size(101, 30);
            this.inactiveCharactersTextbox.TabIndex = 58;
            // 
            // autoSellEveryNRunsTextbox
            // 
            this.autoSellEveryNRunsTextbox.Location = new System.Drawing.Point(94, 160);
            this.autoSellEveryNRunsTextbox.Name = "autoSellEveryNRunsTextbox";
            this.autoSellEveryNRunsTextbox.Size = new System.Drawing.Size(50, 20);
            this.autoSellEveryNRunsTextbox.TabIndex = 56;
            this.autoSellEveryNRunsTextbox.Text = "1";
            this.autoSellEveryNRunsTextbox.TextChanged += new System.EventHandler(this.autoSellEveryNRunsTextbox_TextChanged);
            // 
            // autoSellEveryNRunsLabel
            // 
            this.autoSellEveryNRunsLabel.AutoSize = true;
            this.autoSellEveryNRunsLabel.Location = new System.Drawing.Point(7, 163);
            this.autoSellEveryNRunsLabel.Name = "autoSellEveryNRunsLabel";
            this.autoSellEveryNRunsLabel.Size = new System.Drawing.Size(17, 13);
            this.autoSellEveryNRunsLabel.TabIndex = 57;
            this.autoSellEveryNRunsLabel.Text = "Φ";
            // 
            // fullAutomationButton
            // 
            this.fullAutomationButton.Location = new System.Drawing.Point(58, 632);
            this.fullAutomationButton.Name = "fullAutomationButton";
            this.fullAutomationButton.Size = new System.Drawing.Size(158, 34);
            this.fullAutomationButton.TabIndex = 55;
            this.fullAutomationButton.Text = "Full Automation";
            this.fullAutomationButton.UseVisualStyleBackColor = true;
            this.fullAutomationButton.Click += new System.EventHandler(this.fullAutomationButton_Click);
            // 
            // configPathLabel
            // 
            this.configPathLabel.AutoSize = true;
            this.configPathLabel.Location = new System.Drawing.Point(3, 75);
            this.configPathLabel.Name = "configPathLabel";
            this.configPathLabel.Size = new System.Drawing.Size(62, 13);
            this.configPathLabel.TabIndex = 54;
            this.configPathLabel.Text = "Config Path";
            // 
            // configPathTextbox
            // 
            this.configPathTextbox.Location = new System.Drawing.Point(100, 72);
            this.configPathTextbox.Name = "configPathTextbox";
            this.configPathTextbox.Size = new System.Drawing.Size(173, 20);
            this.configPathTextbox.TabIndex = 53;
            this.configPathTextbox.Text = "G:\\EVE\\eve.exe";
            // 
            // adjustBuysCheckbox
            // 
            this.adjustBuysCheckbox.AutoSize = true;
            this.adjustBuysCheckbox.Location = new System.Drawing.Point(100, 270);
            this.adjustBuysCheckbox.Name = "adjustBuysCheckbox";
            this.adjustBuysCheckbox.Size = new System.Drawing.Size(81, 17);
            this.adjustBuysCheckbox.TabIndex = 52;
            this.adjustBuysCheckbox.Text = "Adjust Buys";
            this.adjustBuysCheckbox.UseVisualStyleBackColor = true;
            this.adjustBuysCheckbox.CheckedChanged += new System.EventHandler(this.adjustBuysCheckbox_CheckedChanged);
            // 
            // adjustSellsCheckbox
            // 
            this.adjustSellsCheckbox.AutoSize = true;
            this.adjustSellsCheckbox.Location = new System.Drawing.Point(100, 246);
            this.adjustSellsCheckbox.Name = "adjustSellsCheckbox";
            this.adjustSellsCheckbox.Size = new System.Drawing.Size(80, 17);
            this.adjustSellsCheckbox.TabIndex = 51;
            this.adjustSellsCheckbox.Text = "Adjust Sells";
            this.adjustSellsCheckbox.UseVisualStyleBackColor = true;
            this.adjustSellsCheckbox.CheckedChanged += new System.EventHandler(this.adjustSellsCheckbox_CheckedChanged);
            // 
            // autoSellerButton
            // 
            this.autoSellerButton.Location = new System.Drawing.Point(58, 592);
            this.autoSellerButton.Name = "autoSellerButton";
            this.autoSellerButton.Size = new System.Drawing.Size(158, 34);
            this.autoSellerButton.TabIndex = 50;
            this.autoSellerButton.Text = "Create Sell Orders and Replentish Buy Orders";
            this.autoSellerButton.UseVisualStyleBackColor = true;
            this.autoSellerButton.Click += new System.EventHandler(this.autoSellerButton_Click);
            // 
            // tradesShipsCheckbox
            // 
            this.tradesShipsCheckbox.AutoSize = true;
            this.tradesShipsCheckbox.Location = new System.Drawing.Point(7, 270);
            this.tradesShipsCheckbox.Name = "tradesShipsCheckbox";
            this.tradesShipsCheckbox.Size = new System.Drawing.Size(88, 17);
            this.tradesShipsCheckbox.TabIndex = 49;
            this.tradesShipsCheckbox.Text = "Trades Ships";
            this.tradesShipsCheckbox.UseVisualStyleBackColor = true;
            this.tradesShipsCheckbox.CheckedChanged += new System.EventHandler(this.tradesShipsCheckbox_CheckedChanged);
            // 
            // tradesItemsCheckbox
            // 
            this.tradesItemsCheckbox.AutoSize = true;
            this.tradesItemsCheckbox.Location = new System.Drawing.Point(7, 246);
            this.tradesItemsCheckbox.Name = "tradesItemsCheckbox";
            this.tradesItemsCheckbox.Size = new System.Drawing.Size(87, 17);
            this.tradesItemsCheckbox.TabIndex = 48;
            this.tradesItemsCheckbox.Text = "Trades Items";
            this.tradesItemsCheckbox.UseVisualStyleBackColor = true;
            this.tradesItemsCheckbox.CheckedChanged += new System.EventHandler(this.tradesItemsCheckbox_CheckedChanged);
            // 
            // stationIDLabel
            // 
            this.stationIDLabel.AutoSize = true;
            this.stationIDLabel.Location = new System.Drawing.Point(46, 470);
            this.stationIDLabel.Name = "stationIDLabel";
            this.stationIDLabel.Size = new System.Drawing.Size(54, 13);
            this.stationIDLabel.TabIndex = 47;
            this.stationIDLabel.Text = "Station ID";
            // 
            // threshHoldsLabel
            // 
            this.threshHoldsLabel.AutoSize = true;
            this.threshHoldsLabel.Location = new System.Drawing.Point(64, 314);
            this.threshHoldsLabel.Name = "threshHoldsLabel";
            this.threshHoldsLabel.Size = new System.Drawing.Size(157, 13);
            this.threshHoldsLabel.TabIndex = 46;
            this.threshHoldsLabel.Text = "Buy Order Quantity Threshholds";
            // 
            // activeCharacterLabel
            // 
            this.activeCharacterLabel.AutoSize = true;
            this.activeCharacterLabel.Location = new System.Drawing.Point(2, 193);
            this.activeCharacterLabel.Name = "activeCharacterLabel";
            this.activeCharacterLabel.Size = new System.Drawing.Size(91, 13);
            this.activeCharacterLabel.TabIndex = 45;
            this.activeCharacterLabel.Text = "Active Characters";
            // 
            // resolutionLabel
            // 
            this.resolutionLabel.AutoSize = true;
            this.resolutionLabel.Location = new System.Drawing.Point(7, 132);
            this.resolutionLabel.Name = "resolutionLabel";
            this.resolutionLabel.Size = new System.Drawing.Size(57, 13);
            this.resolutionLabel.TabIndex = 44;
            this.resolutionLabel.Text = "Resolution";
            // 
            // elementsListbox
            // 
            this.elementsListbox.FormattingEnabled = true;
            this.elementsListbox.Items.AddRange(new object[] {
            "2560x1440",
            "1440x900"});
            this.elementsListbox.Location = new System.Drawing.Point(100, 124);
            this.elementsListbox.Name = "elementsListbox";
            this.elementsListbox.Size = new System.Drawing.Size(111, 30);
            this.elementsListbox.TabIndex = 43;
            this.elementsListbox.SelectedIndexChanged += new System.EventHandler(this.elementsListbox_SelectedIndexChanged);
            // 
            // EVEPathLabel
            // 
            this.EVEPathLabel.AutoSize = true;
            this.EVEPathLabel.Location = new System.Drawing.Point(3, 43);
            this.EVEPathLabel.Name = "EVEPathLabel";
            this.EVEPathLabel.Size = new System.Drawing.Size(82, 13);
            this.EVEPathLabel.TabIndex = 42;
            this.EVEPathLabel.Text = "EVE Client Path";
            // 
            // EVEPathTextbox
            // 
            this.EVEPathTextbox.Location = new System.Drawing.Point(100, 40);
            this.EVEPathTextbox.Name = "EVEPathTextbox";
            this.EVEPathTextbox.Size = new System.Drawing.Size(173, 20);
            this.EVEPathTextbox.TabIndex = 41;
            this.EVEPathTextbox.Text = "G:\\EVE\\eve.exe";
            // 
            // prototypeButton
            // 
            this.prototypeButton.Location = new System.Drawing.Point(58, 531);
            this.prototypeButton.Name = "prototypeButton";
            this.prototypeButton.Size = new System.Drawing.Size(158, 25);
            this.prototypeButton.TabIndex = 40;
            this.prototypeButton.Text = "Login as Selected";
            this.prototypeButton.UseVisualStyleBackColor = true;
            this.prototypeButton.Click += new System.EventHandler(this.prototypeButton_Click);
            // 
            // stationsListBox
            // 
            this.stationsListBox.FormattingEnabled = true;
            this.stationsListBox.Items.AddRange(new object[] {
            "60003760",
            "60008494",
            "60004588",
            "60011866"});
            this.stationsListBox.Location = new System.Drawing.Point(105, 461);
            this.stationsListBox.Name = "stationsListBox";
            this.stationsListBox.Size = new System.Drawing.Size(97, 30);
            this.stationsListBox.TabIndex = 39;
            this.stationsListBox.SelectedIndexChanged += new System.EventHandler(this.stationsListBox_SelectedIndexChanged);
            // 
            // threshHoldTextbox5
            // 
            this.threshHoldTextbox5.Location = new System.Drawing.Point(67, 434);
            this.threshHoldTextbox5.Name = "threshHoldTextbox5";
            this.threshHoldTextbox5.Size = new System.Drawing.Size(78, 20);
            this.threshHoldTextbox5.TabIndex = 38;
            this.threshHoldTextbox5.Text = "500000000";
            // 
            // threshHoldTextbox4
            // 
            this.threshHoldTextbox4.Location = new System.Drawing.Point(67, 408);
            this.threshHoldTextbox4.Name = "threshHoldTextbox4";
            this.threshHoldTextbox4.Size = new System.Drawing.Size(78, 20);
            this.threshHoldTextbox4.TabIndex = 37;
            this.threshHoldTextbox4.Text = "100000000";
            // 
            // threshHoldTextbox3
            // 
            this.threshHoldTextbox3.Location = new System.Drawing.Point(67, 382);
            this.threshHoldTextbox3.Name = "threshHoldTextbox3";
            this.threshHoldTextbox3.Size = new System.Drawing.Size(78, 20);
            this.threshHoldTextbox3.TabIndex = 36;
            this.threshHoldTextbox3.Text = "60000000";
            // 
            // threshHoldTextbox2
            // 
            this.threshHoldTextbox2.Location = new System.Drawing.Point(67, 356);
            this.threshHoldTextbox2.Name = "threshHoldTextbox2";
            this.threshHoldTextbox2.Size = new System.Drawing.Size(78, 20);
            this.threshHoldTextbox2.TabIndex = 35;
            this.threshHoldTextbox2.Text = "40000000";
            // 
            // threshHoldTextbox1
            // 
            this.threshHoldTextbox1.Location = new System.Drawing.Point(67, 330);
            this.threshHoldTextbox1.Name = "threshHoldTextbox1";
            this.threshHoldTextbox1.Size = new System.Drawing.Size(78, 20);
            this.threshHoldTextbox1.TabIndex = 34;
            this.threshHoldTextbox1.Text = "20000000";
            // 
            // qtyTextbox5
            // 
            this.qtyTextbox5.Location = new System.Drawing.Point(160, 434);
            this.qtyTextbox5.Name = "qtyTextbox5";
            this.qtyTextbox5.Size = new System.Drawing.Size(50, 20);
            this.qtyTextbox5.TabIndex = 33;
            this.qtyTextbox5.Text = "1";
            // 
            // qtyTextbox4
            // 
            this.qtyTextbox4.Location = new System.Drawing.Point(160, 408);
            this.qtyTextbox4.Name = "qtyTextbox4";
            this.qtyTextbox4.Size = new System.Drawing.Size(50, 20);
            this.qtyTextbox4.TabIndex = 32;
            this.qtyTextbox4.Text = "1";
            // 
            // qtyTextbox3
            // 
            this.qtyTextbox3.Location = new System.Drawing.Point(160, 382);
            this.qtyTextbox3.Name = "qtyTextbox3";
            this.qtyTextbox3.Size = new System.Drawing.Size(50, 20);
            this.qtyTextbox3.TabIndex = 31;
            this.qtyTextbox3.Text = "2";
            // 
            // qtyTextbox2
            // 
            this.qtyTextbox2.Location = new System.Drawing.Point(160, 356);
            this.qtyTextbox2.Name = "qtyTextbox2";
            this.qtyTextbox2.Size = new System.Drawing.Size(50, 20);
            this.qtyTextbox2.TabIndex = 30;
            this.qtyTextbox2.Text = "3";
            // 
            // qtyTextbox1
            // 
            this.qtyTextbox1.Location = new System.Drawing.Point(160, 330);
            this.qtyTextbox1.Name = "qtyTextbox1";
            this.qtyTextbox1.Size = new System.Drawing.Size(50, 20);
            this.qtyTextbox1.TabIndex = 29;
            this.qtyTextbox1.Text = "5";
            // 
            // saveSettingsButton
            // 
            this.saveSettingsButton.Location = new System.Drawing.Point(58, 500);
            this.saveSettingsButton.Name = "saveSettingsButton";
            this.saveSettingsButton.Size = new System.Drawing.Size(158, 25);
            this.saveSettingsButton.TabIndex = 28;
            this.saveSettingsButton.Text = "Save Settings";
            this.saveSettingsButton.UseVisualStyleBackColor = true;
            this.saveSettingsButton.Click += new System.EventHandler(this.saveSettingsButton_Click);
            // 
            // pathLabel
            // 
            this.pathLabel.AutoSize = true;
            this.pathLabel.Location = new System.Drawing.Point(3, 17);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(86, 13);
            this.pathLabel.TabIndex = 27;
            this.pathLabel.Text = "Market Log Path";
            // 
            // pathTextBox
            // 
            this.pathTextBox.Location = new System.Drawing.Point(100, 14);
            this.pathTextBox.Name = "pathTextBox";
            this.pathTextBox.Size = new System.Drawing.Size(173, 20);
            this.pathTextBox.TabIndex = 26;
            this.pathTextBox.Text = "A:\\Users\\nox\\Documents\\EVE\\logs\\Marketlogs\\";
            // 
            // excepListBox
            // 
            this.excepListBox.FormattingEnabled = true;
            this.excepListBox.Location = new System.Drawing.Point(279, 13);
            this.excepListBox.Name = "excepListBox";
            this.excepListBox.Size = new System.Drawing.Size(391, 654);
            this.excepListBox.TabIndex = 10;
            this.excepListBox.SelectedIndexChanged += new System.EventHandler(this.excepListBox_SelectedIndexChanged);
            // 
            // characterListBox
            // 
            this.characterListBox.FormattingEnabled = true;
            this.characterListBox.Items.AddRange(new object[] {
            this.characterListBox.Location = new System.Drawing.Point(6, 209);
            this.characterListBox.Name = "characterListBox";
            this.characterListBox.Size = new System.Drawing.Size(126, 30);
            this.characterListBox.TabIndex = 24;
            this.characterListBox.SelectedIndexChanged += new System.EventHandler(this.characterListBox_SelectedIndexChanged);
            // 
            // timingLabel
            // 
            this.timingLabel.AutoSize = true;
            this.timingLabel.Location = new System.Drawing.Point(3, 101);
            this.timingLabel.Name = "timingLabel";
            this.timingLabel.Size = new System.Drawing.Size(82, 13);
            this.timingLabel.TabIndex = 23;
            this.timingLabel.Text = "Timing Multiplier";
            // 
            // timingTextBox
            // 
            this.timingTextBox.Location = new System.Drawing.Point(100, 98);
            this.timingTextBox.Name = "timingTextBox";
            this.timingTextBox.Size = new System.Drawing.Size(50, 20);
            this.timingTextBox.TabIndex = 22;
            this.timingTextBox.Text = "50";
            // 
            // nTimesTextBox
            // 
            this.nTimesTextBox.Location = new System.Drawing.Point(45, 291);
            this.nTimesTextBox.Name = "nTimesTextBox";
            this.nTimesTextBox.Size = new System.Drawing.Size(50, 20);
            this.nTimesTextBox.TabIndex = 20;
            this.nTimesTextBox.Text = "1";
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(58, 561);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(158, 25);
            this.runButton.TabIndex = 19;
            this.runButton.Text = "Run Order Adjuster n Times";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // nTimesLabel
            // 
            this.nTimesLabel.AutoSize = true;
            this.nTimesLabel.Location = new System.Drawing.Point(26, 294);
            this.nTimesLabel.Name = "nTimesLabel";
            this.nTimesLabel.Size = new System.Drawing.Size(13, 13);
            this.nTimesLabel.TabIndex = 21;
            this.nTimesLabel.Text = "n";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.mainTab);
            this.tabControl.Controls.Add(this.modificationLogTab);
            this.tabControl.Controls.Add(this.autoSellerTab);
            this.tabControl.Controls.Add(this.exceptionsTab);
            this.tabControl.Location = new System.Drawing.Point(1, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(689, 697);
            this.tabControl.TabIndex = 25;
            // 
            // exceptionsTab
            // 
            this.exceptionsTab.BackColor = System.Drawing.Color.Gainsboro;
            this.exceptionsTab.Controls.Add(this.exceptionsListbox);
            this.exceptionsTab.Location = new System.Drawing.Point(4, 22);
            this.exceptionsTab.Name = "exceptionsTab";
            this.exceptionsTab.Padding = new System.Windows.Forms.Padding(3);
            this.exceptionsTab.Size = new System.Drawing.Size(681, 671);
            this.exceptionsTab.TabIndex = 3;
            this.exceptionsTab.Text = "Exceptions";
            // 
            // exceptionsListbox
            // 
            this.exceptionsListbox.FormattingEnabled = true;
            this.exceptionsListbox.Location = new System.Drawing.Point(126, 11);
            this.exceptionsListbox.Name = "exceptionsListbox";
            this.exceptionsListbox.Size = new System.Drawing.Size(407, 654);
            this.exceptionsListbox.TabIndex = 0;
            // 
            // ETGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(687, 699);
            this.Controls.Add(this.tabControl);
            this.Name = "ETGUI";
            this.Text = "noxiousET - Beta 1.0.1.6";
            this.Load += new System.EventHandler(this.noxiousET_load);
            this.autoSellerTab.ResumeLayout(false);
            this.modificationLogTab.ResumeLayout(false);
            this.mainTab.ResumeLayout(false);
            this.mainTab.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.exceptionsTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage autoSellerTab;
        private System.Windows.Forms.ListView runsStatsListView;
        private System.Windows.Forms.ColumnHeader runs;
        private System.Windows.Forms.ColumnHeader scanedOrders;
        private System.Windows.Forms.ColumnHeader modifiedOrders;
        private System.Windows.Forms.ColumnHeader runDuration;
        private System.Windows.Forms.ListBox autoSellerListBox;
        private System.Windows.Forms.TabPage modificationLogTab;
        private System.Windows.Forms.ListView modifiedOrdersListView;
        private System.Windows.Forms.ColumnHeader runHeader;
        private System.Windows.Forms.ColumnHeader itemNameHeader;
        private System.Windows.Forms.ColumnHeader Type;
        private System.Windows.Forms.ColumnHeader bestSellOrder;
        private System.Windows.Forms.ColumnHeader bestBuyOrder;
        private System.Windows.Forms.ColumnHeader myOrderPrice;
        private System.Windows.Forms.ColumnHeader newOrderPrice;
        private System.Windows.Forms.TabPage mainTab;
        private System.Windows.Forms.ListBox excepListBox;
        private System.Windows.Forms.ListBox characterListBox;
        private System.Windows.Forms.Label timingLabel;
        private System.Windows.Forms.TextBox timingTextBox;
        private System.Windows.Forms.TextBox nTimesTextBox;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.Label nTimesLabel;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TextBox pathTextBox;
        private System.Windows.Forms.Button saveSettingsButton;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.TextBox qtyTextbox1;
        private System.Windows.Forms.TextBox qtyTextbox5;
        private System.Windows.Forms.TextBox qtyTextbox4;
        private System.Windows.Forms.TextBox qtyTextbox3;
        private System.Windows.Forms.TextBox qtyTextbox2;
        private System.Windows.Forms.TextBox threshHoldTextbox5;
        private System.Windows.Forms.TextBox threshHoldTextbox4;
        private System.Windows.Forms.TextBox threshHoldTextbox3;
        private System.Windows.Forms.TextBox threshHoldTextbox2;
        private System.Windows.Forms.TextBox threshHoldTextbox1;
        private System.Windows.Forms.ListBox stationsListBox;
        private System.Windows.Forms.Button prototypeButton;
        private System.Windows.Forms.Label EVEPathLabel;
        private System.Windows.Forms.TextBox EVEPathTextbox;
        private System.Windows.Forms.ListBox elementsListbox;
        private System.Windows.Forms.Label activeCharacterLabel;
        private System.Windows.Forms.Label resolutionLabel;
        private System.Windows.Forms.Label stationIDLabel;
        private System.Windows.Forms.Label threshHoldsLabel;
        private System.Windows.Forms.CheckBox tradesShipsCheckbox;
        private System.Windows.Forms.CheckBox tradesItemsCheckbox;
        private System.Windows.Forms.Button autoSellerButton;
        private System.Windows.Forms.CheckBox adjustBuysCheckbox;
        private System.Windows.Forms.CheckBox adjustSellsCheckbox;
        private System.Windows.Forms.Label configPathLabel;
        private System.Windows.Forms.TextBox configPathTextbox;
        private System.Windows.Forms.Button fullAutomationButton;
        private System.Windows.Forms.TextBox autoSellEveryNRunsTextbox;
        private System.Windows.Forms.Label autoSellEveryNRunsLabel;
        private System.Windows.Forms.TabPage exceptionsTab;
        private System.Windows.Forms.ListBox exceptionsListbox;
        private System.Windows.Forms.Label inactiveCharactersLabel;
        private System.Windows.Forms.ListBox inactiveCharactersTextbox;
        private System.Windows.Forms.Button addAcctiveButton;
        private System.Windows.Forms.Button removeActiveButton;
        private System.Windows.Forms.TextBox maxOrdersTextbox;
        private System.Windows.Forms.Label maxOrdersLabel;

    }
}

