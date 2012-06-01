using System.Collections.Generic;

namespace noxiousET.src.model.data.modules
{
    class Modules
    {
        public Dictionary<int, int> longNameTypeIDs { set; get; }
        public Dictionary<int, int> fittableModuleTypeIDs { set; get; }
        public Dictionary<int, string> typeNames { set; get; }

        public Modules()
        {
            longNameTypeIDs = new Dictionary<int,int>();
            fittableModuleTypeIDs = new Dictionary<int, int>();
            typeNames = new Dictionary<int, string>();
        }
    }
}
