Automatically loads `.idrsjson` file in the `plugins` folder produced by [ItemDisplayPlacementHelper](https://thunderstore.io/package/KingEnderBrine/ItemDisplayPlacementHelper/) export and applies it to a body specified inside it.

Replaces existing values in an ItemDisplayRuleSet. File order loading is undefined, so if multiple files add display for the same item/equipment whichever was loaded last will be the winner.
If `skinName` is not empty, `R2API.Skins` is used to add the rules only for that skin.

Any `AssetBundle` specified in the file must be loaded by the time catalogs are initialized.

If you want to manually pass json you can call `IDRSJsonLoaderPlugin.ParseAndUpdate()`.