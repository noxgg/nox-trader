using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace noxiousET.src.etevent
{
	public sealed class EventDispatcher
	{
	    private static readonly EventDispatcher instance = new EventDispatcher();

        private EventDispatcher()
        {
        }

	    public static EventDispatcher Instance
	    {
	        get { return instance; }
	    }

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
        public delegate void GetTypesFromFileRequestHandler(object sourceObject, string character);
        public event GetTypesFromFileRequestHandler getTypesFromFileRequestHandler;
        public delegate void GetTypesFromQuickbarRequestHandler(object sourceObject, string character, string firstItemName, string lastItemName);
        public event GetTypesFromQuickbarRequestHandler getTypesFromQuickbarRequestHandler;
        public delegate void UpdateActionToTakeRequestHandler(object sourceObject, string character, string typeId, string buyOrSell, string action);
        public event UpdateActionToTakeRequestHandler updateActionToTakeRequestHandler;

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

        public void getTypesFromFileRequest(String character)
        {
            this.getTypesFromFileRequestHandler(this, character);
        }

        public void getTypesFromQuickbarRequest(String character, String firstItemId, String lastItemId)
        {
            this.getTypesFromQuickbarRequestHandler(this, character, firstItemId, lastItemId);
        }

        public void updateActionToTakeRequest(string character, string typeId, string buyOrSell, string action)
        {
            this.updateActionToTakeRequestHandler(this, character, typeId, buyOrSell, action);
        }
	}
}
