﻿using System;
using noxiousET.src.data.uielements;

namespace noxiousET.src.data.io
{
    public class UiElementsio : TextFileio
    {
        UiElements uiElements;


        public UiElementsio(String path, String fileName, UiElements uiElements) : base (path, fileName)
        {
            this.uiElements = uiElements;
            load();
        }

        public int load()
        {
            try
            {
                readOpen();

                uiElements.closeMarketWindow[0] = readLineAsInt();
                uiElements.closeMarketWindow[1] = readLineAsInt();
                uiElements.exportItem[0] = readLineAsInt();
                uiElements.exportItem[1] = readLineAsInt();
                uiElements.placeBuyOrder[0] = readLineAsInt();
                uiElements.placeBuyOrder[1] = readLineAsInt();
                uiElements.exportOrderList[0] = readLineAsInt();
                uiElements.exportOrderList[1] = readLineAsInt();
                uiElements.sellTop[0] = readLineAsInt();
                uiElements.sellTop[1] = readLineAsInt();
                uiElements.sellSortByType[0] = readLineAsInt();
                uiElements.sellSortByType[1] = readLineAsInt();
                uiElements.buyTop[0] = readLineAsInt();
                uiElements.buyTop[1] = readLineAsInt();
                uiElements.buySortByType[0] = readLineAsInt();
                uiElements.buySortByType[1] = readLineAsInt();
                uiElements.modifyOffset[0] = readLineAsInt();
                uiElements.modifyOffset[1] = readLineAsInt();
                uiElements.viewDetailsOffset[0] = readLineAsInt();
                uiElements.viewDetailsOffset[1] = readLineAsInt();
                uiElements.copyOffset[0] = readLineAsInt();
                uiElements.copyOffset[1] = readLineAsInt();
                uiElements.buyOrderBox[0] = readLineAsInt();
                uiElements.buyOrderBox[1] = readLineAsInt();
                uiElements.buyOrderQtyBox[0] = readLineAsInt();
                uiElements.buyOrderQtyBox[1] = readLineAsInt();
                uiElements.sellOrderBox[0] = readLineAsInt();
                uiElements.sellOrderBox[1] = readLineAsInt();
                uiElements.modifyOrderBox[0] = readLineAsInt();
                uiElements.modifyOrderBox[1] = readLineAsInt();
                uiElements.OrderBoxCancel[0] = readLineAsInt();
                uiElements.OrderBoxCancel[1] = readLineAsInt();
                uiElements.OrderBoxOK[0] = readLineAsInt();
                uiElements.OrderBoxOK[1] = readLineAsInt();
                uiElements.errorCheck[0] = readLineAsInt();
                uiElements.errorCheck[1] = readLineAsInt();
                uiElements.confirmErrorCheck[0] = readLineAsInt();
                uiElements.confirmErrorCheck[1] = readLineAsInt();
                uiElements.lineHeight = readLineAsInt();
                uiElements.visLines[0] = readLineAsInt();
                uiElements.visLines[1] = readLineAsInt();
                uiElements.maxOrders = readLineAsInt();
                uiElements.itemsSort[0] = readLineAsInt();
                uiElements.itemsSort[1] = readLineAsInt();
                uiElements.itemsTop[0] = readLineAsInt();
                uiElements.itemsTop[1] = readLineAsInt();
                uiElements.itemsLineHeight = readLineAsInt();
                uiElements.itemsViewDetailsOffset[0] = readLineAsInt();
                uiElements.itemsViewDetailsOffset[1] = readLineAsInt();
                uiElements.itemsSellItemOffset[0] = readLineAsInt();
                uiElements.itemsSellItemOffset[1] = readLineAsInt();
                uiElements.itemsViewModuleDetailExtraOffset = readLineAsInt();
                uiElements.loginScreenUserName[0] = readLineAsInt();
                uiElements.loginScreenUserName[1] = readLineAsInt();
                uiElements.loginScreenPW[0] = readLineAsInt();
                uiElements.loginScreenPW[1] = readLineAsInt();
                uiElements.selectCharacterScreenIdentification[0] = readLineAsInt();
                uiElements.selectCharacterScreenIdentification[1] = readLineAsInt();
                uiElements.selectCharacterActive[0] = readLineAsInt();
                uiElements.selectCharacterActive[1] = readLineAsInt();
                uiElements.selectCharacterAlt1[0] = readLineAsInt();
                uiElements.selectCharacterAlt1[1] = readLineAsInt();
                uiElements.selectCharacterAlt2[0] = readLineAsInt();
                uiElements.selectCharacterAlt2[1] = readLineAsInt();
                uiElements.parseErrorMessage[0] = readLineAsInt();
                uiElements.parseErrorMessage[1] = readLineAsInt();
                uiElements.parseErrorMessageCopyOffset[0] = readLineAsInt();
                uiElements.parseErrorMessageCopyOffset[1] = readLineAsInt();
                uiElements.closeItems[0] = readLineAsInt();
                uiElements.closeItems[1] = readLineAsInt();
                uiElements.confirmationCopyOffset[0] = readLineAsInt();
                uiElements.confirmationCopyOffset[1] = readLineAsInt();
                uiElements.pasteOffset[0] = readLineAsInt();
                uiElements.pasteOffset[1] = readLineAsInt();
                uiElements.endOfItemsList = readLineAsInt();
                uiElements.itemsMenuTouchesBottomOfScreen = readLineAsInt();
                uiElements.bringMarketWindowToFront[0] = readLineAsInt();
                uiElements.bringMarketWindowToFront[1] = readLineAsInt();
                uiElements.marketSearchTab[0] = readLineAsInt();
                uiElements.marketSearchTab[1] = readLineAsInt();
                uiElements.marketSearchInputBox[0] = readLineAsInt();
                uiElements.marketSearchInputBox[1] = readLineAsInt();
                uiElements.marketSearchResult[0] = readLineAsInt();
                uiElements.marketSearchResult[1] = readLineAsInt();
                uiElements.marketSearchExecute[0] = readLineAsInt();
                uiElements.marketSearchExecute[1] = readLineAsInt();
                uiElements.confirmingOrderAdjustment = readLineAsInt();

                readClose();

                return 0;
            }
            catch
            {
                readClose();
                return 1;
            }
        }
    }
}
