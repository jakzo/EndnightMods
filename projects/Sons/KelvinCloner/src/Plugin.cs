using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Sons.Gameplay;
using Sons.Characters;
using TheForest.Utils;
using Bolt;
using System.Collections;
using System.Linq;
using System;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Sons.Animation;

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
    AddComponent<OrderBroadcaster>();
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
    CharacterManager.Instance.DebugAddCharacter("robby 1");
  }
}

public class OrderBroadcaster : MonoBehaviour {
  private static OrderBroadcaster Instance;

  public void Awake() { Instance = this; }

  public void Destroy() {
    if (Instance == this)
      Instance = null;
  }

  private static Robby[] _listeningRobbies;

  private static bool _isPreparingBroadcastOrder {
    get => _listeningRobbies != null;
  }

  private static void WithAllRobbies(Robby interactedRobby,
                                     Action<Robby, Robby> action) {
    Instance?.StartCoroutine(
        RobbyEnumerator(interactedRobby, action).WrapToIl2Cpp());
  }
  private static IEnumerator RobbyEnumerator(Robby interactedRobby,
                                             Action<Robby, Robby> action) {
    if (_listeningRobbies == null)
      yield break;

    foreach (var robby in _listeningRobbies) {
      yield return new WaitForSeconds(0.5f);
      action(robby, interactedRobby);
    }
  }

  [HarmonyPatch(typeof(Robby), nameof(Robby.LocalPlayerStartGivingOrders))]
  class Robby_LocalPlayerStartGivingOrders_Patch {
    [HarmonyPrefix()]
    internal static void Prefix(Robby __instance) {
      if (!Instance || !Input.GetKeyAlt()) {
        _listeningRobbies = null;
        return;
      }

      Log.Debug("Preparing order to broadcast to all Kelvins");
      _listeningRobbies = GameObject.FindObjectsOfType<Robby>()
                              .Where(robby => robby != __instance)
                              .ToArray();
      WithAllRobbies(__instance,
                     ModifiedSource._Robby.LocalPlayerStartGivingOrders);
    }
  }

  [HarmonyPatch(typeof(Robby), nameof(Robby.OnLocalPlayerGiveOrderFinished))]
  class Robby_OnLocalPlayerGiveOrderFinished_Patch {
    [HarmonyPostfix()]
    internal static void Postfix(Robby __instance) {
      if (_isPreparingBroadcastOrder) {
        WithAllRobbies(__instance, (robby, interactedRobby) => {
          // TODO: Copy relevant _tactiPad values
          robby._tactiPad._selectedCarousel =
              interactedRobby._tactiPad._selectedCarousel;
          ModifiedSource._Robby.OnLocalPlayerGiveOrderFinished(robby,
                                                               interactedRobby);
        });
        _listeningRobbies = null;
        Log.Debug("Order was broadcast to all Kelvins");
      }
    }
  }

  [HarmonyPatch(typeof(Robby), nameof(Robby.OnLocalPlayerGiveOrderCancelled))]
  class Robby_OnLocalPlayerGiveOrderCancelled_Patch {
    [HarmonyPostfix()]
    internal static void Postfix(Robby __instance) {
      if (_isPreparingBroadcastOrder) {
        WithAllRobbies(__instance,
                       ModifiedSource._Robby.OnLocalPlayerGiveOrderCancelled);
        _listeningRobbies = null;
        Log.Debug("Order broadcast was cancelled");
      }
    }
  }
}
}
