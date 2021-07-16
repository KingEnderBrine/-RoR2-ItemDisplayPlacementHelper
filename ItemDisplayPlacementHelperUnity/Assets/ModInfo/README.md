# Description
A mod/tool to simplify process of item display placement for item/character developers.

![](https://cdn.discordapp.com/attachments/706089456855154778/806917306822164510/unknown.png)

# General usage
### Before start
You need to have an item display (default or character-specific), which you will edit and replace its values in code when you have done.

### Enter editing scene
First of all, you need to enter special scene where everything happens. This can be done by pressing `F2` (You need to be in the main menu).

### Pick a model
You can search for a model by localized name, body object name or model object name.
By default only models that are visible in logbook are included in list, but you can include everything from `BodyCatalog` by toggle `Inlude all bodies from BodyCatalog`.

![](https://cdn.discordapp.com/attachments/706089456855154778/806921419564777602/unknown.png)

### Select DisplayRuleGroup to work with
You can search for an item/equipment by name.

Each item display can be enabled/disabled by clicking on corresponding checkbox.
Only one equipment display can be enabled at a time, so if you enable new equipment old one will be disabled.

Click on a gear to start/end item display placement (All changes will be lost when you disable item display, so be careful with that).

* `Enable all` - enable all item displays for current model (except equipment)
* `Disable all` - enable all item displays for current model (inluding equipment)

![](https://cdn.discordapp.com/attachments/706089456855154778/807277696924778516/unknown.png)

### Select ItemDisplayRule
Click on a row with item display prefab name to start edit it.

![](https://cdn.discordapp.com/attachments/706089456855154778/807279240658812938/unknown.png)

### Edit ItemDisplayRule
You can type in values in corresponding fields to see how item display will look on a character.

![](https://cdn.discordapp.com/attachments/706089456855154778/807279742885298256/unknown.png)

Or you can use Unity-like item editing on the scene:

#### Moving
![](https://cdn.discordapp.com/attachments/706089456855154778/807279850959142933/unknown.png)

#### Rotating
White circle will rotate object around to camera view axis.

![](https://cdn.discordapp.com/attachments/706089456855154778/807279893179662356/unknown.png)

#### Scaling
Gray cube in center will scale all axes simultaneously.

![](https://cdn.discordapp.com/attachments/706089456855154778/807279934145298539/unknown.png)

### Copy values
When you placed your item where you want you can copy values for that position by clicking `Copy IDR values` button to paste these values in your code.

![](https://cdn.discordapp.com/attachments/706089456855154778/841245116856532992/unknown.png)

You can change copy format by clicking on the gear icon next to `Copy IDR values`

#### Available formats

* `Block`

Copied values will look something like that:
```cs
childName = "Head",
localPos = new Vector3(-0.0007F, 0.1699F, 0.028F),
localAngles = new Vector3(332.5399F, 359.8748F, 0.0883F),
localScale = new Vector3(0.7228F, 0.7228F, 0.7228F)
```

* `Inline`

Copied values will look something like that:
```cs
"Head", new Vector3(-0.0007F, 0.1699F, 0.028F), new Vector3(332.5399F, 359.8748F, 0.0883F), new Vector3(0.7228F, 0.7228F, 0.7228F)
```

* `ForParsing`

Copied values will look something like that:
```cs
Pelvis,-0.203,-0.058,0.058,63.11126,330.4519,181.0795,0.85025,0.85025,0.85025
```

* `Custom`

You can make your own format by using placeholders to specify where values should be placed.

# Top panel
![](https://cdn.discordapp.com/attachments/706089456855154778/807280033189068820/unknown.png)

### ConfigureSensitivity
* `Fast Coefficient` - camera movement speed multiplier when holding `SHIFT`
* `Slow Coefficient` - camera movement speed multiplier when holding `CTRL`

### Enable fade
Toggle fade when you are close to objects.

### Enable time
Toggle TimeScale between 0 and 1.

### Object Editing
![](https://cdn.discordapp.com/attachments/706089456855154778/807280172184502352/unknown.png)

Editing space:
* `Global` - move/rotate object in world coordinates
* `Local` - move/rotate object in object local coordinates

Editor mode:
* `Move` - move object along axis.
* `Rotate` - rotate object along axis.
* `Scale` - scale object along axis, or along all axes.

Scaling is always done in local space.

# Keybindings
#### General
* `F2` - Enter `ItemDisplayPlacementHelper` scene (You need to be in the main menu)
* `F` - Move camera focus to currently selected item display
* `G` - Toggle fade
* `T` - Toggle time

#### Object transform
* `W` - Select object movement mode
* `E` - Select object rotation mode
* `R` - Select object scaling mode
* `X` - Toggle object editing space (Global/Local)

#### Camera movement
* `RightMouseButton` - Hold and move mouse to look around
* `ALT` + `RightMouseButton` - Hold and move mouse to rotate camera around focus point
* `MiddleMouseButton` - Hold and move mouse to move camera
* `ALT` + `MiddleMouseButton` - Press once to move focus point to point on the model under mouse.
* `MouseWheel` - Scroll to zoom in/out.
* `ALT` + `MouseWheel` - Scroll to move to/from focus point
* `CTRL` - Hold to slow down camera movement speed
* `SHIFT` - Hold to speed up camera movement speed

# More features
If you have a feature in mind that would be good to have in this mod feel free to ping `@KingEnderBrine` in RoR modding discord.


If you want to support me, [you can do this here](https://www.buymeacoffee.com/KingEnderBrine)

# Changelog
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