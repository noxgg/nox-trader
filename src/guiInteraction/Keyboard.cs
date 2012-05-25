using System.Windows.Forms;

namespace noxiousET.src.guiInteraction
{
    class Keyboard
    {
        public static void send(string s) 
        {
            SendKeys.SendWait(s);
        }
    }
}
