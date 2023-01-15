using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    public static class RoomMod
    {
        //
        // variables
        //

        private static readonly List<Creature> creaturesInRoomList = new();

        //
        //
        //

        internal static void OnEnable_JollyCoop()
        {
            On.Room.AddObject += Room_AddObject;
            On.Room.ctor += Room_ctor;
            On.Room.PlaySound_SoundID_BodyChunk_bool_float_float += Room_PlaySound;
            On.Room.Update += Room_Update_JollyCoop;
        }

        internal static void OnEnable_ShelterBehaviors()
        {
            On.Room.Update += Room_Update_ShelterBehaviors;
        }

        // ---------------- //
        // public functions //
        // ---------------- //

        public static int CameraViewingPoint(Room? room, Vector2 p) // pick correct camera position when player is out of room boundary
        {
            if (room == null)
            {
                return 0;
            }

            Vector2 offset = new(683f, 384f);
            int cameraIndex = 0;
            float sqrMagnitude = (room.cameraPositions[cameraIndex] + offset - p).sqrMagnitude;

            for (int index = 1; index < room.cameraPositions.Length; index++)
            {
                float sqrMagnitude_ = (room.cameraPositions[index] + offset - p).sqrMagnitude;
                if (sqrMagnitude_ < sqrMagnitude)
                {
                    sqrMagnitude = sqrMagnitude_;
                    cameraIndex = index;
                }
            }
            return cameraIndex;
        }

        public static void DoorClosed(Room? room)
        {
            if (room == null)
            {
                return;
            }

            // some stuff might break in SaveState.SessionEnded and SaveState.BringUpToDate otherwise // if !easyModeEnabled then this should lead to a game over anyway
            foreach (AbstractCreature abstractPlayer in room.game.Players)
            {
                if (abstractPlayer.Room != room.abstractRoom)
                {
                    AbstractCreatureMod.DropOrDestroyAllObjects(abstractPlayer);
                    AbstractCreatureMod.Teleport(abstractPlayer, room.abstractRoom, shortcutNode: 0);
                }
            }

            bool anyPlayerAlive = false;
            foreach (AbstractCreature abstractPlayer in room.game.Players)
            {
                if (abstractPlayer.state.alive)
                {
                    anyPlayerAlive = true;
                    if (abstractPlayer.realizedCreature is Player player && player.FoodInRoom(room, false) >= player.slugcatStats.foodToHibernate)
                    {
                        room.game.Win(false);
                        return;
                    }
                }
            }

            if (anyPlayerAlive)
            {
                room.game.Win(true);
            }
            else
            {
                room.game.GoToDeathScreen();
            }
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void Room_AddObject(On.Room.orig_AddObject orig, Room room, UpdatableAndDeletable uAD)
        {
            // changed if statement bc otherwise objects might not being updated when a ROOM MISMATCH occurs
            // RemoveObject does not always remove instantly // prevent duplicates
            if (room.game != null && room.updateList.Contains(uAD))
            {
                room.updateList.Remove(uAD);
            }
            orig(room, uAD);
        }

        private static void Room_ctor(On.Room.orig_ctor orig, Room room, RainWorldGame game, World world, AbstractRoom abstractRoom)
        {
            orig(room, game, world, abstractRoom);
            if (abstractRoom.name == "SB_L01")
            {
                JollyCoop.PlayerHK.appliedGhost = true; // skip JollyCoop's room script
            }
        }

        private static ChunkSoundEmitter Room_PlaySound(On.Room.orig_PlaySound_SoundID_BodyChunk_bool_float_float orig, Room room, SoundID soundId, BodyChunk chunk, bool loop, float vol, float pitch)
        {
            // prevent sound spam when player is offscreen
            bool allCamerasOutsideRoom = true;
            foreach (RoomCamera roomCamera in room.game.cameras)
            {
                if (roomCamera.room == room)
                {
                    allCamerasOutsideRoom = false;
                    break;
                }
            }

            if (allCamerasOutsideRoom || soundId == SoundID.Mushroom_Trip_LOOP)
            {
                soundId = SoundID.None;
            }
            return orig(room, soundId, chunk, loop, vol, pitch);
        }

        private static void Room_Update_JollyCoop(On.Room.orig_Update orig, Room room)
        {
            // collision between slugcats (and creatures that are being carried by slugcats)
            if (MainMod.isSlugcatCollisionEnabled || room.game == null)
            {
                orig(room);
                return;
            }

            if (creaturesInRoomList.Count > 0) // had a problem with DeerFix when throwing puff balls // orig(room) never returned
            {
                Debug.Log("JollyCoopFixesAndStuff: Slugcat collisions could not be reset normally. Reset now.");
                foreach (Creature creature in creaturesInRoomList)
                {
                    creature.CollideWithObjects = true;
                }
                creaturesInRoomList.Clear();
            }

            // disable collision for now and handle collision manually after calling orig();
            foreach (AbstractCreature abstractPlayer in room.game.Players)
            {
                if (abstractPlayer.Room == room.abstractRoom && abstractPlayer.realizedCreature is Player player)
                {
                    foreach (Creature.Grasp? grasp in player.grasps)
                    {
                        if (grasp?.grabbed is Creature creature && player.Grabability(creature) != Player.ObjectGrabability.Drag)
                        {
                            creaturesInRoomList.Add(creature);
                            creature.CollideWithObjects = false;
                        }
                    }

                    // seems like CollideWithObjects is not enough;
                    // need to check iAmBeingCarried too;
                    // otherwise backPlayers can collide with creatures that are being eating;
                    if (player.CollideWithObjects && !JollyCoop.PlayerHK.iAmBeingCarried[player.playerState.playerNumber])
                    {
                        creaturesInRoomList.Add(player);
                        player.CollideWithObjects = false;
                    }
                }
            }
            orig(room);

            foreach (Creature creatureA in creaturesInRoomList)
            {
                creatureA.CollideWithObjects = true;
                foreach (PhysicalObject physicalObjectB in room.physicalObjects[creatureA.collisionLayer])
                {
                    // disable collision of players and creatures that they are carrying;
                    // including creatures that backPlayers are carrying;
                    if ((physicalObjectB is not Creature creatureB || !creaturesInRoomList.Contains(creatureB)) && Mathf.Abs(creatureA.bodyChunks[0].pos.x - physicalObjectB.bodyChunks[0].pos.x) < creatureA.collisionRange + physicalObjectB.collisionRange && Mathf.Abs(creatureA.bodyChunks[0].pos.y - physicalObjectB.bodyChunks[0].pos.y) < creatureA.collisionRange + physicalObjectB.collisionRange)
                    {
                        bool hasCollided = false;
                        bool isGrabbed = false;

                        // is grabbing;
                        // only remaining case where this is needed is when
                        // the player drags a creature;
                        if (creatureA.Template.grasps > 0)
                        {
                            foreach (Creature.Grasp? grasp in creatureA.grasps)
                            {
                                if (grasp != null && grasp.grabbed == physicalObjectB)
                                {
                                    isGrabbed = true;
                                    break;
                                }
                            }
                        }

                        // is being grabbed;
                        // creatureB_.Template.grasps > 0 takes also care of creatureB_.grasps != null;
                        if (!isGrabbed && physicalObjectB is Creature creatureB_ && creatureB_.Template.grasps > 0)
                        {
                            foreach (Creature.Grasp? grasp in creatureB_.grasps)
                            {
                                if (grasp != null && grasp.grabbed == creatureA)
                                {
                                    isGrabbed = true;
                                    break;
                                }
                            }
                        }

                        if (!isGrabbed)
                        {
                            foreach (BodyChunk playerBodyChunk in creatureA.bodyChunks)
                            {
                                foreach (BodyChunk pOBodyChunk in physicalObjectB.bodyChunks)
                                {
                                    if (playerBodyChunk.collideWithObjects && pOBodyChunk.collideWithObjects && Custom.DistLess(playerBodyChunk.pos, pOBodyChunk.pos, playerBodyChunk.rad + pOBodyChunk.rad))
                                    {
                                        float radiusCombined = playerBodyChunk.rad + pOBodyChunk.rad;
                                        float distance = Vector2.Distance(playerBodyChunk.pos, pOBodyChunk.pos);
                                        Vector2 direction = Custom.DirVec(playerBodyChunk.pos, pOBodyChunk.pos);
                                        float massProportion = pOBodyChunk.mass / (playerBodyChunk.mass + pOBodyChunk.mass);

                                        playerBodyChunk.pos -= (radiusCombined - distance) * direction * massProportion;
                                        playerBodyChunk.vel -= (radiusCombined - distance) * direction * massProportion;
                                        pOBodyChunk.pos += (radiusCombined - distance) * direction * (1f - massProportion);
                                        pOBodyChunk.vel += (radiusCombined - distance) * direction * (1f - massProportion);

                                        if (playerBodyChunk.pos.x == pOBodyChunk.pos.x)
                                        {
                                            playerBodyChunk.vel += Custom.DegToVec(Random.value * 360f) * 0.0001f;
                                            pOBodyChunk.vel += Custom.DegToVec(Random.value * 360f) * 0.0001f;
                                        }

                                        if (!hasCollided)
                                        {
                                            creatureA.Collide(physicalObjectB, playerBodyChunk.index, pOBodyChunk.index);
                                            physicalObjectB.Collide(creatureA, pOBodyChunk.index, playerBodyChunk.index);
                                        }
                                        hasCollided = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            creaturesInRoomList.Clear();
        }

        private static void Room_Update_ShelterBehaviors(On.Room.orig_Update orig, Room room)
        {
            orig(room);
            if (room.abstractRoom.shelter) // MainMod.isShelterBehaviorsEnabled is always true
            {
                for (int uADIndex = room.updateList.Count - 1; uADIndex >= 0; --uADIndex)
                {
                    if (room.updateList[uADIndex] is ShelterBehaviors.ShelterBehaviorManager shelterBehaviorManager)
                    {
                        if (!WorldMod.moddedShelters.Contains(room.abstractRoom.name))
                        {
                            Debug.Log("JollyCoopFixesAndStuff: Shelter " + room.abstractRoom.name + " is modded.");
                            WorldMod.moddedShelters.Add(room.abstractRoom.name);
                        }

                        // shelterbehaviors assumes hard mode under certain conditions => intercept;
                        // otherwise you get send to the death screen directly if someone is dead;
                        if (MainMod.isEasyModeEnabled && shelterBehaviorManager.closing && shelterBehaviorManager.hasNoDoors && !shelterBehaviorManager.broken && room.game.Players.Count > 1)
                        {
                            Debug.Log("JollyCoopFixesAndStuff: ShelterBehaviors is closing shelter. Intercept and finish cycle.");
                            DoorClosed(room);
                            room.RemoveObject(shelterBehaviorManager);
                        }
                    }
                }
            }
        }
    }
}