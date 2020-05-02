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

		private static AudicaGameStateManager audicaGameState;
		private Encoder encoder;
		private HTTPServer httpServer;

		public static Patch playSong;
		public static Patch restartSong;
		public static Patch endSong;
		public static Patch targetHit;
		public static Patch targetMiss;
		public static Patch targetMissAim;
		public static Patch targetMissEarlyLate;
		public static Patch misfire;

		public override void OnApplicationStart() {
			Instance instance = Manager.CreateInstance("TimingAssist");
			AudicaHTTPStatus.playSong = instance.Patch(SDK.GetClass("LaunchPanel").GetMethod("Play"), typeof(AudicaHTTPStatus).GetMethod("PlaySong"));
			AudicaHTTPStatus.restartSong = instance.Patch(SDK.GetClass("InGameUI").GetMethod("Restart"), typeof(AudicaHTTPStatus).GetMethod("RestartSong"));
			AudicaHTTPStatus.endSong = instance.Patch(SDK.GetClass("InGameUI").GetMethod("ReturnToSongList"), typeof(AudicaHTTPStatus).GetMethod("EndSong"));

			AudicaHTTPStatus.targetHit = instance.Patch(SDK.GetClass("GameplayStats").GetMethod("ReportTargetHit"), typeof(AudicaHTTPStatus).GetMethod("TargetHit"));
			AudicaHTTPStatus.targetMiss = instance.Patch(SDK.GetClass("GameplayStats").GetMethod("ReportShotNothing"), typeof(AudicaHTTPStatus).GetMethod("TargetMiss"));
			AudicaHTTPStatus.targetMissAim = instance.Patch(SDK.GetClass("GameplayStats").GetMethod("ReportTargetHit"), typeof(AudicaHTTPStatus).GetMethod("TargetMissAim"));
			AudicaHTTPStatus.targetMissEarlyLate = instance.Patch(SDK.GetClass("GameplayStats").GetMethod("ReportTargetEarlyLate"), typeof(AudicaHTTPStatus).GetMethod("TargetMissEarlyLate"));
			AudicaHTTPStatus.misfire = instance.Patch(SDK.GetClass("GameplayStats").GetMethod("ReportMisfire"), typeof(AudicaHTTPStatus).GetMethod("Misfire"));

			AudicaHTTPStatus.audicaGameState = new AudicaGameStateManager();

			this.httpServer = new HTTPServer(() => {
				return this.encoder.Status(
					AudicaHTTPStatus.audicaGameState.GameState,
					AudicaHTTPStatus.audicaGameState.SongState
				);
			});
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


		/** TARGET EVENT HANDLING **/
		public unsafe static void TargetHit(IntPtr @this, IntPtr cue, float tick, Vector2 targetHitPos) {
			AudicaHTTPStatus.targetHit.InvokeOriginal(@this, new IntPtr[] {
				cue,
				new IntPtr((void*)(&tick)),
				new IntPtr((void*)(&targetHitPos))
			});

			AudicaTargetHitState targetHit = AudicaHTTPStatus.audicaGameState.TargetHit(targetHitPos);
			// TODO: feed output into JSON parser then to HTTP server as event
		}

		public unsafe static void TargetMiss(IntPtr @this) {
			AudicaHTTPStatus.targetMiss.InvokeOriginal(@this);

			AudicaTargetFailState targetMiss = AudicaHTTPStatus.audicaGameState.TargetMiss();
		}

		public unsafe static void TargetMissAim(IntPtr @this, IntPtr cue, Vector2 targetMissPos) {
			AudicaHTTPStatus.targetMissAim.InvokeOriginal(@this, new IntPtr[] {
				cue,
				new IntPtr((void*)(&targetMissPos))
			});

			AudicaTargetFailState targetMiss = AudicaHTTPStatus.audicaGameState.TargetMissAim();
		}

		public unsafe static void TargetMissEarlyLate(IntPtr @this, IntPtr cue, float tick) {
			AudicaHTTPStatus.targetMissEarlyLate.InvokeOriginal(@this, new IntPtr[] {
				cue,
				new IntPtr((void*)(&tick)),
			});

			AudicaTargetFailState targetMiss = AudicaHTTPStatus.audicaGameState.TargetMissEarlyLate(tick);
		}

		public unsafe static void Misfire(IntPtr @this) {
			AudicaHTTPStatus.misfire.InvokeOriginal(@this);

			// TODO (event with hand that misfired?)
		}
	}
}
