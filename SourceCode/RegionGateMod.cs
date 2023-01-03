using System.Collections.Generic;

namespace JollyCoopFixesAndStuff
{
    internal static class RegionGateMod
    {
        private static readonly List<RoomCamera> moveCameraList = new();
        private static readonly List<AbstractCreature> moveCreaturesList = new();

        internal static void OnEnable()
        {
            On.RegionGate.KarmaBlinkRed += RegionGate_KarmaBlinkRed; // fix bug
            On.RegionGate.PlayersInZone += RegionGate_PlayersInZone; // fix bug
            On.RegionGate.PlayersStandingStill += RegionGate_PlayersStandingStill; // fix bug
            On.RegionGate.Update += RegionGate_Update; // teleport dead players in easy mode to new region
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static bool RegionGate_KarmaBlinkRed(On.RegionGate.orig_KarmaBlinkRed orig, RegionGate regionGate)
        {
            // got NullReferenceException spam from this while playing coop // player was maybe not realized
            if (regionGate.room.game.Players[0].realizedCreature == null)
            {
                return false;
            }
            return orig(regionGate);
        }

        private static int RegionGate_PlayersInZone(On.RegionGate.orig_PlayersInZone orig, RegionGate regionGate)
        {
            if (regionGate.room.game.Players.Count <= 1)
            {
                return orig(regionGate);
            }

            // move cameras instead of this
            //foreach (RoomCamera roomCamera in regionGate.room.game.cameras)
            //{
            //    if (roomCamera.room != regionGate.room)
            //    {
            //        return -1;
            //    }
            //}

            int? allAlivePlayerPosition = null; // acts as allPlayerPosition when easy mode is not enabled
            foreach (AbstractCreature abstractPlayer in regionGate.room.game.Players)
            {
                if (abstractPlayer.state.alive || !MainMod.isEasyModeEnabled)
                {
                    int specificPlayerPosition = regionGate.DetectZone(abstractPlayer);
                    allAlivePlayerPosition ??= specificPlayerPosition;

                    if (specificPlayerPosition != allAlivePlayerPosition)
                    {
                        return -1;
                    }
                }
            }
            return allAlivePlayerPosition ?? -1;
        }

        private static bool RegionGate_PlayersStandingStill(On.RegionGate.orig_PlayersStandingStill orig, RegionGate regionGate)
        {
            // ignore inputs
            return true;

            //if (regionGate.room.game.Players.Count <= 1)
            //{
            //    return orig(regionGate);
            //}

            //foreach (AbstractCreature abstractPlayer in regionGate.room.game.Players)
            //{
            //    // check alive // players in dens might otherwise block region gates since touchedNoInputCounter gets frozen
            //    // why does JollyCoop freezes touchedNoInputCounter when piggy back riding // ignore that case for now
            //    if (abstractPlayer.state.alive && abstractPlayer.realizedCreature is Player player && player.touchedNoInputCounter < 20 && !JollyCoop.PlayerHK.iAmBeingCarried[player.playerState.playerNumber])
            //    {
            //        return false;
            //    }
            //}
            //return true;
        }

        private static void RegionGate_Update(On.RegionGate.orig_Update orig, RegionGate regionGate, bool eu)
        {
            if (MainMod.isEasyModeEnabled && regionGate.room.game.Players.Count > 1)
            {
                switch (regionGate.mode)
                {
                    case RegionGate.Mode.MiddleClosed:
                        if (regionGate.startCounter >= 60)
                        {
                            foreach (AbstractCreature abstractPlayer in regionGate.room.game.Players)
                            {
                                // teleporting dead players, otherwise saving in the new region might get bugged
                                if (abstractPlayer.state.dead && abstractPlayer.Room != regionGate.room.abstractRoom)
                                {
                                    AbstractCreatureMod.DropOrDestroyAllObjects(abstractPlayer);
                                    AbstractCreatureMod.Teleport(abstractPlayer, regionGate.room.abstractRoom);
                                    AbstractCreatureMod.LogPlayer(abstractPlayer);
                                    moveCreaturesList.Add(abstractPlayer);
                                }
                            }

                            // move cameras instead of not opening the gate when cameras are outside of regionGate.room
                            foreach (RoomCamera roomCamera in regionGate.room.game.cameras)
                            {
                                if (roomCamera.room != regionGate.room)
                                {
                                    moveCameraList.Add(roomCamera);
                                }
                            }
                        }
                        else
                        {
                            if (moveCreaturesList.Count > 0)
                            {
                                moveCreaturesList.Clear();
                            }

                            if (moveCameraList.Count > 0)
                            {
                                moveCameraList.Clear();
                            }
                        }
                        break;
                    case RegionGate.Mode.ClosingAirLock:
                        // looping over regionGate.room.game.cameras might spam MoveCamera()
                        foreach (RoomCamera roomCamera in moveCameraList)
                        {
                            roomCamera.MoveCamera(regionGate.room, 0); // assuming that region gates only have one camera position
                        }

                        moveCameraList.Clear();
                        break;
                    case RegionGate.Mode.Waiting:
                        if (!regionGate.waitingForWorldLoader)
                        {
                            RainWorldGameMod.ShareAllRelationships(regionGate.room.game);
                            if (moveCreaturesList.Count > 0)
                            {
                                // move teleported (dead) players to offscreen den
                                foreach (AbstractCreature abstractCreature in moveCreaturesList)
                                {
                                    AbstractCreatureMod.Teleport(abstractCreature, regionGate.room.world.offScreenDen);
                                    AbstractCreatureMod.LogPlayer(abstractCreature);
                                }
                                moveCreaturesList.Clear();
                            }
                        }
                        break;
                }
            }

            if (regionGate.mode == RegionGate.Mode.MiddleClosed)
            {
                regionGate.room.game.Players[0].Realize(); // make sure karma can be accessed via player 1
            }
            orig(regionGate, eu);
        }
    }
}