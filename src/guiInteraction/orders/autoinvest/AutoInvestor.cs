using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.io;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uidata;
using noxiousET.src.helpers;
using noxiousET.src.orders;

namespace noxiousET.src.guiInteraction.orders.autoinvester
{
    internal class AutoInvestor : OrderBot
    {
        private readonly MarketOrderio _marketOrderio;
        private int _itemsScanned;
        private int _ordersCreated;
        private const int InvestorFailureLimit = 4;

        public AutoInvestor(ClientConfig clientConfig, EveUi ui, Paths paths, Character character,
                            Modules modules, OrderAnalyzer orderAnalyzer)
            : base(clientConfig, ui, paths, character, modules, orderAnalyzer)
        {
            _marketOrderio = new MarketOrderio();
            _marketOrderio.Path = paths.LogPath;
        }

        private string GenerateInvestmentHtml(List<string> potentialInvestments)
        {
            string hexColor = RandomHexColor();
            var pageMarkup = new List<object>();
            pageMarkup.Add("<body bgcolor='" + hexColor + "'>");
            pageMarkup.AddRange(from typeId in potentialInvestments
                                select "<a href='#' onclick='CCPEVE.showMarketDetails(" + typeId.ToString() + ")'>x</a>");
            var textFileio = new TextFileio(Paths.LocalDropboxPath, EtConstants.MarketSearchHtml);
            textFileio.Save(pageMarkup);
            return hexColor;
        }

        private List<String> GeneratePotentialInvestments()
        {
            return Character.TradeHistory.Keys.Select(typeId => typeId.ToString()).ToList();
        }


        private void OpenBrowserWindow()
        {
            const int tries = 5;
            for (int i = 0; i < tries; i++)
            {
                SetForegroundWindow(EveHandle);
                Wait(20);
                Keyboard.Shortcut(Keyboard.VkLAlt, Keyboard.VkB);
                Wait(20);
                Keyboard.Shortcut(Keyboard.VkLcontrol, Keyboard.VkC);
                Wait(20);
                string clipboardText = Clipboard.GetTextFromClipboard();
                if (clipboardText.Contains(EtConstants.InGameBrowserWebUrlPrefix) || clipboardText.Contains(EtConstants.InGameBrowserLocalFileUrlPrefix))
                    return;
            }
            throw new Exception("Failed to open browser window!");
        }

        private void NavigateToInvestmentsPage(string pageIdentifier)
        {
            const int tries = 6;
            for (int i = 0; i < tries; i++)
            {
                try
                {

                    InputValue(4, 2, EveUi.BrowserUrlBar, EtConstants.InGameBrowserLocalFileUrlPrefix + Paths.WebDropboxPath + EtConstants.MarketSearchHtml);
                    Keyboard.Send("{ENTER}");
                    PixelReader pixelReader = new PixelReader();
                    string discoveredColor = pixelReader.GetPixelHexColor(EveUi.InvestmentPageIdentifier[0], EveUi.InvestmentPageIdentifier[1]);
                    if (discoveredColor.Equals(pageIdentifier))
                    {
                        Mouse.WaitDuration = Timing;
                        return;
                    }
                }
                catch
                {
                }
                Mouse.WaitDuration *= 2;
            }
            Mouse.WaitDuration = Timing;
            throw new Exception("Failed to navigate to investments page!");
        }

        private string RandomHexColor()
        {
            var random = new Random((int)DateTime.Now.Ticks);
            const string chars = "0123456789ABCDEF";
            var buffer = new char[7];
            buffer[0] = '#';
            for (int i = 1; i < 7; i++)
            {
                buffer[i] = chars[random.Next(chars.Length)];
            }
            return new string(buffer);
        }

        public void Execute(Character character)
        {
            Character = character;
            _itemsScanned = 0;
            _ordersCreated = 0;

            if (!IsEveRunningForSelectedCharacter())
                throw new Exception("Auto investor ould not find EVE Client for selected character.");
            Mouse.PointAndClick(Left, EveUi.MarketCloseButton, 0, 5, 5);
            GetCurrentOrders(character);
            List<String> potentialInvestments = GeneratePotentialInvestments();
            String pageIdentifier = GenerateInvestmentHtml(potentialInvestments);
            OpenBrowserWindow();
            NavigateToInvestmentsPage(pageIdentifier);

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                CreateInvestments(potentialInvestments.Count);

                stopwatch.Stop();
                Logger.Log(character.Name + ": AI scanned " + _itemsScanned + " and made " + _ordersCreated +
                           " buys " + stopwatch.Elapsed.ToString());
            }
            catch (Exception e)
            {
                Logger.Log("Auto investor failed to complete run.");
                Logger.LogError(e.Message);
            }
        }

        private void GetCurrentOrders(Character character)
        {
            ExportOrders(4, 30);
            if (OrderAnalyzer.OrderSet.GetNumberOfActiveOrders() >= character.MaximumOrders)
            {
                throw new Exception(
                    "This character already has the maximum number of orders! Cannot create additional investments");
            }
        }

        private void CreateInvestments(int potentialInvestmentCount)
        {
            int consecutiveFailures = 0;
            int count = 0;
            int freeOrders = Character.MaximumOrders - OrderAnalyzer.OrderSet.GetNumberOfActiveOrders();
            List<int> tradeQueue = Modules.GetTypeIdsAlphabetizedByItemName(Character.TradeHistory.Keys);

            int[] cursorPosition = new[] {EveUi.InvestmentPageFirstEntry[0], EveUi.InvestmentPageFirstEntry[1]};

            do
            {
                try
                {
                    List<String[]> orderData = ExecuteQueryAndExportResult(5, 1.2, cursorPosition);
                    OrderAnalyzer.AnalyzeInvestment(orderData, Convert.ToString(Character.StationId));
                    int currentTypeId = OrderAnalyzer.GetTypeId();;

                    if (!OrderAnalyzer.IsSomeOrderOwned())
                    {
                        //Uses data from orderAnalyzer.analyzeInvestment to decide if a buy order should be made
                        int quantity = GetBuyOrderQuantity(OrderAnalyzer.GetBuyPrice(), OrderAnalyzer.GetSellPrice());

                        if (quantity > 0)
                        {
                            OpenAndIdentifyBuyWindow(currentTypeId, OrderAnalyzer.GetSellPrice());
                            PlaceBuyOrder(currentTypeId, quantity);
                            freeOrders--;
                            _ordersCreated++;
                            //logger.log(modules.typeNames[currentTypeId] + " should create buy order.");
                            consecutiveFailures = 0;
                        }
                    consecutiveFailures = 0;
                    }
                }
                catch (Exception e)
                {
                    ++consecutiveFailures;
                    if (consecutiveFailures > InvestorFailureLimit)
                        return;
                    Logger.Log(e.Message);
                }
                ++count;
                if (count % EveUi.InvestmentPageVisibleColumns == 0)
                {
                    cursorPosition[EtConstants.X] = EveUi.InvestmentPageFirstEntry[EtConstants.X];
                    cursorPosition[EtConstants.Y] += EveUi.InvestmentPageRowHeight;
                }
                else
                {
                    cursorPosition[EtConstants.X] += EveUi.InvestmentPageColumnWidth;
                }

                _itemsScanned++;
                if (freeOrders == 0)
                    return;
            } while (count < potentialInvestmentCount);
        }

        private List<String[]> ExecuteQueryAndExportResult(int tries, double timingScaleFactor, int[] cursorPosition)
        {
            DirectoryEraser.Nuke(Paths.LogPath);
            if (_marketOrderio.GetNumberOfFilesInDirectory(Paths.LogPath) != 0)
                throw new Exception("Could not clean log path directory");

            for (int i = 0; i < tries; i++)
            {
                /* The in-game browser doesn't recognize click events if we move directly on top of 
                 * an element and click on it. To get around this, we move the cursor slightly while
                 * on top of a target element, which forces the IGB to recognize the subsequent click. */
                Mouse.PointCursor(new[] {cursorPosition[0] + 1, cursorPosition[1] + 1});
                Mouse.PointAndClick(Left, cursorPosition, 5, 5, 1);
                Mouse.PointAndClick(Left, EveUi.MarketExportButton, 10, 1, 10);
                String fileName = _marketOrderio.GetNewestFileNameInDirectory(Paths.LogPath);
                if (fileName != null)
                {
                    Mouse.WaitDuration = Timing;
                    _marketOrderio.FileName = fileName;
                    List<String[]> result = _marketOrderio.Read();
                    if (result != null && OrderAnalyzer.IsNewOrderData(result))
                        return result;
                }
                Mouse.WaitDuration = Convert.ToInt32(Mouse.WaitDuration*timingScaleFactor);
                if (i > 2)
                    ErrorCheck();
            }
            Mouse.WaitDuration = Timing;
            throw new Exception("Could not export query result");
        }
    }
}