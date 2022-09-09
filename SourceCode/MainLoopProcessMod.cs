using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    public static class MainLoopProcessMod
    {
        internal static void OnEnable()
        {
            On.MainLoopProcess.RawUpdate += MainLoopProcess_RawUpdate; // removes slow motion // speeds up shortcuts while slow down is active
        }

        // ---------------- //
        // public functions //
        // ---------------- //

        public static void SetMaxUpdateShortcut(int framesPerSecond)
        {
            SBCameraScroll.RoomCameraMod.maxUpdateShortcut = framesPerSecond > 33 ? 3f : 2f; // framesPerSecond: 40f/1.21f ~ 33 & updateShortCut: 3f/1.21f < 2.5f rounded to 2
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void MainLoopProcess_RawUpdate(On.MainLoopProcess.orig_RawUpdate orig, MainLoopProcess mainLoopProcess, float dt)
        {
            if (mainLoopProcess is RainWorldGame game && game.IsStorySession)
            {
                mainLoopProcess.framesPerSecond = 40;
                foreach (AbstractCreature abstractPlayer in game.Players)
                {
                    if (abstractPlayer.state.alive && abstractPlayer.realizedCreature is Player player && player.Adrenaline > 0.0f)
                    {
                        mainLoopProcess.framesPerSecond = Mathf.RoundToInt(40f / Mathf.Lerp(1f, 1.5f, player.Adrenaline));
                        if (game.updateShortCut == Mathf.RoundToInt(3f / Mathf.Lerp(1f, 1.5f, player.Adrenaline)) - 1)
                        {
                            game.updateShortCut = 2;
                        }
                        break;
                    }
                }

                if (MainMod.isSBCameraScrollEnabled)
                {
                    SetMaxUpdateShortcut(mainLoopProcess.framesPerSecond);
                }
            }
            orig(mainLoopProcess, dt);
        }
    }
}