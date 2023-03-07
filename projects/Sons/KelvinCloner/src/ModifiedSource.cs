using System;
using UnityEngine;
using Sons.Gameplay;
using Sons.Input;
using TheForest.Utils;
using Sons.Gui;
using Sons.Animation;
using TheForest.Items.Inventory;
using Sons.Ai.Vail.StimuliTypes;
using Bolt;
using static Sons.Gameplay.Robby;

// Logic derived from decompilation of the game and modified for our purposes
// Only logic affecting Robby and not global state or the player is kept
// First interaction logic (where the player gives note to Robby) is removed
// Removed lines are commented out
namespace Jakzo.EndnightMods.KelvinCloner.ModifiedSource {
public class _Robby {
  public static void LocalPlayerStartGivingOrders(Robby This,
                                                  Robby interactedRobby) {
    This._isFirstInteraction = false;
    // ShoulderTapActor._interactionUIActive = true;
    // ShoulderTapActor._tappedActor.SetCurrentMenu(
    //     ShoulderTapActor.CurrentMenu.Interaction);
    LocalPlayerOpenTactipad(This, interactedRobby);
  }

  public static void LocalPlayerOpenTactipad(Robby This,
                                             Robby interactedRobby) {
    InitPadIfNeeded(This, interactedRobby);
    if (This._originalFov != 0f) {
      //   This.StartCoroutine(
      //       Robby.ResetFov(This._originalFov, This._resetFovTime));
      This._originalFov = 0f;
    }
    This._localPadState = PadState.Open;
    This._padStateChangeFrame = Time.frameCount;
    // InputSystem.SetState(InputState.Menu, true);
    // This.BuildTactipadUI();
    // This._tactiPad.Show(true); // TODO: Check if we need this
    // This._tactiPad.transform.parent =
    //     LocalPlayer.RobbyInteraction._rightHandHeld;
    // This._tactiPad.transform.localPosition = new Vector3();    // todo
    // This._tactiPad.transform.localScale = new Vector3();       // todo
    // This._tactiPad.transform.localRotation = new Quaternion(); // todo
    // This.LockPlayer(true);
    // This.LocalPlayerStartFaceRobby();
    // LocalPlayer.Animator.Play(AnimationHashes.RobbyPadRaiseHash);
    // This._tactiPad._animator.Play(AnimationHashes.RobbyPadRaiseHash);
    if (!BoltNetwork.isClient) {
      This.ServerStartTakingOrders(LocalPlayer.PlayerStimuli);
      This.DropNow();
    }
    This.SendOrderEvent(OrderEventType.WaitForOrders, GlobalTargets.OnlyServer);
  }

  public static void InitPadIfNeeded(Robby This, Robby interactedRobby) {
    // This._tactiPad =
    //     LocalPlayer.RobbyInteraction.CreatePadAndPenIfNeeded();
    This._tactiPad = _PlayerRobbyInteraction.CreatePadAndPenIfNeeded(
        LocalPlayer.RobbyInteraction);

    This._tactiPad._onOrderFinished =
        new Action(() => OnLocalPlayerGiveOrderFinished(This, interactedRobby));
    This._tactiPad._onOrderCanceled = new Action(
        () => OnLocalPlayerGiveOrderCancelled(This, interactedRobby));
  }

  public static void OnLocalPlayerGiveOrderFinished(Robby This,
                                                    Robby interactedRobby) {
    // Get the selection from the interacted Robby's _tactiPad instead
    // var newPadState =
    //     This._tactiPad.GetSelection(0) == (int)CarouselName.DropLocation
    //         ? PadState.LowerToGiveItem
    //         : PadState.GivingNote;
    var newPadState = interactedRobby._tactiPad.GetSelection(0) ==
                              (int)CarouselName.DropLocation
                          ? PadState.LowerToGiveItem
                          : PadState.GivingNote;

    if (!This._actor._isDead && This._actor.IsDying())
      newPadState = PadState.Cancelling;
    LocalPlayerCloseTactipad(This, newPadState);
  }

  public static void OnLocalPlayerGiveOrderCancelled(Robby This,
                                                     Robby interactedRobby) =>
      OnLocalPlayerGiveOrderFinished(This, interactedRobby);

  public static void LocalPlayerCloseTactipad(Robby This,
                                              PadState newPadState) {
    // ShoulderTapActor.OnHideInteractionUI(
    //     newPadState == PadState.GivingNote ? 10f : 2f);
    This._localPadState = newPadState;
    This._padStateChangeFrame = Time.frameCount;
    This._faceLerpTime = -1f;
    // InputSystem.SetState(InputState.Menu, false);

    // Remove first interaction logic
    // var animation = newPadState == PadState.GivingNote
    //                     ? This._isFirstInteraction
    //                           ? AnimationHashes.RobbyPadGiveNoteHash
    //                           : AnimationHashes.RobbyPadGiveOrderFastHash
    //                     : AnimationHashes.RobbyPadLowerHash;
    var animation = newPadState == PadState.GivingNote
                        ? AnimationHashes.RobbyPadGiveOrderFastHash
                        : AnimationHashes.RobbyPadLowerHash;

    // LocalPlayer.Animator.Play(animation);
    // if (animation == AnimationHashes.RobbyPadGiveOrderFastHash)
    //   This.LocalPlayerStartFaceRobby();
    // else
    //   This._tactiPad._animator.Play(animation);

    // Remove first interaction logic
    // This.DoRobbyAnimatorFinishOrdersTransition(
    //     newPadState == PadState.GivingNote, This._isFirstInteraction);
    This.DoRobbyAnimatorFinishOrdersTransition(
        newPadState == PadState.GivingNote, false);

    if (BoltNetwork.isClient) {
      This.SendOrderEvent(newPadState == PadState.GivingNote
                              ? OrderEventType.FinishOrderNoNote
                              : OrderEventType.FinishOrderGiveNote,
                          GlobalTargets.OnlyServer);
    }
  }
}

public class _PlayerRobbyInteraction {

  public static RobbyWorldUi
  CreatePadAndPenIfNeeded(PlayerRobbyInteraction This) {
    if (!This._tactiPad) {
      This._tactiPad = GameObject.Instantiate(This._guiPrefab);
      This._tactiPad.gameObject.active = false;
      This._tactiPad.transform.parent = This._rightHandHeld;
      This._tactiPad.transform.localPosition = Vector3.zero;
      This._tactiPad.transform.localRotation = Quaternion.identity;
      This._tactiPad.transform.localScale = This._rightHandHeld.lossyScale;
      if (This._isRemote)
        This._tactiPad.DisableUI();
    }
    if (!This._tactiPen) {
      This._tactiPen = GameObject.Instantiate(This._penPrefab);
      This._tactiPen.gameObject.active = false;
      This._tactiPen.transform.parent = This._leftHandHeld;
      // TODO: Figure out actual values here
      This._tactiPen.transform.localPosition = Vector3.zero;
      This._tactiPen.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
      This._tactiPen.transform.localScale = This._leftHandHeld.lossyScale;
    }
    return This._tactiPad;
  }
}
}
