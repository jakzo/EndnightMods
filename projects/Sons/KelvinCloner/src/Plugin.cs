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
  public override void Load() { AddComponent<Cloner>(); }
}

public static class Log {
  public static void Dbg(string msg) {
#if DEBUG
    Plugin.Instance.Log.LogInfo("[debug] " + msg);
#endif
  }
  public static void Info(string msg) => Plugin.Instance.Log.LogInfo(msg);
  public static void Error(string msg) => Plugin.Instance.Log.LogError(msg);
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
