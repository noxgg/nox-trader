using System;

namespace noxiousET.src
{
    internal static class EtConstants
    {
        public const String TimingMultiplierKey = "timingMultiplier";
        public const String IterationsKey = "iterations";
        public const String XResolutionKey = "xResolution";
        public const String YResolutionKey = "yResolution";
        public const String LogPathKey = "logPath";
        public const String ClientPathKey = "clientPath";
        public const String ConfigPathKey = "configPath";
        public const String EveSettingsPathKey = "eveSettingsPath";
        public const String AccountLoginKey = "login";
        public const String AccountPasswordKey = "password";
        public const String CharacterTradeItemsKey = "tradeItems";
        public const String CharacterTradeShipsKey = "tradeShips";
        public const String CharacterAdjustSellsKey = "adjustSells";
        public const String CharacterAdjustBuysKey = "adjustBuys";
        public const String CharacterMaximumOrdersKey = "maximumOrders";
        public const String CharacterStationIdKey = "stationid";
        public const String AccountIdKey = "accountId";
        public const String CharacterIdKey = "characterId";
        public const String CharacterLoginColorKey = "loginColor";
        public const String CharacterKey = "character";
        public const String CharacterActiveState = "activeState";
        public const String CharacterThreshholdPrice = "threshholdPrice";
        public const String CharacterThreshholdQuantity = "threshholdQuantity";
        public const String ClipboardNullValue = "0";
        public const String OrderWindowClosedVerificationText= "mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm";
        public const String OrderWindowClosedVerificationSubstring = "mmmmmm";
        public const int Sell = 0;
        public const int Buy = 1;
        public const int X = 0;
        public const int Y = 1;
        public const int NoOrdersExist = 0;
        public const int SellOrderExists = 1;
        public const int BuyOrderExists = 2;
        public const int BuyAndSellOrdersExist = 3;
        public const int OrderDataColumnPrice = 0;
        public const int OrderDataColumnVolumeRemaining = 1;
        public const int OrderDataColumnTypeId = 2;
        public const int OrderDataColumnRange = 3;
        public const int OrderDataColumnOrderId = 4;
        public const int OrderDataColumnVolumeEntered = 5;
        public const int OrderDataColumnMinimumVolume = 6;
        public const int OrderDataColumnIsBuyOrder = 7;
        public const int OrderDataColumnIssueDate = 8;
        public const int OrderDataColumnDuration = 9;
        public const int OrderDataColumnStationId = 10;
        public const int OrderDataColumnRegionId = 11;
        public const int OrderDataColumnSolarSystemId = 12;
        public const int OrderDataColumnJumps = 13;
        public const int StationRange = -1;

        public const int ErrorParseFailure = 0;
        public const int AlertInputAboveRegionalAverage = 1;
        public const int AlertInputBelowRegionalAverage = 2;
        public const int AlertOrderUpdateRateExceeded = 3;
        public const int AlertInput200PercentAboveAverage = 4;
        public const int AlertInput200PercentBelowAverage = 5;
        public const int ErrorConnectionFailure = 6;
        public const int ErrorBadLogin = 7;
        public const int ErrorUnableToConnect = 8;
        public const int ErrorConnectionClosed = 9;
        public const int AlertItemDestruction = 10;
        public const int ErrorNoOrdersFound = 11;
        public const int AlertImplantsLostOnDeath = 12;
        public const int PromptCancelOrderConfirmation = 13;
        public const int UnknownError = 69;
        public const int ErrorNoErrorFound = 0;
        public const bool IsBuyOrder = true;
        public const bool IsSellOrder = false;


        public const String KbMouseLock = "kbmousemutex";
        public const String UnpauseLock = "unpausemutex";
    }
}