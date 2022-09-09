using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
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

        // copied from SplitScreenMod
        public delegate void delHandleCoopCamera(Player player, int playerNumber);

        public static void PlayerHK_HandleCoopCamera(delHandleCoopCamera orig, Player player, int playerNumber)
        {
            return; // ignore what JollyCoop does // JollyCoop can interfere when cycle camera is used
        }
    }
}