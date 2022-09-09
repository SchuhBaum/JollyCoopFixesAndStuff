namespace JollyCoopFixesAndStuff
{
    public static class AbstractRoomMod
    {
        // ---------------- //
        // public functions //
        // ---------------- //

        public static WorldCoordinate? GetShortcutCoordinates(AbstractRoom? abstractRoom, int shortcutNode)
        {
            if (abstractRoom == null)
            {
                return null;
            }

            WorldCoordinate worldCoordinate = new WorldCoordinate(abstractRoom.index, -1, -1, -1);
            if (shortcutNode > -1 && shortcutNode < abstractRoom.nodes.Length && abstractRoom.nodes[shortcutNode].type == AbstractRoomNode.Type.Exit && abstractRoom.realizedRoom?.shortCutsReady == true)
            {
                worldCoordinate = abstractRoom.realizedRoom.LocalCoordinateOfNode(shortcutNode); // why is newCoord's node = -1?
                worldCoordinate.abstractNode = shortcutNode;
            }
            return worldCoordinate;
        }
    }
}