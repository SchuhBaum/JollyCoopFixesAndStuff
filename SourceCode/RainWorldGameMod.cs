using RWCustom;
using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    public static class RainWorldGameMod
    {
        public static bool[][] mapPressed = new bool[0][];

        internal static void OnEnable()
        {
            On.RainWorldGame.ctor += RainWorldGame_ctor; // follow player 1 by default // fix player IDs
            On.RainWorldGame.Update += RainWorldGame_Update; // camera changes // map for everyone // improve compatibility with ShelterBehaviors
        }

        // ---------------- //
        // public functions //
        // ---------------- //

        public static RoomCamera? GetRoomCameraClosestTo(RainWorldGame game, AbstractCreature? abstractCreature)
        {
            if (game.cameras.Length == 0 || abstractCreature == null || abstractCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat)
            {
                return null;
            }

            int cameraNumber = 0;
            if (game.cameras.Length > 1) // select nearest camera
            {
                int distance = int.MaxValue;
                foreach (RoomCamera roomCamera in game.cameras)
                {
                    if (roomCamera?.room != null)
                    {
                        if (roomCamera.followAbstractCreature == abstractCreature)
                        {
                            cameraNumber = roomCamera.cameraNumber;
                            break;
                        }

                        if (Custom.ManhattanDistance(abstractCreature.pos, roomCamera.room.GetWorldCoordinate(roomCamera.pos + roomCamera.sSize / 2f)) is int distance_ && distance_ < distance)
                        {
                            distance = distance_;
                            cameraNumber = roomCamera.cameraNumber;
                        }
                    }
                }
            }
            return game.cameras[cameraNumber];
        }

        public static RoomCamera? GetRoomCameraFollowedBy(RainWorldGame game, AbstractCreature? abstractCreature)
        {
            if (abstractCreature == null)
            {
                return null;
            }

            foreach (RoomCamera roomCamera in game.cameras)
            {
                if (roomCamera.followAbstractCreature == abstractCreature)
                {
                    return roomCamera;
                }
            }
            return null;
        }

        public static RoomCamera? GetRoomCameraWithHUDOwnedBy(RainWorldGame game, Creature? creature)
        {
            if (creature == null)
            {
                return null;
            }

            foreach (RoomCamera roomCamera in game.cameras)
            {
                if (roomCamera.hud?.owner == creature)
                {
                    return roomCamera;
                }
            }
            return null;
        }

        public static void Glow(RainWorldGame game)
        {
            foreach (AbstractCreature abstractPlayer in game.Players)
            {
                if (abstractPlayer.realizedCreature is Player player && !player.glowing)
                {
                    Debug.Log("JollyCoopFixesAndStuff: Glow " + player + ".");
                    player.glowing = true;
                }
            }
        }

        public static bool IsFollowedByACamera(RainWorldGame game, AbstractCreature? abstractCreature)
        {
            if (abstractCreature == null)
            {
                return false;
            }

            foreach (RoomCamera roomCamera in game.cameras)
            {
                if (roomCamera.followAbstractCreature == abstractCreature)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsMapPressed(RainWorldGame game, int playerNumber) => mapPressed[playerNumber][0] && !mapPressed[playerNumber][1] && game.pauseMenu == null; // assuming that mapPressed at playerNumber exists

        public static void LogAllPlayer(RainWorldGame game)
        {
            foreach (AbstractCreature abstractPlayer in game.Players)
            {
                AbstractCreatureMod.LogPlayer(abstractPlayer);
            }
        }

        public static void ReturnHUD(RainWorldGame game, AbstractCreature? abstractCreature)
        {
            if (GetRoomCameraWithHUDOwnedBy(game, abstractCreature?.realizedCreature) is RoomCamera roomCamera)
            {
                RoomCameraMod.TakeHUD(roomCamera, roomCamera.followAbstractCreature);
            }
        }

        public static void SetPlayerWithIndex0(RainWorldGame game, AbstractCreature? abstractPlayer) // used in VoidSea
        {
            if (abstractPlayer == null || game.Players.Count <= 1 || game.Players[0] == abstractPlayer)
            {
                return;
            }

            // restore order
            AbstractCreature abstractPlayer_ = game.Players[0];
            int playerNumber_ = ((PlayerState)abstractPlayer_.state).playerNumber;

            if (playerNumber_ != 0)
            {
                game.Players[0] = game.Players[playerNumber_];
                game.Players[playerNumber_] = abstractPlayer_;
            }

            // set new Player with playerIndex 0
            int playerNumber = ((PlayerState)abstractPlayer.state).playerNumber;
            game.Players[playerNumber] = game.Players[0];
            game.Players[0] = abstractPlayer;
        }

        public static void ShareAllRelationships(RainWorldGame game)
        {
            if (!MainMod.isSharedRelationshipsEnabled || game.Players.Count <= 1) // player count needs to be known
            {
                return;
            }

            // sync relationships with new players // needs to happen only once
            foreach (AbstractRoom abstractRoom in game.world.abstractRooms)
            {
                foreach (AbstractCreature abstractCreature in abstractRoom.creatures)
                {
                    ShareRelationship(game, abstractCreature);
                }

                foreach (AbstractWorldEntity abstractWorldEntity in abstractRoom.entitiesInDens)
                {
                    if (abstractWorldEntity is AbstractCreature abstractCreature)
                    {
                        ShareRelationship(game, abstractCreature);
                    }
                }
            }
        }

        public static void ShareRelationship(RainWorldGame game, AbstractCreature? abstractCreature)
        {
            if (abstractCreature?.state.socialMemory is SocialMemory socialMemory)
            {
                int playerCount = game.Players.Count;
                bool[] hasRelationship = new bool[playerCount];
                SocialMemory.Relationship? sharedRelationship = null;

                foreach (SocialMemory.Relationship relationship in socialMemory.relationShips)
                {
                    if (relationship.subjectID.spawner == -1)
                    {
                        int playerNumber = relationship.subjectID.number;
                        if (playerNumber >= 0 && playerNumber < playerCount)
                        {
                            sharedRelationship ??= relationship;
                            hasRelationship[playerNumber] = true;
                        }
                    }
                }

                if (sharedRelationship != null)
                {
                    foreach (AbstractCreature abstractPlayer in game.Players)
                    {
                        int playerNumber = ((PlayerState)abstractPlayer.state).playerNumber;
                        if (!hasRelationship[playerNumber])
                        {
                            Debug.Log("JollyCoopFixesAndStuff: Add relationship between " + abstractCreature + " and Slugcat " + abstractPlayer.ID + ".");
                            SocialMemory.Relationship relationship = new SocialMemory.Relationship(abstractPlayer.ID)
                            {
                                know = sharedRelationship.know,
                                like = sharedRelationship.like,
                                tempLike = sharedRelationship.tempLike,

                                fear = sharedRelationship.fear,
                                tempFear = sharedRelationship.tempFear
                            };
                            socialMemory.relationShips.Add(relationship);
                        }
                    }
                }
            }
        }

        public static void TakeRoomCamera(RainWorldGame game, RoomCamera? roomCamera, AbstractCreature? abstractCreature)
        {
            if (abstractCreature == null || abstractCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat)
            {
                return;
            }

            RoomCameraMod.TakeRoomCamera(roomCamera, abstractCreature);
            RoomRealizerMod.TakeRoomRealizer(game.roomRealizer, abstractCreature);
        }

        public static void TakeRoomCameraAndHUD(RainWorldGame game, RoomCamera? roomCamera, AbstractCreature? abstractCreature)
        {
            if (abstractCreature == null || abstractCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat)
            {
                return;
            }

            RoomCameraMod.TakeHUD(roomCamera, abstractCreature);
            RoomCameraMod.TakeRoomCamera(roomCamera, abstractCreature);
            RoomRealizerMod.TakeRoomRealizer(game.roomRealizer, abstractCreature);
        }

        public static void UpdateMapPressed(RainWorldGame game)
        {
            for (int playerIndex = 0; playerIndex < game.Players.Count; playerIndex++)
            {
                mapPressed[playerIndex][1] = mapPressed[playerIndex][0];
                mapPressed[playerIndex][0] = game.rainWorld.options.controls[playerIndex] is Options.ControlSetup controlSetup && (!controlSetup.gamePad && Input.GetKey(controlSetup.KeyboardMap) || controlSetup.gamePad && Input.GetKey(controlSetup.GamePadMap));
            }
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame game, ProcessManager manager)
        {
            orig(game, manager);
            int cameraCount = game.cameras.Length;

            Debug.Log("JollyCoopFixesAndStuff: cameraCount " + cameraCount);
            WorldMod.moddedShelters.Clear(); // used when ShelterBehaviors mod is enabled

            foreach (AbstractCreature abstractPlayer in game.Players)
            {
                int playerNumber = ((PlayerState)abstractPlayer.state).playerNumber;
                EntityID entityID = new EntityID(-1, playerNumber);

                if (abstractPlayer.ID != entityID) // I had multiple player with the ID of player 2
                {
                    Debug.Log("JollyCoopFixesAndStuff: PlayerNumber is " + playerNumber + ". Change " + abstractPlayer.ID + " to " + entityID + ".");
                    abstractPlayer.ID = entityID;
                }
            }

            if (game.IsStorySession) // crashed the arena mode otherwise
            {
                MainMod.isEasyModeEnabled = (bool?)MainMod.GetNonPublicField(JollyCoop.JollyMod.config, "easyMode") ?? true;

                Debug.Log("JollyCoopFixesAndStuff: hasPlayerPointers " + MainMod.hasPlayerPointers);
                Debug.Log("JollyCoopFixesAndStuff: isEasyModeEnabled " + MainMod.isEasyModeEnabled);
                Debug.Log("JollyCoopFixesAndStuff: isSharedRelationshipsEnabled " + MainMod.isSharedRelationshipsEnabled);
                Debug.Log("JollyCoopFixesAndStuff: isSlugcatCollisionEnabled " + MainMod.isSlugcatCollisionEnabled);

                // use weak table // maybe
                mapPressed = new bool[game.Players.Count][];
                for (int playerIndex = 0; playerIndex < game.Players.Count; playerIndex++)
                {
                    mapPressed[playerIndex] = new bool[2];
                }

                ShareAllRelationships(game);

                // JollyCoop focuses player 2 instead of 1 at the start of the cycle
                for (int cameraNumber = 0; cameraNumber < game.cameras.Length; cameraNumber++)
                {
                    // redundant for cameraNumber > 0 bc SplitScreenMod already does this
                    if (cameraNumber < game.Players.Count)
                    {
                        TakeRoomCameraAndHUD(game, game.cameras[cameraNumber], game.Players[cameraNumber]);
                    }
                }
            }
        }

        private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame game)
        {
            orig(game);
            if (game.cameras[0].hud == null || !game.IsStorySession)
            {
                return;
            }

            UpdateMapPressed(game);
            if (MainMod.isShelterBehaviorsEnabled)
            {
                foreach (AbstractCreature abstractPlayer in game.Players)
                {
                    if (abstractPlayer.Room.shelter && abstractPlayer.state.alive && WorldMod.moddedShelters.Contains(abstractPlayer.Room.name))
                    {
                        foreach (AbstractCreature abstractPlayer_ in game.Players)
                        {
                            if (abstractPlayer_.state.dead && abstractPlayer_.Room != abstractPlayer.Room)
                            {
                                if (MainMod.isEasyModeEnabled)
                                {
                                    Debug.Log("JollyCoopFixesAndStuff: ShelterBehaviors is active. Teleport " + abstractPlayer_ + " early.");
                                    AbstractCreatureMod.DropOrDestroyAllObjects(abstractPlayer_);
                                    AbstractCreatureMod.Teleport(abstractPlayer_, abstractPlayer.Room);
                                    abstractPlayer_.RealizeInRoom();

                                    if (abstractPlayer_.realizedCreature is Player player && player.stillInStartShelter)
                                    {
                                        player.stillInStartShelter = false;
                                    }
                                }
                                else if (abstractPlayer_.realizedCreature == null)
                                {
                                    abstractPlayer_.Realize();
                                }
                            }
                        }
                        break;
                    }
                }
            }

            if (game.Players.Count <= 1)
            {
                return;
            }

            if (!JollyCoop.JollyMod.config.cycleCamera) // direct camera
            {
                foreach (AbstractCreature abstractPlayer in game.Players)
                {
                    if (abstractPlayer.state.alive && abstractPlayer.state is PlayerState playerState && IsMapPressed(game, playerState.playerNumber)) // can switch the camera even when in a shortcut
                    {
                        if (GetRoomCameraFollowedBy(game, abstractPlayer) is RoomCamera roomCamera)
                        {
                            TakeRoomCameraAndHUD(game, roomCamera, abstractPlayer);
                        }
                        else
                        {
                            TakeRoomCameraAndHUD(game, GetRoomCameraClosestTo(game, abstractPlayer), abstractPlayer);
                        }
                    }
                }
            }
            else // cycling camera
            {
                for (int playerIndex = 0; playerIndex < game.Players.Count; playerIndex++)
                {
                    // the first game.cameras.Length many players own their camera and control who is followed // even when they are dead
                    // other players can only take the map of the nearest one (when in the same room)
                    if (IsMapPressed(game, playerIndex))
                    {
                        AbstractCreature abstractPlayer = game.Players[playerIndex];
                        if (playerIndex >= 0 && playerIndex < game.cameras.Length) // cameraNumber == playerIndex
                        {
                            int playerCount = game.Players.Count;
                            int playerNumber_ = game.cameras[playerIndex].followAbstractCreature?.state is PlayerState playerState_ ? playerState_.playerNumber : playerIndex;

                            for (int _ = 0; _ < playerCount; _++) // go a full round // because the camera can be left behind outside of your control // by warping for example
                            {
                                playerNumber_ = (playerNumber_ + 1) % playerCount;
                                AbstractCreature abstractPlayer_ = game.Players[playerNumber_];

                                if ((playerNumber_ >= game.cameras.Length && !IsFollowedByACamera(game, abstractPlayer_) || playerNumber_ == playerIndex) && abstractPlayer_.state.alive) // the camera receiver needs to be alive
                                {
                                    if (abstractPlayer.state.alive) // when the camera controlling player is alive // only give the camera // take the HUD yourself
                                    {
                                        RoomCameraMod.TakeHUD(game.cameras[playerIndex], abstractPlayer);
                                        TakeRoomCamera(game, game.cameras[playerIndex], abstractPlayer_);
                                    }
                                    else
                                    {
                                        TakeRoomCameraAndHUD(game, game.cameras[playerIndex], abstractPlayer_);
                                    }
                                    break;
                                }
                            }
                        }
                        else if (abstractPlayer.state.alive)
                        {
                            // give the HUD back // assumes that abstractPlayer has at most one HUD
                            ReturnHUD(game, abstractPlayer);
                            if (GetRoomCameraFollowedBy(game, abstractPlayer) is RoomCamera roomCamera)
                            {
                                // the camera controlling player might have the HUD instead of the followed player
                                RoomCameraMod.TakeHUD(roomCamera, abstractPlayer);
                            }
                            else
                            {
                                RoomCameraMod.TakeHUD(GetRoomCameraClosestTo(game, abstractPlayer), abstractPlayer);
                            }
                        }
                    }
                }
            }
        }
    }
}