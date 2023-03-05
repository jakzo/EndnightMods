using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Sons.Gameplay;

namespace Jakzo.EndnightMods.KelvinCloner {
[BepInPlugin("Jakzo.EndnightMods.KelvinCloner", "KelvinCloner",
             Jakzo.EndnightMods.KelvinCloner.AppVersion.Value)]
[BepInProcess("SonsOfTheForest.exe")]
public class Plugin : BasePlugin {
  public static Plugin Instance;
  public Plugin() { Instance = this; }
  public override void Load() {
    Jakzo.EndnightMods.Log.Logger = Log;
    Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
    AddComponent<Cloner>();
  }
}

public class Cloner : MonoBehaviour {
  public void Update() {
    if (Input.GetKeyAlt() && UnityEngine.Input.GetKeyDown(KeyCode.K)) {
      Log.Info("Cloning Kelvin");
      SpawnRobby();
    }
  }

  public void SpawnRobby() {
    Sons.Characters.CharacterManager.Instance.DebugAddCharacter("robby 1");
  }
}

public static class OrderBroadcaster {
  private static bool _isBroadcastingOrder = false;

  [HarmonyPatch(typeof(Robby), nameof(Robby.LocalPlayerStartGivingOrders))]
  class Robby_LocalPlayerStartGivingOrders_Patch {
    [HarmonyPrefix()]
    internal static bool Prefix(Robby __instance) {
      if (_isBroadcastingOrder || !Input.GetKeyAlt())
        return true;

      Log.Debug("Broadcasting order to all");
      _isBroadcastingOrder = true;
      var allRobbies = GameObject.FindObjectsOfType<Robby>();
      foreach (var robby in allRobbies) {
        if (robby != __instance)
          robby.LocalPlayerStartGivingOrders();
      }
      // Make last call to original Robby for player orientation
      __instance.LocalPlayerStartGivingOrders();
      _isBroadcastingOrder = false;
      return false;
    }
  }
}
}
