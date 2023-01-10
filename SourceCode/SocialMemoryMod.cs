namespace JollyCoopFixesAndStuff
{
    internal static class SocialMemoryMod
    {
        internal static void OnEnable()
        {
            // these hooks make sharing relationships more consistent;
            // one downside is that this does not work when looping over all relationships;
            // this happens for example when selecting a friend to follow when tamed;
            // this means that tamed lizards will only follow player0;
            // this makes sharing and syncing relationships necessary as well;

            On.SocialMemory.GetKnow += SocialMemory_GetKnow;
            On.SocialMemory.GetLike += SocialMemory_GetLike;
            On.SocialMemory.GetRelationship += SocialMemory_GetRelationship;
            On.SocialMemory.GetTempLike += SocialMemory_GetTempLike;
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static float SocialMemory_GetKnow(On.SocialMemory.orig_GetKnow orig, SocialMemory socialMemory, EntityID subjectID)
        {
            // overseer has spawner -1 and number 5
            if (subjectID.spawner != -1 || subjectID.number < 0 || subjectID.number > 3 || !MainMod.isSharedRelationshipsEnabled) return orig(socialMemory, subjectID);

            // assumes that player0 has always ID -1.0;
            return orig(socialMemory, new EntityID(-1, 0));
        }

        private static float SocialMemory_GetLike(On.SocialMemory.orig_GetLike orig, SocialMemory socialMemory, EntityID subjectID)
        {
            if (subjectID.spawner != -1 || subjectID.number < 0 || subjectID.number > 3 || !MainMod.isSharedRelationshipsEnabled) return orig(socialMemory, subjectID);
            return orig(socialMemory, new EntityID(-1, 0));
        }

        private static SocialMemory.Relationship SocialMemory_GetRelationship(On.SocialMemory.orig_GetRelationship orig, SocialMemory socialMemory, EntityID subjectID)
        {
            if (subjectID.spawner != -1 || subjectID.number < 0 || subjectID.number > 3 || !MainMod.isSharedRelationshipsEnabled) return orig(socialMemory, subjectID);
            return orig(socialMemory, new EntityID(-1, 0));
        }

        private static float SocialMemory_GetTempLike(On.SocialMemory.orig_GetTempLike orig, SocialMemory socialMemory, EntityID subjectID)
        {
            if (subjectID.spawner != -1 || subjectID.number < 0 || subjectID.number > 3 || !MainMod.isSharedRelationshipsEnabled) return orig(socialMemory, subjectID);
            return orig(socialMemory, new EntityID(-1, 0));
        }
    }
}