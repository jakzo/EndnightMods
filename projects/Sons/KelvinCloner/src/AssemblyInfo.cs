using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly:AssemblyTitle(Jakzo.EndnightMods.KelvinCloner.BuildInfo.NAME)]
[assembly:AssemblyDescription(
    Jakzo.EndnightMods.KelvinCloner.BuildInfo.DESCRIPTION)]
[assembly:AssemblyConfiguration("")]
[assembly:AssemblyCompany(Jakzo.EndnightMods.Metadata.COMPANY)]
[assembly:AssemblyProduct(Jakzo.EndnightMods.KelvinCloner.BuildInfo.NAME)]
[assembly:AssemblyCopyright("Created by " + Jakzo.EndnightMods.Metadata.AUTHOR)]
[assembly:AssemblyTrademark(Jakzo.EndnightMods.Metadata.COMPANY)]
[assembly:AssemblyCulture("")]
[assembly:ComVisible(false)]
//[assembly: Guid("")]
[assembly:AssemblyVersion(Jakzo.EndnightMods.KelvinCloner.AppVersion.Value)]
[assembly:AssemblyFileVersion(Jakzo.EndnightMods.KelvinCloner.AppVersion.Value)]
[assembly:NeutralResourcesLanguage("en")]

namespace Jakzo.EndnightMods.KelvinCloner {
public static class BuildInfo {
  public const string NAME = "KelvinCloner";
  public const string DESCRIPTION = "Allows having multiple helpers.";
}
}
