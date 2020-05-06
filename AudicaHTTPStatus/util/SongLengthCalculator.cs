using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudicaHTTPStatus {

    class SongLengthCalculator {
        private SongList.SongData song;

        public SongLengthCalculator(SongList.SongData songData) {
            this.song = songData;
        }

        public float GetSongPeriodMilliseconds(float startTick, float endTick) {

            float lengthMilliseconds = 0;

            // we want the total ticks for the chunks from each tempo change to the next.
            for (int i = 0; i < this.song.tempos.Length; i ++) {

                float startChunkTick = this.song.tempos[i].tick;
                float endChunkTick = endTick;

                // if it's NOT the last tempo change, grab the tick from the next one instead.
                if (i != this.song.tempos.Length - 1) {
                    endChunkTick = this.song.tempos[i + 1].tick;
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
                float chunkMilliseconds = GetTicksMillisconds(chunkTickLength, this.song.tempos[i].tempo);
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
