using System.Resources;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;

[assembly: AssemblyTitle(AudicaHTTPStatus.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(AudicaHTTPStatus.BuildInfo.Company)]
[assembly: AssemblyProduct(AudicaHTTPStatus.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + AudicaHTTPStatus.BuildInfo.Author)]
[assembly: AssemblyTrademark(AudicaHTTPStatus.BuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(AudicaHTTPStatus.BuildInfo.Version)]
[assembly: AssemblyFileVersion(AudicaHTTPStatus.BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonModInfo(typeof(AudicaHTTPStatus.AudicaHTTPStatus), AudicaHTTPStatus.BuildInfo.Name, AudicaHTTPStatus.BuildInfo.Version, AudicaHTTPStatus.BuildInfo.Author, AudicaHTTPStatus.BuildInfo.DownloadLink)]


// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonModGame(null, null)]