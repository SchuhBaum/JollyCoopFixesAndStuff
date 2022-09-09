## JollyCoopFixesAndStuff
###### Version: 1.15

This is a mod for Rain World v1.5.

### Description
Attempts to fix or work around some of the more critical bugs in JollyCoop v1.6.6.
- Rooms should no longer get unloaded when a player is in them.
-  Starvation mode should no longer be triggered when the first player died before enough food was collected.
-  All players can use the map.
-  Tried to work around some jank with region gates. Like for example the region gate triggering a region change too early after some player died.
-  Disabled the offscreen player sprites. These could freeze and sometimes lead to softlocks when changing regions.

### Installation
1. (ModLoader) `BepInEx` and `BOI` can be downloaded from [RainDB](https://www.raindb.net/) under `Tools`.  
  **NOTE:** Rain World's BepInEx is a modified version. Don't download it from GitHub.
2. (Dependency) The mod `JollyCoop` can be downloaded from [GitHub](https://github.com/Garrakx/Jolly-Coop/releases) or [RainDB](https://www.raindb.net/).
3. (Dependency) The mod `ConfigMachine` can be downloaded from [RainDB](https://www.raindb.net/) under `Tools`.
4. Download the file  `JollyCoopFixesAndStuff.dll` from [Releases](https://github.com/SchuhBaum/JollyCoopFixesAndStuff/releases) and place it in the folder `[Steam]\SteamApps\common\Rain World\Mods`.
5. Start `[Steam]\SteamApps\common\Rain World\BOI\BlepOutIn.exe`.
6. Click `Select path` and enter the game's path `[Steam]\SteamApps\common\Rain World`. Enable the mod `JollyCoopFixesAndStuff.dll` and its dependencies. Then launch the game as normal. 

### Contact
If you have feedback, you can message me on Discord `@SchuhBaum#7246` or write an email to SchuhBaum71@gmail.com.

### License
There are two licenses available - MIT and Unlicense. You can choose which one you want to use. 

### Changelog
v0.20:
- Rooms should get loaded when an offscreen player is carried into them by a creature.


v0.30:
- Fixed a bug which crashed the arena mode.
- When all players are dead in JollyCoop's easy mode, game over should always trigger now.


v0.40:
- Food sharing should now work as intended. Food counting when saving should work. Reworked the code for fixing unintended starvation mode. Should be more reliable now.
- Added an option interface. Added the option easy mode (EM) (enabled by default) (sorry, I can't access jollycoop's config for that).
- Reworked the behaviour of region gates and shelters when EM is enabled. You can now travel in other regions when someone died or in a den. Should fix softlocks when saving in the new region.


v0.50:
- Moon and Pebbles should react to other player when player 1 is not around.
- The mushroom effect gets shared. Removes slow motion and sound loop. Adds eating sound. Visual cue remains.
- Removed the ability to steal items from other player. Removed item blinking when you cannot pick them up.
- When EM is enabled (default), added a room script to the final room. Player 1 and 2 gets revived. All others get removed. This is a setup that should prevent a softlock from JollyCoop's room script.
- Camera can be switched when the player is in a shortcut. Camera should select the correct camera position when the player is outside of the room boundaries.
- Restructured code. Hopefully I didn't break anything. Reworked teleportation function. Should extend to cases where players are in shortcuts and such. Context: (Dead) players get teleported before saving in shelters and region transitions. If this fails or they get teleported back, it can lead to softlocks.
- Items carried by players outside of shelters don't get teleported/saved anymore.
- Fixed a bug where the same EntityId was used for multiple slugcats when spawning. Never had an issue with this outside of debug logging but hey.


v0.60:
- Added support for AutoUpdate.
- Fixed a bug where backspears would appear in front of slugcat when changing cameras.
- Reintroduced a (slight) slow motion effect after eating mushrooms. This effect scales with the mushroom effect such that slugcats running speed stays constant.
- Some additional code restructuring.


v0.70:
- Mushroom counter is shared and not decreased if any player is in a shortcut.
- Shortcuts should operate at normal speed when mushroom effect is active.
- When switching cameras, only the map button of the active control setup (either keyboard or gamepad) is checked.
- When eating a neuron fly, the glow ability is shared immediately.
- When EM is enabled (default), benefits from feeding creatures are shared.
- By jumping, you can release yourself from being grabbed by other players.


v0.80:
- Fixed a bug when jumping out of being grabbed by another player.
- The mod ShelterBehaviors might skip ShelterDoor's DoorClosed() function. Added a workaround in that case when EM is enabled (default). Still, compatibility might be limited since currently ShelterBehaviors assumes hard mode.
- Added support for ExtendedGates when EM is enabled (default). Not 100% clean but should be functional.


v0.90:
- Extended compatibility with ShelterBehaviors. When ShelterBehaviors is active, dead players are teleported early and realized in shelters.
- Fixed some bugs regarding the ShelterBehaviors workaround.
- Fixed a bug, where player would be teleported early to a non-modded shelter.
- Fixed a bug, where a swallowed creature (like a VultureGrub) could prevent the player from getting realized after teleportation.
- Simplified region gate fixes. Previous changes for ExtendedGates support shouldn't be needed anymore.
- Players should now be able to use region gates while piggy back riding.
- Fixed a bug, where the RoomRealizer was not following player 1 at the start of a cycle.


v1.00:
- When EM is enabled (default), relationships between players and creatures get shared with new players when initializing. This happens once. They still can individually change afterwards.
- When EM is disabled, extended some changes and fixes: Region gates only open when all players (dead or alive) are present. When ShelterBehavior is used, dead players are realized but not teleported to modded shelters. This prevents a softlock. (Non-modded) shelters close only when all players (dead or alive) are in the shelter.
- BackPlayers can only be dropped while holding down or up. This makes handling backSpears easier while carrying a player.
- Fixed a bug where dead backPlayers would be grabbed instead of items on the ground.
- Using Reflection for accessing JollyCoop's config. Removed the options interface.
- Some code restructuring.
- Fixed a bug, where object not changing rooms properly (ROOM MISMATCH) would not get updated anymore. This became a problem for the swarmer in the Automaton mod.
- Made sharing relationships a separate option (enabled by default). This option is independent of JollyCoop's easy mode. Context: Before this, relationships would only be shared when easy mode was enabled. This option is meant to prevent tamed lizards from eating your friends.
- When FancySlugcats mod is enabled, slugcat characters are matched with their player numbers automatically. Should improve compatibility. Fixed a bug with the implementation.
- Restructured code.
- Added a cooldown for the shaveDownPerformance function. The game can feel laggy when this function gets spammed. (TODO: check later again)
- Skip sounds outside of the camera room.
- Changed implementation and fixed a bug where swallowed creatures would not be able to regurgitate.
- Always pre-loading probable next room (TODO: monitor if it can have an negativ impact; check later again). Otherwise some rooms never gets loaded unless entered. The performance budget estimation seems to largely vary at times. For example the scavenger base in garbage waste was estimated 2000 before loading and 400 after loading (max budget is 1500 => didn't pre-load).
- Added support for SBCameraScroll mod, i.e. syncs shortcut player position when mushroom effect is active.
- Added an option for collisions between slugcats. Disabled by default.
- Fixed a bug where the mushroom effect got stuck.
- Disabled the deaf sound loop (from explosions). This sound could get stuck forever.


v1.10:
- Fixed a bug where the collision could not be reset when using DeerFix and throwing puff balls.
- (slugcat collisions) Simplified implementation.
- Fixed a bug where a backPlayer could collide with an enemy that is being eaten.
- Fixed a bug where ShelterBehaviors became a mandatory dependency.
- Fixed a bug where the sounds were omitted when using SplitScreenMod.
- Added an option to disable PlayerPointer. Their position is based on the first camera. This conflicts with the split screen mod.
- Changes to improve support for multiple cameras. Taking a camera chooses the closest one if in the same room.
- Fixed a bug where the mushroom counter was not properly synced for all players.
- Disabled PlayerPointer automatically when SplitScreenMod is used.
- When using gates, cameras no longer need to be in the gate room. Instead they are moved automatically when the gate is closing. Before, you might got stuck when using SplitScreenMod (<=1.2 only?).
- When a slugcat with a camera dies, this camera switches automatically. Switches only to slugcats without a camera (unless only one player is alive and multiple cameras exist).
- Gates can now be used without standing still.
- Fixed a bug where food would be consumed when ending the cycle even with full stomach.
- Fixed a bug where you would start with too much food when fast traveling.
- Reworked camera logic for the cycling camera. Only one player controls each camera (even when this player is dead) and the others can only take the map (HUD) of the nearest camera (when in the same room). The camera cycles through all players which are not: dead, have already a camera or control a camera themselfes.
- Dropping a backPlayer requires holding down (holding up is removed) -- for consistency with dropping other things.
- Reworked void sea. Room scripts are removed (mine and JollyCoop's). The first player entering the void sea screen transition can ascend and the rest dies. The surviving player is set internally as player with the index 0 (TODO: check again later; so far I didn't had side effects).


v1.15:
- Fixed a bug where a dead player could take the HUD.
- Fixed a bug when using SplitScreenMod and a cycling camera where you couldn't focus the camera on yourself anymore after warping.
- Players can release being grabbed by other players by pressing jump. Before it was by jumping.
- Fixed a bug where JollyCoop would spam AddFood() forever.
- Now a BepInEx plugin.
- Restructured code.
- Removed room gravity script from SS_E08. The script sets the gravity based on one player position otherwise.