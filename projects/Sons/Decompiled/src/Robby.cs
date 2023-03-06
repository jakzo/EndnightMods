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

namespace Decompiled.Gameplay {
// Handles Kelvin's logic (assigning and performing tasks)
class _Robby : Robby {
  void LocalPlayerStartGivingOrders() {
    _isFirstInteraction = false;
    ShoulderTapActor._interactionUIActive = true;
    ShoulderTapActor._tappedActor.SetCurrentMenu(
        ShoulderTapActor.CurrentMenu.Interaction);
    LocalPlayerOpenTactipad();
  }

  void LocalPlayerOpenTactipad() {
    InitPadIfNeeded();
    if (_originalFov != 0f) {
      StartCoroutine(ResetFov(_originalFov, _resetFovTime));
      _originalFov = 0f;
    }
    _localPadState = PadState.Open;
    _padStateChangeFrame = Time.frameCount;
    InputSystem.SetState(InputState.Menu, true);
    BuildTactipadUI();
    _tactiPad.Show(true);
    _tactiPad.transform.parent = LocalPlayer.RobbyInteraction._rightHandHeld;
    _tactiPad.transform.localPosition = new Vector3();    // todo
    _tactiPad.transform.localScale = new Vector3();       // todo
    _tactiPad.transform.localRotation = new Quaternion(); // todo
    LockPlayer(true);
    LocalPlayerStartFaceRobby();
    LocalPlayer.Animator.Play(AnimationHashes.RobbyPadRaiseHash);
    _tactiPad._animator.Play(AnimationHashes.RobbyPadRaiseHash);
    if (!BoltNetwork.isClient) {
      ServerStartTakingOrders(LocalPlayer.PlayerStimuli);
      DropNow();
    }
    SendOrderEvent(OrderEventType.WaitForOrders, GlobalTargets.OnlyServer);
  }

  void InitPadIfNeeded() {
    _tactiPad = LocalPlayer.RobbyInteraction.CreatePadAndPenIfNeeded();
    _tactiPad._onOrderFinished = new Action(OnLocalPlayerGiveOrderFinished);
    _tactiPad._onOrderCanceled = new Action(OnLocalPlayerGiveOrderCancelled);
  }

  void OnLocalPlayerGiveOrderFinished() {
    var newPadState =
        _tactiPad.GetSelection(0) == (int)CarouselName.DropLocation
            ? PadState.LowerToGiveItem
            : PadState.GivingNote;
    if (!_actor._isDead && _actor.IsDying())
      newPadState = PadState.Cancelling;
    LocalPlayerCloseTactipad(newPadState);
  }

  void OnLocalPlayerGiveOrderCancelled() => OnLocalPlayerGiveOrderFinished();

  void BuildTactipadUI() {
    // TODO: UI stuff...
  }

  void LockPlayer(bool locked) {
    LocalPlayer.AnimControl.lockGravity = locked;
    LocalPlayer.MainRotator.enabled = !locked;
    LocalPlayer.FpCharacter.Locked = locked;
    LocalPlayer.AnimControl.doingFullBodyAction = locked;
    LocalPlayer.Inventory.BlockTogglingInventory = locked; // probably
    LocalPlayer.Rigidbody.useGravity = !locked;
    LocalPlayer.Rigidbody.isKinematic = locked;
    if (locked) {
      LocalPlayer.Inventory.StashHeldItems(true, true);
      BoltSetReflectedShim.SetLayerWeightReflected(LocalPlayer.Animator, 2, 0f);
      BoltSetReflectedShim.SetLayerWeightReflected(LocalPlayer.Animator, 5, 0f);
      LocalPlayer.AnimControl.RegisterLayerBehaviourActivators(
          _fullBodyDisableSpineActivators);
      LocalPlayer.AnimControl.AnimControlledPlayer(true, false);
    } else {
      LocalPlayer.ScriptSetup.SetHitDetection(true);
      LocalPlayer.FpCharacter.SetIsInMidAction(false);
      if (LocalPlayer.Inventory.CurrentView !=
          PlayerInventory.PlayerViews.Inventory) {
        LocalPlayer.FpCharacter.UnLockView();
      }
      LocalPlayer.AnimControl.UnregisterLayerBehaviourActivators(
          _fullBodyDisableSpineActivators);
      LocalPlayer.AnimControl.AnimControlledPlayer(false, false);
      if (LocalPlayer.Inventory.IsRightHandHolding(351)) { // 351 = grab bag
        LocalPlayer.AnimControl.SetPlayerLocked(true);
        LocalPlayer.CamRotator.enabled = false;
        LocalPlayer.MainRotator.enabled = false;
        LocalPlayer.FpCharacter.LockView(true, false, false);
        LocalPlayer.AnimControl.SetSplineBlendForItem(351, true, 1.075f);
      } else {
        LocalPlayer.Inventory.EquipPreviouslyHeldItems();
      }
    }
  }

  void LocalPlayerStartFaceRobby(PadState newPadState) {
    // TODO: Position/rotation stuff...
  }

  void ServerStartTakingOrders(PlayerStimuli playerStimuli) {
    _interactingPlayer = playerStimuli;
    _followTransform = _interactingPlayer.transform;
    _followStimuliInstance.enabled = true;
    playerStimuli.StartActorInteract(_actor);
    if (!BoltNetwork.isClient) {
      ClearRobbyOrderHashes();
      _robbyAnimator.SetBoolID(AnimationHashes.padRaiseHash, true);
    }
  }

  void DropNow() {
    _dropStimuliInstance.enabled = true;
    _actor.AddImmediateStimuli(new DropNowStimuli());
  }

  void LocalPlayerCloseTactipad(PadState newPadState) {
    ShoulderTapActor.OnHideInteractionUI(
        newPadState == PadState.GivingNote ? 10f : 2f);
    _localPadState = newPadState;
    _padStateChangeFrame = Time.frameCount;
    _faceLerpTime = -1f;
    InputSystem.SetState(InputState.Menu, false);
    var animation = newPadState == PadState.GivingNote
                        ? _isFirstInteraction
                              ? AnimationHashes.RobbyPadGiveNoteHash
                              : AnimationHashes.RobbyPadGiveOrderFastHash
                        : AnimationHashes.RobbyPadLowerHash;
    LocalPlayer.Animator.Play(animation);
    if (animation == AnimationHashes.RobbyPadGiveOrderFastHash)
      LocalPlayerStartFaceRobby();
    else
      _tactiPad._animator.Play(animation);
    DoRobbyAnimatorFinishOrdersTransition(newPadState == PadState.GivingNote,
                                          _isFirstInteraction);
    if (BoltNetwork.isClient) {
      SendOrderEvent(newPadState == PadState.GivingNote
                         ? OrderEventType.FinishOrderNoNote
                         : OrderEventType.FinishOrderGiveNote,
                     GlobalTargets.OnlyServer);
    }
  }

  void LocalPlayerStartFaceRobby() {
    // TODO: Animation stuff...
  }

  void DoRobbyAnimatorFinishOrdersTransition(bool giveNote, bool longNote) {
    if (BoltNetwork.isClient)
      return;
    ClearRobbyOrderHashes();
    _robbyAnimator.SetBoolID(giveNote ? longNote
                                            ? AnimationHashes.padGiveNoteHash
                                            : AnimationHashes.padGiveOrderHash
                                      : AnimationHashes.padLowerHash,
                             true);
  }

  void ClearRobbyOrderHashes() {
    // TODO: Animation stuff...
  }

  void SendOrderEvent(OrderEventType orderEventType, GlobalTargets targets) {
    var evt = RobbyOrderEvent.Create(targets);
    evt.orderEventType = (int)orderEventType;
    evt.target = GetComponentInParent<BoltEntity>();
    evt.playerEntity = LocalPlayer.Transform.GetComponent<BoltEntity>();
    evt.Send();
  }
}
}
