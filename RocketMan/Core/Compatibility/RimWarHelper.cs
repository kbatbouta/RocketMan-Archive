using System;
namespace RocketMan
{
    public class RimWarHelper : ModHelper
    {
        public override string PackageID
        {
            get
            {
                return "Torann.RimWar";
            }
        }
        public override string Name
        {
            get
            {
                return "Rim War";
            }
        }

        private static RimWarHelper instance;

        public static RimWarHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RimWarHelper();
                }
                return instance;
            }
        }
    }
}
