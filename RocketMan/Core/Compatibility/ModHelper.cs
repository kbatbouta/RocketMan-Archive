using System;
using System.Linq;
using Verse;

namespace RocketMan
{
    public abstract class ModHelper
    {
        private bool initiated = false;
        private bool isLoaded = false;

        public abstract string PackageID { get; }
        public abstract string Name { get; }

        public virtual bool IsLoaded()
        {
            if (initiated) return isLoaded;
            initiated = true;
            isLoaded = LoadedModManager.RunningMods.Any(
                m => m.Name == Name || m.PackageId == PackageID
                );
            if (isLoaded) Log.Message(string.Format("ROCKETMAN: Rocketman detected {0}!", Name));
            return isLoaded;
        }
    }
}
