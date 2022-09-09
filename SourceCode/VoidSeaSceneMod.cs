namespace JollyCoopFixesAndStuff
{
    public static class VoidSeaSceneMod
    {
        internal static void OnEnable()
        {
            On.VoidSea.VoidSeaScene.Update += VoidSeaScene_Update;
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void VoidSeaScene_Update(On.VoidSea.VoidSeaScene.orig_Update orig, VoidSea.VoidSeaScene voidSeaScene, bool eu)
        {
            if (voidSeaScene.room.game.Players.Count <= 1)
            {
                orig(voidSeaScene, eu);
                return;
            }

            // the first player going deep enough for voidSeaMode ascends and the rest dies
            RainWorldGame game = voidSeaScene.room.game;
            if (game.cameras[0] is RoomCamera roomCamera && !roomCamera.voidSeaMode)
            {
                foreach (AbstractCreature abstractPlayer in game.Players)
                {
                    if (abstractPlayer.realizedCreature is Player player_ && player_.room == voidSeaScene.room)
                    {
                        if (player_.mainBodyChunk.pos.y < 240f)
                        {
                            bool playerDied = false;
                            foreach (AbstractCreature abstractPlayer_ in game.Players)
                            {
                                if (abstractPlayer_ != abstractPlayer)
                                {
                                    if (abstractPlayer_.state.alive)
                                    {
                                        abstractPlayer_.Die();
                                        playerDied = true;
                                    }

                                    if (abstractPlayer_.Room != game.world.offScreenDen)
                                    {
                                        AbstractCreatureMod.DropOrDestroyAllObjects(abstractPlayer_);
                                        AbstractCreatureMod.Teleport(abstractPlayer_, game.world.offScreenDen);
                                        AbstractCreatureMod.LogPlayer(abstractPlayer_);
                                    }
                                }
                            }

                            if (playerDied)
                            {
                                game.cameras[0].hud.textPrompt.AddMessage(game.rainWorld.inGameTranslator.Translate("You must continue alone now."), 20, 200, true, true);
                            }

                            RainWorldGameMod.SetPlayerWithIndex0(game, abstractPlayer);
                            roomCamera.voidSeaMode = true;
                        }
                    }
                }
            }
            orig(voidSeaScene, eu);
        }
    }
}