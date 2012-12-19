using System;

namespace noxiousET.src.etevent
{
    public sealed class EventDispatcher
    {
        #region Delegates

        public delegate void AutoAdjusterEventHandler(object sourceObject, String e);

        public delegate void AutoListerEventHandler(object sourceObject, String e);

        public delegate void CharacterSettingUpdatedHandler(
            object sourceObject, String character, String key, String value);

        public delegate void ClientSettingUpdatedHandler(object sourceObject, String character, String key, String value
            );

        public delegate void GenericErrorEventHandler(object sourceObject, String e);

        public delegate void GenericEventHandler(object sourceObject, String s);

        public delegate void GetTypesFromFileRequestHandler(object sourceObject, string character);

        public delegate void GetTypesFromQuickbarRequestHandler(
            object sourceObject, string character, string firstItemName, string lastItemName);

        public delegate void SaveAllSettingsRequestHandler(object sourceObject);

        public delegate void UpdateActionToTakeRequestHandler(
            object sourceObject, string character, string typeId, string buyOrSell, string action);

        #endregion

        private static readonly EventDispatcher instance = new EventDispatcher();

        private EventDispatcher()
        {
        }

        public static EventDispatcher Instance
        {
            get { return instance; }
        }

        public event GenericEventHandler GenericEvent;

        public event GenericErrorEventHandler GenericErrorEvent;

        public event AutoAdjusterEventHandler AutoAdjusterEvent;

        public event AutoListerEventHandler AutoListerEvent;

        public event CharacterSettingUpdatedHandler characterSettingUpdatedHandler;

        public event ClientSettingUpdatedHandler clientSettingUpdatedHandler;

        public event SaveAllSettingsRequestHandler saveAllSettingsRequestHandler;

        public event GetTypesFromFileRequestHandler getTypesFromFileRequestHandler;

        public event GetTypesFromQuickbarRequestHandler getTypesFromQuickbarRequestHandler;

        public event UpdateActionToTakeRequestHandler updateActionToTakeRequestHandler;

        public void Log(string message)
        {
            GenericEvent(this, message);
        }

        public void LogError(string message)
        {
            GenericErrorEvent(this, message);
        }

        public void AutoAdjusterLog(string message)
        {
            AutoAdjusterEvent(this, message);
        }

        public void AutoListerLog(string message)
        {
            AutoListerEvent(this, message);
        }

        public void CharacterSettingUpdated(String character, String key, String value)
        {
            characterSettingUpdatedHandler(this, character, key, value);
        }

        public void ClientSettingUpdated(String character, String key, String value)
        {
            clientSettingUpdatedHandler(this, character, key, value);
        }

        public void SaveAllSettingsRequest()
        {
            saveAllSettingsRequestHandler(this);
        }

        public void GetTypesFromFileRequest(String character)
        {
            getTypesFromFileRequestHandler(this, character);
        }

        public void GetTypesFromQuickbarRequest(String character, String firstItemId, String lastItemId)
        {
            getTypesFromQuickbarRequestHandler(this, character, firstItemId, lastItemId);
        }

        public void UpdateActionToTakeRequest(string character, string typeId, string buyOrSell, string action)
        {
            updateActionToTakeRequestHandler(this, character, typeId, buyOrSell, action);
        }
    }
}