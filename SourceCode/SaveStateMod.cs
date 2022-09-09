using System;
using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    internal static class SaveStateMod
    {
        internal static void OnEnable()
        {
            On.SaveState.ApplyCustomEndGame += SaveState_ApplyCustomEndGame; // fix food counting when fast traveling
            On.SaveState.SessionEnded += SaveState_SessionEnded; // fix food counting before saving
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void SaveState_ApplyCustomEndGame(On.SaveState.orig_ApplyCustomEndGame orig, SaveState saveState, RainWorldGame game, bool addFiveCycles)
        {
            orig(saveState, game, addFiveCycles);
            if (game.Players.Count > 0)
            {
                saveState.food = ((PlayerState)game.Players[0].state).foodInStomach; // the HUD seems to not display the correct food count // use this as food count instead // otherwise the player start with a lot of food after fast traveling
                game.rainWorld.progression.SaveWorldStateAndProgression(false);
            }
        }

        private static void SaveState_SessionEnded(On.SaveState.orig_SessionEnded orig, SaveState saveState, RainWorldGame game, bool survived, bool newMalnourished)
        {
            if (game.Players.Count <= 1)
            {
                orig(saveState, game, survived, newMalnourished);
                return;
            }

            // fix food counting // otherwise all food will be counted together and capped at max // for example, two hunters with 6 food = 12 but capped = min(12, 9) minus 6 needed to hilbernate = 3 => you start with 3 food next cycle
            for (int playerIndex = 1; playerIndex < game.Players.Count; playerIndex++)
            {
                ((PlayerState)game.Players[playerIndex].state).foodInStomach = 0;
            }
            RainWorldGameMod.LogAllPlayer(game);

            orig(saveState, game, survived, newMalnourished);
        }
    }
}