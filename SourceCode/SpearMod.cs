namespace JollyCoopFixesAndStuff
{
    internal static class SpearMod
    {
        internal static void OnEnable()
        {
            On.Spear.RecreateSticksFromAbstract += Spear_RecreateSticksFromAbstract; // reload backspears if player got abstracted
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void Spear_RecreateSticksFromAbstract(On.Spear.orig_RecreateSticksFromAbstract orig, Spear spear)
        {
            if (spear.mode != Weapon.Mode.Free)
            {
                return;
            }

            foreach (AbstractPhysicalObject.AbstractObjectStick abstractObjectStick in spear.abstractPhysicalObject.stuckObjects)
            {
                if (abstractObjectStick is Player.AbstractOnBackStick abstractOnBackStick && abstractOnBackStick.Player.realizedObject is Player player && player.spearOnBack != null)
                {
                    player.spearOnBack.spear = spear;
                    player.spearOnBack.abstractStick = abstractOnBackStick;
                    spear.ChangeMode(Weapon.Mode.OnBack);
                }
            }
            orig(spear);
        }
    }
}