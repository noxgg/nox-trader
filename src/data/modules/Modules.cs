using System.Collections.Generic;
using System.Linq;

namespace noxiousET.src.data.modules
{
    internal class Modules
    {
        public Modules()
        {
            LongNameTypeIDs = new Dictionary<int, int>();
            FittableModuleTypeIDs = new Dictionary<int, int>();
            TypeNames = new Dictionary<int, string>();
        }

        public Dictionary<int, int> LongNameTypeIDs { set; get; }
        public Dictionary<int, int> FittableModuleTypeIDs { set; get; }
        public Dictionary<int, string> TypeNames { set; get; }

        public List<string> GetAlphabetizedItemNames(ICollection<int> typeids)
        {
            List<string> names = typeids.Select(typeid => TypeNames[typeid]).ToList();
            names.Sort();
            return names;
        }

        public List<int> GetTypeIdsAlphabetizedByItemName(ICollection<int> typeids)
        {
            var names = new List<string>();
            var reverseTypeNames = new Dictionary<string, int>();
            foreach (int typeid in typeids)
            {
                names.Add(TypeNames[typeid]);
                reverseTypeNames.Add(TypeNames[typeid], typeid);
            }
            names.Sort();

            return names.Select(s => reverseTypeNames[s]).ToList();
        }
    }
}