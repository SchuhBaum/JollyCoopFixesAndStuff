using System;
using System.Reflection;
using BepInEx;
using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    [BepInPlugin("SchuhBaum.JollyCoopFixesAndStuff", "JollyCoopFixesAndStuff", "1.16")]
    public class MainMod : BaseUnityPlugin
    {
        //
        // AutoUpdate
        //

        public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/8/4";
        public int version = 33;
        public string keyE = "AQAB";
        public string keyN = "0Sb8AUUh0jkFOuNDGJti4jL0iTB4Oug0pM8opATxJH8hfAt6FW3//Q4wb4VfTHZVP3+zHMX6pxcqjdvN0wt/0SWyccfoFhx2LupmT3asV4UDPBdQNmDeA/XMfwmwYb23yxp0apq3kVJNJ3v1SExvo+EPQP4/74JueNBiYshKysRK1InJfkrO1pe1WxtcE7uIrRBVwIgegSVAJDm4PRCODWEp533RxA4FZjq8Hc4UP0Pa0LxlYlSI+jJ+hUrdoA6wd+c/R+lRqN2bjY9OE/OktAxqgthEkSXTtmZwFkCjds0RCqZTnzxfJLN7IheyZ69ptzcB6Zl7kFTEofv4uDjCYNic52/C8uarj+hl4O0yU4xpzdxhG9Tq9SAeNu7h6Dt4Impbr3dAonyVwOhA/HNIz8TUjXldRs0THcZumJ/ZvCHO3qSh7xKS/D7CWuwuY5jWzYZpyy14WOK55vnEFS0GmTwjR+zZtSUy2Y7m8hklllqHZNqRYejoORxTK4UkL4GFOk/uLZKVtOfDODwERWz3ns/eOlReeUaCG1Tole7GhvoZkSMyby/81k3Fh16Z55JD+j1HzUCaoKmT10OOmLF7muV7RV2ZWG0uzvN2oUfr5HSN3TveNw7JQPd5DvZ56whr5ExLMS7Gs6fFBesmkgAwcPTkU5pFpIjgbyk07lDI81k=";


        public readonly string author = "SchuhBaum";

        public static bool hasPlayerPointers = true;

        public static bool isSlugcatCollisionEnabled = false;
        public static bool isEasyModeEnabled = true;
        public static bool isJollyCoopEnabled = false;

        public static bool isSplitScreenModEnabled = false;
        public static bool isSBCameraScrollEnabled = false;
        public static bool isSharedRelationshipsEnabled = true;
        public static bool isShelterBehaviorsEnabled = false;

        public static bool matchCharacterWithPlayerNumber = false;

        //
        // ConfigMachine
        //

        public static MainMod? instance;
        public static OptionalUI.OptionInterface LoadOI() => new MainModOptions();

        // ---------------- //
        // public functions //
        // ---------------- //

        public MainMod() => instance = this;

        public void OnEnable()
        {
            On.RainWorld.Start += RainWorld_Start; // look for dependencies and initialize hooks
        }


        // ----------------- //
        // private functions //
        // ----------------- //

        private void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld rainWorld)
        {
            Debug.Log("JollyCoopFixesAndStuff: Version " + Info.Metadata.Version);

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;
                if (name == "FancySlugcats" || name == "CustomTail")
                {
                    matchCharacterWithPlayerNumber = true;
                }
                else if (name == "JollyCoop")
                {
                    isJollyCoopEnabled = true;
                }
                else if (name == "ShelterBehaviors")
                {
                    isShelterBehaviorsEnabled = true;
                }
                else if (name == "SBCameraScroll")
                {
                    isSBCameraScrollEnabled = true;
                }
                else if (name == "SplitScreenMod")
                {
                    isSplitScreenModEnabled = true;
                }
            }

            if (!isJollyCoopEnabled)
            {
                Debug.Log("JollyCoopFixesAndStuff: JollyCoop not found. Mod does nothing.");
            }
            else
            {
                Debug.Log("JollyCoopFixesAndStuff: JollyCoop found. Initialize hooks.");
                AbstractCreatureMod.OnEnable();
                FriendTrackerMod.OnEnable();
                MainLoopProcessMod.OnEnable();

                MushroomMod.OnEnable();
                OracleBehaviorMod.OnEnable();
                OracleSwarmerMod.OnEnable();
                PlayerHKMod.OnEnable(); // Override JollyCoop's camera logic.

                PlayerMod.OnEnable();
                RainWorldGameMod.OnEnable();
                RegionGateMod.OnEnable();
                RoomCameraMod.OnEnable();

                RoomMod.OnEnable_JollyCoop();
                RoomRealizerMod.OnEnable();
                RoomSpecificScriptMod.OnEnable();
                SaveStateMod.OnEnable();

                ShelterDoorMod.OnEnable();
                ShortcutHandlerMod.OnEnable();
                SpearMod.OnEnable();
                SpearOnBackMod.OnEnable();

                VoidSeaSceneMod.OnEnable();
                VultureMod.OnEnable();

                if (!matchCharacterWithPlayerNumber)
                {
                    Debug.Log("JollyCoopFixesAndStuff: FancySlugcats and CustomTail not found.");
                }
                else
                {
                    Debug.Log("JollyCoopFixesAndStuff: FancySlugcats and/or CustomTail found. Match slugcat characters with their player numbers automatically.");
                }

                if (!isShelterBehaviorsEnabled)
                {
                    Debug.Log("JollyCoopFixesAndStuff: ShelterBehaviors not found.");
                }
                else
                {
                    Debug.Log("JollyCoopFixesAndStuff: ShelterBehaviors found. Initialize hooks. Adept teleportation before saving.");

                    RoomMod.OnEnable_ShelterBehaviors();
                }

                if (!isSBCameraScrollEnabled)
                {
                    Debug.Log("JollyCoopFixesAndStuff: SBCameraScroll not found.");
                }
                else
                {
                    Debug.Log("JollyCoopFixesAndStuff: SBCameraScroll found. Synchronize shortcut position updates when mushroom effect is active.");
                }

                if (!isSplitScreenModEnabled)
                {
                    Debug.Log("JollyCoopFixesAndStuff: SplitScreenMod not found.");
                }
                else
                {
                    Debug.Log("JollyCoopFixesAndStuff: SplitScreenMod found. Link PlayerMeters to cameras. Disable PlayerPointer.");

                    HUDMod.OnEnable();
                    PlayerMeterMod.OnEnable();
                }
            }
            orig(rainWorld);
        }

        internal static object? GetNonPublicField(object? instance, string fieldName) => GetField(instance?.GetType(), instance, fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

        private static object? GetField(Type? type, object? instance, string fieldName, BindingFlags bindingFlags)
        {
            try
            {
                return type?.GetField(fieldName, bindingFlags).GetValue(instance);
            }
            catch (Exception exception)
            {
                Debug.Log("JollyCoopFixesAndStuff: " + exception);
            }
            return null;
        }
    }
}