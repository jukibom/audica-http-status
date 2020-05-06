using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudicaHTTPStatus {

    /**
     * Calculates arbitrary time periods within a song. THIS CLASS SHOULD BE RECONSTRUCTED ON SONG START!
     * Note that on certain maps (you know who you are) calculating the lengths can be extremely expensive. 
     * Use with caution and cache where appropriate.
     */
    class SongLengthCalculator {

        public static SongDataHolder songDataHolder;
        public static SongCues songCues;

        public SongLengthCalculator() {
            SongLengthCalculator.songDataHolder = UnityEngine.Object.FindObjectOfType<SongDataHolder>();
            SongLengthCalculator.songCues = UnityEngine.Object.FindObjectOfType<SongCues>();
        }

        public float GetSongPeriodMilliseconds(float startTick, float endTick) {
            SongList.SongData song = SongDataHolder.I.songData;
            float lengthMilliseconds = 0;

            if (song == null) return lengthMilliseconds;

            // we want the total ticks for the chunks from each tempo change to the next.
            for (int i = 0; i < song.tempos.Length; i ++) {

                float startChunkTick = song.tempos[i].tick;
                float endChunkTick = endTick;

                // if it's NOT the last tempo change, grab the tick from the next one instead.
                if (i != song.tempos.Length - 1) {
                    endChunkTick = song.tempos[i + 1].tick;
                }

                // cull obvious outliers (chunks entirely outside our range)
                if (endChunkTick < startTick) continue;
                if (startChunkTick > endTick) continue;

                // reduce ticks if current chunk includes the start or end ticks provided
                if (startChunkTick < startTick && endChunkTick > startTick) { startChunkTick = startTick; }
                if (startChunkTick < endTick && endChunkTick > endTick)     { endChunkTick = endTick; }

                // complete chunk length in ticks
                float chunkTickLength = endChunkTick - startChunkTick;

                // accumulator
                float chunkMilliseconds = GetTicksMillisconds(chunkTickLength, song.tempos[i].tempo);
                lengthMilliseconds += chunkMilliseconds;
            }

            return lengthMilliseconds;
        }

        private float GetTicksMillisconds(float ticks, float tempo) {
            return ticks * this.GetTickMilliseconds(tempo);
        }

        private float GetTickMilliseconds(float tempo) {
            return 60000 / (tempo * 480);
        }
    }
}
