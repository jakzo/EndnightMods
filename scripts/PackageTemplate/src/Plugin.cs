using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace Jakzo.EndnightMods.$$NAME$$ {
[BepInPlugin("Jakzo.EndnightMods.$$NAME$$", "$$NAME$$",
             Jakzo.EndnightMods.$$NAME$$.AppVersion.Value)]
[BepInProcess("SonsOfTheForest.exe")]
public class Plugin : BasePlugin {
  public static Plugin Instance;
  public Plugin() { Instance = this; }
  public override void Load() { Jakzo.EndnightMods.Log.Logger = Log; }
}
}
