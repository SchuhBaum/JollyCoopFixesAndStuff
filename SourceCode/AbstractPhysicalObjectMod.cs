using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    public static class AbstractPhysicalObjectMod
    {
        // ---------------- //
        // public functions //
        // ---------------- //

        public static void Abstractize(AbstractPhysicalObject? abstractPhysicalObject)
        {
            if (abstractPhysicalObject == null || abstractPhysicalObject.realizedObject == null)
            {
                return;
            }

            Debug.Log("JollyCoopFixesAndStuff: Abstractize " + abstractPhysicalObject + ".");
            if (abstractPhysicalObject.realizedObject is PhysicalObject physicalObject)
            {
                abstractPhysicalObject.Room.realizedRoom?.RemoveObject(physicalObject);
                physicalObject.Destroy();
            }
            abstractPhysicalObject.realizedObject = null;
        }

        public static void AllGraspsLetGoOfThisObject(AbstractPhysicalObject? abstractPhysicalObject)
        {
            if (abstractPhysicalObject == null)
            {
                return;
            }

            for (int aOSIndex = abstractPhysicalObject.stuckObjects.Count - 1; aOSIndex >= 0; --aOSIndex)
            {
                AbstractPhysicalObject.AbstractObjectStick abstractObjectStick = abstractPhysicalObject.stuckObjects[aOSIndex];
                if (abstractObjectStick.B == abstractPhysicalObject) // only called when I am being grabbed
                {
                    ReleaseGrasp(abstractObjectStick); // does nothing when A is a spear
                }
            }
        }

        public static void ChangeRooms(AbstractPhysicalObject? abstractPhysicalObject, WorldCoordinate newCoord)
        {
            if (abstractPhysicalObject == null || abstractPhysicalObject.pos == newCoord)
            {
                return;
            }

            AbstractRoom oldAbstractRoom = abstractPhysicalObject.Room;
            AbstractRoom? newAbstractRoom = abstractPhysicalObject.world.GetAbstractRoom(newCoord);

            if (newAbstractRoom == null)
            {
                return;
            }
            Room? newRoom = newAbstractRoom.realizedRoom;

            if (newRoom == null || !newCoord.TileDefined)
            {
                Abstractize(abstractPhysicalObject); // if I destroy spears then they might get disabled // if I dont destroy slugcat then slugcat might update more than once(?), at least slugcat is super fast with no adrenaline
            }

            if (oldAbstractRoom != newAbstractRoom)
            {
                abstractPhysicalObject.pos = newCoord;
                abstractPhysicalObject.InDen = newAbstractRoom.offScreenDen;

                if (newRoom != null && newCoord.TileDefined)
                {
                    abstractPhysicalObject.RealizeInRoom();
                    if (abstractPhysicalObject.realizedObject is PhysicalObject physicalObject)
                    {
                        physicalObject.room = newRoom; // just to be sure // can be null or different from abstractPhysicalObject.Room.realizedRoom in some situations
                        if (newAbstractRoom.shelter)
                        {
                            foreach (AbstractPhysicalObject connectedAbstractPhysicalObject in abstractPhysicalObject.GetAllConnectedObjects())
                            {
                                connectedAbstractPhysicalObject.pos.Tile = newCoord.Tile;
                                if (connectedAbstractPhysicalObject.realizedObject != null)
                                {
                                    foreach (BodyChunk bodyChunk in connectedAbstractPhysicalObject.realizedObject.bodyChunks)
                                    {
                                        bodyChunk.HardSetPosition(newRoom.MiddleOfTile(newCoord));
                                        bodyChunk.vel = new Vector2(0.0f, 0.0f);
                                    }
                                }
                            }
                        }

                        oldAbstractRoom.realizedRoom?.RemoveObject(physicalObject);
                        newRoom.AddObject(physicalObject);
                    }
                }

                oldAbstractRoom.RemoveEntity(abstractPhysicalObject);
                newAbstractRoom.AddEntity(abstractPhysicalObject);
            }
        }

        public static void Destroy(AbstractPhysicalObject? abstractPhysicalObject)
        {
            if (abstractPhysicalObject == null)
            {
                return;
            }

            if (abstractPhysicalObject.realizedObject is PhysicalObject physicalObject)
            {
                abstractPhysicalObject.Room.realizedRoom?.RemoveObject(physicalObject);
                physicalObject.Destroy();
            }
            abstractPhysicalObject.Destroy();
        }

        public static void DropImpaledSpears(AbstractPhysicalObject? abstractPhysicalObject)
        {
            if (abstractPhysicalObject == null)
            {
                return;
            }

            for (int aOSIndex = abstractPhysicalObject.stuckObjects.Count - 1; aOSIndex >= 0; --aOSIndex)
            {
                AbstractPhysicalObject.AbstractObjectStick abstractObjectStick = abstractPhysicalObject.stuckObjects[aOSIndex];
                if (abstractObjectStick is AbstractPhysicalObject.ImpaledOnSpearStick)
                {
                    abstractObjectStick.Deactivate();
                }
            }
        }

        public static void DropOrDestroySpear(AbstractPhysicalObject.AbstractObjectStick? abstractObjectStick, bool destroy = false)
        {
            if (abstractObjectStick == null)
            {
                return;
            }

            Spear? spear = null;
            if (abstractObjectStick.A.type == AbstractPhysicalObject.AbstractObjectType.Spear)
            {
                spear = (Spear)abstractObjectStick.A.realizedObject;
            }
            else if (abstractObjectStick.B.type == AbstractPhysicalObject.AbstractObjectType.Spear)
            {
                spear = (Spear)abstractObjectStick.B.realizedObject;
            }

            spear?.ChangeMode(Weapon.Mode.Free);
            abstractObjectStick.Deactivate();

            if (destroy && spear != null)
            {
                Debug.Log("JollyCoopFixesAndStuff: Destroy " + spear.abstractPhysicalObject + ".");
                Destroy(spear?.abstractPhysicalObject);
            }
        }

        public static void DropOrDestroyStuckSpears(AbstractPhysicalObject? abstractPhysicalObject)
        {
            if (abstractPhysicalObject == null)
            {
                return;
            }

            for (int aOSIndex = abstractPhysicalObject.stuckObjects.Count - 1; aOSIndex >= 0; --aOSIndex)
            {
                AbstractPhysicalObject.AbstractObjectStick abstractObjectStick = abstractPhysicalObject.stuckObjects[aOSIndex];

                // as I understand these cases: 1) spear stuck in main chunks, 2) spear stuck in non-main chunks, and 3) spear pinned chunk to ground
                if (abstractObjectStick is AbstractPhysicalObject.AbstractSpearStick || abstractObjectStick is AbstractPhysicalObject.AbstractSpearAppendageStick || abstractObjectStick is AbstractPhysicalObject.ImpaledOnSpearStick)
                {
                    DropOrDestroySpear(abstractObjectStick, destroy: abstractPhysicalObject.realizedObject is Creature creature && creature.inShortcut);
                }
            }
        }

        public static void ReleaseGrasp(AbstractPhysicalObject.AbstractObjectStick? abstractObjectStick)
        {
            if (abstractObjectStick == null)
            {
                return;
            }

            if (abstractObjectStick is AbstractPhysicalObject.CreatureGripStick creatureGripStick)
            {
                // similar to AbstractCreature.DropCarriedObject(creatureGripStick.grasp);
                ((AbstractCreature)creatureGripStick.A).realizedCreature?.ReleaseGrasp(creatureGripStick.grasp);
                abstractObjectStick.Deactivate();
            }
            else if (abstractObjectStick is Player.AbstractOnBackStick abstractOnBackStick && abstractOnBackStick.A is AbstractCreature abstractCreature && abstractOnBackStick.B is AbstractCreature) // JollyCoop uses this to store their players
            {
                AbstractCreatureMod.DropPlayerOnBack(abstractCreature);
            }
        }

        public static void Teleport(AbstractPhysicalObject? abstractPhysicalObject, WorldCoordinate? newCoord)
        {
            if (abstractPhysicalObject == null || newCoord == null || abstractPhysicalObject.pos == newCoord)
            {
                return;
            }

            DropImpaledSpears(abstractPhysicalObject);
            foreach (AbstractPhysicalObject connectedAbstractPhysicalObject in abstractPhysicalObject.GetAllConnectedObjects())
            {
                Debug.Log("JollyCoopFixesAndStuff: Teleport " + connectedAbstractPhysicalObject + " to " + newCoord + ".");
                ChangeRooms(connectedAbstractPhysicalObject, (WorldCoordinate)newCoord);
            }
        }
    }
}