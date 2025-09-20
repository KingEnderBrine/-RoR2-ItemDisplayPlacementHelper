# Description
A mod/tool to simplify process of item display placement for item/character developers.

![](https://github.com/KingEnderBrine/-RoR2-ItemDisplayPlacementHelper/blob/master/Screenshots/1.png?raw=true)

# General usage
### Before start
You need to have an item display (default or character-specific), which you will edit and replace its values in code when you have done.

### Enter editing scene
First of all, you need to enter special scene where everything happens. This can be done by pressing `F2` (You need to be in the main menu).

### Pick a model
You can search for a model by localized name, body object name or model object name.
By default only models that are visible in logbook are included in list, but you can include everything from `BodyCatalog` by toggle `Inlude all bodies from BodyCatalog`.

![](https://github.com/KingEnderBrine/-RoR2-ItemDisplayPlacementHelper/blob/master/Screenshots/2.png?raw=true)

### Select DisplayRuleGroup to work with
You can search for an item/equipment by name.

Each item display can be enabled/disabled by clicking on corresponding checkbox.
Only one equipment display can be enabled at a time, so if you enable new equipment old one will be disabled.

Click on a gear to start/end item display placement (All changes will be lost when you disable item display, so be careful with that).

* `Enable all` - enable all item displays for current model (except equipment)
* `Disable all` - enable all item displays for current model (inluding equipment)

![](https://github.com/KingEnderBrine/-RoR2-ItemDisplayPlacementHelper/blob/master/Screenshots/3.png?raw=true)

### Select ItemDisplayRule
Click on a row with item display prefab name to start edit it.

![](https://github.com/KingEnderBrine/-RoR2-ItemDisplayPlacementHelper/blob/master/Screenshots/4.png?raw=true)

### Edit ItemDisplayRule
You can type in values in corresponding fields to see how item display will look on a character.

![](https://github.com/KingEnderBrine/-RoR2-ItemDisplayPlacementHelper/blob/master/Screenshots/5.png?raw=true)

Or you can use Unity-like item editing on the scene:

#### Moving
![](https://github.com/KingEnderBrine/-RoR2-ItemDisplayPlacementHelper/blob/master/Screenshots/6.png?raw=true)

#### Rotating
White circle will rotate object around to camera view axis.

![](https://github.com/KingEnderBrine/-RoR2-ItemDisplayPlacementHelper/blob/master/Screenshots/7.png?raw=true)

#### Scaling
Gray cube in center will scale all axes simultaneously.

![](https://github.com/KingEnderBrine/-RoR2-ItemDisplayPlacementHelper/blob/master/Screenshots/8.png?raw=true)

### Copy values
When you placed your item where you want you can copy values for that position by clicking `Copy IDR values` button to paste these values in your code.

![](https://github.com/KingEnderBrine/-RoR2-ItemDisplayPlacementHelper/blob/master/Screenshots/9.png?raw=true)

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

#### Placeholders description

* **{m:childName}** - Child name as ***"string"***
* **{m:modelName}** - Model GameObject name as ***"string"***
* **{m:bodyName}** - Body GameObject name as ***"string"***
* **{m:localPos.z:X}** - Local position as ***new Vector3(...)***
* **{m:localAngles.z:X}** - Local rotation as ***new Vector3(...)***
* **{m:localScale.z:X}** - Local scale as ***new Vector3(...)***
* **{m:itemName}** - ItemDef/EquipmentDef name as ***"string"***
* **{m:objectName}** - Current IDR GameObject name as ***"string"***

**m:** - Modificator for the output value. Available options:
* **r** - Output raw value:
  * For a string will not add **"** around the value
  * For a Vector3 fields will not add **F** after the value
**:X** - Maximum digits after the decimal point. If not specified 5 will be used e.g. {localPos} is the same as {localPos:5}

**.z** - For Vector3 fields you can access individual fields, not specifying any will print **new Vector3(...)**

# Top panel
![](https://github.com/KingEnderBrine/-RoR2-ItemDisplayPlacementHelper/blob/master/Screenshots/10.png?raw=true)

### ConfigureSensitivity
* `Fast Coefficient` - camera movement speed multiplier when holding `SHIFT`
* `Slow Coefficient` - camera movement speed multiplier when holding `CTRL`

### Enable fade
Toggle fade when you are close to objects.

### Enable time
Toggle TimeScale between 0 and 1.

### DynamicBones ignore TimeScale
Toggle if DynamicBones should work when TimeScale is 0

### Object Editing
![](https://github.com/KingEnderBrine/-RoR2-ItemDisplayPlacementHelper/blob/master/Screenshots/11.png?raw=true)

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