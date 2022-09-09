namespace JollyCoopFixesAndStuff
{
    public static class RoomCameraMod
    {
        internal static void OnEnable()
        {
            On.RoomCamera.ApplyPositionChange += RoomCamera_ApplyPositionChange; // change overlay of backspear when changing camera
        }

        // ---------------- //
        // public functions //
        // ---------------- //

        public static void TakeHUD(RoomCamera? roomCamera, AbstractCreature? abstractCreature)
        {
            if (roomCamera == null || roomCamera.hud == null || abstractCreature == null || abstractCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat)
            {
                return;
            }

            // changes hud.owner such that everyone can use the map // you can use the map while dead unless you are in an offscreen den
            if (abstractCreature.realizedCreature is Player player && player != roomCamera.hud.owner)
            {
                // Debug.Log("JollyCoopFixesAndStuff: " + abstractCreature + " takes HUD from camera " + roomCamera.cameraNumber + ".");
                roomCamera.hud.owner = player;
            }
        }

        public static void TakeRoomCamera(RoomCamera? roomCamera, AbstractCreature? abstractCreature)
        {
            if (roomCamera == null || abstractCreature == null || abstractCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat)
            {
                return;
            }

            if (roomCamera.followAbstractCreature != abstractCreature)
            {
                // Debug.Log("JollyCoopFixesAndStuff: " + abstractCreature + " takes camera " + roomCamera.cameraNumber + ".");
                roomCamera.followAbstractCreature = abstractCreature;
                JollyCoop.PlayerHK.currentPlayerWithCamera = ((PlayerState)abstractCreature.state).playerNumber;
            }

            if (abstractCreature.realizedCreature is Player player && abstractCreature.Room.realizedRoom is Room room && room != roomCamera.room) // player.room can be null between room transitions
            {
                roomCamera.MoveCamera(room, RoomMod.CameraViewingPoint(room, player.mainBodyChunk.pos));
            }
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void RoomCamera_ApplyPositionChange(On.RoomCamera.orig_ApplyPositionChange orig, RoomCamera roomCamera)
        {
            orig(roomCamera);
            if (roomCamera.room != null)
            {
                foreach (PhysicalObject physicalObject in roomCamera.room.physicalObjects[2]) // collision layer 2
                {
                    if (physicalObject is Spear spear && spear.mode == Weapon.Mode.OnBack)
                    {
                        spear.inFrontOfObjects = 1;
                        spear.ChangeOverlap(false);
                    }
                }
            }
        }
    }
}