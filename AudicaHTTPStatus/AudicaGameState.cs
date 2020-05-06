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
        public string songId;
		public string songName;
		public string songArtist;
		public string songAuthor;
		public string difficulty;       // "beginner" | "standard" | "advanced" | "expert"
		public string classification;   // "ost" | "dlc" | "extra" | "custom"
		public string songLength;       // UTC
		public string timeElapsed;      // UTC
		public string timeRemaining;    // UTC
		public float progress;          // 0-1, 0 = start, 1 = end
		public float currentTick;       // Hmx.Audio.MidiPlayCursor.GetCurrentTick
        public float ticksTotal;
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
		public static GameplayModifiers modifiers;
        public static PlayerPreferences prefs;
        public static KataConfig config;
        public static SongCues songCues;

		// State containers
		private AudicaGameState gameState;
        private AudicaSongState songState;

        // Util
        private SongLengthCalculator songCalculator;

        private bool songPlaying = false;
        public SongList.SongData songData { get; set; } 

        public AudicaGameStateManager() {
            this.initialiseStateManagers();
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
            this.pollGameState();
            this.pollSongState();
		}

		public void SongStart(SongList.SongData song) {
			MelonModLogger.Log("Song started");

            this.initialiseStateManagers();
            this.songData = song;
            this.songCalculator = new SongLengthCalculator(song);
            this.songPlaying = true;
        }

        public void SongRestart() {
			MelonModLogger.Log("Song restarted");
            this.initialiseStateManagers();
            this.clearSongState();
		}

		public void SongEnd() {
			MelonModLogger.Log("Song ended");
            this.songPlaying = false;
            this.clearSongState();
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

        private void initialiseStateManagers() {
            AudicaGameStateManager.scoreKeeper = UnityEngine.Object.FindObjectOfType<ScoreKeeper>();
            AudicaGameStateManager.targetTracker = UnityEngine.Object.FindObjectOfType<TargetTracker>();
            AudicaGameStateManager.gameplayStats = UnityEngine.Object.FindObjectOfType<GameplayStats>();
            AudicaGameStateManager.modifiers = UnityEngine.Object.FindObjectOfType<GameplayModifiers>();
            AudicaGameStateManager.prefs = UnityEngine.Object.FindObjectOfType<PlayerPreferences>();
            AudicaGameStateManager.config = UnityEngine.Object.FindObjectOfType<KataConfig>();
            AudicaGameStateManager.songCues = UnityEngine.Object.FindObjectOfType<SongCues>();
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

            // this is potentially read all the time but only updates if a song is playing so we should initialise everything to actual sane values ...
            // sigh.

            MelonLoader.MelonModLogger.Log("Clearing song state...");

            this.songState.songId = "";
            this.songState.songName = "";
            this.songState.songArtist = "";
            this.songState.songAuthor = "";
            this.songState.difficulty = "";
            this.songState.classification = "";
            this.songState.songLength = TimeSpan.FromSeconds(0).ToString();
            this.songState.timeElapsed = TimeSpan.FromSeconds(0).ToString();
            this.songState.timeRemaining = TimeSpan.FromSeconds(0).ToString();
            this.songState.progress = 0;
            this.songState.currentTick = 0;
            this.songState.ticksTotal = 0;
            this.songState.songSpeed = 1;
            this.songState.health = 1;
            this.songState.score = 0;
            this.songState.scoreMultiplier = 1;
            this.songState.streak = 0;
            this.songState.highScore = 0;
            this.songState.isNoFailMode = AudicaGameStateManager.prefs ? AudicaGameStateManager.prefs.NoFail.mVal : false;
            this.songState.isPracticeMode = AudicaGameStateManager.config ? AudicaGameStateManager.config.practiceMode : false;
            this.songState.isFullComboSoFar = true;
            this.songState.modifiers = AudicaGameStateManager.modifiers ? AudicaGameStateManager.modifiers.GetCurrentModifiers()
                .Select((GameplayModifiers.Modifier mod) => GameplayModifiers.GetModifierString(mod))
                .ToList<string>()
                : new List<string>();
        }

        private void pollGameState() {
            // prefs (here's hoping color utility isn't dog slow!)
            this.gameState.leftColor = AudicaGameStateManager.prefs ? ColorUtility.ToHtmlStringRGB(AudicaGameStateManager.prefs.GunColorLeft.mVal) : "#000000";
            this.gameState.rightColor = AudicaGameStateManager.prefs ? ColorUtility.ToHtmlStringRGB(AudicaGameStateManager.prefs.GunColorRight.mVal) : "#000000";
            this.gameState.targetSpeed = AudicaGameStateManager.prefs ? AudicaGameStateManager.prefs.TargetSpeedMultiplier.mVal : 1;
            this.gameState.meleeSpeed = AudicaGameStateManager.prefs ? AudicaGameStateManager.prefs.MeleeSpeedMultiplier.mVal : 1;
            this.gameState.aimAssist = AudicaGameStateManager.prefs ? AudicaGameStateManager.prefs.AimAssistAmount.mVal : 1;
        }

        // TODO: need to make all these classes load on GoToTitle() ?

        private void pollSongState() {
            if (this.songPlaying) {

                string songClass = "custom";
                if (this.songData.IsCoreSong()) {
                    songClass = "ost";
                }
                if (this.songData.dlc) {
                    songClass = "dlc";
                }
                if (this.songData.extrasSong) {
                    songClass = "extras";
                }


                // We don't want to calculate the ticks to the end of the song, it keeps playing!
                // Instead get the last target (plus its length) as the end ticks
                UnhollowerBaseLib.Il2CppReferenceArray<SongCues.Cue> cues = AudicaGameStateManager.songCues.mCues.cues;
                SongCues.Cue endCue = cues[cues.Length - 1];
                float songEndTicks = endCue.tick + endCue.tickLength;

                float currentTicks = AudicaGameStateManager.scoreKeeper.mLastTick;
                float totalTicks = songEndTicks;

                // TODO: lol not this
                long totalTimeMs = Convert.ToInt64(this.songCalculator.GetSongPeriodMilliseconds(0, totalTicks));
                long currentTimeMs = Convert.ToInt64(this.songCalculator.GetSongPeriodMilliseconds(0, currentTicks));
                long remainingTimeMs = Convert.ToInt64(this.songCalculator.GetSongPeriodMilliseconds(currentTicks, totalTicks));

                this.songState.songId = this.songData.songID;
                this.songState.songName = this.songData.title;
                this.songState.songArtist = this.songData.artist;
                this.songState.songAuthor = this.songData.author;
                //this.songState.songBpm = this.songData.tempos;        // uhh oh this is an array of all bpm changes. eep
                this.songState.difficulty = KataConfig.GetDifficultyName(AudicaGameStateManager.config.GetDifficulty());
                this.songState.classification = songClass;
                this.songState.songLength = TimeSpan.FromMilliseconds(totalTimeMs).ToString();
                this.songState.timeElapsed = TimeSpan.FromMilliseconds(currentTimeMs).ToString();
                this.songState.timeRemaining = TimeSpan.FromMilliseconds((totalTimeMs - currentTimeMs)).ToString();
                this.songState.progress = currentTicks / totalTicks;
                this.songState.currentTick = currentTicks;
                this.songState.ticksTotal = totalTicks;
                this.songState.songSpeed = KataConfig.GetCueDartSpeedMultiplier();      // TODO: not a clue what this value actually is but it's not the speed multiplier!
                this.songState.health = AudicaGameStateManager.scoreKeeper.GetHealth();
                this.songState.score = AudicaGameStateManager.scoreKeeper.mScore;
                this.songState.scoreMultiplier = AudicaGameStateManager.scoreKeeper.GetRawMultiplier();
                this.songState.streak = AudicaGameStateManager.scoreKeeper.GetStreak();
                this.songState.highScore = AudicaGameStateManager.scoreKeeper.GetHighScore();
                this.songState.isNoFailMode = AudicaGameStateManager.prefs.NoFail.mVal;
                this.songState.isPracticeMode = AudicaGameStateManager.config.practiceMode;
                this.songState.isFullComboSoFar = AudicaGameStateManager.scoreKeeper.GetIsFullComboSoFar();
                this.songState.modifiers = AudicaGameStateManager.modifiers.GetCurrentModifiers()
                    .Select((GameplayModifiers.Modifier mod) => GameplayModifiers.GetModifierString(mod))
                    .ToList<string>();
            }
        }
    }
}
