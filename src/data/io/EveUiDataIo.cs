using System;
using System.Linq;
using noxiousET.src.data.uidata;

namespace noxiousET.src.data.io
{
    public class EveUiDataIo : TextFileio
    {
        private readonly EveUi _eveUi;


        public EveUiDataIo(String path, String fileName, EveUi eveUi) : base(path, fileName)
        {
            _eveUi = eveUi;
            Load();
        }

        public int Load()
        {
            try
            {
                ReadOpen();

                var lines = ReadFile();

                foreach (var line in lines)
                {
                    String[] keyValuePair = line.Split('=');;
                    String key = keyValuePair[0];
                    int[] values = keyValuePair[1].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                    switch (key)
                    {
                        case EveUi.AlertCancelButtonKey:
                            _eveUi.AlertCancelButton = values;
                            break;
                        case EveUi.AlertConfirmButtonKey:
                            _eveUi.AlertConfirmButton = values;
                            break;
                        case EveUi.AlertContextMenuCopyOffsetKey:
                            _eveUi.AlertContextMenuCopyOffset = values;
                            break;
                        case EveUi.AlertMessageBodyKey:
                            _eveUi.AlertMessageBody = values;
                            break;
                        case EveUi.BuyBidPriceFieldKey:
                            _eveUi.BuyBidPriceField = values;
                            break;
                        case EveUi.BuyQuantityFieldKey:
                            _eveUi.BuyQuantityField = values;
                            break;
                        case EveUi.CharacterSelectActiveSlotKey:
                            _eveUi.CharacterSelectActiveSlot = values;
                            break;
                        case EveUi.CharacterSelectSlot2Key:
                            _eveUi.CharacterSelectSlot2 = values;
                            break;
                        case EveUi.CharacterSelectSlot3Key:
                            _eveUi.CharacterSelectSlot3 = values;
                            break;
                        case EveUi.CharacterSelectTipKey:
                            _eveUi.CharacterSelectTip = values;
                            break;
                        case EveUi.ChatCopyOffsetKey:
                            _eveUi.ChatCopyOffset = values;
                            break;
                        case EveUi.confirmingOrderAdjustmentKey:
                            _eveUi.confirmingOrderAdjustment = values[0];
                            break;
                        case EveUi.ContextMenuCancelOrderOffsetKey:
                            _eveUi.ContextMenuCancelOrderOffset = values;
                            break;
                        case EveUi.ContextMenuCopyOffsetKey:
                            _eveUi.ContextMenuCopyOffset = values;
                            break;
                        case EveUi.ContextMenuModifyOrderOffsetKey:
                            _eveUi.ContextMenuModifyOrderOffset = values;
                            break;
                        case EveUi.ContextMenuPasteOffsetKey:
                            _eveUi.ContextMenuPasteOffset = values;
                            break;
                        case EveUi.ContextMenuViewDetailsOffsetKey:
                            _eveUi.ContextMenuViewDetailsOffset = values;
                            break;
                        case EveUi.HangarCloseButtonKey:
                            _eveUi.HangarCloseButton = values;
                            break;
                        case EveUi.HangarContextMenuExtraOffsetYForModulesKey:
                            _eveUi.HangarContextMenuExtraYOffsetForModules = values[0];
                            break;
                        case EveUi.HangarContextMenuSellItemOffsetKey:
                            _eveUi.HangarContextMenuSellItemOffset = values;
                            break;
                        case EveUi.HangarContextMenuViewDetailsOffsetKey:
                            _eveUi.HangarContextMenuViewDetailsOffset = values;
                            break;
                        case EveUi.HangarFirstRowKey:
                            _eveUi.HangarFirstRow = values;
                            break;
                        case EveUi.HangarRowHeightKey:
                            _eveUi.HangarRowHeight = values[0];
                            break;
                        case EveUi.LoginPasswordFieldKey:
                            _eveUi.LoginPasswordField = values;
                            break;
                        case EveUi.LoginUserNameFieldKey:
                            _eveUi.LoginUserNameField = values;
                            break;
                        case EveUi.MarketCloseButtonKey:
                            _eveUi.MarketCloseButton = values;
                            break;
                        case EveUi.MarketExportButtonKey:
                            _eveUi.MarketExportButton = values;
                            break;
                        case EveUi.MarketPlaceBuyButtonKey:
                            _eveUi.MarketPlaceBuyButton = values;
                            break;
                        case EveUi.MarketWindowDeadspaceKey:
                            _eveUi.MarketWindowDeadspace = values;
                            break;
                        case EveUi.MarketWindowQuickbarFirstRowKey:
                            _eveUi.MarketWindowQuickbarFirstRow = values;
                            break;
                        case EveUi.MarketWindowQuickbarScrollbarBottomKey:
                            _eveUi.MarketWindowQuickbarScrollbarBottom = values;
                            break;
                        case EveUi.MarketWindowQuickbarScrollbarTopKey:
                            _eveUi.MarketWindowQuickbarScrollbarTop = values;
                            break;
                        case EveUi.MarketWindowQuickbarScrollbarUnfixedPositionKey:
                            _eveUi.MarketWindowQuickbarScrollbarUnfixedPosition = values;
                            break;
                        case EveUi.MarketWindowQuickbarVisibleRowsKey:
                            _eveUi.MarketWindowQuickbarVisibleRows = values[0];
                            break;
                        case EveUi.ModifyBidFieldKey:
                            _eveUi.ModifyBidField = values;
                            break;
                        case EveUi.OrderBoxCancelKey:
                            _eveUi.OrderBoxCancel = values;
                            break;
                        case EveUi.OrderBoxConfirmKey:
                            _eveUi.OrderBoxConfirm = values;
                            break;
                        case EveUi.SellBidPriceFieldKey:
                            _eveUi.SellBidPriceField = values;
                            break;
                        case EveUi.StandardRowHeightKey:
                            _eveUi.StandardRowHeight = values[0];
                            break;
                        case EveUi.WalletBuyListFirstRowKey:
                            _eveUi.WalletBuyListFirstRow = values;
                            break;
                        case EveUi.WalletBuyListSortByTypeKey:
                            _eveUi.WalletBuyListSortByType = values;
                            break;
                        case EveUi.WalletExportButtonKey:
                            _eveUi.WalletExportButton = values;
                            break;
                        case EveUi.WalletSellListFirstRowKey:
                            _eveUi.WalletSellListFirstRow = values;
                            break;
                        case EveUi.WalletSellListSortByTypeKey:
                            _eveUi.WalletSellListSortByType = values;
                            break;
                        case EveUi.WalletVisibleRowsKey:
                            _eveUi.WalletVisibleRows = values;
                            break;
                    }
                }
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