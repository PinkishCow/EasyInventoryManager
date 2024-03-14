using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace EasyInventoryManager
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool UseSaddlebag { get; set; } = false;

        public bool DepositCrystals { get; set; } = false;

        public bool DepositAll { get; set; } = false;

        public bool UsePersonalHouse { get; set; } = false;

        public bool UseFCHouse { get; set; } = false;

        public bool retardTest { get; set; } = false;

        public bool iSlots { get; set; } = false;

        public bool retSlots { get; set; } = false;

        public bool retainerCount { get; set; } = false;

        public bool retIndex { get; set; } = false;

        public bool getInvItems {  get; set; } = false;


        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public static Configuration Load(DalamudPluginInterface pluginInterface)
        {
            var config = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            config.pluginInterface = pluginInterface;
            return config;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
