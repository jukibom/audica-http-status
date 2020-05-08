using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudicaHTTPStatus.util;
using UnityEngine;

namespace AudicaHTTPStatus {

	class AudicaGameStateManager {

		// Audica game classes
		public static ScoreKeeper scoreKeeper;
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
            this.initialiseStateManagers();
            this.songCalculator = new SongLengthCalculator();
            this.songData = song;
            this.songPlaying = true;
        }

        public void SongRestart() {
            this.initialiseStateManagers();
            this.clearSongState();
		}

		public void SongEnd() {
            this.songPlaying = false;
            this.clearSongState();
        }

        private void initialiseStateManagers() {
            AudicaGameStateManager.scoreKeeper = UnityEngine.Object.FindObjectOfType<ScoreKeeper>();
            AudicaGameStateManager.modifiers = UnityEngine.Object.FindObjectOfType<GameplayModifiers>();
            AudicaGameStateManager.prefs = UnityEngine.Object.FindObjectOfType<PlayerPreferences>();
            AudicaGameStateManager.config = UnityEngine.Object.FindObjectOfType<KataConfig>();
            AudicaGameStateManager.songCues = UnityEngine.Object.FindObjectOfType<SongCues>();
        }

		private void clearGameState() {
			this.gameState = new AudicaGameState();
        }

        private void clearSongState() {
            this.songState = new AudicaSongState();

            // this is potentially read all the time but only updates if a song is playing so we should initialise everything to actual sane values ...
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

                float currentTick = AudicaGameStateManager.scoreKeeper.mLastTick;

                float totalTimeMs = this.songCalculator.SongLengthMilliseconds;
                float currentTimeMs = this.songCalculator.GetSongPositionMilliseconds(currentTick);
                float remainingTimeMs = totalTimeMs - currentTimeMs;
                
                this.songState.songId = this.songData.songID;
                this.songState.songName = this.songData.title;
                this.songState.songArtist = this.songData.artist;
                this.songState.songAuthor = this.songData.author;
                this.songState.difficulty = KataConfig.GetDifficultyName(AudicaGameStateManager.config.GetDifficulty());
                this.songState.classification = songClass;
                this.songState.songLength = TimeSpan.FromMilliseconds(Convert.ToInt64(totalTimeMs)).ToString();
                this.songState.timeElapsed = TimeSpan.FromMilliseconds(Convert.ToInt64(currentTimeMs)).ToString();
                this.songState.timeRemaining = TimeSpan.FromMilliseconds(Convert.ToInt64(remainingTimeMs)).ToString();
                this.songState.progress = currentTimeMs / totalTimeMs;
                this.songState.currentTick = currentTick;
                this.songState.ticksTotal = songEndTicks;
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
