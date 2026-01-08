# GunnerModPC

## Features
- [ Enabled] Extra vehicles at the Grafenwoehr tank range.
	- Playable: T-34-85 and T-54A
	- Targets: OH-58A, AH-1, Mi-2, Mi-8T, Mi-24, Mi-24V (Soviet), Mi-24V (East German)
	  Currently there is an issue with the helicopter targets in which they lose control during the loading screen and some may crash before you gain control.
	  Also, the first time you press `Shift` to enter aim mode, it teleports the camera to a helicopter and you have switch to some other vehicle to fix it.
- [ Enabled] Live shot damage report
- [ Enabled] FPS counter
- [ Enabled] Custom 3rd person crosshair color
- [ Enabled] Map selector dropdown fix
- [Disabled] Removed 3rd person crosshair

All features can be toggled in `\Gunner, HEAT, PC!\Bin\UserData\MelonPreferences.cfg`. Make sure to run the mod at least once to generate the settings.

Everything should be working on the latest game version.

## Installation
- Install MelonLoader: [link](https://github.com/LavaGang/MelonLoader.Installer/blob/master/README.md#how-to-install-re-install-or-update-melonloader).
- Download `GunnerModPC.dll` from the latest release ([link](https://github.com/Andrix44/GunnerModPC/releases/latest)) and copy it to `\Gunner, HEAT, PC!\Bin\Mods`.

## Images
![T-34-85 at Grafenwoehr](https://github.com/Andrix44/GunnerModPC/assets/13806656/101581ed-2a18-4930-a4d6-4892860a5b99)
<img width="2560" height="1440" alt="Helicopter targets" src="https://github.com/user-attachments/assets/f8ef7369-b107-4042-8af5-f43fc54f5722" />


## Building
You can simply build the project with Visual Studio, but first you have to fix the references so that they point to your game DLLs.
