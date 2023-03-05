using System.Text.RegularExpressions;

const string PROJECT_CONFIG_SECTION =
    "GlobalSection(ProjectConfigurationPlatforms) = postSolution";

void CreateNewProject() {
  if (Args.Count != 3) {
    Console.WriteLine(
        "Usage: csi ./scripts/CreateNewProject \"game\" \"ProjectName\" \"Description of project.\"");
    throw new ArgumentException("Wrong number of arguments provided");
  }

  var projectUuid = GenerateUuid();
  var templateVars = new TemplateVars() {
    Game = Args[0],
    Name = Args[1],
    Description = Args[2],
    ProjectUuid = projectUuid,
  };

  CopyTemplateDir(templateVars, Path.Combine("scripts", "PackageTemplate"),
                  Path.Combine("projects", templateVars.GameCapitalized,
                               templateVars.Name));

  var solutionContents = File.ReadAllText("EndnightMods.sln");
  var lastProjectIdx =
      solutionContents.LastIndexOf("EndProject") + "EndProject".Length;
  var projectConfigIdx = solutionContents.LastIndexOf(PROJECT_CONFIG_SECTION) +
                         PROJECT_CONFIG_SECTION.Length;
  var newSolutionContents =
      solutionContents.Substring(0, lastProjectIdx) +
      $"\nProject(\"{{{GenerateUuid()}}}\") = \"{templateVars.GameCapitalized}{templateVars.Name}\", \"projects\\{templateVars.GameCapitalized}\\{templateVars.Name}\\{templateVars.Name}.csproj\", \"{{{projectUuid}}}\"\nEndProject" +
      solutionContents.Substring(lastProjectIdx,
                                 projectConfigIdx - lastProjectIdx) +
      string.Join(
          "",
          new[] { "Debug", "Release" }.SelectMany(
              type => new[] { "ActiveCfg", "Build.0" }.Select(
                  config =>
                      $"\n    {{{projectUuid}}}.{type}|Any CPU.{config} = {type}|Any CPU"))) +
      "\n" + solutionContents.Substring(projectConfigIdx);
  File.WriteAllText("EndnightMods.sln", newSolutionContents);
}

void CopyTemplateDir(TemplateVars templateVars, string from, string to) {
  Directory.CreateDirectory(to);
  foreach (var filePath in Directory.EnumerateFiles(from)) {
    var newFilename = templateVars.ReplaceContents(Path.GetFileName(filePath));
    var newContents = templateVars.ReplaceContents(File.ReadAllText(filePath));
    File.WriteAllText(Path.Combine(to, newFilename), newContents);
  }
  foreach (var dirPath in Directory.EnumerateDirectories(from))
    CopyTemplateDir(templateVars, dirPath,
                    Path.Combine(to, Path.GetFileName(dirPath)));
}

string GenerateUuid() =>
    $"{RandomStr(8)}-{RandomStr(4)}-{RandomStr(4)}-{RandomStr(4)}-{RandomStr(12)}";

var random = new Random();
string RandomStr(int len) {
  var chars = "0123456789ABCDEF";
  var stringChars = new char[len];
  for (int i = 0; i < stringChars.Length; i++) {
    stringChars[i] = chars[random.Next(chars.Length)];
  }
  return new String(stringChars);
}

try {
  CreateNewProject();
  Console.WriteLine("All done!");
} catch (Exception ex) {
  Console.WriteLine(ex);
  Environment.Exit(1);
}

class TemplateVars {
  public string Game;
  public string Name;
  public string Description;
  public string ProjectUuid;

  public string GameCapitalized {
    get => char.ToUpper(Game[0]) + Game.Substring(1).ToLower();
  }

  public string ReplaceContents(string contents) =>
      Regex.Replace(contents, @"\$\$(\w*)\$\$", (match) => {
        switch (match.Groups[1].Value.ToUpper()) {
        case "NAME":
          return Name;
        case "DESCRIPTION":
          return Description;
        case "GAME":
          return Game;
        case "GAME_UPPER":
          return Game.ToUpper();
        case "GAME_LOWER":
          return Game.ToLower();
        case "GAME_CAPITALIZED":
          return GameCapitalized;
        case "PROJECT_UUID":
          return ProjectUuid;
        default:
          return match.Value;
        }
      });
}
