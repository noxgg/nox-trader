namespace noxiousET.src.data.uielements
{
    public class UiElements
    {
        public UiElements()
        {
            MarketCloseButton = new int[2] {1721, 10};
            MarketExportButton = new int[2] {1190, 978};
            MarketPlaceBuyButton = new int[2] {1298, 978};
            WalletExportButton = new int[2] {1762, 1425};
            WalletSellListFirstRow = new int[2] {1920, 121};
            WalletSellListSortByType = new int[2] {1920, 103};
            WalletBuyListFirstRow = new int[2] {1920, 524};
            WalletBuyListSortByType = new int[2] {1920, 504};
            ContextMenuModifyOrderOffset = new int[2] {52, 10};
            ContextMenuViewDetailsOffset = new int[2] {52, 58};
            ContextMenuCopyOffset = new int[2] {38, 26};
            BuyBidPriceField = new int[2] {1367, 1027};
            BuyQuantityField = new int[2] {1367, 1166};
            SellBidPriceField = new int[2] {1367, 1031};
            ModifyBidField = new int[2] {1367, 1254};
            OrderBoxCancel = new int[2] {1521, 1337};
            OrderBoxConfirm = new int[2] {1463, 1330};
            AlertCancelButton = new int[2] {1287, 813};
            AlertConfirmButton = new int[2] {1275, 813};
            WalletVisibleRows = new int[2] {19, 41}; //sell, buy
            HangarFirstRow = new int[2] {121, 86};
            HangarContextMenuViewDetailsOffset = new int[2] {62, 42};
            HangarContextMenuSellItemOffset = new int[2] {62, 57};
            LoginUserNameField = new int[2] {1226, 1246};
            LoginPasswordField = new int[2] {1226, 1264};
            CharacterSelectTip = new int[2] {1079, 500};
            CharacterSelectActiveSlot = new int[2] {235, 454};
            CharacterSelectSlot2 = new int[2] {138, 791};
            CharacterSelectSlot3 = new int[2] {343, 791};
            AlertMessageBody = new int[2] {1300, 735};
            AlertContextMenuCopyOffset = new int[2] {23, 13};
            HangarCloseButton = new int[2] {426, 9};
            ChatCopyOffset = new int[2] {25, 13};
            ContextMenuPasteOffset = new int[2] {38, 42};
            StandardRowHeight = 20;
            HangarRowHeight = 21;
            HangarContextMenuExtraXOffsetForModules = -17;
            MarketWindowDeadspace = new int[2] {1300, 735};
            MarketWindowQuickbarFirstRow = new int[2] {1300, 735};
            MarketWindowQuickbarScrollbarTop = new int[2] {0, 0};
            MarketWindowQuickbarScrollbarBottom = new int[2] {0, 0};
            MarketWindowQuickbarVisibleRows = 15;
            MarketWindowQuickbarScrollbarUnfixedPosition = new int[2] {469, 200};
            confirmingOrderAdjustment = 55;
            ContextMenuCancelOrderOffset = new int[2] {72, 88};
        }

        public int confirmingOrderAdjustment { set; get; }
        public int HangarContextMenuExtraXOffsetForModules { set; get; }
        public int HangarRowHeight { set; get; }
        public int MarketWindowQuickbarVisibleRows { set; get; }
        public int StandardRowHeight { set; get; }
        public int[] AlertCancelButton { set; get; }
        public int[] AlertConfirmButton { set; get; }
        public int[] AlertContextMenuCopyOffset { set; get; }
        public int[] AlertMessageBody { set; get; }
        public int[] BuyBidPriceField { set; get; }
        public int[] BuyQuantityField { set; get; }
        public int[] CharacterSelectActiveSlot { set; get; }
        public int[] CharacterSelectSlot2 { set; get; }
        public int[] CharacterSelectSlot3 { set; get; }
        public int[] CharacterSelectTip { set; get; }
        public int[] ChatCopyOffset { set; get; }
        public int[] ContextMenuCancelOrderOffset { set; get; }
        public int[] ContextMenuCopyOffset { set; get; }
        public int[] ContextMenuModifyOrderOffset { set; get; }
        public int[] ContextMenuPasteOffset { set; get; }
        public int[] ContextMenuViewDetailsOffset { set; get; }
        public int[] HangarCloseButton { set; get; }
        public int[] HangarContextMenuSellItemOffset { set; get; }
        public int[] HangarContextMenuViewDetailsOffset { set; get; }
        public int[] HangarFirstRow { set; get; }
        public int[] LoginPasswordField { set; get; }
        public int[] LoginUserNameField { set; get; }
        public int[] MarketCloseButton { set; get; }
        public int[] MarketExportButton { set; get; }
        public int[] MarketPlaceBuyButton { set; get; }
        public int[] MarketWindowDeadspace { set; get; }
        public int[] MarketWindowQuickbarFirstRow { set; get; }
        public int[] MarketWindowQuickbarScrollbarBottom { set; get; }
        public int[] MarketWindowQuickbarScrollbarTop { set; get; }
        public int[] MarketWindowQuickbarScrollbarUnfixedPosition { set; get; }
        public int[] ModifyBidField { set; get; }
        public int[] OrderBoxCancel { set; get; }
        public int[] OrderBoxConfirm { set; get; }
        public int[] SellBidPriceField { set; get; }
        public int[] WalletBuyListFirstRow { set; get; }
        public int[] WalletBuyListSortByType { set; get; }
        public int[] WalletExportButton { set; get; }
        public int[] WalletSellListFirstRow { set; get; }
        public int[] WalletSellListSortByType { set; get; }
        public int[] WalletVisibleRows { set; get; }
    };
}