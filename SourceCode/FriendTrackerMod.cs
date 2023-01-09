using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    internal static class FriendTrackerMod
    {
        internal static void OnEnable()
        {
            On.FriendTracker.GiftRecieved += FriendTracker_GiftRecieved; // improve relationships for all players when feeding lizards
            On.FriendTracker.ItemOffered += FriendTracker_ItemOffered; // other players cannot be offered to lizards // otherwise you could reuse them when relationships are shared // lizards drop dead slugcats when they become friends
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void FriendTracker_GiftRecieved(On.FriendTracker.orig_GiftRecieved orig, FriendTracker friendTracker)
        {
            if (!MainMod.isSharedRelationshipsEnabled || friendTracker.AI.creature.world.game.Players.Count <= 1)
            {
                orig(friendTracker);
                return;
            }

            SocialEventRecognizer.OwnedItemOnGround giftOfferedToMe = friendTracker.giftOfferedToMe;
            orig(friendTracker);

            if (giftOfferedToMe != null && friendTracker.giftOfferedToMe == null && giftOfferedToMe.owner is Player player)
            {
                foreach (AbstractCreature abstractPlayer in player.abstractCreature.world.game.Players)
                {
                    if (abstractPlayer.realizedCreature is Player player_ && player_ != player && friendTracker.AI is FriendTracker.IHaveFriendTracker iHaveFriendTracker)
                    {
                        Debug.Log("JollyCoopFixesAndStuff: Gift recieved from " + player_ + ".");
                        giftOfferedToMe.owner = player_;
                        iHaveFriendTracker.GiftRecieved(giftOfferedToMe);
                    }
                }
            }
        }

        private static void FriendTracker_ItemOffered(On.FriendTracker.orig_ItemOffered orig, FriendTracker friendTracker, Tracker.CreatureRepresentation creatureRepresentation, PhysicalObject item)
        {
            if (item is Player) return;
            orig(friendTracker, creatureRepresentation, item);
        }
    }
}