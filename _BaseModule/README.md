# Model project for Write Everywhre modules mods

This is a project to be used as starter point to any new module pack of Write Everywhere mod for Cities Skylines 2.

## Setting up

- Rename this project and solution to setup the DLL name this module will have.
	- TIP: To avoid conflicts, start the project name with a acronym representing your name, example: `XYZ_MyWEModule` being XYZ the acronym that represents you.
- Add resources at each `Resources/` subfolder as instructed locally there
- Change csproj file details (DisplayName, ShortDescription, ForumLink, other social media links tags)
- Setup thumbnail into `Properties/Thumbnail.png`
- Add screenshots at `Screenshots/` folder (jpg files, optional)
- Publish the mod using "Publish..." option in context menu of the project
- After having a modId in Paradox Mods, setup it into the ModId parameter at csproj file

After that, just don't forget to change the Version parameter in the csproj file if you need to upload new versions of the module!