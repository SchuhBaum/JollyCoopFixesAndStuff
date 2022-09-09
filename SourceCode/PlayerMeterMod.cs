using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    public static class PlayerMeterMod
    {
        public static Dictionary<HUD.HudPart, RoomCamera> playerMeterRoomCamera = new Dictionary<HUD.HudPart, RoomCamera>();

        internal static void OnEnable()
        {
            if (Type.GetType("JollyCoop.PlayerMeter, JollyCoop") is Type playerMeter)
            {
                try
                {
                    new Hook(playerMeter.GetMethod("Draw", BindingFlags.Public | BindingFlags.Instance), typeof(PlayerMeterMod).GetMethod("PlayerMeter_Draw"));
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

        public delegate void delDraw(HUD.HudPart playerMeter, float timeStacker);

        public static void PlayerMeter_Draw(delDraw orig, HUD.HudPart playerMeter, float timeStacker)
        {
            // SplitScreenMod adds a fix for the PlayerMeter // but that fix is based on hud.owner instead of roomCamera.followAbstractCreature // cycle camera can have different players for both -- given the changes I made to cycle camera
            if (JollyCoop.JollyMod.config.cycleCamera && playerMeterRoomCamera[playerMeter].followAbstractCreature?.realizedCreature is Player player && playerMeter.hud.owner is Player player_ && player != player_)
            {
                playerMeter.hud.owner = player;
                orig(playerMeter, timeStacker);
                playerMeter.hud.owner = player_;
            }
            else
            {
                orig(playerMeter, timeStacker);
            }
        }
    }
}