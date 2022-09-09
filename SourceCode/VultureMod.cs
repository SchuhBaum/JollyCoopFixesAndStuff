namespace JollyCoopFixesAndStuff
{
    internal static class VultureMod
    {
        internal static void OnEnable()
        {
            On.Vulture.AccessSkyGate += Vulture_AccessSkyGate; // kill player faster when carried away by vulture
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void Vulture_AccessSkyGate(On.Vulture.orig_AccessSkyGate orig, Vulture vulture, WorldCoordinate start, WorldCoordinate dest)
        {
            orig(vulture, start, dest);
            foreach (Creature.Grasp? grasp in vulture.grasps)
            {
                // happens anyway but can take up to 20 sec otherwise
                if (grasp?.grabbed is Player player && player.playerState.alive)
                {
                    player.Die();
                }
            }
        }
    }
}