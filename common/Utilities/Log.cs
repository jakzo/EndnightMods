using BepInEx.Logging;

namespace Jakzo.EndnightMods {
public static class Log {
  public static ManualLogSource Logger;

  public static void Debug(string msg) {
#if DEBUG
    Logger.LogInfo("[debug] " + msg);
#endif
  }
  public static void Info(string msg) => Logger.LogInfo(msg);
  public static void Error(string msg) => Logger.LogError(msg);
}
}
