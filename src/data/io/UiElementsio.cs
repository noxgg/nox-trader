using System;
using noxiousET.src.data.uielements;

namespace noxiousET.src.data.io
{
    public class UiElementsio : TextFileio
    {
        private readonly UiElements _uiElements;


        public UiElementsio(String path, String fileName, UiElements uiElements) : base(path, fileName)
        {
            _uiElements = uiElements;
            Load();
        }

        public int Load()
        {
            try
            {
                ReadOpen();

                _uiElements.AlertCancelButton[0] = ReadLineAsInt();
                _uiElements.AlertCancelButton[1] = ReadLineAsInt();
                _uiElements.AlertConfirmButton[0] = ReadLineAsInt();
                _uiElements.AlertConfirmButton[1] = ReadLineAsInt();
                _uiElements.AlertContextMenuCopyOffset[0] = ReadLineAsInt();
                _uiElements.AlertContextMenuCopyOffset[1] = ReadLineAsInt();
                _uiElements.AlertMessageBody[0] = ReadLineAsInt();
                _uiElements.AlertMessageBody[1] = ReadLineAsInt();
                _uiElements.BuyBidPriceField[0] = ReadLineAsInt();
                _uiElements.BuyBidPriceField[1] = ReadLineAsInt();
                _uiElements.BuyQuantityField[0] = ReadLineAsInt();
                _uiElements.BuyQuantityField[1] = ReadLineAsInt();
                _uiElements.CharacterSelectActiveSlot[0] = ReadLineAsInt();
                _uiElements.CharacterSelectActiveSlot[1] = ReadLineAsInt();
                _uiElements.CharacterSelectSlot2[0] = ReadLineAsInt();
                _uiElements.CharacterSelectSlot2[1] = ReadLineAsInt();
                _uiElements.CharacterSelectSlot3[0] = ReadLineAsInt();
                _uiElements.CharacterSelectSlot3[1] = ReadLineAsInt();
                _uiElements.CharacterSelectTip[0] = ReadLineAsInt();
                _uiElements.CharacterSelectTip[1] = ReadLineAsInt();
                _uiElements.ChatCopyOffset[0] = ReadLineAsInt();
                _uiElements.ChatCopyOffset[1] = ReadLineAsInt();
                _uiElements.confirmingOrderAdjustment = ReadLineAsInt();
                _uiElements.ContextMenuCancelOrderOffset[0] = ReadLineAsInt();
                _uiElements.ContextMenuCancelOrderOffset[1] = ReadLineAsInt();
                _uiElements.ContextMenuCopyOffset[0] = ReadLineAsInt();
                _uiElements.ContextMenuCopyOffset[1] = ReadLineAsInt();
                _uiElements.ContextMenuModifyOrderOffset[0] = ReadLineAsInt();
                _uiElements.ContextMenuModifyOrderOffset[1] = ReadLineAsInt();
                _uiElements.ContextMenuPasteOffset[0] = ReadLineAsInt();
                _uiElements.ContextMenuPasteOffset[1] = ReadLineAsInt();
                _uiElements.ContextMenuViewDetailsOffset[0] = ReadLineAsInt();
                _uiElements.ContextMenuViewDetailsOffset[1] = ReadLineAsInt();
                _uiElements.HangarCloseButton[0] = ReadLineAsInt();
                _uiElements.HangarCloseButton[1] = ReadLineAsInt();
                _uiElements.HangarContextMenuExtraXOffsetForModules = ReadLineAsInt();
                _uiElements.HangarContextMenuSellItemOffset[0] = ReadLineAsInt();
                _uiElements.HangarContextMenuSellItemOffset[1] = ReadLineAsInt();
                _uiElements.HangarContextMenuViewDetailsOffset[0] = ReadLineAsInt();
                _uiElements.HangarContextMenuViewDetailsOffset[1] = ReadLineAsInt();
                _uiElements.HangarFirstRow[0] = ReadLineAsInt();
                _uiElements.HangarFirstRow[1] = ReadLineAsInt();
                _uiElements.HangarRowHeight = ReadLineAsInt();
                _uiElements.LoginPasswordField[0] = ReadLineAsInt();
                _uiElements.LoginPasswordField[1] = ReadLineAsInt();
                _uiElements.LoginUserNameField[0] = ReadLineAsInt();
                _uiElements.LoginUserNameField[1] = ReadLineAsInt();
                _uiElements.MarketCloseButton[0] = ReadLineAsInt();
                _uiElements.MarketCloseButton[1] = ReadLineAsInt();
                _uiElements.MarketExportButton[0] = ReadLineAsInt();
                _uiElements.MarketExportButton[1] = ReadLineAsInt();
                _uiElements.MarketPlaceBuyButton[0] = ReadLineAsInt();
                _uiElements.MarketPlaceBuyButton[1] = ReadLineAsInt();
                _uiElements.MarketWindowDeadspace[0] = ReadLineAsInt();
                _uiElements.MarketWindowDeadspace[1] = ReadLineAsInt();
                _uiElements.MarketWindowQuickbarFirstRow[0] = ReadLineAsInt();
                _uiElements.MarketWindowQuickbarFirstRow[1] = ReadLineAsInt();
                _uiElements.MarketWindowQuickbarScrollbarBottom[0] = ReadLineAsInt();
                _uiElements.MarketWindowQuickbarScrollbarBottom[1] = ReadLineAsInt();
                _uiElements.MarketWindowQuickbarScrollbarTop[0] = ReadLineAsInt();
                _uiElements.MarketWindowQuickbarScrollbarTop[1] = ReadLineAsInt();
                _uiElements.MarketWindowQuickbarScrollbarUnfixedPosition[0] = ReadLineAsInt();
                _uiElements.MarketWindowQuickbarScrollbarUnfixedPosition[1] = ReadLineAsInt();
                _uiElements.MarketWindowQuickbarVisibleRows = ReadLineAsInt();
                _uiElements.ModifyBidField[0] = ReadLineAsInt();
                _uiElements.ModifyBidField[1] = ReadLineAsInt();
                _uiElements.OrderBoxCancel[0] = ReadLineAsInt();
                _uiElements.OrderBoxCancel[1] = ReadLineAsInt();
                _uiElements.OrderBoxConfirm[0] = ReadLineAsInt();
                _uiElements.OrderBoxConfirm[1] = ReadLineAsInt();
                _uiElements.SellBidPriceField[0] = ReadLineAsInt();
                _uiElements.SellBidPriceField[1] = ReadLineAsInt();
                _uiElements.StandardRowHeight = ReadLineAsInt();
                _uiElements.WalletBuyListFirstRow[0] = ReadLineAsInt();
                _uiElements.WalletBuyListFirstRow[1] = ReadLineAsInt();
                _uiElements.WalletBuyListSortByType[0] = ReadLineAsInt();
                _uiElements.WalletBuyListSortByType[1] = ReadLineAsInt();
                _uiElements.WalletExportButton[0] = ReadLineAsInt();
                _uiElements.WalletExportButton[1] = ReadLineAsInt();
                _uiElements.WalletSellListFirstRow[0] = ReadLineAsInt();
                _uiElements.WalletSellListFirstRow[1] = ReadLineAsInt();
                _uiElements.WalletSellListSortByType[0] = ReadLineAsInt();
                _uiElements.WalletSellListSortByType[1] = ReadLineAsInt();
                _uiElements.WalletVisibleRows[0] = ReadLineAsInt();
                _uiElements.WalletVisibleRows[1] = ReadLineAsInt();


                ReadClose();

                return 0;
            }
            catch
            {
                ReadClose();
                return 1;
            }
        }
    }
}