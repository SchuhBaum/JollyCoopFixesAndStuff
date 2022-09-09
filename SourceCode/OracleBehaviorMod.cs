namespace JollyCoopFixesAndStuff
{
    internal class OracleBehaviorMod
    {
        internal static void OnEnable()
        {
            On.OracleBehavior.Update += OracleBehavior_Update; // pebbles and moon react to other player
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void OracleBehavior_Update(On.OracleBehavior.orig_Update orig, OracleBehavior oracleBehavior, bool eu)
        {
            foreach (AbstractCreature abstractPlayer in oracleBehavior.oracle.room.game.Players)
            {
                if (abstractPlayer.Room == oracleBehavior.oracle.room.abstractRoom && abstractPlayer.realizedCreature is Player player)
                {
                    oracleBehavior.player = player;
                    break;
                }
            }
            orig(oracleBehavior, eu);
        }
    }
}