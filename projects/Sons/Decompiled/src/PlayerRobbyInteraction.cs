using System;
using UnityEngine;
using Sons.Gameplay;
using Sons.Input;
using TheForest.Utils;
using Sons.Gui;
using Sons.Animation;
using TheForest.Items.Inventory;
using Sons.Ai.Vail.StimuliTypes;
using Sons.Ai.Vail;
using Bolt;

namespace Decompiled.Gameplay {
class _PlayerRobbyInteraction : PlayerRobbyInteraction {
  RobbyWorldUi CreatePadAndPenIfNeeded() {
    if (!_tactiPad) {
      _tactiPad = GameObject.Instantiate(_guiPrefab);
      _tactiPad.gameObject.active = false;
      _tactiPad.transform.parent = _rightHandHeld;
      _tactiPad.transform.localPosition = Vector3.zero;
      _tactiPad.transform.localRotation = Quaternion.identity;
      _tactiPad.transform.localScale = _rightHandHeld.lossyScale;
      if (_isRemote)
        _tactiPad.DisableUI();
    }
    if (!_tactiPen) {
      _tactiPen = GameObject.Instantiate(_penPrefab);
      _tactiPen.gameObject.active = false;
      _tactiPen.transform.parent = _leftHandHeld;
      // TODO: Figure out actual values here
      _tactiPen.transform.localPosition = Vector3.zero;
      _tactiPen.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
      _tactiPen.transform.localScale = _leftHandHeld.lossyScale;
    }
    return _tactiPad;
  }
}
}
