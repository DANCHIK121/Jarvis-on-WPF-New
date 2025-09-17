namespace Jarvis_on_WPF_New.VoskModel
{
    partial class VoskModelClass
    {
        private static byte[] EnhanceAudioQuality(byte[] buffer, int length)
        {
            if (length == 0) return buffer;

            short[] samples = new short[length / 2];
            Buffer.BlockCopy(buffer, 0, samples, 0, length);

            // Шумоподавление
            for (int i = 0; i < samples.Length; i++)
            {
                if (Math.Abs(samples[i]) < 300) // Более агрессивное шумоподавление
                    samples[i] = 0;
            }

            NormalizeAudio(samples);

            byte[] processed = new byte[length];
            Buffer.BlockCopy(samples, 0, processed, 0, length);
            return processed;
        }

        private static void NormalizeAudio(short[] samples)
        {
            short maxAmplitude = 0;
            foreach (var sample in samples)
            {
                if (Math.Abs(sample) > maxAmplitude)
                    maxAmplitude = Math.Abs(sample);
            }

            if (maxAmplitude > 1000 && maxAmplitude < 10000)
            {
                float gain = 8000f / maxAmplitude;
                for (int i = 0; i < samples.Length; i++)
                {
                    int amplified = (int)(samples[i] * gain);
                    samples[i] = (short)Math.Clamp(amplified, short.MinValue, short.MaxValue);
                }
            }
        }

        private static bool IsAudioLoudEnough(byte[] audio)
        {
            if (audio.Length == 0) return false;

            short[] samples = new short[audio.Length / 2];
            Buffer.BlockCopy(audio, 0, samples, 0, audio.Length);

            double sum = 0;
            int count = 0;
            foreach (var sample in samples)
            {
                if (Math.Abs(sample) > 100) // Игнорируем совсем тихие samples
                {
                    sum += sample * sample;
                    count++;
                }
            }

            if (count == 0) return false;

            double rms = Math.Sqrt(sum / count);
            return rms > 200; // Пониженный порог для лучшей чувствительности
        }
    }
}
