const string BIN_DIR = "bin";
const string PROJECTS_DIR = "projects";

void CopyBuildsToBin() {
  if (Directory.Exists(BIN_DIR))
    Directory.Delete(BIN_DIR, true);
  Directory.CreateDirectory(BIN_DIR);

  foreach (var gameDir in Directory.EnumerateDirectories(PROJECTS_DIR)) {
    var gameName = Path.GetFileName(gameDir);
    foreach (var projectDir in Directory.EnumerateDirectories(gameDir)) {
      var buildFilename = $"{Path.GetFileName(projectDir)}.dll";
      var buildFile = Path.Combine(projectDir, "bin", "Debug", buildFilename);
      File.Copy(buildFile, Path.Combine(BIN_DIR, $"{gameName}{buildFilename}"));
    }
  }
}

try {
  CopyBuildsToBin();
  Console.WriteLine("All done!");
} catch (Exception ex) {
  Console.WriteLine(ex);
  Environment.Exit(1);
}
