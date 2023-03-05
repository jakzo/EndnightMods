using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly:AssemblyTitle(Jakzo.EndnightMods.$$NAME$$.BuildInfo.NAME)]
[assembly:AssemblyDescription(
    Jakzo.EndnightMods.$$NAME$$.BuildInfo.DESCRIPTION)]
[assembly:AssemblyConfiguration("")]
[assembly:AssemblyCompany(Jakzo.EndnightMods.Metadata.COMPANY)]
[assembly:AssemblyProduct(Jakzo.EndnightMods.$$NAME$$.BuildInfo.NAME)]
[assembly:AssemblyCopyright("Created by " + Jakzo.EndnightMods.Metadata.AUTHOR)]
[assembly:AssemblyTrademark(Jakzo.EndnightMods.Metadata.COMPANY)]
[assembly:AssemblyCulture("")]
[assembly:ComVisible(false)]
//[assembly: Guid("")]
[assembly:AssemblyVersion(Jakzo.EndnightMods.$$NAME$$.AppVersion.Value)]
[assembly:AssemblyFileVersion(Jakzo.EndnightMods.$$NAME$$.AppVersion.Value)]
[assembly:NeutralResourcesLanguage("en")]

namespace Jakzo.EndnightMods.$$NAME$$ {
public static class BuildInfo {
  public const string NAME = "$$NAME$$";
  public const string DESCRIPTION = "$$DESCRIPTION$$";
}
}
