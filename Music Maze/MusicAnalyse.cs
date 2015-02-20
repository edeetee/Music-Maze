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

using CSCore;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;


namespace Music_Maze
{
    class MusicAnalyse
    {
        string path;

        Signal signal;

        WaveFileReader audio;
        ISoundOut player;



        public MusicAnalyse(string path)
        {
            this.path = path;
            
            using(var wavFile = File.Open(path, FileMode.Open))
            {
                var decoded = new WaveDecoder(wavFile).Decode();

                signal = new WaveRectifier(true).Apply(new LowPassFilter(0.9f).Apply(decoded));
                //signal = new HighPassFilter(1f).Apply(decoded);
                //signal = new WaveRectifier(false).Apply(decoded);
            }

            //var wavFile = File.Open(path, FileMode.Open)

            audio = new WaveFileReader(path);

            player = new WasapiOut();
            player.Initialize(audio);
        }

        public void Play()
        {
            player.Play();
        }

        public int CurrentIndex()
        {
            var pos = (double)audio.GetPosition().TotalMilliseconds;
            var length = (double)audio.GetLength().TotalMilliseconds;
            var a = Math.Round(pos / length);

            return (int)Math.Round( (pos / length) * signal.Length);
        }

        public float CurrentMagnitude()
        {
            int radius = 3000;

            var i = CurrentIndex();

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
        }
    }
}
