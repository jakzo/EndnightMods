using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;
using Sons.Ai.Vail;
using Sons.Gameplay;
using Sons.Save;
using HarmonyLib;
using System.Reflection;
using System;
using System.Collections;
using TheForest.Utils;
using Sons.Animation;

namespace Jakzo.EndnightMods.KelvinCloner {
[BepInPlugin("Jakzo.EndnightMods.KelvinCloner", "KelvinCloner",
             Jakzo.EndnightMods.KelvinCloner.AppVersion.Value)]
[BepInProcess("SonsOfTheForest.exe")]
public class Plugin : BasePlugin {
  public static Plugin Instance;
  public Plugin() { Instance = this; }
  public override void Load() {
    AddComponent<Cloner>();
    Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
  }
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
      StartCoroutine(SpawnRobby().WrapToIl2Cpp());
    }
  }

  public IEnumerator SpawnRobby() {
    try {
      var (spawnPos, spawnRot) = GetSpawnLocation();
      VailWorldSimulation.SetRobbySpawnLocation(spawnPos, spawnRot);
    } catch (PlayerNotFoundException) {
      Log.Error("Player not found! (are you actually in-game?)");
      yield break;
    }

    var allRobbys = GameObject.FindObjectsOfType<Robby>();
    Robby lastCreatedRobby = null;
    int lastRobbyIndex = 0;
    foreach (var robby in allRobbys) {
      var transform = robby.gameObject.transform;
      while (transform.transform.parent)
        transform = transform.transform.parent;
      var idx = transform.GetSiblingIndex();
      if (!lastCreatedRobby || idx > lastRobbyIndex) {
        lastCreatedRobby = robby;
        lastRobbyIndex = idx;
      }
    }
    if (!lastCreatedRobby) {
      Log.Error("Could not find cloned Kelvin!");
      yield break;
    }

    // TODO: Does it set this back to true if just one dies?
    GameState.SetIsRobbyDead(false);

    yield return new WaitForEndOfFrame();

    // Robby spawns injured so speed through the animation
    HelpUp(lastCreatedRobby);
    lastCreatedRobby._robbyAnimator.speed = 1e9f;
    yield return new WaitForSeconds(0.1f);
    yield return new WaitForEndOfFrame();
    lastCreatedRobby._robbyAnimator.speed = 1f;
  }

  // Reimplementation of Robby.LocalPlayerHelpUp() with some things removed
  private void HelpUp(Robby robby) {
    robby._isFirstInteraction = false; // true;
    robby.InitPadIfNeeded();
    // BoltSetReflectedShim.SetLayerWeightReflected(
    //     LocalPlayer.Animator,
    //     LocalPlayer.Animator.GetLayerIndex("fullBodyActions"), 1f);
    robby.LockPlayer(true);
    LocalPlayer.ScriptSetup.SetHitDetection(false);
    LocalPlayer.FpCharacter.SetIsInMidAction(true);
    // LocalPlayer.Transform.position = robby._helpUpPlayerMarker.position;
    // LocalPlayer.Transform.rotation = robby._helpUpPlayerMarker.rotation;
    // LocalPlayer.Animator.Play(AnimationHashes.RobbyHelpUpHash);
    // if (robby._forceIntroFOV)
    //   LocalPlayer.MainCam.fieldOfView = robby._introFOV;
    robby._originalFov = LocalPlayer.MainCam.fieldOfView;
    robby._helpingPlayer = LocalPlayer.Transform;
    robby._robbyAnimator.Play(AnimationHashes.RobbyHelpUpHash);
    robby.SetInjuredState(Robby.InjuredState.GettingUp);
    if (!BoltNetwork.isClient) {
      robby.ServerHelpUp(LocalPlayer.Transform);
    } else {
      robby.SendOrderEvent(Robby.OrderEventType.HelpUp,
                           Bolt.GlobalTargets.OnlyServer);
    }
  }

  private (Vector3, Quaternion) GetSpawnLocation() {
    var player = GetPlayerTransform();
    return (player.position + player.rotation * new Vector3(0f, 0.5f, 1.5f),
            player.rotation * Quaternion.Euler(0f, 90f, 0f));
  }

  private Transform GetPlayerTransform() {
    var player = GameObject.FindObjectOfType<FirstPersonCharacter>();
    if (!player)
      throw new PlayerNotFoundException();
    return player.transform;
  }

  private class PlayerNotFoundException : Exception {}

  private bool GetKeyAlt() =>
      Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
}
}
