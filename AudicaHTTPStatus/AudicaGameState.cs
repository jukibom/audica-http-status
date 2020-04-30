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
		public float aimAssist;			// 1 = 100%
	}
	
	struct AudicaSongState {
		public string songName;
		public string songMapper;
		public string difficulty;		// "beginner" | "standard" | "advanced" | "expert"
		public string classification;	// "ost" | "dlc" | "custom"
		public string songLength;		// UTC
		public string timeElapsed;		// UTC
		public string timeRemaining;	// UTC
		public float progress;          // 0-1, 0 = start, 1 = end
		public float songSpeed;			// 1 = 100%
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
		public int targetNumber;
		public string type;         // "melee" | "standard" | "sustain" | "vertical" | "horizontal" | "chain-start" | "chain" | "bomb"
		public int timingScore;
		public int aimScore;
		public int tick;        // TODO	ScoreKeepet.mLastTick		// too drunk for thi
	}

	struct AudicaTargetFailState {
		public int targetNumber;
		public string type;			// "melee" | "standard" | "sustain" | "vertical" | "horizontal" | "chain-start" | "chain" | "bomb"
		public string reason;		// "no-fire" | "miss" | "early" | "late"
	}

	class AudicaGameStateManager {

		// Audica game classes
		public static ScoreKeeper scoreKeeper;

		// State containers
		private AudicaGameState gameState;
		private AudicaSongState songState;

		public AudicaGameStateManager() {
			AudicaGameStateManager.scoreKeeper = UnityEngine.Object.FindObjectOfType<ScoreKeeper>();
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

		// Called every update tick, is this needed?
		public void Update() {

		}

		public void SongStart() {
			MelonModLogger.Log("Song started");
		}

		public void SongRestart() {
			MelonModLogger.Log("Song restarted");
		}

		public void SongEnd() {
			MelonModLogger.Log("Song ended");
		}

		public void TargetHit() {
			MelonModLogger.Log("Target Hit");
		}

		private void clearGameState() {
			this.gameState = new AudicaGameState();
		}
	}
}
