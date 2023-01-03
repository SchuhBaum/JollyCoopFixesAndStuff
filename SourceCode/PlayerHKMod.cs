using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    public static class PlayerHKMod
    {
        internal static void OnEnable()
        {
            // copied from SplitScreenMod
            if (Type.GetType("JollyCoop.PlayerHK, JollyCoop") is Type playerHK)
            {
                try
                {
                    Debug.Log("JollyCoopFixesAndStuff: Modify JollyCoop's AddFood function. Fix infinite loop.");
                    new Hook(playerHK.GetMethod("AddFoodHK", BindingFlags.Public | BindingFlags.Static), typeof(PlayerHKMod).GetMethod("PlayerHK_AddFoodHK"));
                }
                catch (Exception exception)
                {
                    Debug.Log("JollyCoopFixesAndStuff: " + exception);
                }

                try
                {
                    Debug.Log("JollyCoopFixesAndStuff: Override JollyCoop's HandleCoopCamera function.");
                    new Hook(playerHK.GetMethod("HandleCoopCamera", BindingFlags.Public | BindingFlags.Static), typeof(PlayerHKMod).GetMethod("PlayerHK_HandleCoopCamera"));
                }
                catch (Exception exception)
                {
                    Debug.Log("JollyCoopFixesAndStuff: " + exception);
                }
            }
        }

        // ---------------- //
        // public functions //
        // ---------------- //

        public static void PlayerHK_AddFoodHK(Action<On.Player.orig_AddFood, Player, int> orig, On.Player.orig_AddFood orig_AddFood, Player player, int add)
        {
            if (JollyCoop.PlayerHK.IsArena)
            {
                orig(orig_AddFood, player, add);
                return;
            }

            // can be stuck in a loop // sharedFood is based on the maximum foodInStomach (which can be higher than maxFood) 
            // for example the hunter cutscene gives always five foodInStomach // other players try to reach that number but might not be able to => infinite loop
            // foodInStomach and sharedFood need to be clamped // sharedFood to break the loop & foodInStomach to not start another one next frame

            for (int playerIndex = 0; playerIndex < JollyCoop.PlayerHK.sharedFood.Length; ++playerIndex)
            {
                if (JollyCoop.PlayerHK.sharedFood[playerIndex] > player.slugcatStats.maxFood)
                {
                    JollyCoop.PlayerHK.sharedFood[playerIndex] = (byte)player.slugcatStats.maxFood; // byte?
                }
            }

            foreach (AbstractCreature abstractPlayer in player.abstractCreature.world.game.Players)
            {
                PlayerState playerState = (PlayerState)abstractPlayer.state;
                if (playerState.foodInStomach > player.slugcatStats.maxFood)
                {
                    playerState.foodInStomach = player.slugcatStats.maxFood;
                }
            }
            orig(orig_AddFood, player, add);
        }

        public static void PlayerHK_HandleCoopCamera(Action<Player, int> _1, Player _2, int _3)
        {
            return; // ignore what JollyCoop does // JollyCoop can interfere when cycle camera is used
        }
    }
}