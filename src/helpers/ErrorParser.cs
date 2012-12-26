using System;
using System.Text.RegularExpressions;

namespace noxiousET.src.helpers
{
    //return codes
    //0 no error found
    //1 mod order input above regional average
    //2 mod order input below regional average
    //3 less than 5 minutes since last mod order attempt
    //4 mod order input 200%+ above regional average
    //5 mod order input 200%+ below regional average
    //6 Could not connect to the specified address.
    //7 Bad login/password
    //8 Unable to connect to the selected server. Please check the address and try again.
    //9 The connection to the server was closed.
    
    class ErrorParser
    {
        Regex real;
        public ErrorParser()
        {
             real = new Regex(@"\d*\.\d*");
        }

        public int parse(string message)
        {
            double variance = 0;
            if (message.Contains("above regional average"))
            {
                try
                {
                    variance = Convert.ToDouble(real.Match(message).ToString());
                    if (variance < 200)
                        return EtConstants.AlertInputAboveRegionalAverage;
                    return EtConstants.AlertInput200PercentAboveAverage;
                }
                catch
                {
                    return EtConstants.ErrorParseFailure;
                }
            }
            if (message.Contains("below regional average"))
            {
                try
                {
                    variance = Convert.ToDouble(real.Match(message).ToString());
                    if (variance < 200)
                        return EtConstants.AlertInputBelowRegionalAverage;
                    return EtConstants.AlertInput200PercentBelowAverage;
                }
                catch
                {
                    return EtConstants.ErrorParseFailure;
                }
            }
            if (message.Contains("cannot modify an order within a certain time"))
                return EtConstants.AlertOrderUpdateRateExceeded;
            if (message.Contains("Could not connect to the specified address"))
                return EtConstants.ErrorConnectionFailure;
            if (message.Contains("Incorrect username or password"))
                return EtConstants.ErrorBadLogin;
            if (message.Contains("Unable to connect to the selected server. Please check the address"))
                return EtConstants.ErrorUnableToConnect;
            if (message.Contains("connection to the server was closed"))
                return EtConstants.ErrorConnectionClosed;
            if (message.Contains("You are about to throw away the following items:"))
                return EtConstants.AlertItemDestruction;
            if (message.Contains("There were no buy or sell orders found."))
                return EtConstants.ErrorNoOrdersFound;
            if (message.Contains("Implants are lost when unplugged and when you die."))
                return EtConstants.AlertImplantsLostOnDeath;
            if (message.Contains("Are you sure you want to cancel this order?"))
                return EtConstants.PromptCancelOrderConfirmation;
            return EtConstants.UnknownError;
        }
    }
}
