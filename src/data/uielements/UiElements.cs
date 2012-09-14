namespace noxiousET.src.data.uielements
{
    public class UiElements
    {
        public int[] closeMarketWindow { set; get; }
        public int[] exportItem { set; get; }
        public int[] placeBuyOrder { set; get; }
        public int[] exportOrderList { set; get; }
        public int[] sellTop { set; get; }
        public int[] sellSortByType { set; get; }
        public int[] buyTop { set; get; }
        public int[] buySortByType { set; get; }

        public int[] modifyOffset { set; get; }
        public int[] viewDetailsOffset { set; get; }

        public int[] copyOffset { set; get; }
        public int[] buyOrderBox { set; get; }
        public int[] buyOrderQtyBox { set; get; }
        public int[] sellOrderBox { set; get; }
        public int[] modifyOrderBox { set; get; }
        public int[] OrderBoxCancel { set; get; }
        public int[] OrderBoxOK { set; get; }

        public int[] errorCheck { set; get; }
        public int[] confirmErrorCheck { set; get; }

        public int[] visLines { set; get; }

        public int[] itemsSort { set; get; }
        public int[] itemsTop { set; get; }
        public int[] itemsViewDetailsOffset { set; get; }
        public int[] itemsSellItemOffset { set; get; }

        public int[] loginScreenUserName { set; get; }
        public int[] loginScreenPW { set; get; }
        public int[] selectCharacterScreenIdentification { set; get; }

        public int[] selectCharacterActive { set; get; }
        public int[] selectCharacterAlt1 { set; get; }
        public int[] selectCharacterAlt2 { set; get; }

        public int[] parseErrorMessage { set; get; }
        public int[] parseErrorMessageCopyOffset { set; get; }
        public int[] closeItems { set; get; }
        public int[] confirmationCopyOffset { set; get; }
        public int[] pasteOffset { set; get; }

        public int lineHeight { set; get; }
        public int maxOrders { set; get; }
        public int itemsLineHeight { set; get; }
        public int itemsViewModuleDetailExtraOffset { set; get; }
        public int endOfItemsList { set; get; }
        public int itemsMenuTouchesBottomOfScreen { set; get; }
        public int[] bringMarketWindowToFront { set; get; }
        public int[] marketWindowQuickbarFirstRow { set; get; }
        public int[] marketWindowQuickbarScrollbarTop { set; get; }
        public int[] marketWindowQuickbarScrollbarBottom { set; get; }
        public int marketWindowQuickbarVisibleRows { set; get; }
        public int[] marketWindowQuickbarScrollbarUnfixedPosition { set; get; }
        public int confirmingOrderAdjustment { set; get; }


        public UiElements() 
        {
            closeMarketWindow = new int[2] { 1721, 10 };
            exportItem = new int[2] { 1190, 978 };
            placeBuyOrder = new int[2] { 1298, 978 };
            exportOrderList = new int[2] { 1762, 1425 };
            sellTop = new int[2] { 1920, 121 };
            sellSortByType = new int[2] { 1920, 103 };
            buyTop = new int[2] { 1920, 524 };
            buySortByType = new int[2] { 1920, 504 };

            modifyOffset = new int[2] { 52, 10 };
            viewDetailsOffset = new int[2] { 52, 58 };

            copyOffset = new int[2] { 38, 26 };
            buyOrderBox = new int[2] { 1367, 1027 };
            buyOrderQtyBox = new int[2] { 1367, 1166 };
            sellOrderBox = new int[2] { 1367, 1031 };
            modifyOrderBox = new int[2] { 1367, 1254 };
            OrderBoxCancel = new int[2] { 1521, 1337 };
            OrderBoxOK = new int[2] { 1463, 1330 };

            errorCheck = new int[2] { 1287, 813 };
            confirmErrorCheck = new int[2] { 1275, 813 };

            visLines = new int[2] { 19, 41 }; //sell, buy

            itemsSort = new int[2] { 121, 68 };
            itemsTop = new int[2] { 121, 86 };
            itemsViewDetailsOffset = new int[2] { 62, 42 };
            itemsSellItemOffset = new int[2] { 62, 57 };

            loginScreenUserName = new int[2] { 1226, 1246 };
            loginScreenPW = new int[2] { 1226, 1264 };

            selectCharacterScreenIdentification = new int[2] { 1079, 500 };
            selectCharacterActive = new int[2] { 235, 454 };
            selectCharacterAlt1 = new int[2] { 138, 791 };
            selectCharacterAlt2 = new int[2] { 343, 791 };

            parseErrorMessage = new int[2] { 1300, 735 };
            parseErrorMessageCopyOffset = new int[2] { 23, 13 };
            closeItems = new int[2] { 426, 9 };
            confirmationCopyOffset = new int[2] { 25, 13 };
            pasteOffset = new int[2] { 38, 42 };

            lineHeight = 20;
            maxOrders = 141;
            itemsLineHeight = 21;
            itemsViewModuleDetailExtraOffset = -17;
            endOfItemsList = 720;
            itemsMenuTouchesBottomOfScreen = 573;


            bringMarketWindowToFront = new int[2] { 1300, 735 };
            marketWindowQuickbarFirstRow = new int[2] { 1300, 735 };
            marketWindowQuickbarScrollbarTop= new int[2] { 0, 0 };
            marketWindowQuickbarScrollbarBottom = new int[2] { 0, 0 };
            marketWindowQuickbarVisibleRows = 15;
            marketWindowQuickbarScrollbarUnfixedPosition = new int[2] { 469, 200 };

            confirmingOrderAdjustment = 55;
        }

    };
}
