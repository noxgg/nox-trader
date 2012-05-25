using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace noxiousET.src.data.modules
{
    class Modules
    {
        public Dictionary<int, int> longNameTypeIDs { set; get; }
        public Dictionary<int, int> ignoreErrorCheckTypeIDs { set; get; }
        public Dictionary<int, int> fittableModuleTypeIDs { set; get; }

        public Modules()
        {
            longNameTypeIDs = new Dictionary<int,int>();
            ignoreErrorCheckTypeIDs = new Dictionary<int,int>();
            fittableModuleTypeIDs = new Dictionary<int, int>();
        }
    }
}
