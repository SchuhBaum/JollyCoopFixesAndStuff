using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    internal static class PlayerMod
    {
        internal static void OnEnable()
        {
            On.Player.AddFood += Player_AddFood; // share food fix
            On.Player.BiteEdibleObject += Player_BiteEdibleObject; // adds eat sound to mushrooms
            On.Player.CanIPickThisUp += Player_CanIPickThisUp; // no more stealing from other player // remove blinking when you cannot pickup items // don't grab (dead) backPlayers
            On.Player.ctor += Player_ctor; // fix bug when realizing player with swallowed creature from different region

            On.Player.FoodInRoom_bool += Player_FoodInRoom; // don't consume food with player 2 to 4 when hibernating
            On.Player.Regurgitate += Player_Regurgitate;
            On.Player.Update += Player_Update; // avoid offscreen player sprites related bug by removing it // sync mushroom counter between player // only drop backPlayers when holding down // release grasp from other players when pressing jump
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void Player_AddFood(On.Player.orig_AddFood orig, Player player, int add)
        {
            orig(player, add);
            foreach (AbstractCreature abstractPlayer in player.abstractCreature.world.game.Players) // player.room can be null in some situations
            {
                PlayerState playerState = (PlayerState)abstractPlayer.state;
                if (playerState.foodInStomach < player.FoodInStomach)
                {
                    playerState.foodInStomach = player.FoodInStomach; // try to fix food sharing as early as possible
                }
            }
        }

        private static void Player_BiteEdibleObject(On.Player.orig_BiteEdibleObject orig, Player player, bool eu)
        {
            foreach (Creature.Grasp grasp in player.grasps)
            {
                if (grasp?.grabbed is Mushroom)
                {
                    player.room?.PlaySound(SoundID.Slugcat_Bite_Dangle_Fruit, player.mainBodyChunk);
                    break;
                }
            }
            orig(player, eu);
        }

        private static bool Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player player, PhysicalObject physicalObject)
        {
            // don't grab (dead) backPlayers
            if (physicalObject is Player player_)
            {
                foreach (JollyCoop.PlayerHK.PlayerOnBack? playerOnBack in JollyCoop.PlayerHK.backPlayers)
                {
                    if (playerOnBack != null && playerOnBack.playerOnBack == player_)
                    {
                        return false;
                    }
                }
            }

            foreach (Creature.Grasp? grasp in physicalObject.grabbedBy)
            {
                if (grasp?.grabber is Player)
                {
                    return false;
                }
            }

            Player.ObjectGrabability objectGrabability = player.Grabability(physicalObject);
            bool canGrabOneHanded = player.grasps[0] == null || player.grasps[1] == null;

            if (objectGrabability == Player.ObjectGrabability.OneHand && !canGrabOneHanded)
            {
                return false;
            }
            else if (objectGrabability == Player.ObjectGrabability.BigOneHand && !(player.CanPutSpearToBack || canGrabOneHanded && player.grasps[0]?.grabbed is not Spear && player.grasps[1]?.grabbed is not Spear))
            {
                return false;
            }
            return orig(player, physicalObject);
        }

        private static void Player_ctor(On.Player.orig_ctor orig, Player player, AbstractCreature abstractCreature, World world)
        {
            orig(player, abstractCreature, world);

            // match slugcat character with player number // helps linking players to their corresponding fancy slugcats // prevents softlock when JollyCoop's player ability option is not set correctly
            if (MainMod.matchCharacterWithPlayerNumber)
            {
                PlayerState playerState = player.playerState;
                int playerNumber = playerState.playerNumber;
                playerState.slugcatCharacter = playerNumber;

                if (playerNumber >= 0 && playerNumber < JollyCoop.JollyMod.config.playerCharacters.Length)
                {
                    JollyCoop.JollyMod.config.playerCharacters[playerNumber] = playerNumber;
                }
            }
        }

        private static int Player_FoodInRoom(On.Player.orig_FoodInRoom_bool orig, Player player, bool eatAndDestroy)
        {
            if (player.playerState.playerNumber != 0)
            {
                return 0;
            }
            return orig(player, eatAndDestroy);
        }

        private static void Player_Regurgitate(On.Player.orig_Regurgitate orig, Player player)
        {
            // the room number of a swallowed creature AI might be in an unloaded region // update position
            if (player.objectInStomach is AbstractCreature objectInStomach && objectInStomach.abstractAI is AbstractCreatureAI abstractAI && abstractAI.world != objectInStomach.world)
            {
                abstractAI.world = objectInStomach.world;
                Debug.Log("JollyCoopFixesAndStuff: objectInStomach " + objectInStomach);
                Debug.Log("JollyCoopFixesAndStuff: Match objectInStomach.abstractAI.world with objectInStomach.world.");
            }
            orig(player);
        }

        private static void Player_Update(On.Player.orig_Update orig, Player player, bool eu)
        {
            int playerNumber = player.playerState.playerNumber;

            // I had some NullReferenceException spam from JollyOffScreen or frozen offscreen player symbols // remove them for now
            if (!JollyCoop.PlayerHK.hasOffscreenHolo[playerNumber])
            {
                JollyCoop.PlayerHK.hasOffscreenHolo[playerNumber] = true;
            }

            // the PlayerPointers are based on camera 1 and can conflict with the split screen mod
            if (!MainMod.hasPlayerPointers && !JollyCoop.PlayerHK.hasPointer[playerNumber])
            {
                JollyCoop.PlayerHK.hasPointer[playerNumber] = true;
            }

            player.deaf = 0; // this sound loop can get stuck // disable for now
            orig(player, eu);

            if (player.mushroomCounter > 0)
            {
                // synchronize with other player // player in shortcuts don't update the mushroom counter on their own
                foreach (AbstractCreature abstractPlayer in player.abstractCreature.world.game.Players)
                {
                    if (abstractPlayer.realizedCreature is Player player_ && player_.inShortcut)
                    {
                        player_.mushroomCounter = player.mushroomCounter;
                    }
                }
            }

            JollyCoop.PlayerHK.PlayerOnBack playerOnBack = JollyCoop.PlayerHK.backPlayers[player.playerState.playerNumber];
            if (playerOnBack.HasAPlayer && player.input[0].y != -1)
            {
                playerOnBack.increment = false;
            }

            if (player.input[0].jmp && !player.input[1].jmp && player.grabbedBy?.Count > 0)
            {
                for (int graspIndex = player.grabbedBy.Count - 1; graspIndex >= 0; graspIndex--)
                {
                    if (player.grabbedBy[graspIndex] is Creature.Grasp grasp && grasp.grabber is Player player_)
                    {
                        player_.ReleaseGrasp(grasp.graspUsed); // list is modified
                    }
                }
            }
        }
    }
}