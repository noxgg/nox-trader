using System.Collections.Generic;

namespace noxiousET.src.data.modules
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

        public List<string> getAlphabetizedItemNames(ICollection<int> typeids)
        {
            List<string> names = new List<string>();
            foreach (int typeid in typeids)
            {
                names.Add(typeNames[typeid]);
            }
            names.Sort();
            return names;
        }

        public List<int> getTypeIdsAlphabetizedByItemName(ICollection<int> typeids)
        {
            List<string> names = new List<string>();
            Dictionary<string, int> reverseTypeNames = new Dictionary<string, int>();
            foreach (int typeid in typeids)
            {
                names.Add(typeNames[typeid]);
                reverseTypeNames.Add(typeNames[typeid], typeid);
            }
            names.Sort();

            List<int> sortedTypeIds = new List<int>();
            foreach (string s in names)
            {
                sortedTypeIds.Add(reverseTypeNames[s]);
            }
            return sortedTypeIds;
        }
    }
}
