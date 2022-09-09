namespace JollyCoopFixesAndStuff
{
    internal static class SpearOnBackMod
    {
        internal static void OnEnable()
        {
            On.Player.SpearOnBack.GraphicsModuleUpdated += SpearOnBack_GraphicsModuleUpdated;
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void SpearOnBack_GraphicsModuleUpdated(On.Player.SpearOnBack.orig_GraphicsModuleUpdated orig, Player.SpearOnBack spearOnBack, bool actuallyViewed, bool eu)
        {
            if (spearOnBack.spear?.slatedForDeletetion == true)
            {
                spearOnBack.spear = null; // prevents backspear from being dropped when spear gets abstracted
            }
            orig(spearOnBack, actuallyViewed, eu);
        }
    }
}