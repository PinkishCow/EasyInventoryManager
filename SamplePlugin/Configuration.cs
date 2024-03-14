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

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
