using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using noxiousET.src.data;
using noxiousET.src.guiInteraction.login;
using noxiousET.src.data.io;
using noxiousET.src.guiInteraction;

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


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ETGUI());
        }
    }
}
