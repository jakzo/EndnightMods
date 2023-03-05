using UnityEngine;

namespace Jakzo.EndnightMods {
public static class Input {
  public static bool GetKeyAlt() => UnityEngine.Input.GetKey(KeyCode.LeftAlt) ||
                                    UnityEngine.Input.GetKey(KeyCode.RightAlt);
}
}
