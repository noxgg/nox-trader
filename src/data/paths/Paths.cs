using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace noxiousET.src.data.paths
{
    class Paths
    {
        public string logPath { set; get; }
        public string EVEPath { set; get; }
        public string configPath { set; get; }

        public Paths()
        {
            logPath = @"A:\Users\nox\Documents\EVE\logs\Marketlogs\";
            EVEPath = @"G:\EVE\eve.exe";
            configPath = @"D:\Dropbox\Dropbox\Apps\noxiousETConfig\"; 
        }
    }
}
