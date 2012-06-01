using System.Windows.Forms;

namespace noxiousET.src.model.guiInteraction
{
    class Keyboard
    {
        public static void send(string s) 
        {
            SendKeys.SendWait(s);
        }
    }
}
