using System;
using System.Windows.Forms;
using System.Threading;
using noxiousET.src.data;
using noxiousET.src.guiInteraction;
using noxiousET.src.control;

namespace noxiousET
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DataManager dataManager = new DataManager();
            PuppetMaster puppetMaster = new PuppetMaster(dataManager);
            CharacterInfoProvider characterInfoProvider = new CharacterInfoProvider(dataManager.characterManager, dataManager.modules);
            ClientConfigInfoProvider clientConfigInfoProvider = new ClientConfigInfoProvider(dataManager.paths, dataManager.clientConfig);
            OrderReviewInfoProvider orderReviewInfoProvider = new OrderReviewInfoProvider(puppetMaster.orderReviewer);
            AutomationRequester manualExecution = new AutomationRequester(puppetMaster);
            Mouse.suspendEvent = new ManualResetEvent(true);
            Application.Run(new etview(characterInfoProvider, clientConfigInfoProvider, orderReviewInfoProvider, manualExecution));
        }
    }
}
