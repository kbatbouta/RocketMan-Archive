using System.Linq;
using Verse;

namespace RocketMan
{
    public class DubsPreformanceAnalyzerHelper : ModHelper
    {
        private static DubsPreformanceAnalyzerHelper instance;
        private bool initialized;
        private bool isLoaded;

        public override string PackageID => "Dubwise.DubsPerformanceAnalyzerButchered";

        public override string Name => "Dubs Performance Analyzer - Wiri's Butchery";

        public static DubsPreformanceAnalyzerHelper Instance
        {
            get
            {
                if (instance == null) instance = new DubsPreformanceAnalyzerHelper();
                return instance;
            }
        }

        public override bool IsLoaded()
        {
            if (initialized) return isLoaded;
            initialized = true;
            isLoaded = LoadedModManager.RunningMods.Any(
                m => m.Name == "Dubs Performance Analyzer - Wiri's Butchery"
                     || m.PackageId == "Dubwise.DubsPerformanceAnalyzerButchered"
                     || m.Name == "Dubs Performance Analyzer");
            if (isLoaded) Log.Message(string.Format("ROCKETMAN: Rocketman detected {0}!", Name));
            return isLoaded;
        }

        [Main.OnInitialization]
        private static void Initialize() => _ = Instance;
    }
}