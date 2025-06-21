**1.7.6**

* Fixed an error when selecting a character with a skin with no `nameToken`.

**1.7.5**

* Fixed an issue where selecting items for editing would not show display rule options.

**1.7.4**

* Fixed an issue where selecting some bodies would completely freeze the game.

**1.7.3**

* Fixed for `Memory Optimization` update.

**1.7.2**

* Fixed an issue where copying values with custom format would truncate all text after the last placeholder.
* Added new placeholders `modelName` and `bodyName` for custom format.

**1.7.1**

* Readme update.

**1.7.0**

* Fixes for `Seekers of the Storm` update.

**1.6.2**

* Fixed incorrect filtering by selected content pack if there are multiple content packs with the same identifier.

**1.6.1**

* Added support for `R2API.Skins` skin-specific IDRS

**1.6.0**

* Added `DynamicBones ignore TimeScale` toggle to the top panel.
* Added `Enable animator` toggle to the bottom panel.
* Added refresh button to the top right corner in `Item display rule` section. It disables and enables item display while keeping your changes, can be useful if item display does something based on the values you change but only during `Start`/`Awake`.
* `ItemFollower`'s follower instance will now also scale when you change scale.

**1.5.4**

* Changed decimal separator to always be `.`

**1.5.3**

* Fixed incorrect separator used for number parsing resulting in inability to input decimal numbers.

**1.5.2**

* Fixed `Enable fade` being turned on by default.
* Now `RuntimeInspector` should be in front of the mod's UI
* `Enable all`/`Disable all` buttons now work only on filtered items (from selected content pack and/or containing search string) instead of all.

**1.5.1**

* Fixed 1 of rotation axes missing

**1.5.0**

* Fixes for Survivors of the Void update.
* Fixed some icon being blured on low texture quality.
* Dropdown for showing vanilla/modded items now will show item from selected content pack

**1.4.2**

* Fixed an issue where if placeholder didn't have a modificator parsing would fail.

**1.4.1**

* Added new copy format.
	* `ForParsing` - as it stands from the name useful for parsing copied value, all values are comma-separated without any spaces in the following order: childName, localPos.xyz, localAngles.xyz, localScale.xyz. 

**1.4.0**

* Moved fade toggle from `F` to `G`
* Now pressing `F` will nove camera focus to currently selected item display
* Added different copy formats.
	* `Block` - default format, the one that was used pre `1.4.0`
	* `Inline` - all values are in one line and comma-separated without field names
	* `Custom` - you can make your own format using available placeholders
* Removed `Copy precision` field as it's now part of the copy format system

**1.3.0**

* Fixed item filter not working correctly.
* Added camera movement like in Blender.

**1.2.0**

* Removed r2api dependency.

**1.1.1**

* Added `Tab` navigation between IDR input fields.

**1.1.0**

* Added `Model animator parameters` which will give you a little bit more flexibility on testing how your idrs looks.
* Added ability to play animations by layer and state name.

**1.0.2**

* Updated version number in mod dll.

**1.0.1**

* Added new dropdown to show only vanilla items / only modded items / all items (Request by DestroyedClone).
* Fixed an issue when changing model child names will contain bones from old model.
* Fixed an issue when having time scale set to less than 0.01 was causing issues with axes collsion detection.

**1.0.0**

* Mod release.