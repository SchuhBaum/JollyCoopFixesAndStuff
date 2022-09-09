using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    internal static class RoomSpecificScriptMod
    {
        internal static void OnEnable()
        {
            On.RoomSpecificScript.AddRoomSpecificScript += AddRoomSpecificScript;
        }

        //
        // private
        //

        private static void AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
        {
            orig(room);

            if (room.abstractRoom.name == "SS_E08")
            {
                foreach (UpdatableAndDeletable updatableAndDeletable in room.updateList)
                {
                    if (updatableAndDeletable is RoomSpecificScript.SS_E08GradientGravity)
                    {
                        Debug.Log("JollyCoopFixesAndStuff: Remove room gravity script for SS_E08. Set gravity to fixed value.");
                        room.RemoveObject(updatableAndDeletable);
                        room.gravity = 0.5f;
                        return;
                    }
                }
            }
        }
    }
}