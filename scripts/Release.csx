#r "System.Xml"

using System.Xml;

string[] semverTypes = { "major", "minor", "patch" };

string SemverIncrement(string version, int semverTypeIdx) {
  var parts = version.Split('.');
  parts[semverTypeIdx] = (int.Parse(parts[semverTypeIdx]) + 1).ToString();
  int idx = semverTypeIdx;
  while (++idx < parts.Length)
    parts[idx] = "0";
  return String.Join(".", parts);
}

bool UpdateThunderstoreManifest(string projectRelativePath, string newVersion) {
  string manifestPath = $"{projectRelativePath}/thunderstore/manifest.json";
  if (!File.Exists(manifestPath)) {
    Console.WriteLine($"No ThunderStore manifest found. Skipping.");
    return false;
  }

  var manifestJson = File.ReadAllText(manifestPath);
  const string MANIFEST_SEARCH_TERM = "\"version_number\": \"";
  var manifestStartIdx = manifestJson.IndexOf(MANIFEST_SEARCH_TERM);
  if (manifestStartIdx == -1)
    throw new Exception("Manifest version not found");
  manifestStartIdx += MANIFEST_SEARCH_TERM.Length;
  var manifestEndIdx = manifestJson.IndexOf("\"", manifestStartIdx);
  File.WriteAllText(manifestPath, manifestJson.Substring(0, manifestStartIdx) +
                                      newVersion +
                                      manifestJson.Substring(manifestEndIdx));
  return true;
}

void UpdateChangelog(string projectRelativePath, string newVersion,
                     string changelogDescription) {
  string changelogPath = $"{projectRelativePath}/CHANGELOG.md";
  if (!File.Exists(changelogPath)) {
    Console.WriteLine($"No changelog found. Skipping.");
    return;
  }

  var oldChangelog = File.ReadAllText(changelogPath);
  var newChangelog =
      $"## {newVersion}\n\n{changelogDescription}\n\n{oldChangelog}";
  File.WriteAllText(changelogPath, newChangelog);

  Console.WriteLine("CHANGELOG.md updated");
}

void ReleaseProject(string gameName, string projectName, string semverTypeArg,
                    string changelogDescription) {
  var semverTypeIdx =
      Array.FindIndex(semverTypes, type => type == semverTypeArg);
  if (semverTypeIdx == -1)
    throw new Exception($"Unknown semver increment type: {semverTypeArg}");

  var projectRelativePath = Path.Combine("projects", gameName, projectName);
  var appVersionPath = Path.Combine(projectRelativePath, "AppVersion.cs");
  var appCode = File.ReadAllText(appVersionPath);
  const string APP_VERSION_SEARCH_TERM = "Value = \"";
  var appStartIdx = appCode.IndexOf(APP_VERSION_SEARCH_TERM);
  if (appStartIdx == -1)
    throw new Exception("App version not found");
  appStartIdx += APP_VERSION_SEARCH_TERM.Length;
  var appEndIdx = appCode.IndexOf("\"", appStartIdx);
  var oldVersion = appCode.Substring(appStartIdx, appEndIdx - appStartIdx);
  var newVersion = SemverIncrement(oldVersion, semverTypeIdx);

  Console.WriteLine($"Old version = {oldVersion}");
  Console.WriteLine($"Version increment type = {semverTypeArg}");
  Console.WriteLine($"New version = {newVersion}");

  File.WriteAllText(appVersionPath, appCode.Substring(0, appStartIdx) +
                                        newVersion +
                                        appCode.Substring(appEndIdx));

  var isThunderStorePackage =
      UpdateThunderstoreManifest(projectRelativePath, newVersion);

  UpdateChangelog(projectRelativePath, newVersion, changelogDescription);

  Console.WriteLine("Setting Github action outputs");
  var escapedChangelog = changelogDescription.Replace("%", "'%25'")
                             .Replace("\n", "'%0A'")
                             .Replace("\r", "'%0D'");
  var githubOutput = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
  File.AppendAllText(githubOutput, $"new_version={newVersion}\n");
  File.AppendAllText(githubOutput, $"changelog={escapedChangelog}\n");
  var releaseThunderstore = isThunderStorePackage ? "true" : "false";
  File.AppendAllText(githubOutput,
                     $"release_thunderstore={releaseThunderstore}\n");
}

try {
  var gameName = Args[0];
  var projectName = Args[1];
  var semverTypeArg = Args[2].ToLower();
  var changelogDescription = Args[3];
  ReleaseProject(gameName, projectName, semverTypeArg, changelogDescription);

  Console.WriteLine("All done!");
} catch (Exception ex) {
  Console.WriteLine(ex);
  Environment.Exit(1);
}
