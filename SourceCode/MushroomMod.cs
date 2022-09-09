namespace JollyCoopFixesAndStuff
{
    internal static class MushroomMod
    {
        internal static void OnEnable()
        {
            On.Mushroom.BitByPlayer += Mushroom_BitByPlayer; // share mushroom effect
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void Mushroom_BitByPlayer(On.Mushroom.orig_BitByPlayer orig, Mushroom mushroom, Creature.Grasp grasp, bool eu)
        {
            orig(mushroom, grasp, eu);
            foreach (AbstractCreature abstractPlayer in mushroom.abstractPhysicalObject.world.game.Players) // mushroom.room is null when in room transition // doesn't matter in this case
            {
                if (abstractPlayer.realizedCreature is Player player && player != grasp?.grabber)
                {
                    player.mushroomCounter += 320;
                }
            }
        }
    }
}