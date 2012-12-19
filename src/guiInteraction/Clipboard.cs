using System;
using System.Threading;
using System.Windows.Forms;

namespace noxiousET.src.guiInteraction
{
    internal class Clipboard
    {
        public static string GetTextFromClipboard()
        {
            try
            {
                IDataObject dataObj = System.Windows.Forms.Clipboard.GetDataObject();
                return dataObj.GetData(DataFormats.Text).ToString();
            }
            catch
            {
                return "0";
            }
        }

        public static void SetClip(String inputText)
        {
            new SetClipboardHelper(DataFormats.Text, inputText).Go();
        }
    }

    internal abstract class StaHelper
    {
        private readonly ManualResetEvent _complete = new ManualResetEvent(false);
        public bool DontRetryWorkOnFailed { get; set; }

        public void Go()
        {
            var thread = new Thread(DoWork)
                             {
                                 IsBackground = true,
                             };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        // Thread entry method
        private void DoWork()
        {
            try
            {
                _complete.Reset();
                Work();
            }
            catch (Exception)
            {
                if (DontRetryWorkOnFailed)
                    throw;
                else
                {
                    try
                    {
                        Thread.Sleep(1000);
                        Work();
                    }
                    catch
                    {
                        // ex from first exception
                    }
                }
            }
            finally
            {
                _complete.Set();
            }
        }

        // Implemented in base class to do actual work.
        protected abstract void Work();
    }

    internal class SetClipboardHelper : StaHelper
    {
        private readonly object _data;
        private readonly string _format;

        public SetClipboardHelper(string format, object data)
        {
            _format = format;
            _data = data;
        }

        protected override void Work()
        {
            var obj = new DataObject(
                _format,
                _data
                );

            System.Windows.Forms.Clipboard.SetDataObject(obj, true);
        }
    }
}