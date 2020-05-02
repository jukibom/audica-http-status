using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace AudicaHTTPStatus {

	struct AudicaGameState {
		public string leftColor;        // hex value
		public string rightColor;       // hex value
		public float targetSpeed;       // 1 = 100%
		public float meleeSpeed;        // 1 = 100%
		public float aimAssist;         // 1 = 100%
	}
	
	struct AudicaSongState {
		public string songName;
		public string songMapper;
		public string difficulty;       // "beginner" | "standard" | "advanced" | "expert"
		public string classification;   // "ost" | "dlc" | "custom"
		public string songLength;       // UTC
		public string timeElapsed;      // UTC
		public string timeRemaining;    // UTC
		public float progress;          // 0-1, 0 = start, 1 = end
		public float currentTick;         // Hmx.Audio.MidiPlayCursor.GetCurrentTick
		public float songSpeed;         // 1 = 100%
		public float health;
		public int score;
		public int scoreMultiplier;
		public int streak;
		public int highScore;
		public bool isNoFailMode;
		public bool isPracticeMode;
		public bool isFullComboSoFar;
		public List<string> modifiers;
	}

	struct AudicaTargetHitState {
		public int targetIndex;
		public string type;         // "melee" | "standard" | "sustain" | "vertical" | "horizontal" | "chain-start" | "chain" | "bomb"
		public string hand;         // "left" | "right" | "either" | "none" (e.g. for bombs)
		public float score;
		public float timingScore;
		public float aimScore;
		public float tick;
		public Vector2 targetHitPosition;
	}

	struct AudicaTargetFailState {
		public int targetIndex;
		public string type;         // "melee" | "standard" | "sustain" | "vertical" | "horizontal" | "chain-start" | "chain" | "bomb"
		public string hand;         // "left" | "right" | "either" | "none" (e.g. for bombs)
		public string reason;       // "miss" | "aim" | "early" | "late"
	}

	class AudicaGameStateManager {

		// Audica game classes
		public static ScoreKeeper scoreKeeper;
		public static TargetTracker targetTracker;
		public static GameplayStats gameplayStats;
        public static PlayerPreferences prefs;

		// State containers
		private AudicaGameState gameState;
		private AudicaSongState songState;

        private bool songPlaying = false;

        public AudicaGameStateManager() {
			AudicaGameStateManager.scoreKeeper = UnityEngine.Object.FindObjectOfType<ScoreKeeper>();
			AudicaGameStateManager.targetTracker = UnityEngine.Object.FindObjectOfType<TargetTracker>();
			AudicaGameStateManager.gameplayStats = UnityEngine.Object.FindObjectOfType<GameplayStats>();
            AudicaGameStateManager.prefs = UnityEngine.Object.FindObjectOfType<PlayerPreferences>();

            this.clearGameState();
            this.clearSongState();
		}

		public AudicaGameState GameState {
			get {
				return this.gameState;
			}
		}

		public AudicaSongState SongState {
			get {
				return this.songState;
			}
		}

        // Called every tick, don't do anything too heavy in here!
		public void Update() {
            this.songState.currentTick = this.songPlaying ? AudicaGameStateManager.scoreKeeper.mLastTick : 0;

            // prefs (here's hoping color utility isn't dog slow!)
            this.gameState.leftColor = ColorUtility.ToHtmlStringRGB(AudicaGameStateManager.prefs.GunColorLeft.mVal);
            this.gameState.rightColor = ColorUtility.ToHtmlStringRGB(AudicaGameStateManager.prefs.GunColorRight.mVal);
            this.gameState.targetSpeed = AudicaGameStateManager.prefs.TargetSpeedMultiplier.mVal;
            this.gameState.meleeSpeed = AudicaGameStateManager.prefs.MeleeSpeedMultiplier.mVal;
            this.gameState.aimAssist = AudicaGameStateManager.prefs.AimAssistAmount.mVal;
		}

		public void SongStart() {
			MelonModLogger.Log("Song started");
            this.songPlaying = true;

        }

		public void SongRestart() {
			MelonModLogger.Log("Song restarted");
		}

		public void SongEnd() {
			MelonModLogger.Log("Song ended");
            this.songPlaying = false;
        }

		public AudicaTargetHitState TargetHit(Vector2 targetHitPos) {
			AudicaTargetHitState targetHit = new AudicaTargetHitState();
			SongCues.Cue cue = AudicaGameStateManager.targetTracker.mLastEitherHandTarget.target.GetCue();

			targetHit.targetIndex = cue.index;
			targetHit.type = this.cueToTargetType(cue);
			targetHit.hand = this.cueToHand(cue);
			targetHit.timingScore = AudicaGameStateManager.gameplayStats.GetLastTimingScore();
			targetHit.aimScore = AudicaGameStateManager.gameplayStats.GetLastAimScore();
			targetHit.score = targetHit.timingScore + targetHit.aimScore;		// TODO: may need to multiply by combo? Need to test
			targetHit.tick = cue.tick;
			targetHit.targetHitPosition = targetHitPos;

			return targetHit;
		}



		public AudicaTargetFailState TargetMiss() {
			AudicaTargetFailState targetMiss = new AudicaTargetFailState();
			SongCues.Cue cue = AudicaGameStateManager.targetTracker.mLastEitherHandTarget.target.GetCue();

			targetMiss.targetIndex = cue.index;
			targetMiss.type = this.cueToTargetType(cue);
			targetMiss.hand = this.cueToHand(cue);
			targetMiss.reason = "miss";
			return targetMiss;
		}

		public AudicaTargetFailState TargetMissAim() {
			AudicaTargetFailState targetMiss = new AudicaTargetFailState();
			SongCues.Cue cue = AudicaGameStateManager.targetTracker.mLastEitherHandTarget.target.GetCue();

			targetMiss.targetIndex = cue.index;
			targetMiss.type = this.cueToTargetType(cue);
			targetMiss.hand = this.cueToHand(cue);
			targetMiss.reason = "aim";
			return targetMiss;
		}

		public AudicaTargetFailState TargetMissEarlyLate(float tick) {
			AudicaTargetFailState targetMiss = new AudicaTargetFailState();
			SongCues.Cue cue = AudicaGameStateManager.targetTracker.mLastEitherHandTarget.target.GetCue();

			targetMiss.targetIndex = cue.index;
			targetMiss.type = this.cueToTargetType(cue);
			targetMiss.hand = this.cueToHand(cue);
			targetMiss.reason = tick < cue.tick ? "early" : "late";
		
			return targetMiss;
		}

		private string cueToTargetType(SongCues.Cue cue) {
			string type = "";
			switch (cue.behavior) {
				case Target.TargetBehavior.Melee: type = "melee"; break;
				case Target.TargetBehavior.Standard: type = "standard"; break;
				case Target.TargetBehavior.Hold: type = "sustain"; break;
				case Target.TargetBehavior.Vertical: type = "vertical"; break;
				case Target.TargetBehavior.Horizontal: type = "horizontal"; break;
				case Target.TargetBehavior.ChainStart: type = "chain-start"; break;
				case Target.TargetBehavior.Chain: type = "chain"; break;
				case Target.TargetBehavior.Dodge: type = "bomb"; break;
			}
			return type;
		}

		private string cueToHand(SongCues.Cue cue) {
			string hand = "";
			switch(cue.handType) {
				case Target.TargetHandType.Left: hand = "left"; break;
				case Target.TargetHandType.Right: hand = "right"; break;
				case Target.TargetHandType.Either: hand = "either"; break;
				case Target.TargetHandType.None: hand = "none"; break;
			}
			return hand;
		}

		private void clearGameState() {
			this.gameState = new AudicaGameState();
        }

        private void clearSongState() {
            this.songState = new AudicaSongState();
        }
    }
}
