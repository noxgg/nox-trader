using noxiousET.src.etevent;
using noxiousET.src.model.data.characters;
using noxiousET.src.model.data.client;
using noxiousET.src.model.data.modules;
using noxiousET.src.model.data.paths;
using noxiousET.src.model.data.uielements;
using noxiousET.src.model.orders;

namespace noxiousET.src.model.guiInteraction.orders
{
    class OrderBot : GuiBot
    {
        protected OrderAnalyzer orderAnalyzer;
        protected int timingBackup;
        protected int consecutiveFailures;
        protected Modules modules;

        public OrderBot(ClientConfig clientConfig, UiElements uiElements, Paths paths, Character character, Modules modules, EventDispatcher eventDispatcher)
            : base(clientConfig, uiElements, paths, character, eventDispatcher)
        {
            this.modules = modules;
            orderAnalyzer = new OrderAnalyzer(eventDispatcher);
            timingBackup = timingMultiplier;
            consecutiveFailures = 0;
        }

        //TODO, merge with confirmOrder
        protected int cancelOrder(int longOrderXoffset, int longOrderYOffset)
        {
            int failCount = 0;
            string temp = "0";
            do
            {
                mouse.pointAndClick(LEFT, uiElements.OrderBoxCancel[0], uiElements.OrderBoxCancel[1] + longOrderYOffset, 1, 1, 1);

                if (failCount > 0 && failCount % 3 == 0)
                        errorCheck();

                //Right click where OK should no longer exist.
                mouse.pointAndClick(RIGHT, uiElements.OrderBoxCancel[0] + longOrderXoffset, uiElements.OrderBoxCancel[1], 0, 1, 1);
                //Click on copy
                mouse.offsetAndClick(LEFT, uiElements.confirmationCopyOffset, 0, 1, 1);

                temp = Clipboard.getTextFromClipboard();
                ++failCount;
            } while (string.Compare(temp, "0") == 0 && failCount < 9);
            lastOrderModified = false;
            if (string.Compare(temp, "0") != 0)
            {
                Clipboard.setClip("0");
                return 0;
            }
            else
                return 1;
        }
    }
}
