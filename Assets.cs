namespace GrayHare.Asteroids;

internal static class Assets
{
    public static AssetsManifest EnsureCreated(string contentRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRoot);

        string soundsDir = Path.Combine(contentRoot, "sounds");
        Directory.CreateDirectory(soundsDir);

        string shootPath = Path.Combine(soundsDir, "shoot.wav");
        string explosionSoundPath = Path.Combine(soundsDir, "explosion.wav");
        string sirenPath = Path.Combine(soundsDir, "siren.wav");

        if (!File.Exists(shootPath))
        {
            File.WriteAllBytes(shootPath, CreateBeepWav(44100, 880f, 0.06f, 0.4f));
        }

        if (!File.Exists(explosionSoundPath))
        {
            byte[] explosionData = GenerateExplosionSound(durationSeconds: 0.4);
            byte[] explosionWav = CreateWav(explosionData);
            File.WriteAllBytes(explosionSoundPath, explosionWav);
        }

        if (!File.Exists(sirenPath))
        {
            byte[] sirenData = GenerateSirenSound(durationSeconds: 5.0);
            byte[] sirenWav = CreateWav(sirenData);
            File.WriteAllBytes(sirenPath, sirenWav);
        }

        return new AssetsManifest(
            shootSoundPath: "sounds/shoot.wav",
            explosionSoundPath: "sounds/explosion.wav",
            sirenSoundPath: "sounds/siren.wav"
        );
    }

    private static byte[] CreateBeepWav(int sampleRate, float frequency, float durationSeconds, float amplitude = 0.35f)
    {
        int sampleCount = (int)(sampleRate * durationSeconds);
        short[] samples = new short[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            double sample = Math.Sin(2 * Math.PI * frequency * i / sampleRate);
            double envelope = 1.0 - ((double)i / sampleCount);
            samples[i] = (short)(sample * envelope * short.MaxValue * amplitude);
        }

        byte[] audioData = new byte[sampleCount * 2];
        Buffer.BlockCopy(samples, 0, audioData, 0, audioData.Length);

        return CreateWav(audioData, sampleRate);
    }

    private static byte[] GenerateSirenSound(double durationSeconds = 5.0, int sampleRate = 44100)
    {
        int totalSamples = (int)(sampleRate * durationSeconds);
        short[] audioBuffer = new short[totalSamples];

        // --- Siren Characteristics ---
        double baseFrequency = 700.0;  // The center pitch of the siren (Hz)
        double frequencySweep = 300.0; // How far up and down the pitch bends (Hz)
        double sweepSpeed = .3;       // How fast the siren wails (1.5 cycles per second)

        // We track the continuous phase of the audio wave to prevent distortion
        double phase = 0.0;

        for (int i = 0; i < totalSamples; i++)
        {
            double time = (double)i / sampleRate;

            // 1. Calculate the current frequency using the LFO
            // Math.Sin here creates the smooth rising and falling "wail"
            double currentFrequency = baseFrequency + frequencySweep * Math.Sin(2.0 * Math.PI * sweepSpeed * time);

            // 2. Accumulate the phase 
            // (This tells the wave exactly where it should be in its current cycle)
            phase += (2.0 * Math.PI * currentFrequency) / sampleRate;
            if (phase > 2.0 * Math.PI)
            {
                phase -= 2.0 * Math.PI; // Keep the phase mathematically within one cycle
            }

            // 3. Generate the actual audio waveform (A sine wave is standard for alarms)
            // If you want a harsher, more aggressive siren, you could use a Square wave here instead.
            double sample = Math.Sin(phase);

            // 4. Scale the float (-1.0 to 1.0) to a 16-bit integer, playing at 80% max volume
            double finalSample = sample * (short.MaxValue * 0.8);

            audioBuffer[i] = (short)finalSample;
        }

        // Convert the 16-bit short array into an 8-bit byte array
        byte[] byteBuffer = new byte[totalSamples * 2];
        Buffer.BlockCopy(audioBuffer, 0, byteBuffer, 0, byteBuffer.Length);

        return byteBuffer;
    }

    private static byte[] GenerateExplosionSound(double durationSeconds = 1.5, int sampleRate = 44100)
    {
        Random random = Random.Shared;

        int totalSamples = (int)(sampleRate * durationSeconds);
        short[] audioBuffer = new short[totalSamples];

        double previousSample = 0;
        // The alpha controls the low-pass filter. 
        // A lower value (e.g., 0.05) cuts off more high frequencies, resulting in a deeper boom.
        double lowPassAlpha = 0.05;

        for (int i = 0; i < totalSamples; i++)
        {
            // 1. Generate basic white noise (values between -1.0 and 1.0)
            double noise = (random.NextDouble() * 2.0) - 1.0;

            // 2. Apply a Low-Pass Filter to muffle the harsh high frequencies
            double filteredSample = previousSample + lowPassAlpha * (noise - previousSample);
            previousSample = filteredSample;

            // 3. Apply an Exponential Decay Envelope
            // This makes the sound start loud and fade out rapidly.
            double time = (double)i / sampleRate;
            double envelope = Math.Exp(-5.0 * time); // 5.0 dictates the speed of the fade

            // 4. Combine and scale the float (-1.0 to 1.0) into the 16-bit integer range
            double finalSample = filteredSample * envelope;

            // Boost volume slightly and clamp to prevent audio clipping distortion
            finalSample *= 32000.0 * 2.5;
            if (finalSample > short.MaxValue)
            {
                finalSample = short.MaxValue;
            }
            if (finalSample < short.MinValue)
            {
                finalSample = short.MinValue;
            }

            audioBuffer[i] = (short)finalSample;
        }

        // Convert the 16-bit short array into an 8-bit byte array
        byte[] byteBuffer = new byte[totalSamples * 2];
        Buffer.BlockCopy(audioBuffer, 0, byteBuffer, 0, byteBuffer.Length);

        return byteBuffer;
    }

    private static byte[] CreateWav(byte[] audioData, int sampleRate = 44100, short channels = 1, short bitsPerSample = 16)
    {
        using MemoryStream ms = new MemoryStream();
        using BinaryWriter bw = new BinaryWriter(ms);

        int byteRate = sampleRate * channels * (bitsPerSample / 8);
        short blockAlign = (short)(channels * (bitsPerSample / 8));
        int dataSize = audioData.Length;

        // --- RIFF Header ---
        bw.Write("RIFF".ToCharArray());
        bw.Write(36 + dataSize);       // File size minus the first 8 bytes
        bw.Write("WAVE".ToCharArray());

        // --- Format Subchunk ---
        bw.Write("fmt ".ToCharArray());
        bw.Write(16);                  // Subchunk1Size (16 for PCM)
        bw.Write((short)1);            // AudioFormat (1 = Uncompressed PCM)
        bw.Write(channels);            // NumChannels
        bw.Write(sampleRate);          // SampleRate
        bw.Write(byteRate);            // ByteRate
        bw.Write(blockAlign);          // BlockAlign
        bw.Write(bitsPerSample);       // BitsPerSample

        // --- Data Subchunk ---
        bw.Write("data".ToCharArray());
        bw.Write(dataSize);            // Size of the actual audio data
        bw.Write(audioData);           // The raw audio bytes

        bw.Flush();
        return ms.ToArray();
    }
}
