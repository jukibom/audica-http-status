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
using System.Runtime.InteropServices;
using UnityEngine.PostProcessing;
using Harmony;
using UnhollowerBaseLib;
using System.Reflection;

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

        internal static AudicaGameStateManager AudicaGameState { get; set; }
        internal static AudicaTargetStateManager AudicaTargetState { get; set; }

        public static SongList.SongData selectedSongData;
		private Encoder encoder;
		private HTTPServer httpServer;

        public override void OnApplicationStart() {
            var instance = HarmonyInstance.Create("AudicaHTTPStatus");
            Hooks.ApplyHooks(instance);

			AudicaHTTPStatus.AudicaGameState = new AudicaGameStateManager();
			AudicaHTTPStatus.AudicaTargetState = new AudicaTargetStateManager();

            this.encoder = new Encoder();

			this.httpServer = new HTTPServer(() => {
				return this.encoder.Status(
					AudicaHTTPStatus.AudicaGameState.GameState,
					AudicaHTTPStatus.AudicaGameState.SongState
				);
			});
			this.httpServer.Initialise();
		}

		public override void OnApplicationQuit() {
			this.httpServer.Shutdown();
		}

		public override void OnUpdate() {
			AudicaHTTPStatus.AudicaGameState.Update();
		}
	}



    internal static class Hooks {
        public static void ApplyHooks(HarmonyInstance instance) {
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }


        // song selection + play state
        [HarmonyPatch(typeof(SongSelectItem), "OnSelect", new Type[0])]
        public static class SongSelectPatch {
            public static void Postfix(SongSelectItem __instance) {
                AudicaHTTPStatus.selectedSongData = __instance.mSongData;
            }
        }

        [HarmonyPatch(typeof(ScoreKeeper), "Start", new Type[0])]
        public static class StartSongPatch {
            public static void Postfix() {
                AudicaHTTPStatus.AudicaTargetState.SongStart();
                AudicaHTTPStatus.AudicaGameState.SongStart(AudicaHTTPStatus.selectedSongData);
            }
        }

        [HarmonyPatch(typeof(InGameUI), "Restart", new Type[0])]
        public static class RestartSongPatch {
            public static void Postfix() {
                AudicaHTTPStatus.AudicaGameState.SongRestart();
            }
        }

        [HarmonyPatch(typeof(InGameUI), "ReturnToSongList", new Type[0])]
        public static class EndSongPatch {
            public static void Postfix() {
                AudicaHTTPStatus.AudicaGameState.SongEnd();
            }
        }
        

        // target event handling
        [HarmonyPatch(typeof(GameplayStats), "ReportTargetHit", new Type[] { typeof(SongCues.Cue), typeof(float), typeof(Vector2) })]
        public static class TargetHitPatch {
            public static void Postfix(ref GameplayStats __instance, ref SongCues.Cue cue, ref Vector2 targetHitPos) {
                MelonLoader.MelonModLogger.Log("Target Hit! " + targetHitPos.ToString());
                AudicaTargetHitState targetHit = AudicaHTTPStatus.AudicaTargetState.TargetHit(__instance, cue, targetHitPos);
                // TODO: feed output into JSON parser then to HTTP server as websocket event
            }
        }

        [HarmonyPatch(typeof(GameplayStats), "ReportShotNothing", new Type[0])]
        public static class ShotNothingPatch {
            public static void Postfix() {
                MelonLoader.MelonModLogger.Log("Shot nothing!");
                AudicaTargetFailState targetMiss = AudicaHTTPStatus.AudicaTargetState.TargetMiss();
                // TODO: feed output into JSON parser then to HTTP server as websocket event
            }
        }

        [HarmonyPatch(typeof(GameplayStats), "ReportTargetAimMiss", new Type[] { typeof(SongCues.Cue), typeof(Vector2) })]
        public static class TargetAimMissPatch {
            public static void Postfix() {
                MelonLoader.MelonModLogger.Log("Target Miss (aim)!");
                AudicaTargetFailState targetMiss = AudicaHTTPStatus.AudicaTargetState.TargetMissAim();
                // TODO: feed output into JSON parser then to HTTP server as websocket event
            }
        }

        [HarmonyPatch(typeof(GameplayStats), "ReportTargetEarlyLate", new Type[] { typeof(SongCues.Cue), typeof(float) })]
        public static class TargetEarlyLatePatch {
            public static void Postfix() {
                MelonLoader.MelonModLogger.Log("Target Miss (timing)!");
                //AudicaTargetFailState targetMiss = AudicaHTTPStatus.TargetState.TargetMissEarlyLate(tick);
            }
        }

        [HarmonyPatch(typeof(GameplayStats), "ReportMisfire", new Type[0])]
        public static class GunMisfirePatch {
            public static void Postfix() {
                MelonLoader.MelonModLogger.Log("Misfire!");
                // TODO (event with hand that misfired?)
            }
        }
    }
}
