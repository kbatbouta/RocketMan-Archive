namespace RocketMan
{
    public class RimWarHelper : ModHelper
    {
        private static RimWarHelper instance;
        public override string PackageID => "Torann.RimWar";

        public override string Name => "Rim War";

        public static RimWarHelper Instance
        {
            get
            {
                if (instance == null) instance = new RimWarHelper();
                return instance;
            }
        }
    }
}