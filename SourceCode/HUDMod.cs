using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    internal static class HUDMod
    {
        internal static void OnEnable()
        {
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD hud, RoomCamera roomCamera)
        {
            orig(hud, roomCamera);
            foreach (HUD.HudPart hudPart in hud.parts)
            {
                if (hudPart.ToString() == "JollyCoop.PlayerMeter" && !PlayerMeterMod.playerMeterRoomCamera.ContainsKey(hudPart))
                {
                    // cleanup old PlayerMeter
                    foreach (HUD.HudPart playerMeter in PlayerMeterMod.playerMeterRoomCamera.Keys)
                    {
                        if (PlayerMeterMod.playerMeterRoomCamera[playerMeter].cameraNumber == roomCamera.cameraNumber)
                        {
                            Debug.Log("JollyCoopFixesAndStuff: Remove old PlayerMeter.");
                            PlayerMeterMod.playerMeterRoomCamera.Remove(playerMeter);
                            break;
                        }
                    }

                    // add new PlayerMeter
                    Debug.Log("JollyCoopFixesAndStuff: Link new PlayerMeter to roomCamera " + roomCamera.cameraNumber + ".");
                    PlayerMeterMod.playerMeterRoomCamera.Add(hudPart, roomCamera);
                }
            }
        }
    }
}