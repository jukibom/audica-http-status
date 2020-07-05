using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public UnityEngine.Vector2 targetHitPosition;
    }

    struct AudicaTargetFailState {
        public int targetIndex;
        public string type;         // "melee" | "standard" | "sustain" | "vertical" | "horizontal" | "chain-start" | "chain" | "bomb"
        public string hand;         // "left" | "right" | "either" | "none" (e.g. for bombs)
        public string reason;       // "miss" | "aim" | "early" | "late"
    }
}