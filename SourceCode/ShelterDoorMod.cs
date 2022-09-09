namespace JollyCoopFixesAndStuff
{
    internal static class ShelterDoorMod
    {
        internal static void OnEnable()
        {
            On.ShelterDoor.Close += ShelterDoor_Close; // fix player counting when EM is disabled
            On.ShelterDoor.DoorClosed += ShelterDoor_DoorClosed; // teleport player // fix shelter softlocks
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void ShelterDoor_Close(On.ShelterDoor.orig_Close orig, ShelterDoor shelterDoor)
        {
            if (MainMod.isEasyModeEnabled || shelterDoor.room.game.Players.Count <= 1)
            {
                orig(shelterDoor);
            }
            else if (!MainMod.isEasyModeEnabled)
            {
                foreach (AbstractCreature abstractPlayer in shelterDoor.room.game.Players)
                {
                    if (abstractPlayer.Room != shelterDoor.room.abstractRoom)
                    {
                        return;
                    }
                }
                orig(shelterDoor);
            }
        }

        private static void ShelterDoor_DoorClosed(On.ShelterDoor.orig_DoorClosed orig, ShelterDoor shelterDoor)
        {
            if (!MainMod.isEasyModeEnabled || shelterDoor.room.game.Players.Count <= 1)
            {
                orig(shelterDoor);
                return;
            }
            RoomMod.DoorClosed(shelterDoor.room);
        }
    }
}