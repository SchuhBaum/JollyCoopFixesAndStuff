namespace JollyCoopFixesAndStuff
{
    internal static class SocialEventRecognizerMod
    {
        internal static void OnEnable()
        {
            On.SocialEventRecognizer.Killing += SocialEventRecognizer_Killing; // share kills with player 0 // otherwise kills from other players are not counted for hunter 
        }

        //
        // private
        //

        // I assume here that player 0 has playerNumber equal to 0 // this assumption should only be violated when reaching the void sea // it doesn't matter in that case
        private static void SocialEventRecognizer_Killing(On.SocialEventRecognizer.orig_Killing orig, SocialEventRecognizer socialEventRecognizer, Creature killer, Creature victim)
        {
            orig(socialEventRecognizer, killer, victim);

            if (killer is not Player player) return;
            if (player.playerState.playerNumber == 0) return;
            if (socialEventRecognizer.room.game.GetStorySession?.playerSessionRecords[0] is not PlayerSessionRecord playerSessionRecord) return;

            // not calling orig here
            // instead I add the kill manually
            // this way I removed the assumption that player 0 needs to be realized

            // only share the kill;
            // the reputation is shared to some extend anyways
            // since you have global reputation values;
            //RainWorldGame game = socialEventRecognizer.room.game;
            //game.session.creatureCommunities.InfluenceLikeOfPlayer(victim.Template.communityID, socialEventRecognizer.room.world.RegionNumber, playerNumber: 0, -0.05f * victim.Template.communityInfluence, 0.25f, 0f);

            playerSessionRecord.AddKill(victim);
        }
    }
}