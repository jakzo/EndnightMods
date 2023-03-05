using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace Jakzo.EndnightMods.KelvinCloner {
[BepInPlugin("Jakzo.EndnightMods.KelvinCloner", "KelvinCloner",
             Jakzo.EndnightMods.KelvinCloner.AppVersion.Value)]
[BepInProcess("SonsOfTheForest.exe")]
public class Plugin : BasePlugin {
  public static Plugin Instance;
  public Plugin() { Instance = this; }
  public override void Load() {
    Jakzo.EndnightMods.Log.Logger = Log;
    AddComponent<Cloner>();
  }
}

public class Cloner : MonoBehaviour {
  public void Update() {
    if (GetKeyAlt() && Input.GetKeyDown(KeyCode.K)) {
      Log.Info("Cloning Kelvin");
      SpawnRobby();
    }
  }

  public void SpawnRobby() {
    Sons.Characters.CharacterManager.Instance.DebugAddCharacter("robby 1");
  }

  private bool GetKeyAlt() =>
      Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
}
}
