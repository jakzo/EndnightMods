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
class _PlayerStimuli : PlayerStimuli {
  void StartActorInteract(VailActor actor) {
    if (!_interactStimuli) {
      Debug.LogError("Missing interactStimuli on " + name);
      return;
    }
    _interactStimuli._actor = actor;
    _interactStimuli.enabled = true;
    if (BoltNetwork.isClient)
      ClientSendActorInteractEvent(actor);
  }
}
}
