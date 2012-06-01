using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace noxiousET.src.etevent
{
	class EventDispatcher
	{
        public delegate void GenericEventHandler(object sourceObject, String s);
        public event GenericEventHandler genericEvent;
        public delegate void GenericErrorEventHandler(object sourceObject, String e);
        public event GenericErrorEventHandler genericErrorEvent;
        public delegate void AutoAdjusterEventHandler(object sourceObject, String e);
        public event AutoAdjusterEventHandler autoAdjusterEvent;
        public delegate void AutoListerEventHandler(object sourceObject, String e);
        public event AutoListerEventHandler autoListerEvent;
        public delegate void CharacterSettingUpdatedHandler(object sourceObject, String character, String key, String value);
        public event CharacterSettingUpdatedHandler characterSettingUpdatedHandler;
        public delegate void ClientSettingUpdatedHandler(object sourceObject, String character, String key, String value);
        public event ClientSettingUpdatedHandler clientSettingUpdatedHandler;
        public delegate void SaveAllSettingsRequestHandler(object sourceObject);
        public event SaveAllSettingsRequestHandler saveAllSettingsRequestHandler;

        public void log(string message)
        {
            this.genericEvent (this, message);
        }

        public void logError(string message)
        {
            this.genericErrorEvent(this, message);
        }

        public void autoAdjusterLog(string message)
        {
            this.autoAdjusterEvent(this, message);
        }

        public void autoListerLog(string message)
        {
            this.autoListerEvent(this, message);
        }

        public void characterSettingUpdated(String character, String key, String value)
        {
            this.characterSettingUpdatedHandler(this, character, key, value);
        }

        public void clientSettingUpdated(String character, String key, String value)
        {
            this.clientSettingUpdatedHandler(this, character, key, value);
        }

        public void saveAllSettingsRequest()
        {
            this.saveAllSettingsRequestHandler(this);
        }
	}
}
