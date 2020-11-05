using System;
using System.Linq;
using Verse;

namespace RocketMan
{
    public class DubsPreformanceAnalyzerHelper : ModHelper
    {
        private bool isLoaded = false;
        private bool initialized = false;

        public override string PackageID
        {
            get
            {
                return "Dubwise.DubsPerformanceAnalyzerButchered";
            }
        }

        public override string Name
        {
            get
            {
                return "Dubs Performance Analyzer - Wiri's Butchery";
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

        private static DubsPreformanceAnalyzerHelper instance;

        public static DubsPreformanceAnalyzerHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DubsPreformanceAnalyzerHelper();
                }
                return instance;
            }
        }
    }
}
