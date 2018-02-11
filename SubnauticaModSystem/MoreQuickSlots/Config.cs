using System;

namespace MoreQuickSlots
{
    [Serializable]
    public struct ModInfo
    {
        public string id;
        public string displayName;
        public string author;
        public string version;
        public string[] requires;
        public bool enable;
        public string[] assemblies;
        public object config;
    }

    [Serializable]
    public class Config
    {
        public int SlotCount = 12;
    }
}
