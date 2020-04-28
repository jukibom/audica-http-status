using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;

namespace AudicaHTTPStatus {

	class AudicaGameState {
		public static ScoreKeeper scoreKeeper;

		public AudicaGameState() {
			AudicaGameState.scoreKeeper = UnityEngine.Object.FindObjectOfType<ScoreKeeper>();
		}

		// Called every update tick
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
	}
}
