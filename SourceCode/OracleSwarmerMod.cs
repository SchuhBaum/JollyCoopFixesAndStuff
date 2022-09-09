namespace JollyCoopFixesAndStuff
{
    internal static class OracleSwarmerMod
    {
        internal static void OnEnable()
        {
            On.OracleSwarmer.BitByPlayer += OracleSwarmer_BitByPlayer; // share glow immediately
            On.SLOracleSwarmer.BitByPlayer += SLOracleSwarmer_BitByPlayer; // share glow immediately
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void OracleSwarmer_BitByPlayer(On.OracleSwarmer.orig_BitByPlayer orig, OracleSwarmer oracleSwarmer, Creature.Grasp grasp, bool eu)
        {
            orig(oracleSwarmer, grasp, eu);
            if (oracleSwarmer.slatedForDeletetion)
            {
                RainWorldGameMod.Glow(oracleSwarmer.abstractPhysicalObject.world.game);
            }
        }

        private static void SLOracleSwarmer_BitByPlayer(On.SLOracleSwarmer.orig_BitByPlayer orig, SLOracleSwarmer slOracleSwarmer, Creature.Grasp grasp, bool eu)
        {
            orig(slOracleSwarmer, grasp, eu);
            if (slOracleSwarmer.slatedForDeletetion)
            {
                RainWorldGameMod.Glow(slOracleSwarmer.abstractPhysicalObject.world.game);
            }
        }
    }
}