using System.Collections.Generic;
using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    public static class RoomRealizerMod
    {
        private static int shaveDownPerformanceCooldown = 0;

        internal static void OnEnable()
        {
            On.RoomRealizer.CheckForAndDeleteDistantRooms += RoomRealizer_CheckForAndDeleteDistantRooms; // check for other players as well before unloading rooms
            On.RoomRealizer.RemoveNotVisitedRooms += RoomRealizer_RemoveNotVisitedRooms; // check for other players as well before unloading rooms
            On.RoomRealizer.ShaveDownPerformanceTo += RoomRealizer_ShaveDownPerformanceTo; // check for other players as well before unloading rooms
            On.RoomRealizer.Update += RoomRealizer_Update; // load rooms for offscreen player
        }

        // ---------------- //
        // public functions //
        // ---------------- //

        public static void TakeRoomRealizer(RoomRealizer? roomRealizer, AbstractCreature? abstractCreature)
        {
            if (roomRealizer == null || abstractCreature == null || abstractCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat)
            {
                return;
            }

            if (roomRealizer.followCreature != abstractCreature)
            {
                roomRealizer.followCreature = abstractCreature;
                roomRealizer.lastFrameFollowCreatureRoom = abstractCreature.pos.room; // don't unload not-visited rooms when switching cameras
            }
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void RoomRealizer_CheckForAndDeleteDistantRooms(On.RoomRealizer.orig_CheckForAndDeleteDistantRooms orig, RoomRealizer roomRealizer)
        {
            if (roomRealizer.world.game.Players.Count <= 1)
            {
                orig(roomRealizer);
                return;
            }

            if (roomRealizer.followCreature.Room.realizedRoom == null || !roomRealizer.followCreature.Room.realizedRoom.fullyLoaded)
            {
                return;
            }

            RoomRealizer.RealizedRoomTracker realizedRoom = roomRealizer.realizedRooms[Random.Range(0, roomRealizer.realizedRooms.Count)];
            if (realizedRoom.room == roomRealizer.probableNextRoom || !roomRealizer.CanAbstractizeRoom(realizedRoom))
            {
                return;
            }

            // check for other players as well
            foreach (AbstractCreature abstractPlayer in roomRealizer.world.game.Players)
            {
                if (abstractPlayer.Room == realizedRoom.room)
                {
                    return;
                }
            }

            for (int index1 = 0; index1 < realizedRoom.room.connections.Length; index1++)
            {
                if (realizedRoom.room.connections[index1] > -1)
                {
                    AbstractRoom abstractRoom1 = roomRealizer.world.GetAbstractRoom(realizedRoom.room.connections[index1]);
                    if (abstractRoom1 == roomRealizer.probableNextRoom)
                    {
                        return;
                    }

                    foreach (AbstractCreature abstractPlayer in roomRealizer.world.game.Players)
                    {
                        if (abstractRoom1 == abstractPlayer.Room)
                        {
                            return;
                        }
                    }

                    if (roomRealizer.RoomPerformanceEstimation(realizedRoom.room) < roomRealizer.performanceBudget / 4.0)
                    {
                        for (int index2 = 0; index2 < abstractRoom1.connections.Length; index2++)
                        {
                            AbstractRoom abstractRoom2 = roomRealizer.world.GetAbstractRoom(abstractRoom1.connections[index2]);
                            if (abstractRoom2 == roomRealizer.probableNextRoom)
                            {
                                return;
                            }

                            foreach (AbstractCreature abstractPlayer in roomRealizer.world.game.Players)
                            {
                                if (abstractRoom2 == abstractPlayer.Room)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            Debug.Log("JollyCoopFixesAndStuff: Kill distant room " + realizedRoom.room.name + ".");
            roomRealizer.KillRoom(realizedRoom.room);
            roomRealizer.realizedRooms.Remove(realizedRoom);
        }

        private static void RoomRealizer_RemoveNotVisitedRooms(On.RoomRealizer.orig_RemoveNotVisitedRooms orig, RoomRealizer roomRealizer)
        {
            if (roomRealizer.world.game.Players.Count <= 1)
            {
                orig(roomRealizer);
                return;
            }

            // RoomRealizer only tracks one player at a time and might unload rooms where other players are in
            foreach (RoomRealizer.RealizedRoomTracker realizedRoomTracker in roomRealizer.realizedRooms)
            {
                foreach (AbstractCreature abstractPlayer in roomRealizer.world.game.Players)
                {
                    if (realizedRoomTracker.room == abstractPlayer.Room)
                    {
                        realizedRoomTracker.hasBeenVisited = true;
                    }
                }
            }
            orig(roomRealizer);
        }

        private static void RoomRealizer_ShaveDownPerformanceTo(On.RoomRealizer.orig_ShaveDownPerformanceTo orig, RoomRealizer roomRealizer, float currentPerf, float goalPerformance, ref List<RoomRealizer.RealizedRoomTracker> candidates)
        {
            if (roomRealizer.world.game.Players.Count <= 1)
            {
                orig(roomRealizer, currentPerf, goalPerformance, ref candidates);
                return;
            }

            if (shaveDownPerformanceCooldown > 0)
            {
                return;
            }

            Debug.Log("JollyCoopFixesAndStuff.RoomRealizer_ShaveDownPerformanceTo: CurrentPerf is " + currentPerf + ". GoalPerformance is " + goalPerformance + ".");
            shaveDownPerformanceCooldown = 200;

            for (int rRTIndex = candidates.Count - 1; rRTIndex >= 0; --rRTIndex)
            {
                RoomRealizer.RealizedRoomTracker realizedRoomTracker = candidates[rRTIndex];
                foreach (AbstractCreature abstractPlayer in roomRealizer.world.game.Players)
                {
                    if (realizedRoomTracker.room == abstractPlayer.Room)
                    {
                        candidates.Remove(realizedRoomTracker);
                    }
                }
            }
            orig(roomRealizer, currentPerf, goalPerformance, ref candidates);
        }

        private static void RoomRealizer_Update(On.RoomRealizer.orig_Update orig, RoomRealizer roomRealizer)
        {
            if (shaveDownPerformanceCooldown > 0)
            {
                --shaveDownPerformanceCooldown;
            }

            // otherwise: larger rooms might not get loaded at all until entered
            if (roomRealizer.probableNextRoom is AbstractRoom probableNextRoom && probableNextRoom.realizedRoom == null && roomRealizer.currentlyLoadingRoom == null && !roomRealizer.IsRoomRecentlyAbstracted(probableNextRoom))
            {
                roomRealizer.RealizeAndTrackRoom(probableNextRoom, false);
            }

            foreach (AbstractCreature abstractPlayer in roomRealizer.world.game.Players)
            {
                // the room might not load otherwise when carried around by creatures
                if (roomRealizer.followCreature != abstractPlayer && !abstractPlayer.InDen && abstractPlayer.Room.realizedRoom == null)
                {
                    roomRealizer.RealizeAndTrackRoom(abstractPlayer.Room, true);
                }
            }
            orig(roomRealizer);
        }
    }
}