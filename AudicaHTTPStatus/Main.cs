using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Threading;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using NET_SDK;
using NET_SDK.Harmony;
using NET_SDK.Reflection;
using System.Runtime.InteropServices;
using UnityEngine.PostProcessing;

namespace AudicaHTTPStatus
{
	public static class BuildInfo {
		public const string Name = "AudicaHTTPStatus"; // Name of the Mod.  (MUST BE SET)
		public const string Author = "jukibom"; // Author of the Mod.  (Set as null if none)
		public const string Company = null; // Company that made the Mod.  (Set as null if none)
		public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
		public const string DownloadLink = "https://github.com/jukibom/audica-http-status"; // Download Link for the Mod.  (Set as null if none)
	}

	public class AudicaHTTPStatus : MelonMod {

		private static AudicaGameState audicaGameState;
		private HTTPServer httpServer;

		public static Patch playSong;
		public static Patch restartSong;
		public static Patch endSong;

		public override void OnApplicationStart() {
			Instance instance = Manager.CreateInstance("TimingAssist");
			AudicaHTTPStatus.playSong = instance.Patch(SDK.GetClass("LaunchPanel").GetMethod("Play"), typeof(AudicaHTTPStatus).GetMethod("PlaySong"));
			AudicaHTTPStatus.restartSong = instance.Patch(SDK.GetClass("InGameUI").GetMethod("Restart"), typeof(AudicaHTTPStatus).GetMethod("RestartSong"));
			AudicaHTTPStatus.endSong = instance.Patch(SDK.GetClass("InGameUI").GetMethod("ReturnToSongList"), typeof(AudicaHTTPStatus).GetMethod("EndSong"));

			AudicaHTTPStatus.audicaGameState = new AudicaGameState();
			this.httpServer = new HTTPServer();
			this.httpServer.Initialise();
		}

		public override void OnApplicationQuit() {
			this.httpServer.Shutdown();
		}

		public override void OnUpdate() {
			AudicaHTTPStatus.audicaGameState.Update();
		}

		public static void PlaySong(IntPtr @this) {
			AudicaHTTPStatus.playSong.InvokeOriginal(@this);
			AudicaHTTPStatus.audicaGameState.SongStart();
		}

		public static void RestartSong(IntPtr @this) {
			AudicaHTTPStatus.restartSong.InvokeOriginal(@this);
			AudicaHTTPStatus.audicaGameState.SongRestart();
		}

		public static void EndSong(IntPtr @this) {
			AudicaHTTPStatus.endSong.InvokeOriginal(@this);
			AudicaHTTPStatus.audicaGameState.SongEnd();
		}
	}
}
