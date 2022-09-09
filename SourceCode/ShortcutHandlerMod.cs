using System.Collections.Generic;
using UnityEngine;

namespace JollyCoopFixesAndStuff
{
    public static class ShortcutHandlerMod
    {
        internal static readonly List<TeleportationVesselMod> shortCutsReadyWaitingQueue = new List<TeleportationVesselMod>();

        internal static void OnEnable()
        {
            On.ShortcutHandler.Update += ShortcutHandler_Update;
        }

        // -------------- //
        // public classes //
        // -------------- //

        public class TeleportationVesselMod
        {
            public AbstractCreature abstractCreature;
            public AbstractRoom abstractRoom;
            public int shortcutNode;

            public TeleportationVesselMod(AbstractCreature abstractCreature, AbstractRoom abstractRoom, int shortcutNode)
            {
                this.abstractCreature = abstractCreature;
                this.abstractRoom = abstractRoom;
                this.shortcutNode = shortcutNode;
            }
        }

        // ---------------- //
        // public functions //
        // ---------------- //

        public static void CopyAndAddVessel(ShortcutHandler? shortcutHandler, Creature? creature, ShortcutHandler.Vessel? template)
        {
            if (shortcutHandler == null || template == null || template.creature == creature)
            {
                return;
            }

            if (creature != null)
            {
                Debug.Log("JollyCoopFixesAndStuff.CopyAndAddVessel: vessel.creature " + creature);
                if (template is ShortcutHandler.BorderVessel borderVessel)
                {
                    shortcutHandler.borderTravelVessels.Add(new ShortcutHandler.BorderVessel(creature, borderVessel.type, borderVessel.destination, borderVessel.distance, borderVessel.room));
                }
                else if (template is ShortcutHandler.ShortCutVessel shortCutVessel)
                {
                    shortcutHandler.transportVessels.Add(new ShortcutHandler.ShortCutVessel(shortCutVessel.pos, creature, shortCutVessel.room, shortCutVessel.wait));
                }
                else
                {
                    shortcutHandler.betweenRoomsWaitingLobby.Add(new ShortcutHandler.Vessel(creature, template.room));
                }
                creature.inShortcut = true;
            }
        }

        public static ShortcutHandler.Vessel? GetVessel(ShortcutHandler? shortcutHandler, AbstractCreature? abstractCreature)
        {
            if (shortcutHandler == null || abstractCreature == null || abstractCreature.realizedCreature?.inShortcut == false)
            {
                return null;
            }

            foreach (AbstractPhysicalObject abstractPhysicalObject in abstractCreature.GetAllConnectedObjects())
            {
                if (abstractPhysicalObject.realizedObject is Creature creature)
                {
                    foreach (ShortcutHandler.Vessel vessel in shortcutHandler.betweenRoomsWaitingLobby)
                    {
                        if (vessel.creature == creature)
                        {
                            Debug.Log("JollyCoopFixesAndStuff.betweenRoomsWaitingLobby: vessel.creature " + vessel.creature);
                            return vessel;
                        }
                    }

                    foreach (ShortcutHandler.BorderVessel vessel in shortcutHandler.borderTravelVessels)
                    {
                        if (vessel.creature == creature)
                        {
                            Debug.Log("JollyCoopFixesAndStuff.borderTravelVessels: vessel.creature " + vessel.creature);
                            return vessel;
                        }
                    }

                    foreach (ShortcutHandler.ShortCutVessel vessel in shortcutHandler.transportVessels)
                    {
                        if (vessel.creature == creature)
                        {
                            Debug.Log("JollyCoopFixesAndStuff.transportVessels: vessel.creature " + vessel.creature);
                            return vessel;
                        }
                    }
                }
            }
            return null;
        }

        public static void RemoveVessel(ShortcutHandler? shortcutHandler, ShortcutHandler.Vessel? vessel)
        {
            if (shortcutHandler == null || vessel == null)
            {
                return;
            }

            if (vessel is ShortcutHandler.BorderVessel borderVessel)
            {
                shortcutHandler.borderTravelVessels.Remove(borderVessel);
            }
            else if (vessel is ShortcutHandler.ShortCutVessel shortCutVessel)
            {
                shortcutHandler.transportVessels.Remove(shortCutVessel);
            }
            else
            {
                shortcutHandler.betweenRoomsWaitingLobby.Remove(vessel);
            }

            if (vessel.creature != null)
            {
                vessel.creature.inShortcut = false;
            }
        }

        public static void RemoveVessel(ShortcutHandler? shortcutHandler, Creature? creature, ShortcutHandler.Vessel? vessel)
        {
            if (creature == null || vessel?.creature != creature)
            {
                return;
            }

            Debug.Log("JollyCoopFixesAndStuff: Remove vessel with creature " + vessel?.creature + ".");
            RemoveVessel(shortcutHandler, vessel);
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void ShortcutHandler_Update(On.ShortcutHandler.orig_Update orig, ShortcutHandler shortcutHandler)
        {
            orig(shortcutHandler);
            if (shortCutsReadyWaitingQueue.Count > 0)
            {
                foreach (TeleportationVesselMod vessel in shortCutsReadyWaitingQueue)
                {
                    if (vessel.abstractRoom.offScreenDen)
                    {
                        AbstractCreatureMod.Teleport(vessel.abstractCreature, vessel.abstractRoom);
                        shortCutsReadyWaitingQueue.Remove(vessel);
                        break;
                    }
                    else if (vessel.abstractRoom.realizedRoom == null)
                    {
                        Debug.Log("JollyCoopFixesAndStuff: Activate room.");
                        vessel.abstractRoom.world.ActivateRoom(vessel.abstractRoom);
                    }
                    else if (vessel.abstractRoom.realizedRoom.shortCutsReady)
                    {
                        Debug.Log("JollyCoopFixesAndStuff: Waiting is over.");
                        AbstractCreatureMod.Teleport(vessel.abstractCreature, vessel.abstractRoom, vessel.shortcutNode);
                        shortCutsReadyWaitingQueue.Remove(vessel);
                        break;
                    }
                    else
                    {
                        Debug.Log("JollyCoopFixesAndStuff: Waiting..");
                    }
                }
            }
        }
    }
}