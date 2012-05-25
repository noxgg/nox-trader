using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace noxiousET
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
                    variance = Convert.ToDouble(real.Match(message));
                    if (variance < 200)
                        return 1;
                    else
                        return 4;
                }
                catch
                {
                    return 0;
                }
            }
            else if (message.Contains("below regional average"))
            {
                try
                {
                    variance = Convert.ToDouble(real.Match(message).ToString());
                    if (variance < 200)
                        return 2;
                    else
                        return 5;
                }
                catch
                {
                    return 0;
                }
            }
            else if (message.Contains("cannot modify an order within a certain time"))
                return 3;
            else if (message.Contains("Could not connect to the specified address"))
                return 6;
            else if (message.Contains("Incorrect username or password"))
                return 7;
            else if (message.Contains("Unable to connect to the selected server. Please check the address"))
                return 8;
            else if (message.Contains("connection to the server was closed"))
                return 9;
            else
                return 0;
        }
    }
}
