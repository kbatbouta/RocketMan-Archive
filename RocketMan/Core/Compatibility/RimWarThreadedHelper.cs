using System;
namespace RocketMan
{
    public class RimWarThreadedHelper : ModHelper
    {
        public override string PackageID
        {
            get
            {
                return "Torann.RimWarThreaded_copy";
            }
        }
        public override string Name
        {
            get
            {
                return "Rim War - Threaded";
            }
        }

        private static RimWarThreadedHelper instance;

        public static RimWarThreadedHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RimWarThreadedHelper();
                }
                return instance;
            }
        }
    }
}
