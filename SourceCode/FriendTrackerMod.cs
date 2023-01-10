namespace JollyCoopFixesAndStuff
{
    internal static class FriendTrackerMod
    {
        internal static void OnEnable()
        {
            On.FriendTracker.GiftRecieved += FriendTracker_GiftRecieved; // change community-based relationship for player0
            On.FriendTracker.ItemOffered += FriendTracker_ItemOffered; // other players cannot be offered to lizards // otherwise you could reuse them when relationships are shared // lizards drop dead slugcats when they become friends
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void FriendTracker_GiftRecieved(On.FriendTracker.orig_GiftRecieved orig, FriendTracker friendTracker)
        {
            RainWorldGame game = friendTracker.AI.creature.world.game;
            if (!MainMod.isSharedRelationshipsEnabled || game.Players.Count <= 1 || friendTracker.giftOfferedToMe.owner is not Player || game.Players[0].realizedCreature is not Player player)
            {
                orig(friendTracker);
                return;
            }

            // relationships of other players simply refer to player0 with ID.-1.0;
            // for this reason, the community-based relationship needs to be changed for player0;
            friendTracker.giftOfferedToMe.owner = player;
            orig(friendTracker);
        }

        private static void FriendTracker_ItemOffered(On.FriendTracker.orig_ItemOffered orig, FriendTracker friendTracker, Tracker.CreatureRepresentation creatureRepresentation, PhysicalObject item)
        {
            if (item is Player) return;
            orig(friendTracker, creatureRepresentation, item);
        }
    }
}