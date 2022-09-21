using System.Collections.Generic;
using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    public static class AbstractCreatureMod
    {
        internal static void OnEnable()
        {
            On.AbstractCreature.Die += AbstractCreature_Die; // consider dead player as downed
        }

        // ---------------- //
        // public functions //
        // ---------------- //

        // can be used while in a shortcut // possible setup for teleportation 
        public static void DropAllPlayers(AbstractCreature? abstractCreature)
        {
            if (abstractCreature == null)
            {
                return;
            }

            ShortcutHandler shortcutHandler = abstractCreature.world.game.shortcuts;
            ShortcutHandler.Vessel? vessel = ShortcutHandlerMod.GetVessel(shortcutHandler, abstractCreature);

            if (vessel?.creature != abstractCreature.realizedCreature)
            {
                ShortcutHandlerMod.CopyAndAddVessel(shortcutHandler, abstractCreature.realizedCreature, vessel);
            }

            for (int aOSIndex = abstractCreature.stuckObjects.Count - 1; aOSIndex >= 0; --aOSIndex)
            {
                AbstractPhysicalObject.AbstractObjectStick abstractObjectStick = abstractCreature.stuckObjects[aOSIndex];
                if (abstractObjectStick.A == abstractCreature && abstractObjectStick.B is AbstractCreature abstractCreatureB && abstractCreatureB.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
                {
                    ShortcutHandlerMod.CopyAndAddVessel(shortcutHandler, abstractCreatureB.realizedCreature, vessel); // drop dragged creatures into shortcut
                    AbstractPhysicalObjectMod.ReleaseGrasp(abstractObjectStick);
                }
                else if (abstractObjectStick.B == abstractCreature && abstractObjectStick.A is AbstractCreature abstractCreatureA && abstractCreatureA.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
                {
                    AbstractPhysicalObjectMod.ReleaseGrasp(abstractObjectStick);
                }
            }
        }

        // can be used while in a shortcut // possible setup for teleportation 
        public static void DropOrDestroyAllObjects(AbstractCreature? abstractCreature)
        {
            if (abstractCreature == null)
            {
                return;
            }

            ShortcutHandler shortcutHandler = abstractCreature.world.game.shortcuts;
            ShortcutHandler.Vessel? vessel = ShortcutHandlerMod.GetVessel(shortcutHandler, abstractCreature);

            if (vessel?.creature != abstractCreature.realizedCreature)
            {
                ShortcutHandlerMod.CopyAndAddVessel(shortcutHandler, abstractCreature.realizedCreature, vessel);
            }

            for (int aOSIndex = abstractCreature.stuckObjects.Count - 1; aOSIndex >= 0; --aOSIndex)
            {
                AbstractPhysicalObject.AbstractObjectStick abstractObjectStick = abstractCreature.stuckObjects[aOSIndex];
                AbstractPhysicalObjectMod.DropOrDestroySpear(abstractObjectStick, destroy: vessel != null); // destroy if dropped while being in a shortcut

                if (abstractObjectStick.A == abstractCreature && abstractObjectStick.B is AbstractCreature abstractCreatureB)
                {
                    ShortcutHandlerMod.CopyAndAddVessel(shortcutHandler, abstractCreatureB.realizedCreature, vessel);
                }
                AbstractPhysicalObjectMod.ReleaseGrasp(abstractObjectStick); // works both ways: I let other things go and they let go of me
            }
        }

        public static void DropPlayerOnBack(AbstractCreature? abstractCreature)
        {
            if (abstractCreature == null)
            {
                return;
            }

            if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
            {
                int playerNumber = ((PlayerState)abstractCreature.state).playerNumber;
                if (playerNumber < JollyCoop.PlayerHK.backPlayers.Length && JollyCoop.PlayerHK.backPlayers[playerNumber] is JollyCoop.PlayerHK.PlayerOnBack playerOnBack && playerOnBack.HasAPlayer)
                {
                    playerOnBack.DropPlayer();
                }
            }
        }

        public static void LogPlayer(AbstractCreature? abstractCreature)
        {
            if (abstractCreature == null)
            {
                return;
            }

            Debug.Log("JollyCoopFixesAndStuff: ----------------------------------------");
            PlayerState playerState = (PlayerState)abstractCreature.state;
            Debug.Log("JollyCoopFixesAndStuff: Slugcat " + abstractCreature.ID);
            Debug.Log("JollyCoopFixesAndStuff: pos " + abstractCreature.pos);
            Debug.Log("JollyCoopFixesAndStuff: alive " + playerState.alive);
            Debug.Log("JollyCoopFixesAndStuff: foodInStomach " + playerState.foodInStomach);

            if (abstractCreature.realizedCreature is Player player)
            {
                Debug.Log("JollyCoopFixesAndStuff: room?.abstractRoom.name " + player.room?.abstractRoom.name);
                Debug.Log("JollyCoopFixesAndStuff: mainBodyChunk.pos " + player.mainBodyChunk.pos);

                if (player.grasps[0] != null)
                {
                    Debug.Log("JollyCoopFixesAndStuff: grasps[0].grabbed " + player.grasps[0].grabbed);
                }

                if (player.grasps[1] != null)
                {
                    Debug.Log("JollyCoopFixesAndStuff: grasps[1].grabbed " + player.grasps[1].grabbed);
                }

                if (player.objectInStomach != null)
                {
                    Debug.Log("JollyCoopFixesAndStuff: objectInStomach " + player.objectInStomach);
                }
                Debug.Log("JollyCoopFixesAndStuff: inShortcut " + player.inShortcut);
            }
            else
            {
                Debug.Log("JollyCoopFixesAndStuff: Room.name " + abstractCreature.Room.name);
            }
            Debug.Log("JollyCoopFixesAndStuff: ----------------------------------------");
        }

        //public static void Revive(AbstractCreature? abstractCreature)
        //{
        //    if (abstractCreature == null)
        //    {
        //        return;
        //    }

        //    Debug.Log("JollyCoopFixesAndStuff: Revive " + abstractCreature + ".");
        //    abstractCreature.state.alive = true;

        //    if (abstractCreature.realizedCreature != null)
        //    {
        //        abstractCreature.realizedCreature.dead = false;
        //    }

        //    if (abstractCreature.state is PlayerState state)
        //    {
        //        JollyCoop.PlayerHK.downed[state.playerNumber] = false;
        //        JollyCoop.RainWorldGameHK.permaDead[state.playerNumber] = false;
        //    }
        //}

        public static void Teleport(AbstractCreature? abstractCreature, AbstractRoom? abstractRoom, int shortcutNode = -1)
        {
            if (abstractCreature == null || abstractRoom == null)
            {
                return;
            }

            if (shortcutNode != -1 && (abstractRoom.realizedRoom == null || !abstractRoom.realizedRoom.shortCutsReady))
            {
                Debug.Log("JollyCoopFixesAndStuff: Wait until shortcuts are ready.");
                ShortcutHandlerMod.shortCutsReadyWaitingQueue.Add(new ShortcutHandlerMod.TeleportationVesselMod(abstractCreature, abstractRoom, shortcutNode));
                return;
            }

            if (abstractCreature.realizedCreature is Creature creature)
            {
                ShortcutHandler shortcutHandler = creature.abstractCreature.world.game.shortcuts;
                ShortcutHandlerMod.RemoveVessel(shortcutHandler, creature, vessel: ShortcutHandlerMod.GetVessel(shortcutHandler, creature.abstractCreature)); // get creature out of shortcuts // only removes if creature owns the vessel
                creature.inShortcut = false; // otherwise player inputs might get ignored
            }
            AbstractPhysicalObjectMod.Teleport(abstractCreature, AbstractRoomMod.GetShortcutCoordinates(abstractRoom, shortcutNode));
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void AbstractCreature_Die(On.AbstractCreature.orig_Die orig, AbstractCreature abstractCreature)
        {
            orig(abstractCreature);
            // problem: the game over screen (in easy mode) does not always trigger when all players are dead
            // downed/dead and permaDead seem to be different // can a permaDead player not be downed/dead?
            if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
            {
                RainWorldGame game = abstractCreature.world.game;
                List<AbstractCreature> abstractPlayerWithoutCameraOrLastPlayerList = new(); // switch camera to a living slugcat if this dying slugcat has a camera
                RainWorldGameMod.ReturnHUD(game, abstractCreature); // when using the cycling camera setting // the player might die without a camera but with a HUD

                // realizing a player resets the downed status // update downed status for all players
                foreach (AbstractCreature abstractPlayer in game.Players)
                {
                    int playerNumber = ((PlayerState)abstractPlayer.state).playerNumber;
                    if (abstractPlayer.state.alive)
                    {
                        abstractPlayerWithoutCameraOrLastPlayerList.Add(abstractPlayer); // first: add all living player

                    }
                    else if (!JollyCoop.PlayerHK.downed[playerNumber])
                    {
                        JollyCoop.PlayerHK.downed[playerNumber] = true;
                    }
                }

                List<RoomCamera> playerCameras = new();
                foreach (RoomCamera roomCamera in game.cameras)
                {
                    if (abstractPlayerWithoutCameraOrLastPlayerList.Contains(roomCamera.followAbstractCreature) && abstractPlayerWithoutCameraOrLastPlayerList.Count > 1) // second: remove player with a camera // always leave one in // the last one gets all cameras
                    {
                        abstractPlayerWithoutCameraOrLastPlayerList.Remove(roomCamera.followAbstractCreature);
                    }

                    if (roomCamera.followAbstractCreature == abstractCreature)
                    {
                        playerCameras.Add(roomCamera);
                    }
                }

                foreach (RoomCamera roomCamera in playerCameras)
                {
                    if (abstractPlayerWithoutCameraOrLastPlayerList.Count > 0)
                    {
                        Debug.Log("JollyCoopFixesAndStuff: Automatically switch camera from " + abstractCreature + " to " + abstractPlayerWithoutCameraOrLastPlayerList[0] + ".");

                        // ReturnHUD might be needed when using the cycling camera setting // the new player might also have a HUD without being followed by a camera // need to make sure that abstractPlayerWithoutCameraList[0] has at most one HUD
                        RainWorldGameMod.ReturnHUD(game, abstractPlayerWithoutCameraOrLastPlayerList[0]);
                        RainWorldGameMod.TakeRoomCameraAndHUD(game, roomCamera, abstractPlayerWithoutCameraOrLastPlayerList[0]);
                        abstractPlayerWithoutCameraOrLastPlayerList.RemoveAt(0);
                    }
                }
            }
        }
    }
}