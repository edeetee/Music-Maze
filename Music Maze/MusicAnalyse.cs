using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Accord.Audio;
using Accord.Audio.Formats;
using Accord.Audio.Filters;


using OpenTK.Audio.OpenAL;
using OpenTK.Audio;
using System.Diagnostics;

using Un4seen.Bass;


namespace Music_Maze
{
    class MusicAnalyse
    {
        string path;

        FileStream wavFile;
        Signal signal;
        float[] magnitudes;

        int audioStream;

        int audioDuration;
        Stopwatch audioStopwatch;

        public MusicAnalyse(string path)
        {
            this.path = path;
            wavFile = File.Open(path, FileMode.Open);

            //signal = new WaveRectifier(true).Apply(new HighPassFilter(1f).Apply( new WaveDecoder(wavFile).Decode() ));
            signal = new WaveRectifier(true).Apply(new WaveDecoder(wavFile).Decode());

            audioStopwatch = new Stopwatch();
            audioDuration = signal.Duration;

            int radius = 1000;
            int radiusMod = 10;

            int split = 10;

            magnitudes = new float[signal.Length/split];

            for (int i = 0; i < magnitudes.Length; i++ )
            {
                float total = 0;
                for (int k = -radius; k <= radius; k += radiusMod)
                {
                    int index = i * split + k;

                    if (0 < index && index < signal.Length)
                        total += signal.GetSample(0, index) * (float)Math.Cos(k / (radius * 2) * Math.PI);
                    else
                        total += 0.5f;
                }

                magnitudes[i] = Math.Abs(total)/radius*radiusMod*5;

                if (magnitudes[i] > 5f) { }
            }

            wavFile.Close();
        }

        public void Play()
        {
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            audioStream = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_DEFAULT);

            Bass.BASS_ChannelPlay(audioStream, true);
            audioStopwatch.Start();
        }

        public float CurrentMagnitude()
        {
            var pos = (double)Bass.BASS_ChannelGetPosition(audioStream);
            var length = (double)Bass.BASS_ChannelGetLength(audioStream);

            var a = Math.Round(pos / length);

            int i = (int)Math.Round(pos / length * magnitudes.Length);
            //var val = (float)Math.Pow(magnitudes[i], 2);
            return i < magnitudes.Length ? magnitudes[i] : 0f;
        }
    }
}
