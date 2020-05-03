using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleJSON;

namespace AudicaHTTPStatus {
	class Encoder {

		public string Status(AudicaGameState gameState, AudicaSongState songState) {
			JSONObject gameStatus = new JSONObject();
			JSONObject gameStateJSON = new JSONObject();
			JSONObject songStateJSON = new JSONObject();

			JSONArray modifiers = new JSONArray();
			songState.modifiers.ForEach((string modifier) => {
				modifiers.Add(modifier);
			});

			gameStateJSON["leftColor"] = gameState.leftColor;
			gameStateJSON["rightColor"] = gameState.rightColor;
			gameStateJSON["targetSpeed"] = gameState.targetSpeed;
			gameStateJSON["meleeSpeed"] = gameState.meleeSpeed;
			gameStateJSON["aimAssist"] = gameState.aimAssist;
            // TODO: timing assist value?

            songStateJSON["songId"] = songState.songId;
			songStateJSON["songName"] = songState.songName;
			songStateJSON["songArtist"] = songState.songArtist;
			songStateJSON["songAuthor"] = songState.songAuthor;
			songStateJSON["difficulty"] = songState.difficulty;
			songStateJSON["classification"] = songState.classification;
			songStateJSON["songLength"] = songState.songLength;
			songStateJSON["timeElapsed"] = songState.timeElapsed;
			songStateJSON["timeRemaining"] = songState.timeRemaining;
			songStateJSON["progress"] = songState.progress;
			songStateJSON["currentTick"] = songState.currentTick;
			songStateJSON["songSpeed"] = songState.songSpeed;
			songStateJSON["health"] = songState.health;
			songStateJSON["score"] = songState.score;
			songStateJSON["scoreMultiplier"] = songState.scoreMultiplier;
			songStateJSON["streak"] = songState.streak;
			songStateJSON["highScore"] = songState.highScore;
			songStateJSON["isNoFailMode"] = songState.isNoFailMode;
			songStateJSON["isPracticeMode"] = songState.isPracticeMode;
			songStateJSON["isFullComboSoFar"] = songState.isFullComboSoFar;
			songStateJSON["modifiers"] = modifiers;

			gameStatus["gameSettings"] = gameStateJSON;
			gameStatus["songStatus"] = songStateJSON;

			return gameStatus.ToString();
		}

		public string TargetHitEvent(AudicaTargetHitState targetHit) {

			JSONObject targetHitPosition = new JSONObject();
			targetHitPosition["x"] = targetHit.targetHitPosition.x;
			targetHitPosition["y"] = targetHit.targetHitPosition.y;

			JSONObject eventJSON = new JSONObject();
			eventJSON["event"] = "target-hit";
			eventJSON["targetIndex"] = targetHit.targetIndex;
			eventJSON["targetType"] = targetHit.type;
			eventJSON["hand"] = targetHit.hand;
			eventJSON["score"] = targetHit.score;
			eventJSON["timingScore"] = targetHit.timingScore;
			eventJSON["aimScore"] = targetHit.aimScore;
			eventJSON["tick"] = targetHit.tick;
			eventJSON["targetHitPosition"] = targetHitPosition;

			return eventJSON.ToString();
		}
	}
}
