using noxiousET.src.data.characters;
using noxiousET.src.data.client;
using noxiousET.src.data.modules;
using noxiousET.src.data.paths;
using noxiousET.src.data.uielements;

namespace noxiousET.src.guiInteraction.orders
{
    class OrderBot : GuiBot
    {
        protected OrderAnalyzer orderAnalyzer;
        protected int timingBackup;
        protected int consecutiveFailures;
        protected Modules modules;

        public OrderBot(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, Modules modules): base(clientConfig, uiElements, paths, character)
        {
            this.modules = modules;
            orderAnalyzer = new OrderAnalyzer();
            timingBackup = timingMultiplier;
            consecutiveFailures = 0;
        }

        //TODO, merge with confirmOrder
        protected int cancelOrder(int longOrderXoffset, int longOrderYOffset, int typeID)
        {
            int failCount = 0;
            string temp = "0";
            do
            {
                wait(1);
                mouse.pointCursor(uiElements.OrderBoxCancel[0], uiElements.OrderBoxCancel[1] + longOrderYOffset);
                mouse.leftClick(1, 1);


                if (failCount > 0 && failCount % 3 == 0)
                {
                    if (!modules.ignoreErrorCheckTypeIDs.ContainsKey(typeID))//skip missing
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
                mouse.pointCursor(uiElements.OrderBoxCancel[0] + longOrderXoffset, uiElements.OrderBoxCancel[1]);
                mouse.rightClick(1, 1);

                //Click on copy
                mouse.offsetCursor(uiElements.confirmationCopyOffset[0], uiElements.confirmationCopyOffset[1]);
                mouse.leftClick(1, 1);

                temp = Clipboard.GetTextFromClip();
                ++failCount;
            } while (string.Compare(temp, "0") == 0 && failCount < 9);
            lastOrderModified = false;
            if (string.Compare(temp, "0") != 0)
            {
                Clipboard.setClipboardText("0");
                return 0;
            }
            else
                return 1;
        }

        //TODO Refactor out
    }
}
