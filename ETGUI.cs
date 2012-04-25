using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Media;
using System.Collections.Generic;

//TODO: Margin trading scam? Order creation flags. Max order setting. Buying an item with no sell orders shifts buy order quantity box up.
namespace noxiousET
{

    public partial class ETGUI : Form
    {
        [DllImport("user32.dll")] 
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]

        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;

        elementsXY element = new elementsXY();
        orderManager orderSet;
        activeOrder aOrder = new activeOrder();
        PixelReader pixelReader;
        public string path = @"A:\Users\nox\Documents\EVE\logs\Marketlogs\";
        public string EVEPath = @"G:\EVE\eve.exe";
        public string configPath = @"D:\Dropbox\Dropbox\Apps\noxiousETConfig\";
        IntPtr eveHandle;

        int maxOrders = 141;
        int waitDuration = 50;
        int waitDurationBackup = 50;
        int numModified = 0;
        int numScanned = 0;
        int runsToMake = 1;
        int run = 0;
        int consecutiveFailures = 0;
        int autoSellEveryNRuns = 5;
        bool initialized = false;
        static int stopAllActivity = -69;
        int[] longNameItemIDs = { 9957, 13217, 13219, 13222, 19688, 19689, 19692, 19694, 27116, 27118, 27186, 27187, 27190, 27260, 30151, 30153 };
        int[] skipErrorCheckItemIDs = { 12237, 14343, 17174, 17176, 17177, 17178, 17181, 17182, 17703 };
        bool lastOrderModified = false;
        bool tradesItems = true;
        bool tradesShips = true;
        bool adjustSells = true;
        bool adjustBuys = true;
        int autoSellerWaitDuration = 50;
        List<string> exception = new List<string>();
        string selectedCharacter = null;
        string myStationID = null;
        Dictionary<String, int> loginColors;
        
        string p = "A8fd232";
        int runMode = 0;
        int fileNameTrimLength = 10;
        ErrorParser errorParser = new ErrorParser();
        int[,] quantityMatrix = 
            { 
            {20000000, 10},
            {40000000, 5},
            {60000000, 3},
            {100000000, 2},
            {500000000, 1}
            };

        public struct RECT
        {
            public int Left;        // x position of upper-left corner  
            public int Top;         // y position of upper-left corner  
            public int Right;       // x position of lower-right corner  
            public int Bottom;      // y position of lower-right corner  
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            runMode = 1;
            modOrdersPrep(false);
        }

        private void autoSellerButton_Click(object sender, EventArgs e)
        {
            runMode = 2;
            autoSellerPrep();
        }

        private int autoSellerPrep()
        {
            excepListBox.Items.Add("Automatically placing orders...");
            int terminalItemID = 0;
            int[] activeOrders = { 0, 0 };
            int openOrders = 0;
            int buyOrdersCreated = 0;
            int sellOrdersCreated = 0;
            int result = 0;
            autoSellerWaitDuration = waitDurationBackup * 3;

            waitDuration = waitDurationBackup = Convert.ToInt32(timingTextBox.Text);
            if (!isEVERunning())
            {
                return 1;
            }
            else
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                SetForegroundWindow(eveHandle);
                ++run;
                this.Cursor = new Cursor(Cursor.Current.Handle);
                int failCount = 0;
                wipeDirectory(path);
                do
                {
                    if (stopCheck() == stopAllActivity)
                        return stopAllActivity;
                    Thread.Sleep(waitDuration * 20);
                    try
                    {
                        result = exportOrders(element);//Clicks on export orders.
                    }
                    catch
                    {
                        result = 1;
                    }
                    if (result == 1)//Clicks on export orders.
                    {
                        ++failCount;
                        errorCheck();
                        Thread.Sleep(waitDuration * 20);
                    }
                    else
                    {
                        activeOrders = orderSet.getNumOfActiveOrders();
                        openOrders = maxOrders - (activeOrders[0] + activeOrders[1]);
                        failCount = 4;
                    }
                } while (failCount < 3);

                new SetClipboardHelper(DataFormats.Text, "0").Go();
                if (failCount == 4)
                {
                    if (tradesItems)
                    {
                        terminalItemID = 5321;
                        SendKeys.SendWait("{PGUP}");
                        waitDuration = waitDuration * 3;
                        result = autoSeller(ref openOrders, 0, terminalItemID, ref buyOrdersCreated, ref sellOrdersCreated);
                        if (result == stopAllActivity)
                        {
                            waitDuration = autoSellerWaitDuration;
                            aOrder.clearLastBuyOrder();
                            SendKeys.SendWait("{PGUP}");
                            return stopAllActivity;
                        }
                        else if (result == 1)
                        {
                            waitDuration = autoSellerWaitDuration;
                            aOrder.clearLastBuyOrder();
                            SendKeys.SendWait("{PGUP}");
                            return 1;
                        }
                        waitDuration = waitDurationBackup;
                        aOrder.clearLastBuyOrder();
                        wait(1);
                        SendKeys.SendWait("{PGUP}");
                        wait(1);
                        SendKeys.SendWait("{HOME}");
                    }

                    if (tradesShips)
                    {
                        terminalItemID = 2078;
                        SendKeys.SendWait("{PGDN}");
                        waitDuration = waitDuration * 3;
                        result = autoSeller(ref openOrders, 1, terminalItemID, ref buyOrdersCreated, ref sellOrdersCreated);
                        if (result == stopAllActivity)
                        {
                            waitDuration = autoSellerWaitDuration;
                            aOrder.clearLastBuyOrder();
                            SendKeys.SendWait("{PGDN}");
                            return stopAllActivity;
                        }
                        else if (result == 1)
                        {
                            waitDuration = autoSellerWaitDuration;
                            aOrder.clearLastBuyOrder();
                            SendKeys.SendWait("{PGDN}");
                            return 1;
                        }
                        waitDuration = waitDurationBackup;
                        aOrder.clearLastBuyOrder();
                        wait(1);
                        SendKeys.SendWait("{PGDN}");
                        wait(1);
                        SendKeys.SendWait("{HOME}");
                    }
                }
                stopwatch.Stop();
                excepListBox.Items.Add("Automatic order creation completed.");
                excepListBox.Items.Add("Created " + sellOrdersCreated + " sell orders and " + buyOrdersCreated + " buy orders in " + stopwatch.Elapsed.ToString());
            }
            if (runMode == 2)
                displayExceptions();
            return 0;
        }

        int autoSeller(ref int openOrders, int itemType, int terminalItemID, ref int buyOrdersCreated, ref int SellOrdersCreated)
        {
            string itemName;
            int typeID = 0;
            int[] cursorPosition = { 0, 0 };
            int offsetYModifier = 0;//When an item is a ship module, it can be fit to the current ship, which causes an extra row in the context menu. This offset jumps over the extra row (offset pulled from elementsXY when needed). 
            int readFailCounter;
            int copyFailCounter;
            double activeOrderCheck;//0 if there is no active order, 1 for active sell order, 2 for active buy order, 3 for active buy and sell, 4 if no orders to compare against
            double bestSellOrderPrice;
            double bestBuyOrderPrice;
            int buyOrderQuantity;
            int longOrderNameXOffset = 0;
            int longOrderNameYOffset = 0;
            int offsetFlag = 0;
            Modules modules = new Modules();


            string directory = path;

            cursorPosition[0] = element.itemsTop[0];
            cursorPosition[1] = element.itemsTop[1];
            consecutiveFailures = 0;
            wait(10);
            while (openOrders > 0)
            {
                if (stopCheck() == stopAllActivity)
                    return stopAllActivity;
                //debugexcepListBox.Items.Add("Open orders = " + openOrders);
                bestSellOrderPrice = bestBuyOrderPrice = readFailCounter = copyFailCounter = buyOrderQuantity = longOrderNameXOffset = longOrderNameYOffset = offsetFlag = 0;
                activeOrderCheck = -1;

                if (itemType == 0)
                {
                    offsetYModifier = element.itemsViewModuleDetailExtraOffset;
                }
                else
                {
                    offsetYModifier = 0;
                }


                if (Directory.GetFiles(directory).Length > 0)
                {
                    wipeDirectory(directory);
                }
                do
                {
                    if (activeOrderCheck == -1 && readFailCounter > 3)
                    {
                        //debugexcepListBox.Items.Add("Error Checking");
                        if (errorCheck() == stopAllActivity)
                            return stopAllActivity;
                    }
                    if ((activeOrderCheck == -1 || activeOrderCheck == -2)) //Try view details again
                    {
                        //RClick current line
                        wait(1);
                        pointCursor(cursorPosition[0], cursorPosition[1]);
                        rightClick(1,1);

                        //View details

                        autoSellerListBox.Items.Add("Modifier is" + offsetYModifier);//debug
                        autoSellerListBox.Items.Add("Y = " + (cursorPosition[1] + element.itemsViewDetailsOffset[1] + offsetYModifier));//debug
                        
                        pointCursor(cursorPosition[0] + element.itemsViewDetailsOffset[0], cursorPosition[1] + element.itemsViewDetailsOffset[1] + offsetYModifier);
                        leftClick(2,1);

                        //Alternate between offset and no offset
                        if (offsetYModifier == 0 && itemType == 0)
                        {
                            autoSellerListBox.Items.Add("Going to 21");//debug
                            offsetYModifier = element.itemsViewModuleDetailExtraOffset;
                        }
                        else if (itemType == 0)
                        {
                            autoSellerListBox.Items.Add("Going back to 0");//debug
                            offsetYModifier = 0;
                        }
                        if (readFailCounter == 8)//invert
                        {
                            if (offsetYModifier == 0 && itemType == 0)
                            {
                                autoSellerListBox.Items.Add("Going to 21");//debug
                                offsetYModifier = element.itemsViewModuleDetailExtraOffset;
                            }
                            else if (itemType == 0)
                            {
                                autoSellerListBox.Items.Add("Going back to 0");//debug
                                offsetYModifier = 0;
                            }
                        }
                    }
                    //Click on Export Market info
                    pointCursor(element.exportItem[0], element.exportItem[1]);
                    leftClick(5, 3);
                    activeOrderCheck = aOrder.scanOrder(ref orderSet, out bestSellOrderPrice, out bestBuyOrderPrice, out itemName, out typeID, ref path, ref terminalItemID, myStationID, fileNameTrimLength, ref offsetFlag);
                    ++readFailCounter;
                } while ((activeOrderCheck == -1 || activeOrderCheck == -2) && readFailCounter < 17);
                if (readFailCounter >= 17)
                {
                    ++consecutiveFailures;
                    exception.Add("Failed to check an item. Retry limit exceeded.");
                }
                else
                {
                    consecutiveFailures = 0;
                }
                if ((activeOrderCheck == -1 || activeOrderCheck == -2) && consecutiveFailures == 3)
                {
                    return 1;
                }
                else if (activeOrderCheck == -4)
                    return 0;
                //debug
                autoSellerListBox.Items.Add(itemName);
                autoSellerListBox.Items.Add(activeOrderCheck);
                autoSellerListBox.Items.Add(bestSellOrderPrice.ToString());
                autoSellerListBox.Items.Add(bestBuyOrderPrice.ToString());
                autoSellerListBox.Items.Add("");
                if (isItemOnList(ref longNameItemIDs, ref typeID))
                {
                    longOrderNameXOffset = 253;
                    longOrderNameYOffset = 22;
                }

                if (activeOrderCheck == 4)
                {
                    autoSellerListBox.Items.Add(itemName + " not processed. No orders exist for comparision.");
                    autoSellerListBox.Items.Add("");
                }
                offsetYModifier = 0;
                if (activeOrderCheck == 0 || activeOrderCheck == 1)//If a new buy order needs to be placed.
                {
                    double temp;
                    buyOrderQuantity = getBuyOrderQty(ref bestBuyOrderPrice, ref bestSellOrderPrice);
                    
                    if (buyOrderQuantity > 0)
                    {
                        temp = 0;
                        if (cancelOrder(0,0,typeID) == 0)
                        {
                            do
                            {
                                if (copyFailCounter % 4 == 0)
                                {
                                    if (errorCheck() == stopAllActivity)
                                        return stopAllActivity;
                                }
                           
                                //Click on place buy order
                                pointCursor(element.placeBuyOrder[0], element.placeBuyOrder[1]);
                                leftClick(1, 6);
                                leftClick(1, 6);

                                //Right click on the field
                                pointCursor(element.buyOrderBox[0], element.buyOrderBox[1] + longOrderNameYOffset);
                                rightClick(4,2);

                                //Click on copy
                                pointCursor(Cursor.Position.X + element.copyOffset[0], Cursor.Position.Y + element.copyOffset[1]);
                                leftClick(2, 2);
                                try
                                {
                                    temp = Convert.ToDouble(GetTextFromClip().ToString());
                                }
                                catch
                                {
                                    temp = 0;
                                }


                                //If this is the correct item. 
                                if ((temp - 1000) < bestSellOrderPrice && bestBuyOrderPrice < (temp + 1000))
                                {
                                    //Input buy order price.
                                    int modificationFailCount = 0;
                                    do
                                    {
                                        //Double click to highlight
                                        wait(4);
                                        pointCursor(element.buyOrderBox[0], element.buyOrderBox[1] + longOrderNameYOffset);
                                        doubleClick(2, 2);
                                        new SetClipboardHelper(DataFormats.Text, (bestBuyOrderPrice + .01).ToString()).Go();
                                        rightClick(2, 2);
                                        pointCursor(Cursor.Position.X + element.pasteOffset[0], Cursor.Position.Y + element.pasteOffset[1]);
                                        leftClick(2);
                                        new SetClipboardHelper(DataFormats.Text, "0").Go();

                                        ++modificationFailCount;
                                    } while (verifyNewBuyOrderInput(bestBuyOrderPrice, out lastOrderModified, ref longOrderNameYOffset) == false && modificationFailCount < 10);

                                    if (modificationFailCount == 10)
                                    {
                                        temp = 0;
                                    }
                                    if (offsetFlag == 1)
                                        offsetYModifier = 15;
                                    //Input buy order quantity.
                                    if (lastOrderModified)
                                    {
                                        ++buyOrdersCreated;
                                        modificationFailCount = 0;
                                        do
                                        {
                                            //Double click to highlight
                                            wait(4);
                                            pointCursor(element.buyOrderQtyBox[0], element.buyOrderQtyBox[1] + longOrderNameYOffset - offsetYModifier);
                                            doubleClick(6, 1);
                                            doubleClick(1, 4);
                                            SendKeys.SendWait(buyOrderQuantity.ToString());

                                            ++modificationFailCount;
                                        } while (verifyQuantityInput(ref buyOrderQuantity, out lastOrderModified, (longOrderNameYOffset - offsetYModifier)) == false && modificationFailCount < 10);

                                        if (modificationFailCount == 10)
                                        {
                                            temp = 0;
                                        }
                                    }
                                    }
                                else
                                {
                                    temp = 0;
                                }
                                ++copyFailCounter;
                            } while (string.Compare(Convert.ToString(temp), "0") == 0 && copyFailCounter < 10);
                        }
                    }
                    else
                    {
                        autoSellerListBox.Items.Add(itemName + " not purchased. Item no longer exceeds minimum profit threshhold.");
                        autoSellerListBox.Items.Add(bestSellOrderPrice.ToString());
                        autoSellerListBox.Items.Add(bestBuyOrderPrice.ToString());
                        autoSellerListBox.Items.Add("");
                    }
                    new SetClipboardHelper(DataFormats.Text, "0").Go();
                }
                if (lastOrderModified)
                {
                    if (confirmOrder(longOrderNameXOffset, longOrderNameYOffset, typeID, 0, 0) == stopAllActivity)
                        return stopAllActivity;
                    --openOrders;
                    lastOrderModified = false;
                }
                else
                {
                    autoSellerListBox.Items.Add("Failed to buy item " + itemName);
                }
                if (activeOrderCheck == 0 || activeOrderCheck == 2)//If a new sell order needs to be placed.
                {
                    copyFailCounter = 0;
                    if (itemType == 0 && !modules.isFittable(typeID))
                        offsetYModifier = element.itemsViewModuleDetailExtraOffset;
                    else
                        offsetYModifier = 0;

                    double temp;
                    if (errorCheck() == stopAllActivity)
                        return stopAllActivity;
                    if (cancelOrder(0, 0, 0) == 0)
                    {
                        do
                        {
                            temp = 0;
                            //RClick on current line.
                            pointCursor(cursorPosition[0], cursorPosition[1]);
                            rightClick(1, 1);

                            //Click on Sell
                            pointCursor(cursorPosition[0] + element.itemsSellItemOffset[0], cursorPosition[1] + element.itemsSellItemOffset[1] + offsetYModifier);
                            leftClick(1, 1);

                            if (copyFailCounter % 3 == 2)
                            {
                                waitDuration += waitDurationBackup;
                            }
                            if (copyFailCounter % 4 == 3)
                            {
                                if (offsetYModifier == 0 && itemType == 0)
                                {
                                    offsetYModifier = element.itemsViewModuleDetailExtraOffset;
                                }
                                else if (itemType == 0)
                                {
                                    offsetYModifier = 0;
                                }
                            }

                            //Right click on the field
                            wait(5);
                            pointCursor(element.sellOrderBox[0], element.sellOrderBox[1] + longOrderNameYOffset);
                            rightClick(2, 2);

                            //Click on copy
                            pointCursor(Cursor.Position.X + element.copyOffset[0], Cursor.Position.Y + element.copyOffset[1]);
                            leftClick(2, 2);
                            try
                            {
                                temp = Convert.ToDouble(GetTextFromClip().ToString());
                            }
                            catch
                            {
                                temp = 0;
                            }

                            if (waitDuration != autoSellerWaitDuration)
                                waitDuration = autoSellerWaitDuration;

                            if ((temp - 1000) < bestBuyOrderPrice && bestBuyOrderPrice < (temp + 1000))
                            {
                                int modificationFailCount = 0;
                                do
                                {
                                    //Double click to highlight
                                    wait(4);
                                    pointCursor(element.sellOrderBox[0], element.sellOrderBox[1] + longOrderNameYOffset);
                                    doubleClick(2, 2);
                                    new SetClipboardHelper(DataFormats.Text, (bestSellOrderPrice - .01).ToString()).Go();
                                    rightClick(2, 2);
                                    pointCursor(Cursor.Position.X + element.pasteOffset[0], Cursor.Position.Y + element.pasteOffset[1]);
                                    leftClick(2);
                                    new SetClipboardHelper(DataFormats.Text, "0").Go();

                                    ++modificationFailCount;
                                } while (verifyNewSellOrderInput(bestSellOrderPrice, out lastOrderModified, longOrderNameYOffset) == false && modificationFailCount < 10);
                                if (modificationFailCount == 10)
                                {
                                    temp = 0;
                                }
                            }
                            else
                            {
                                temp = 0;
                            }
                            ++copyFailCounter;
                        } while (string.Compare(Convert.ToString(temp), "0") == 0 && copyFailCounter < 6);
                    }
                    new SetClipboardHelper(DataFormats.Text, "0").Go();
                    waitDuration = autoSellerWaitDuration;
                    leftClick(7);

                    if (lastOrderModified)
                    {
                        ++SellOrdersCreated;
                        cursorPosition[1] = cursorPosition[1] - element.itemsLineHeight;
                        if (confirmOrder(longOrderNameXOffset, longOrderNameYOffset, typeID, 0, 0) == stopAllActivity)
                            return stopAllActivity;
                        --openOrders;
                    }
                }
                cursorPosition[1] = cursorPosition[1] + element.itemsLineHeight;
            }
            return 0;
        }

        int getBuyOrderQty(ref double bestBuyOrderPrice, ref double bestSellOrderPrice)
        {
            int i;

            if (bestSellOrderPrice / bestBuyOrderPrice < 1.0736)
            {
                return -1;
            }
            else
            {


                for (i = 0; i < 4; ++i)
                {
                    if (bestBuyOrderPrice < quantityMatrix[i, 0])
                    {
                        return quantityMatrix[i, 1];
                    }
                }
            }
            return quantityMatrix[i, 1];
        }
        int errorCheck()
        {
            pointCursor(element.errorCheck[0], element.errorCheck[1]);
            leftClick(2);
            return stopCheck();
        }

        int stopCheck()
        {
            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName == "taskmgr")
                    return stopAllActivity;
            }
            return 0;
        }

        int confirmErrorCheck()
        {
            pointCursor(element.confirmErrorCheck[0], element.confirmErrorCheck[1]);
            leftClick(2);
            return stopCheck();
        }

        private int modOrdersPrep(bool multiUserMode)
        {

            int sellResult = 0;
            int buyResult = 0;
            bool launchEVEWhenNearlyDone = false;

            excepListBox.Items.Add("Modifying orders...");
            if (!isEVERunning())
            {
                excepListBox.Items.Add("Failed to modify orders. Could not find EVE.");
                return 1;
            }
            else
            {
                int iterations = runsToMake;
                waitDuration = waitDurationBackup;
                for (int i = 0; i < iterations; ++i)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    SetForegroundWindow(eveHandle);
                    ++run;

                    this.Cursor = new Cursor(Cursor.Current.Handle);
                    int failCount = 0;
                    wipeDirectory(path);
                    do
                    {
                        wait(20);
                        int result;
                        try
                        {
                            result = exportOrders(element);//Clicks on export orders.
                        }
                        catch
                        {
                            result = 1;
                        }
                        if (result == 1)
                        {
                            ++failCount;
                            errorCheck();
                            wait(20);
                        }
                        else
                        {
                            failCount = 4;
                        }
                    } while (failCount < 3);

                    //SendKeys.SendWait("^(V)");

                    if (failCount == 4)
                    {
                        wait(30);
                        pointCursor(element.buySortByType[0], element.buySortByType[1]);
                        leftClick(1, 30);
                        pointCursor(element.sellSortByType[0], element.sellSortByType[1]);
                        leftClick(1);
                        if (adjustBuys)
                        {
                            if (multiUserMode && i == iterations - 1 && !adjustSells)
                                launchEVEWhenNearlyDone = true;
                            if ((buyResult = modOrders(ref element, ref element.buyTop, ref element.buySortByType, ref element.visLines, 1, ref numModified, ref numScanned, launchEVEWhenNearlyDone)) == stopAllActivity)
                            {
                                aOrder.clearLastBuyOrder();
                                SendKeys.SendWait("{HOME}");
                                return stopAllActivity;
                            }
                            else if (buyResult != 0)
                            {
                                aOrder.clearLastBuyOrder();
                                SendKeys.SendWait("{HOME}");
                                return buyResult;
                            }
                            aOrder.clearLastBuyOrder();
                            SendKeys.SendWait("{HOME}");
                        }
                        if (adjustSells)
                        {
                            if (multiUserMode && i == iterations - 1)
                                launchEVEWhenNearlyDone = true;
                            if ((sellResult = modOrders(ref element, ref element.sellTop, ref element.sellSortByType, ref element.visLines, 0, ref numModified, ref numScanned, launchEVEWhenNearlyDone)) == stopAllActivity)
                            {
                                aOrder.clearLastBuyOrder();
                                SendKeys.SendWait("{HOME}");
                                return stopAllActivity;
                            }
                            else if (sellResult != 0)
                            {
                                aOrder.clearLastBuyOrder();
                                SendKeys.SendWait("{HOME}");
                                return sellResult;
                            }
                            aOrder.clearLastBuyOrder();
                            SendKeys.SendWait("{HOME}");
                        }
                    }
                    stopwatch.Stop();

                    ListViewItem item = runsStatsListView.Items.Add(run.ToString());
                    item.SubItems.Add(numScanned.ToString());
                    item.SubItems.Add(numModified.ToString());
                    item.SubItems.Add(stopwatch.Elapsed.ToString());



                    if (sellResult != 0 && buyResult != 0)
                        excepListBox.Items.Add("Failed to complete buy run. Failed to complete sell run.");
                    else if (sellResult != 0)
                        excepListBox.Items.Add("Failed to complete sell run.");
                    else if (buyResult != 0)
                        excepListBox.Items.Add("Failed to complete buy run.");
                    else
                    {
                        excepListBox.Items.Add("Modifications completed successfully.");
                        if (runMode == 1)
                            displayExceptions();
                    }
                    excepListBox.Items.Add("Scanned " + numScanned + " items and made " + numModified + " modifications in " + stopwatch.Elapsed.ToString());
                    numScanned = numModified = 0;
                }
                return 0;

            }
        }

        //Try setting orders remotely to reduce lag
        //TODO When the GUI throws an error after editing the last item before a scrolling action, the scrolling action doesn't happen because the warning check
        //occurs after the scrolling action.
        //Returns 0 if successful.
        //Returns 1 if it can't find client.
        //returns 2 if it hangs on order export.
        int modOrders(ref elementsXY element, ref int[] topLine, ref int[] sortByType, ref int[] visLines, int orderType, ref int numModified, ref int numScanned, bool launchNewEVEWhenNearlyDone)
        {

            int typeID = 0;
            int lastTypeID = 0;
            int[] cursorPosition = { 0, 0 };
            int[] activeOrders = orderSet.getNumOfActiveOrders();
            double modifyTo;
            double originalPrice;
            double temp;
            int offset = element.visLines[orderType];
            int readFailCounter;
            int copyFailCounter;
            string directory = path;
            int modificationFailCount = 0;

            consecutiveFailures = 0;

            int ceiling = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(activeOrders[orderType]) / Convert.ToDouble(element.visLines[orderType])));
            for (int j = 0; j < ceiling; ++j)
            {
                cursorPosition[0] = topLine[0];
                cursorPosition[1] = topLine[1];
                int leftToScan = activeOrders[orderType];
                if (j == (ceiling - 1)) //This is the final iteration, so don't go all the way down the list.
                {
                    offset = (activeOrders[orderType] % element.visLines[orderType]);
                }
                for (int i = 0; i < offset; ++i)
                {
                    if (stopCheck() == stopAllActivity)
                        return stopAllActivity;
                    if (launchNewEVEWhenNearlyDone && --leftToScan < 18)
                    {
                        launchNewEVEWhenNearlyDone = false;
                        launchEVE();
                    }
                    lastTypeID = typeID;
                    if (eveHandle != GetForegroundWindow())
                    {
                        SetForegroundWindow(eveHandle);
                    }
                    originalPrice = copyFailCounter = readFailCounter = 0;
                    modifyTo = -1;

                    if (Directory.GetFiles(directory).Length > 0)
                    {
                        wipeDirectory(directory);
                    }
                    do
                    {
                        if (modifyTo == -1 && readFailCounter > 9 && (readFailCounter % 4) == 0)
                        {
                            if (lastOrderModified)
                            {
                                //debugexcepListBox.Items.Add("AConfirming " + typeID);
                                if (confirmOrder(0, 0, typeID, 1, orderType) == stopAllActivity)
                                    return stopAllActivity;
                            }
                            if (errorCheck() == stopAllActivity)
                                return stopAllActivity;
                        }
                        if ((modifyTo == -1 || modifyTo == -2) && readFailCounter % 4 == 0) //Try view details again
                        {
                            //RClick current line
                            pointCursor(cursorPosition[0], cursorPosition[1]);
                            doubleClick(1);//View market details
                        }
                        //Click on Export Market info
                        pointCursor(element.exportItem[0], element.exportItem[1]);
                        leftClick(3, 1);
                        modifyTo = aOrder.bestOrder(ref orderSet, ref orderType, ref run, ref modifiedOrdersListView, ref originalPrice, ref path, ref typeID, fileNameTrimLength, ref exception);

                        if ((readFailCounter % 11) == 0)
                        {
                            waitDuration += waitDurationBackup;
                        }
                        ++readFailCounter;
                    } while ((modifyTo == -1 || modifyTo == -2) && readFailCounter < 29);
                    if (waitDuration != waitDurationBackup)
                    {
                        waitDuration = waitDurationBackup;
                    }
                    if (readFailCounter == 29)
                    {
                        consecutiveFailures++;
                        exception.Add("Failed to modify an item. Exceeded retry limit");
                    }
                    else
                        consecutiveFailures = 0;
                    if (consecutiveFailures == 3)
                    {
                        return 1;
                    }
                    if (modifyTo == -1)
                    {
                        return 2;
                    }
                    if (lastOrderModified)
                    {
                        //debugexcepListBox.Items.Add("BConfirming " + lastTypeID);
                        if (confirmOrder(0, 0, lastTypeID, 1, orderType) == stopAllActivity)
                            return stopAllActivity;
                    }
                    /*if (modifyTo > 0)//If we're going to make a modification, close the window to prevent refresh lag.
                    {
                        //Close the market window
                        pointCursor(element.closeMarketWindow[0], element.closeMarketWindow[1]);
                        leftClick(5);
                    }*/
                    if (modifyTo > 0)
                    {
                        do
                        {
                            //RClick on current line.
                            pointCursor(cursorPosition[0], cursorPosition[1]);
                            rightClick(1,1);

                            //Click on Modify
                            pointCursor(cursorPosition[0] + element.modifyOffset[0], cursorPosition[1] + element.modifyOffset[1]);
                            leftClick(1,1);

                            //Right click on the field
                            pointCursor(element.modifyOrderBox[0], element.modifyOrderBox[1]);
                            rightClick(4,1);

                            //Click on copy
                            pointCursor(Cursor.Position.X + element.copyOffset[0], Cursor.Position.Y + element.copyOffset[1]);
                            leftClick(1, 1);
                            try
                            {
                                temp = Convert.ToDouble(GetTextFromClip().ToString());
                            }
                            catch
                            {
                                temp = 0;
                            }

                            if ((temp - 10000) < originalPrice && originalPrice < (temp + 10000))
                            {
                                modificationFailCount = 0;
                                do
                                {
                                    if (modificationFailCount % 2 == 1)
                                    {
                                        waitDuration += waitDurationBackup;
                                    }
                                    //Double click to highlight
                                    pointCursor(element.modifyOrderBox[0], element.modifyOrderBox[1]);
                                    doubleClick(2, 2);
                                    new SetClipboardHelper(DataFormats.Text, modifyTo.ToString()).Go();
                                    rightClick(2, 2);
                                    pointCursor(Cursor.Position.X + element.pasteOffset[0], Cursor.Position.Y + element.pasteOffset[1]);
                                    leftClick(2);
                                    new SetClipboardHelper(DataFormats.Text, "0").Go();
                                    lastOrderModified = true;
                                    ++modificationFailCount;
                                    copyFailCounter = 10;
                                } while (verifyModifyToInput(modifyTo, temp) == false && modificationFailCount < 10);
                                waitDuration = waitDurationBackup;
                                if (modificationFailCount == 10)
                                {
                                    cancelOrder(0, 0, typeID);
                                    temp = 0;
                                }
                                else
                                {
                                    ++numModified;
                                }
                            }
                            else
                            {
                                temp = 0;
                            }
                            ++copyFailCounter;
                            if (copyFailCounter % 5 == 4)
                            {
                                waitDuration += waitDurationBackup;
                            }
                        } while (string.Compare(Convert.ToString(temp), "0") == 0 && copyFailCounter < 10);
                        new SetClipboardHelper(DataFormats.Text, "0").Go();
                    }
                    if (waitDuration != waitDurationBackup)
                    {
                        waitDuration = waitDurationBackup;
                    }
                    if (copyFailCounter == 10 && modificationFailCount == 10)
                    {
                        exception.Add("Order of price " + originalPrice + " not adjusted. Failed to make modification.");

                    }
                    //Get next line
                    ++numScanned;
                    cursorPosition[1] = cursorPosition[1] + element.lineHeight;
                }
                if (j == (ceiling - 2))
                {
                    pointCursor(cursorPosition[0], cursorPosition[1] - element.lineHeight);
                    leftClick(40, 20);
                    for (int l = 0; l < activeOrders[orderType]; ++l)
                    {
                        SendKeys.SendWait("{UP}");
                    }
                    pointCursor(sortByType[0], sortByType[1]);
                    leftClick(20, 20);
                }
                else if (j < (ceiling - 1))
                {
                    pointCursor(cursorPosition[0], cursorPosition[1] - element.lineHeight);
                    leftClick(40, 20);
                    for (int k = 0; k < element.visLines[orderType]; ++k)
                    {
                        SendKeys.SendWait("{DOWN}");

                    }
                }

            }
            if (lastOrderModified)
            {
                if (confirmOrder(0, 0, typeID, 1, orderType) == stopAllActivity)
                    return stopAllActivity;
            }
            return 0;
        }
        private bool verifyModifyToInput(double modifyTo, double originalPrice)
        {
            double temp;
            //Right click on the field
            wait(1);
            pointCursor(element.modifyOrderBox[0], element.modifyOrderBox[1]);
            rightClick(1,1);

            //Click on copy
            pointCursor(Cursor.Position.X + element.copyOffset[0], Cursor.Position.Y + element.copyOffset[1]);
            leftClick(1, 1);


            try { temp = Convert.ToDouble(GetTextFromClip().ToString()); }
            catch { temp = -1; }

            if (temp - 10000 < modifyTo && modifyTo < temp + 10000 && (temp < originalPrice - .01 || temp > originalPrice + .01))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool verifyNewBuyOrderInput(double desiredInput, out bool lastOrderModified, ref int longOrderNameYOffset)
        {
            double temp;

            //Right click on the field
            wait(1);
            pointCursor(element.sellOrderBox[0], element.sellOrderBox[1] + longOrderNameYOffset);
            rightClick(1,1);

            //Click on copy
            Cursor.Position = new Point(Cursor.Position.X + element.copyOffset[0], Cursor.Position.Y + element.copyOffset[1]);
            leftClick(1, 1);

            try { temp = Convert.ToDouble(GetTextFromClip().ToString()); }
            catch { temp = -1; }

            if (desiredInput - 10000 < temp && temp < desiredInput + 10000 && (temp != desiredInput))
            {
                lastOrderModified = true;
                return true;
            }
            else
            {
                lastOrderModified = false;
                return false;
            }
        }

        private bool verifyNewSellOrderInput(double desiredInput, out bool lastOrderModified, int longOrderNameYOffset)
        {
            //Right click on the field
            wait(1);
            pointCursor(element.sellOrderBox[0], element.sellOrderBox[1] + longOrderNameYOffset);
            rightClick(1,1);

            //Click on copy
            pointCursor(Cursor.Position.X + element.copyOffset[0], Cursor.Position.Y + element.copyOffset[1]);
            leftClick(1, 1);
            double temp = Convert.ToDouble(GetTextFromClip().ToString());
            if (desiredInput - 10000 < temp && temp < desiredInput + 10000 && (temp != desiredInput))
            {
                lastOrderModified = true;
                return true;
            }
            else
            {
                lastOrderModified = false;
                return false;
            }
        }

        private bool verifyQuantityInput(ref int quantity, out bool lastOrderModified, int longOrderNameYOffset)
        {
            //Right click on the field
            wait(1);
            pointCursor(element.buyOrderQtyBox[0], element.buyOrderQtyBox[1] + longOrderNameYOffset);
            rightClick(1,1);

            //Click on copy
            pointCursor(Cursor.Position.X + element.copyOffset[0], Cursor.Position.Y + element.copyOffset[1]);
            leftClick(1, 2);
            int temp;
            try { temp = Convert.ToInt32(GetTextFromClip().ToString()); }
            catch { temp = -1; }

            if (temp == quantity)
            {
                lastOrderModified = true;
                return true;
            }
            else
            {
                lastOrderModified = false;
                return false;
            }
        }

        public static string GetTextFromClip()
        {
            try
            {
                IDataObject dataObj = Clipboard.GetDataObject();
                return dataObj.GetData(DataFormats.Text).ToString();
            }
            catch
            {
                return "0";
            }


        }



        private void playAlertSound()
        {
            SoundPlayer simpleSound = new SoundPlayer(@"c:\Windows\Media\Windows Critical Stop.wav");
            simpleSound.Play();
        }

        private void leftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        private void leftClick(int before, int after)
        {
            wait(before);
            leftClick();
            wait(after);
        }

        private void leftClick(int before)
        {
            wait(before);
            leftClick();
        }

        private void doubleClick()
        {
            leftClick();
            leftClick();
        }

        private void doubleClick(int before, int after)
        {
            wait(before);
            doubleClick();
            wait(after);
        }

        private void doubleClick(int before)
        {
            wait(before);
            doubleClick();
        }

        private void rightClick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }

        private void rightClick(int before, int after)
        {
            wait(before);
            rightClick();
            wait(after);
        }

        private void rightClick(int before)
        {
            wait(before);
            rightClick();
        }

        private void wait(int multiplier)
        {
            Thread.Sleep(waitDuration * multiplier);
        }

        private void pointCursor(int x, int y)
        {
            Cursor.Position = new Point(x, y);
        }

        int exportOrders(elementsXY element)
        {
            errorCheck();
            pointCursor(element.exportOrderList[0], element.exportOrderList[1]);
            leftClick(2);

            string fileName;

            var directory = new DirectoryInfo(path);
            var fileTemp = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            StreamReader file;

            try
            {
                fileTemp = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
                file = new System.IO.StreamReader(directory.ToString() + fileTemp.ToString());
                
            }
            catch
            {
                return 1;
            }
            fileName = fileTemp.ToString();
            orderSet = new orderManager(directory.ToString() + fileName, ref file);

            file.Close();
            return 0;
        }

        private void wipeDirectory(string directory)
        {
            var directoryVar = new DirectoryInfo(directory);
            StreamReader file;

            while (Directory.GetFiles(directory).Length > 0)
            {
            TryLabel:
                try
                {
                    var fileTemp = directoryVar.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
                    file = new System.IO.StreamReader(directory.ToString() + fileTemp.ToString());
                    file.Close();
                    File.Delete(directory.ToString() + fileTemp.ToString());
                }
                catch
                {
                    goto TryLabel;
                }
            }
        }

        bool isItemOnList(ref int[] listToCheck, ref int typeID)
        {
            int length = listToCheck.Length;

            for (int i = 0; i < length; ++i)
            {
                if (listToCheck[i] == typeID)
                {
                    return true;
                }
            }
            return false;
        }


        private void saveSettingsButton_Click(object sender, EventArgs e)
        {
            waitDuration = waitDurationBackup = Convert.ToInt32(timingTextBox.Text);
            runsToMake = Convert.ToInt32(nTimesTextBox.Text);
            path = pathTextBox.Text;
            EVEPath = EVEPathTextbox.Text;
            saveLastUsedSettings();

            populateQtyMatrix();

            saveUserSettings();

        }

        private int populateQtyMatrix()
        {
            quantityMatrix[0, 0] = Convert.ToInt32(threshHoldTextbox1.Text);
            quantityMatrix[1, 0] = Convert.ToInt32(threshHoldTextbox2.Text);
            quantityMatrix[2, 0] = Convert.ToInt32(threshHoldTextbox3.Text);
            quantityMatrix[3, 0] = Convert.ToInt32(threshHoldTextbox4.Text);
            quantityMatrix[4, 0] = Convert.ToInt32(threshHoldTextbox5.Text);
            quantityMatrix[0, 1] = Convert.ToInt32(qtyTextbox1.Text);
            quantityMatrix[1, 1] = Convert.ToInt32(qtyTextbox2.Text);
            quantityMatrix[2, 1] = Convert.ToInt32(qtyTextbox3.Text);
            quantityMatrix[3, 1] = Convert.ToInt32(qtyTextbox4.Text);
            quantityMatrix[4, 1] = Convert.ToInt32(qtyTextbox5.Text);
            return 0;
        }

        private void stationsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            myStationID = stationsListBox.SelectedItem.ToString();
            if (Convert.ToInt32(myStationID) == 60003760)
            {
                fileNameTrimLength = 10;
            }
            else if (Convert.ToInt32(myStationID) == 60008494)
            {
                fileNameTrimLength = 7;
            }
            else if (Convert.ToInt32(myStationID) == 60011179)
            {
                fileNameTrimLength = 11;
            }
            else if (Convert.ToInt32(myStationID) == 60011866)
            {
                fileNameTrimLength = 12;
            }
           
        }

        private void prototypeButton_Click(object sender, EventArgs e)
        {
            launchEVEPrep();
        }

        private int launchEVEPrep()
        {
            if (errorCheck() == stopAllActivity)
                return stopAllActivity;
            if (isSelectedCharacterLoggedIn() && extendedTryToExportOrders() == 0 && setConfirmationValue() == 0)
            {
                return 0;
            }
            int result = launchEVEStateUnknown();

            switch (result)
            {
                case 1:
                    excepListBox.Items.Add("Error logging in. Failed at or before login screen.");
                    break;
                case 2:
                    excepListBox.Items.Add("Error logging in. Failed at character selection.");
                    break;
                case 3:
                    excepListBox.Items.Add("Error logging in. Failed at character selection.");
                    break;
                case 4:
                    excepListBox.Items.Add("Error logging in. Failed to set confirmatino value.");
                    break;
                case -69:
                    return stopAllActivity;
            }
            return 0;
        }

        private int killProcess(string processName)
        {
            try
            {
                Process[] proc = Process.GetProcessesByName(processName);
                foreach (Process p in proc)
                {
                    p.Kill();
                }
            }
            catch
            {
                excepListBox.Items.Add("Could not kill EVE");
                return 1;
            }
            return 0;
        }

        private int closeItemsAndMarketWindows()
        {
            pointCursor(element.closeMarketWindow[0], element.closeMarketWindow[1]);
            leftClick(5,5);
            pointCursor(element.closeItems[0], element.closeItems[1]);
            leftClick(1);
            return 0;
        }

        private int setConfirmationValue()
        {
            wait(2);
            new SetClipboardHelper(DataFormats.Text, "0").Go();
            wait(2);
            int temp = 1;
            for (int i = 0; i < 15; ++i)
            {
                temp = confirmOrder(0, 0, 0, 0, 0);
                if (temp == 0)//Success!
                {
                    return 0;
                }
                else if (temp == stopAllActivity)
                {
                    return stopAllActivity;
                }
                else
                {
                    pointCursor(element.OrderBoxOK[0], element.OrderBoxOK[1]);
                    leftClick(1, 1);
                    SendKeys.SendWait("-1");
                }
            }
            return 1;//Failure
        }

        private int getError()
        {
            string message;
            pointCursor(element.parseErrorMessage[0], element.parseErrorMessage[1]);
            rightClick(1, 1);
            pointCursor(Cursor.Position.X + element.parseErrorMessageCopyOffset[0], Cursor.Position.Y + element.parseErrorMessageCopyOffset[1]);
            leftClick(1, 1);
            message = GetTextFromClip().ToString();

            if (string.Compare(message, "0") != 0)
                        return errorParser.parse(message);
            return 0;
        }

        private int confirmOrder(int longOrderXoffset, int longOrderYOffset, int typeID, int confirmationType, int buyOrSell)
        {
            int failCount = 0;
            string temp = "0";
            int errorFlag = 0;
            do
            {
                wait(1);
                pointCursor(element.OrderBoxOK[0], element.OrderBoxOK[1] + longOrderYOffset);
                leftClick(1, 1);
                if (confirmationType == 1 && failCount > 3)
                {
                    errorFlag = getError();
                    //If the error is 'above regional average' and this is a sell order || it is below/buy
                    if (errorFlag == 1 && buyOrSell == 0 || errorFlag == 2 && buyOrSell == 1)
                    {
                        if (confirmErrorCheck() == stopAllActivity)
                            return stopAllActivity;
                        wait(1);
                        if (confirmErrorCheck() == stopAllActivity)
                            return stopAllActivity;
                    }
                    else
                    {
                        if (errorCheck() == stopAllActivity)
                            return stopAllActivity;
                        wait(1);
                        if (errorCheck() == stopAllActivity)
                            return stopAllActivity;
                    }
                    new SetClipboardHelper(DataFormats.Text, "0").Go();
                }

                //Right click where OK should no longer exist. 
                pointCursor(element.OrderBoxOK[0] + longOrderXoffset, element.OrderBoxOK[1]);
                rightClick(1, 1);

                //Click on copy
                pointCursor(Cursor.Position.X + element.confirmationCopyOffset[0], Cursor.Position.Y + element.confirmationCopyOffset[1]);
                leftClick(1, 1);

                temp = GetTextFromClip().ToString();
                ++failCount;
            } while (string.Compare(temp, "0") == 0 && failCount < 9);
            lastOrderModified = false;
            if (string.Compare(temp, "0") != 0)
            {
                new SetClipboardHelper(DataFormats.Text, "0").Go();
                return 0;
            }
            else
                return 1;
        }
        //TODO, merge with confirmOrder
        private int cancelOrder(int longOrderXoffset, int longOrderYOffset, int typeID)
        {
            int failCount = 0;
            string temp = "0";
            do
            {
                wait(1);
                pointCursor(element.OrderBoxCancel[0], element.OrderBoxCancel[1] + longOrderYOffset);
                leftClick(1, 1);


                if (failCount > 0 && failCount % 3 == 0)
                {
                    if (!isItemOnList(ref skipErrorCheckItemIDs, ref typeID))
                    {
                        if (errorCheck() == stopAllActivity)
                            return stopAllActivity;
                    }
                    else
                    {
                        if (confirmErrorCheck() == stopAllActivity)
                            return stopAllActivity;
                    }
                }

                //Right click where OK should no longer exist. 
                pointCursor(element.OrderBoxCancel[0] + longOrderXoffset, element.OrderBoxCancel[1]);
                rightClick(1, 1);

                //Click on copy
                pointCursor(Cursor.Position.X + element.confirmationCopyOffset[0], Cursor.Position.Y + element.confirmationCopyOffset[1]);
                leftClick(1, 1);

                temp = GetTextFromClip().ToString();
                ++failCount;
            } while (string.Compare(temp, "0") == 0 && failCount < 9);
            lastOrderModified = false;
            if (string.Compare(temp, "0") != 0)
            {
                new SetClipboardHelper(DataFormats.Text, "0").Go();
                return 0;
            }
            else
                return 1;
        }

        private int launchEVEStateUnknown()
        {
            eveHandle = FindWindow("triuiScreen", "EVE");
            Process[] proc = Process.GetProcessesByName("EXEFile");
            if (!(proc.Count() == 1 && eveHandle != IntPtr.Zero))
            {
                killProcess("EXEFile");
                launchEVE();
            }
            int result = 1;

            if (login() != 0)
                return 1;
            if (errorCheck() == stopAllActivity)
                return stopAllActivity;
            if (loginStage2() != 0)
                return 2;
            if (errorCheck() == stopAllActivity)
                return stopAllActivity;
            if (extendedTryToExportOrders() != 0)
                return 3;
            if (errorCheck() == stopAllActivity)
                return stopAllActivity;

            result = setConfirmationValue();

            if (result == stopAllActivity)
                return stopAllActivity;
            else if (result == 1)
                return 4;

            //closeItemsAndMarketWindows();

            excepListBox.Items.Add("Success! Logged in as " + selectedCharacter);
            return 0;
        }

        private int launchEVE()
        {
            Process.Start(EVEPath);
            return 0;
        }

        private int login()
        {
            int failCount = 0;
            eveHandle = IntPtr.Zero; 
            while (eveHandle == IntPtr.Zero && failCount < 60)
            {
            eveHandle = FindWindow("triuiScreen", "EVE");
            Thread.Sleep(1000);
            failCount++;
            }
            if (eveHandle == IntPtr.Zero)
                return 1;
            new SetClipboardHelper(DataFormats.Text, "0").Go();
            errorCheck();
            for (int i = 0; i < 25; ++i)
            {
                if (stopCheck() == stopAllActivity)
                    return stopAllActivity;
                SetForegroundWindow(eveHandle);
                killProcess("Chrome");
                pointCursor(element.loginScreenUserName[0], element.loginScreenUserName[1]);
                rightClick(10, 2);
                pointCursor(Cursor.Position.X + element.modifyOffset[0], Cursor.Position.Y + element.modifyOffset[1]);
                leftClick(10, 2);
                if (GetTextFromClip().ToString().CompareTo("0") != 0)
                {
                    pointCursor(element.loginScreenUserName[0], element.loginScreenUserName[1]);
                    doubleClick(2, 2);
                    doubleClick(2, 2);
                    SendKeys.SendWait(getLoginText());
                    wait(1);
                    pointCursor(element.loginScreenUserName[0], element.loginScreenUserName[1] + 15);
                    doubleClick(2, 2);
                    doubleClick(2, 2);
                    wait(1);
                    //debugexcepListBox.Items.Add(p);
                    SendKeys.SendWait(p);
                    pointCursor(element.loginScreenConnect[0], element.loginScreenConnect[1]);
                    leftClick(1, 1);

                    return 0;
                }
            }
            return 1;
        }

        private int loginStage2()
        {
            int result = 1;
            int errorFlag = 0;
            for (int i = 0; i < 20; ++i)
            {
                if (stopCheck() == stopAllActivity)
                    return stopAllActivity;
                SetForegroundWindow(eveHandle);
                result = loginCharacterSelect();
                if (result == 0)
                {
                    killProcess("Chrome");
                    return 0;
                }
                errorFlag = getError();
                if (errorFlag != 0)
                {
                    errorCheck();
                    doubleClick();
                    Thread.Sleep(1000);
                    return 1;
                }

            }
            return 1;
        }

        private int loginCharacterSelect()
        {
            wait(200);
            if (pixelReader.checkForTarget(loginColors[characterListBox.SelectedItem.ToString()]))
            {
                //pick this character if it is the right one.
                pointCursor(element.loginStage2ActiveCharacter[0], element.loginStage2ActiveCharacter[1]);
                leftClick(10, 2);
                return 0;
            }
                
            //select alt1
            pointCursor(element.loginStage2Alt1[0], element.loginStage2Alt1[1]);
            leftClick(10, 2);
            wait(200);

            if (pixelReader.checkForTarget(loginColors[characterListBox.SelectedItem.ToString()]))
            {
                //pick this character if it is the right one.
                pointCursor(element.loginStage2ActiveCharacter[0], element.loginStage2ActiveCharacter[1]);
                leftClick(10, 2);
                return 0;
            }
            //Select alt2
            pointCursor(element.loginStage2Alt2[0], element.loginStage2Alt2[1]);
            leftClick(10, 2);
            wait(200);

            //Check new character
            if (pixelReader.checkForTarget(loginColors[characterListBox.SelectedItem.ToString()]))
            {
                //pick this character if it is the right one.
                pointCursor(element.loginStage2ActiveCharacter[0], element.loginStage2ActiveCharacter[1]);
                leftClick(10, 2);
                return 0;
            }
            return 1;
        }



        private int extendedTryToExportOrders()
        {

            int result;
            wipeDirectory(path);
            for (int i = 0; i < 20; i++)
            {
                pointCursor(element.loginStage2ActiveCharacter[0], element.loginStage2ActiveCharacter[1]);
                leftClick(10, 2);
                try
                {
                    result = exportOrders(element);//Clicks on export orders.
                }
                catch
                {
                    result = 1;
                    errorCheck();
                }
                if (result == 0)
                    return 0;
                Thread.Sleep(1000);
            }
            return 1;
        }

        private string getLoginText()
        {
            for (int i = 0; i < 4; i++)
            {
                if (characterListBox.SelectedItem.ToString().CompareTo(l[i, 0]) == 0)
                {
                    return l[i,1];
                }
            }
            return "0";
        }

        private bool atLoginScreen()
        {
            eveHandle = FindWindow("triuiScreen", "EVE");
            SetForegroundWindow(eveHandle);
            wait(2);
            new SetClipboardHelper(DataFormats.Text, "0").Go();
            errorCheck();
            pointCursor(element.loginScreenUserName[0], element.loginScreenUserName[1]);
            rightClick(10, 2);
            pointCursor(Cursor.Position.X + element.modifyOffset[0], Cursor.Position.Y + element.modifyOffset[1]);
            leftClick(10, 2);
            if (GetTextFromClip().ToString().CompareTo("0") != 0)
            {
                return true;
            }
            return false;
        }

        private bool checkForEVEnotLoggedIn()
        {
            eveHandle = FindWindow("triuiScreen", "EVE");

            if (eveHandle == IntPtr.Zero)
            {
                excepListBox.Items.Add("EVE Not running");
                return false;
            }
            else
            {
                SetForegroundWindow(eveHandle);
                return true;
            }
        }

        private bool isSelectedCharacterLoggedIn()
        {

            int numOfCharacters = characterListBox.Items.Count;
            int originalSelectedIndex = characterListBox.SelectedIndex;

            initialized = false;
            for (int i = 0; i < numOfCharacters; i++)
            {
                characterListBox.SelectedIndex = i;
                selectedCharacter = characterListBox.SelectedItem.ToString();
                if (isEVERunning() && characterListBox.SelectedIndex == originalSelectedIndex)
                {
                    //debugexcepListBox.Items.Add(selectedCharacter);
                    eveHandle = FindWindow("triuiScreen", "EVE - " + selectedCharacter);

                    characterListBox.SelectedIndex = originalSelectedIndex;
                    selectedCharacter = characterListBox.SelectedItem.ToString();
                    SetForegroundWindow(eveHandle);
                    initialized = true;
                    return true;
                }
                else
                {
                    //debugexcepListBox.Items.Add(selectedCharacter);
                    //debugexcepListBox.Items.Add("Error in check for all");
                }
            }
            characterListBox.SelectedIndex = originalSelectedIndex;
            selectedCharacter = characterListBox.SelectedItem.ToString();
            initialized = true;
            return false;
        }

        private bool isEVERunning()
        {
            eveHandle = FindWindow("triuiScreen", "EVE - " + selectedCharacter);

            if (eveHandle == IntPtr.Zero)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void characterListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (characterListBox.SelectedIndex != -1)
            {
                selectedCharacter = characterListBox.SelectedItem.ToString();
                loadUserSettings();
            }
        }


        public ETGUI()
        {
            InitializeComponent();
         }

        private void noxiousET_load(object sender, EventArgs e)
        {
            loadLastUsedSettings(); 
            pixelReader = new PixelReader(element.loginStage2ActiveCharacter[0] - 5, element.loginStage2ActiveCharacter[1] - 5);
        
        }

        private int loadLastUsedSettings()
        {
            TextReader tr;
            try
            {
                tr = new StreamReader("last.ini");
            }
            catch
            {
                excepListBox.Items.Add("No previous configuration found. Using default settings");
                return 1;
            }
            tr.ReadLine();
            String character;
            character = tr.ReadLine();
            while (character.CompareTo("[Inactive Characters]") != 0)
            {
                addActiveCharacter(character);
                character = tr.ReadLine();
            }
            character = tr.ReadLine();
            while (character.CompareTo("#####") != 0)
            {
                addInactiveCharacter(character);
                character = tr.ReadLine();
            }
            characterListBox.SelectedIndex = Convert.ToInt32(tr.ReadLine());

            path = tr.ReadLine();
            pathTextBox.Text = path;

            EVEPath = tr.ReadLine();
            EVEPathTextbox.Text = EVEPath;
            
            configPath = tr.ReadLine();
            configPathTextbox.Text = configPath;

            elementsListbox.SelectedIndex = Convert.ToInt32(tr.ReadLine());
            timingTextBox.Text = tr.ReadLine();

            autoSellEveryNRunsTextbox.Text = tr.ReadLine();

            waitDuration = waitDurationBackup = Convert.ToInt32(timingTextBox.Text);
            tr.Close();

            loadUIElements(elementsListbox.SelectedItem.ToString());
            initialized = true;
            loadUserSettings();
            return 0;
        }

        private int addActiveCharacter(string character)
        {
            for (int i = characterListBox.Items.Count - 1; i > -1; i--)
            {
                if (characterListBox.Items[i].ToString().CompareTo(character) == 0)
                {
                    return 0;
                }
            }
            characterListBox.Items.Add(character);

            for (int j = inactiveCharactersTextbox.Items.Count - 1; j > -1; j--)
            {
                if (inactiveCharactersTextbox.Items[j].ToString().CompareTo(character) == 0)
                {
                    inactiveCharactersTextbox.Items.RemoveAt(j);
                    return 0;
                }
            }
            return 1;
        }

        private int addInactiveCharacter(string character)
        {
            for (int i = inactiveCharactersTextbox.Items.Count - 1; i > -1; i--)
            {
                if (inactiveCharactersTextbox.Items[i].ToString().CompareTo(character) == 0)
                {
                    return 0;
                }
            }
            inactiveCharactersTextbox.Items.Add(character);

            for (int j = characterListBox.Items.Count -1; j > -1; j--)
            {
                if (characterListBox.Items[j].ToString().CompareTo(character) == 0)
                {
                    characterListBox.Items.RemoveAt(j);
                    return 0;
                }
            }
            return 1;
        }

        private int loadUserSettings()
        {
            if (initialized)
            {
                TextReader tr;
                try
                {
                    tr = new StreamReader(configPath + selectedCharacter + ".ini");
                }
                catch
                {
                    excepListBox.Items.Add("No previous configuration found for user. Using default settings");
                    return 1;
                }
                nTimesTextBox.Text = tr.ReadLine();
                runsToMake = Convert.ToInt32(nTimesTextBox.Text);
                stationsListBox.SelectedIndex = Convert.ToInt32(tr.ReadLine());

                if (Convert.ToInt32(tr.ReadLine()) == 1)
                {
                    tradesItemsCheckbox.Checked = true;
                    tradesItems = true;
                }
                else
                {
                    tradesItemsCheckbox.Checked = false;
                    tradesItems = false;
                }

                if (Convert.ToInt32(tr.ReadLine()) == 1)
                {
                    tradesShipsCheckbox.Checked = true;
                    tradesShips = true;
                }
                else
                {
                    tradesShipsCheckbox.Checked = false;
                    tradesShips = false;
                }

                if (Convert.ToInt32(tr.ReadLine()) == 1)
                {
                    adjustSellsCheckbox.Checked = true;
                    adjustSells = true;
                }
                else
                {
                    adjustSellsCheckbox.Checked = false;
                    adjustSells = false;
                }

                if (Convert.ToInt32(tr.ReadLine()) == 1)
                {
                    adjustBuysCheckbox.Checked = true;
                    adjustBuys = true;
                }
                else
                {
                    adjustBuysCheckbox.Checked = false;
                    adjustBuys = false;
                }

                maxOrdersTextbox.Text = tr.ReadLine();
                maxOrders = Convert.ToInt32(maxOrdersTextbox.Text);

                threshHoldTextbox1.Text = tr.ReadLine();
                qtyTextbox1.Text = tr.ReadLine();
                threshHoldTextbox2.Text = tr.ReadLine();
                qtyTextbox2.Text = tr.ReadLine();
                threshHoldTextbox3.Text = tr.ReadLine();
                qtyTextbox3.Text = tr.ReadLine();
                threshHoldTextbox4.Text = tr.ReadLine();
                qtyTextbox4.Text = tr.ReadLine();
                threshHoldTextbox5.Text = tr.ReadLine();
                qtyTextbox5.Text = tr.ReadLine();
                populateQtyMatrix();
                tr.Close();
            }
            return 0;
        }

        private int saveLastUsedSettings()
        {
            TextWriter tw;
            try
            {
                tw = new StreamWriter("last.ini");
            }
            catch
            {
                excepListBox.Items.Add(configPath + "lastUsed.ini");
                excepListBox.Items.Add("Failed to write to file.");
                return 1;
            }

            tw.WriteLine("[Active Characters]");

            foreach (String s in characterListBox.Items)
            {
                tw.WriteLine(s);
            }

            tw.WriteLine("[Inactive Characters]");

            foreach (String s in inactiveCharactersTextbox.Items)
            {
                tw.WriteLine(s);
            }

            tw.WriteLine("#####");

            tw.WriteLine(characterListBox.SelectedIndex);
            tw.WriteLine(path);
            tw.WriteLine(EVEPath);
            tw.WriteLine(configPath);
            tw.WriteLine(elementsListbox.SelectedIndex);
            tw.WriteLine(timingTextBox.Text);
            tw.WriteLine(autoSellEveryNRuns);
            tw.Close();
            return 0;
        }

        private int saveUserSettings()
        {
            TextWriter tw;
            try
            {
                tw = new StreamWriter(configPath + selectedCharacter + ".ini");
            }
            catch
            {
                excepListBox.Items.Add(configPath + selectedCharacter + ".ini");
                excepListBox.Items.Add("Failed to write to file.");
                return 1;
            }

            tw.WriteLine(nTimesTextBox.Text);
            tw.WriteLine(stationsListBox.SelectedIndex);
            tw.WriteLine(Convert.ToInt32(tradesItems));
            tw.WriteLine(Convert.ToInt32(tradesShips));
            tw.WriteLine(Convert.ToInt32(adjustSells));
            tw.WriteLine(Convert.ToInt32(adjustBuys));
            tw.WriteLine(maxOrdersTextbox.Text);
            for (int i = 0; i < 5; ++i)
            {
                for (int j = 0; j < 2; j++)
                {
                    tw.WriteLine(quantityMatrix[i,j]);
                }
            }
            tw.Close();
            return 0;
        }

        private void elementsListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadUIElements(elementsListbox.SelectedItem.ToString());
        }

        private int loadUIElements(string resolution)
        {
            TextReader tr;
            try
            {
                tr = new StreamReader(configPath + resolution + ".ini");
            }
            catch
            {
                excepListBox.Items.Add("No previous configuration found. Using default resolution settings");
                return 1;
            }
            element.closeMarketWindow[0] = Convert.ToInt32(tr.ReadLine());
            element.closeMarketWindow[1] = Convert.ToInt32(tr.ReadLine());
            element.exportItem[0] = Convert.ToInt32(tr.ReadLine());
            element.exportItem[1] = Convert.ToInt32(tr.ReadLine());
            element.placeBuyOrder[0] = Convert.ToInt32(tr.ReadLine());
            element.placeBuyOrder[1] = Convert.ToInt32(tr.ReadLine());
            element.exportOrderList[0] = Convert.ToInt32(tr.ReadLine());
            element.exportOrderList[1] = Convert.ToInt32(tr.ReadLine());
            element.sellTop[0] = Convert.ToInt32(tr.ReadLine());
            element.sellTop[1] = Convert.ToInt32(tr.ReadLine());
            element.sellSortByType[0] = Convert.ToInt32(tr.ReadLine());
            element.sellSortByType[1] = Convert.ToInt32(tr.ReadLine());
            element.buyTop[0] = Convert.ToInt32(tr.ReadLine());
            element.buyTop[1] = Convert.ToInt32(tr.ReadLine());
            element.buySortByType[0] = Convert.ToInt32(tr.ReadLine());
            element.buySortByType[1] = Convert.ToInt32(tr.ReadLine());
            element.modifyOffset[0] = Convert.ToInt32(tr.ReadLine());
            element.modifyOffset[1] = Convert.ToInt32(tr.ReadLine());
            element.viewDetailsOffset[0] = Convert.ToInt32(tr.ReadLine());
            element.viewDetailsOffset[1] = Convert.ToInt32(tr.ReadLine());
            element.copyOffset[0] = Convert.ToInt32(tr.ReadLine());
            element.copyOffset[1] = Convert.ToInt32(tr.ReadLine());
            element.buyOrderBox[0] = Convert.ToInt32(tr.ReadLine());
            element.buyOrderBox[1] = Convert.ToInt32(tr.ReadLine());
            element.buyOrderQtyBox[0] = Convert.ToInt32(tr.ReadLine());
            element.buyOrderQtyBox[1] = Convert.ToInt32(tr.ReadLine());
            element.sellOrderBox[0] = Convert.ToInt32(tr.ReadLine());
            element.sellOrderBox[1] = Convert.ToInt32(tr.ReadLine());
            element.modifyOrderBox[0] = Convert.ToInt32(tr.ReadLine());
            element.modifyOrderBox[1] = Convert.ToInt32(tr.ReadLine());
            element.OrderBoxCancel[0] = Convert.ToInt32(tr.ReadLine());
            element.OrderBoxCancel[1] = Convert.ToInt32(tr.ReadLine());
            element.OrderBoxOK[0] = Convert.ToInt32(tr.ReadLine());
            element.OrderBoxOK[1] = Convert.ToInt32(tr.ReadLine());
            element.errorCheck[0] = Convert.ToInt32(tr.ReadLine());
            element.errorCheck[1] = Convert.ToInt32(tr.ReadLine());
            element.confirmErrorCheck[0] = Convert.ToInt32(tr.ReadLine());
            element.confirmErrorCheck[1] = Convert.ToInt32(tr.ReadLine());
            element.lineHeight = Convert.ToInt32(tr.ReadLine());
            element.visLines[0] = Convert.ToInt32(tr.ReadLine());
            element.visLines[1] = Convert.ToInt32(tr.ReadLine());
            element.maxOrders = Convert.ToInt32(tr.ReadLine());
            element.itemsSort[0] = Convert.ToInt32(tr.ReadLine());
            element.itemsSort[1] = Convert.ToInt32(tr.ReadLine());
            element.itemsTop[0] = Convert.ToInt32(tr.ReadLine());
            element.itemsTop[1] = Convert.ToInt32(tr.ReadLine());
            element.itemsLineHeight = Convert.ToInt32(tr.ReadLine());
            element.itemsViewDetailsOffset[0] = Convert.ToInt32(tr.ReadLine());
            element.itemsViewDetailsOffset[1] = Convert.ToInt32(tr.ReadLine());
            element.itemsSellItemOffset[0] = Convert.ToInt32(tr.ReadLine());
            element.itemsSellItemOffset[1] = Convert.ToInt32(tr.ReadLine());
            element.itemsViewModuleDetailExtraOffset = Convert.ToInt32(tr.ReadLine());
            element.loginScreenUserName[0] = Convert.ToInt32(tr.ReadLine());
            element.loginScreenUserName[1] = Convert.ToInt32(tr.ReadLine());
            element.loginScreenPW[0] = Convert.ToInt32(tr.ReadLine());
            element.loginScreenPW[1] = Convert.ToInt32(tr.ReadLine());
            element.loginScreenConnect[0] = Convert.ToInt32(tr.ReadLine());
            element.loginScreenConnect[1] = Convert.ToInt32(tr.ReadLine());
            element.loginVerify[0] = Convert.ToInt32(tr.ReadLine());
            element.loginVerify[1] = Convert.ToInt32(tr.ReadLine());
            element.loginStage2[0] = Convert.ToInt32(tr.ReadLine());
            element.loginStage2[1] = Convert.ToInt32(tr.ReadLine());
            element.loginStage2ActiveCharacter[0] = Convert.ToInt32(tr.ReadLine());
            element.loginStage2ActiveCharacter[1] = Convert.ToInt32(tr.ReadLine());
            element.loginStage2Alt1[0] = Convert.ToInt32(tr.ReadLine());
            element.loginStage2Alt1[1] = Convert.ToInt32(tr.ReadLine());
            element.loginStage2Alt2[0] = Convert.ToInt32(tr.ReadLine());
            element.loginStage2Alt2[1] = Convert.ToInt32(tr.ReadLine());
            element.parseErrorMessage[0] = Convert.ToInt32(tr.ReadLine());
            element.parseErrorMessage[1] = Convert.ToInt32(tr.ReadLine());
            element.parseErrorMessageCopyOffset[0] = Convert.ToInt32(tr.ReadLine());
            element.parseErrorMessageCopyOffset[1] = Convert.ToInt32(tr.ReadLine());
            element.closeItems[0] = Convert.ToInt32(tr.ReadLine());
            element.closeItems[1] = Convert.ToInt32(tr.ReadLine());
            element.confirmationCopyOffset[0] = Convert.ToInt32(tr.ReadLine());
            element.confirmationCopyOffset[1] = Convert.ToInt32(tr.ReadLine());
            tr.Close();

            return 0;
        }

        private void tradesItemsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (tradesItemsCheckbox.Checked)
            {
                tradesItems = true;
            }
            else
            {
                tradesItems = false;
            }
        }

        private void tradesShipsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (tradesShipsCheckbox.Checked)
            {
                tradesShips = true;
            }
            else
            {
                tradesShips = false;
            }
        }

        private void adjustSellsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (adjustSellsCheckbox.Checked)
            {
                adjustSells = true;
            }
            else
            {
                adjustSells = false;
            }
        }

        private void adjustBuysCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (adjustBuysCheckbox.Checked)
            {
                adjustBuys = true;
            }
            else
            {
                adjustBuys = false;
            }
        }

        private void fullAutomationButton_Click(object sender, EventArgs e)
        {
            runMode = 0;

            Random random = new Random((int)DateTime.Now.Ticks);
            int minutesToWait = 0;
            int secondsToWait = 0;
            while (true)
            {
                fullAutoManager();
                minutesToWait = random.Next(0, 60);
                secondsToWait = random.Next(0, 60);
                for (int i = 0; i < minutesToWait; i++)
                {
                    Thread.Sleep(60000);
                }
                Thread.Sleep(1000 * secondsToWait);
            }
        }

        private int fullAutoManager()
        {
            int result = 0;
            uint activeEVEPID;
            if (characterListBox.SelectedIndex == -1)
            {
                characterListBox.SelectedIndex = 0;
            }
            int selectedCharacterIndex = characterListBox.SelectedIndex;
            int numActiveCharacters = characterListBox.Items.Count;

            if (characterListBox.Items.Count > 1)
            {
                for (int i = 0; i < 1; ++i)
                {
                    excepListBox.Items.Add("Starting run #" + i + 1 + ". i % autoSellEveryNRuns = " + i % autoSellEveryNRuns);
                    if (i % autoSellEveryNRuns == 0)
                    {
                        excepListBox.Items.Add("==============================================================================================");
                    }
                    for (int j = selectedCharacterIndex; j < selectedCharacterIndex + numActiveCharacters; j++)
                    {
                        characterListBox.SelectedIndex = (j % numActiveCharacters);
                        result = launchEVEPrep();
                        if (result != 0)
                        {
                            if (result == stopAllActivity)
                            {
                                if (exceptionsListbox.Items.Count == 0)
                                    displayExceptions();
                                return 1;
                            }
                            else
                            {
                                --j;
                            }
                        }
                        if (autoSellEveryNRuns > 0 && (i % autoSellEveryNRuns) == 0)
                        {
                            if (autoSellerPrep() == stopAllActivity)
                            {
                                if (exceptionsListbox.Items.Count == 0)
                                    displayExceptions();
                                return 1;
                            }
                        }
                        if (modOrdersPrep(true) == stopAllActivity)
                        {
                            if (exceptionsListbox.Items.Count == 0)
                                displayExceptions();
                            return 1;
                        }
                        try
                        {
                            GetWindowThreadProcessId(eveHandle, out activeEVEPID);
                            Process nukeIt = Process.GetProcessById((int)activeEVEPID);
                            nukeIt.Kill();
                            Thread.Sleep(1000);
                        }
                        catch
                        {
                        }
                    }
                    displayExceptions();
                }
            }
            else
            {
                for (int i = 0; i < 9999; ++i)
                {
                    result = launchEVEPrep();
                    if (result == stopAllActivity)
                    {
                        if (exceptionsListbox.Items.Count == 0)
                            displayExceptions();
                        return 0;
                    }
                    else if (result != 0)
                    {
                        continue;
                    }
                    else
                    {
                        runsToMake = autoSellEveryNRuns;
                        if (runsToMake == 0)
                        {
                            runsToMake = 9999;
                        }
                        else
                        {
                            if (autoSellerPrep() == stopAllActivity)
                            {
                                if (exceptionsListbox.Items.Count == 0)
                                    displayExceptions();
                                runsToMake = Convert.ToInt32(nTimesTextBox.Text);
                                return 0;
                            }
                        }
                        if (modOrdersPrep(false) == stopAllActivity)
                        {
                            if (exceptionsListbox.Items.Count == 0)
                                displayExceptions();
                            runsToMake = Convert.ToInt32(nTimesTextBox.Text);
                            return 0;
                        }
                        runsToMake = Convert.ToInt32(nTimesTextBox.Text);
                    }
                    displayExceptions();
                }
            }
            return 0;
        }

        private int displayExceptions()
        {
            exceptionsListbox.Items.Clear();
            foreach (string s in exception)
            {
                exceptionsListbox.Items.Add(s);
            }
            exception.Clear();
            return 0;
        }

        private void autoSellEveryNRunsTextbox_TextChanged(object sender, EventArgs e)
        {
            autoSellEveryNRuns = Convert.ToInt32(autoSellEveryNRunsTextbox.Text);
        }

        private void addAcctiveButton_Click(object sender, EventArgs e)
        {
            if (inactiveCharactersTextbox.SelectedItems.Count != 0)
            characterListBox.Items.Add(inactiveCharactersTextbox.SelectedItem.ToString());
            inactiveCharactersTextbox.Items.Remove(inactiveCharactersTextbox.SelectedItem);
        }

        private void removeActiveButton_Click(object sender, EventArgs e)
        {
            if (characterListBox.SelectedItems.Count != 0)
            inactiveCharactersTextbox.Items.Add(characterListBox.SelectedItem.ToString());
            characterListBox.Items.Remove(characterListBox.SelectedItem);
        }

        private void maxOrdersTextbox_TextChanged(object sender, EventArgs e)
        {
            maxOrders = Convert.ToInt32(maxOrdersTextbox.Text);
        }

        private void excepListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < 100; i++)
            {
                stopCheck();
            }
            stopwatch.Stop();
            excepListBox.Items.Add(stopwatch.Elapsed.ToString());
        }
    }
}