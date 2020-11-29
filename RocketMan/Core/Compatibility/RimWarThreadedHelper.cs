namespace RocketMan
{
    public class RimWarThreadedHelper : ModHelper
    {
        private static RimWarThreadedHelper instance;
        public override string PackageID => "Torann.RimWarThreaded_copy";
        public override string Name => "Rim War - Threaded";

        public static RimWarThreadedHelper Instance
        {
            get
            {
                if (instance == null) instance = new RimWarThreadedHelper();
                return instance;
            }
        }
    }
}