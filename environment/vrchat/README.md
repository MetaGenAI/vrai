
1. You need to use Windows.
2. Download the vrai repo
3. Download VRChat on Steam
4. Download VRCModInstaller (https://github.com/Slaynash/VRChatModInstaller/releases). Run it and select VRCTools, Harmony and VRCMenuUtils
5. Install Visual Studio (https://visualstudio.microsoft.com/vs/community/)
6. Install Unity
7. Install ML-agents

----------

After installing all this, we need to copy the relevant DLLs to `Mod` folder in the VRChat installation folder (usually `C:\Program Files (x86)\Steam\steamapps\common\VRChat\`):

* `VRCTestingKit.dll` from https://github.com/FusGang/VRCTestingKit/releases
* `VRCModLoader.dll`, `vrcai.dll`, `Newtonsoft.Json.dll`, `ServiceWire.dll` from `vrcai/bin/Debug`
* `System.Threading.dll` from `VRChat_Data/Managed` in the VRChat installation folder (usually `C:\Program Files (x86)\Steam\steamapps\common\VRChat\`)
* `VrcaiMlaCommunicator.dll` from `VrcaiMlaCommunicator/bin/Debug`

Then just start VRChat in desktop mode, and you can use F1 to toggle the VRCTetingKit con, and press TAB to toggle between freeing the mouse for use with the VRCTestingKit UI and using it to control the game
