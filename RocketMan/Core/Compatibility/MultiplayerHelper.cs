using System;
using System.Linq;
using Verse;

namespace RocketMan
{
    public class MultiplayerHelper : ModHelper
    {
        private static MultiplayerHelper instance;
        private bool initialized;
        private bool isLoaded;

        public override string PackageID => "rwmt.Multiplayer";

        public override string Name => "Multiplayer";

        public static MultiplayerHelper Instance
        {
            get
            {
                if (instance == null) instance = new MultiplayerHelper();
                return instance;
            }
        }

        public override bool IsLoaded()
        {
            if (initialized) return isLoaded;
            initialized = true;
            isLoaded = LoadedModManager.RunningMods.Any(
                m => m.Name == "Multiplayer"
                     || m.PackageId == PackageID
                     || (m.Name.ToLower().Contains("multiplayer") && m.PackageId.ToLower().Contains("multiplayer")));
            if (isLoaded) Log.Message(string.Format("ROCKETMAN: Rocketman detected {0}!", Name));
            return isLoaded;
        }

        [Main.OnInitialization]
        private static void Initialize() => _ = Instance;
    }
}
