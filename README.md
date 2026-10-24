# Space Engineers 2 'True Gravity' plugin

Did you know that Space Engineers 2 applies a hidden gravity multiplier of 2 to basically everything? Planets and gravity generators show 1G, but those objects are actually pulled with 2G.
This plugin replaces that multiplier, so 1G really is 1G. The multiplier can be changed in the plugin settings (default 1, the game uses 2). Takes effect the next time a world is loaded.
Note: ships get twice the effective thrust-to-weight compared to vanilla, because the gravity they fight is halved.

Space Engineers 2 defines a hidden `GravityMultiplier` of **2** in
`GameData/Vanilla/Content/System/Configurations/PhysicsSessionConfiguration.def`. 
It is applied on top of every gravity source (planets and gravity generators) for all rigid bodies except characters, so ships,
floating objects and debris are pulled with twice the gravity that the HUD and the gravity generator
show.

## Settings

Open the plugin's settings button in Pulsar.

- **Enabled**: turn the fix on or off (off = vanilla behavior).
- **Gravity multiplier**: default `1` (range 0.1 to 10). The game's own value is `2`.

Changes take effect the next time a world is loaded.

## Notes

- Ships get twice the effective thrust-to-weight compared to vanilla, because the gravity they fight is halved.
- The game's balance (thrusters, impact damage, and so on) may have been tuned around the multiplier of 2.
- The plugin is client side. In a multiplayer game hosted by someone else, the host's physics decides.

## Build

Standard SE2 client plugin, built by Pulsar on the player's machine. See the
[client plugin template](https://github.com/CometWorks/client2-plugin-template) for the development setup.
