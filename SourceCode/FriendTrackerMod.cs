using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    internal static class FriendTrackerMod
    {
        internal static void OnEnable()
        {
            On.FriendTracker.GiftRecieved += FriendTracker_GiftRecieved;
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
    }
}