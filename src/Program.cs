using System;
using System.Windows.Forms;
using noxiousET.src.model.data;
using noxiousET.src.model.guiInteraction;
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
            DataManager dataManager = new DataManager();
            PuppetMaster puppetMaster = new PuppetMaster(dataManager);
            CharacterInfoProvider characterInfoProvider = new CharacterInfoProvider(dataManager.characterManager);
            ClientConfigInfoProvider clientConfigInfoProvider = new ClientConfigInfoProvider(dataManager.paths, dataManager.clientConfig);
            AutomationRequester manualExecution = new AutomationRequester(puppetMaster);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new noxiousETgui(characterInfoProvider, clientConfigInfoProvider, manualExecution, dataManager.eventDispatcher));
        }
    }
}
