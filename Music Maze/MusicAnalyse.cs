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
            var decoded = new WaveDecoder(wavFile).Decode();

            //signal = new WaveRectifier(true).Apply(new HighPassFilter(0.5f).Apply(decoded));
            //signal = new HighPassFilter(1f).Apply(decoded);
            signal = new WaveRectifier(false).Apply(decoded);

            audioStopwatch = new Stopwatch();
            audioDuration = signal.Duration;

            int radius = 1000;
            int radiusMod = 10;

            int split = 10;

            float max = 0f;

            magnitudes = new float[signal.Length/split];
            magnitudes = new float[0];

            for (int i = 0; i < magnitudes.Length; i++ )
            {
                if(i % 10000 == 0)
                {
                    int barLength = 20;
                    int bar = (int)Math.Round(((i)/(float)magnitudes.Length*barLength ));
                    string str = String.Concat(Enumerable.Repeat("█", bar)) + String.Concat(Enumerable.Repeat(" ", barLength - bar));

                    Console.SetCursorPosition(0, 0);
                    Console.Write("Loading..<{0}>", str);
                }

                float total = 0;
                for (int k = -radius; k <= radius; k += radiusMod)
                {
                    int index = i * split + k;

                    if (0 < index && index < signal.Length)
                        total += signal.GetSample(0, index) * (float)Math.Cos(k / (radius * 4) * Math.PI);
                    else
                        total += 0.5f;
                }

                magnitudes[i] = Math.Abs(total)/radius*radiusMod;

                if (magnitudes[i] > max) { max = magnitudes[i]; }
            }

            wavFile.Close();
        }

        public void Play()
        {
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            audioStream = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_DEFAULT);

            Bass.BASS_ChannelPlay(audioStream, true);
            audioStopwatch.Start();

            Console.Clear();
        }

        public float CurrentMagnitude()
        {
            int radius = 3000;

            var pos = (double)Bass.BASS_ChannelGetPosition(audioStream);
            var length = (double)Bass.BASS_ChannelGetLength(audioStream);

            var a = Math.Round(pos / length);

            int i = (int)Math.Round(pos / length * signal.Length);

            float total = 0;

            for (int k = -radius; k <= radius; k++)
            {
                int index = i + k;

                if (0 < index && index < signal.Length)
                    total += signal.GetSample(0, index) * (float)Math.Cos(k / (radius * 4) * Math.PI);
                else
                    total += 0.5f;
            }

            return Math.Abs(total) / radius;

            //var val = (float)Math.Pow(magnitudes[i], 2);
            //return i < magnitudes.Length ? magnitudes[i] : 0f;
        }
    }
}
