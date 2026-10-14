using System;
using UnityEngine;
using VARG.Data;

namespace VARG.Player
{
    public static class ProceduralAudioUtility
    {
        private const int SampleRate = 22050;

        private static AudioClip _laserShootClip;
        private static AudioClip _boingShrinkClip;
        private static AudioClip _capturePopClip;
        private static AudioClip _captureSuccessClip;
        private static AudioClip _captureErrorClip;
        private static AudioClip _missionCompleteClip;

        private static AudioClip _humanScreamClip;
        private static AudioClip _cowMooClip;
        private static AudioClip _chickenCluckClip;
        private static AudioClip _pigSquealClip;

        public static AudioClip GetLaserShootClip()
        {
            if (_laserShootClip == null)
            {
                _laserShootClip = GenerateLaserShootClip();
            }
            return _laserShootClip;
        }

        public static AudioClip GetBoingShrinkClip()
        {
            if (_boingShrinkClip == null)
            {
                _boingShrinkClip = GenerateBoingShrinkClip();
            }
            return _boingShrinkClip;
        }

        public static AudioClip GetCapturePopClip()
        {
            if (_capturePopClip == null)
            {
                _capturePopClip = GenerateCapturePopClip();
            }
            return _capturePopClip;
        }

        public static AudioClip GetCaptureSuccessClip()
        {
            if (_captureSuccessClip == null)
            {
                _captureSuccessClip = GenerateCaptureSuccessClip();
            }
            return _captureSuccessClip;
        }

        public static AudioClip GetCaptureErrorClip()
        {
            if (_captureErrorClip == null)
            {
                _captureErrorClip = GenerateCaptureErrorClip();
            }
            return _captureErrorClip;
        }

        public static AudioClip GetMissionCompleteClip()
        {
            if (_missionCompleteClip == null)
            {
                _missionCompleteClip = GenerateMissionCompleteClip();
            }
            return _missionCompleteClip;
        }

        public static AudioClip GetFleeVoiceClip(TargetType type)
        {
            switch (type)
            {
                case TargetType.Human:
                    if (_humanScreamClip == null) _humanScreamClip = GenerateHumanScreamClip();
                    return _humanScreamClip;

                case TargetType.Cow:
                    if (_cowMooClip == null) _cowMooClip = GenerateCowMooClip();
                    return _cowMooClip;

                case TargetType.Chicken:
                    if (_chickenCluckClip == null) _chickenCluckClip = GenerateChickenCluckClip();
                    return _chickenCluckClip;

                case TargetType.Pig:
                    if (_pigSquealClip == null) _pigSquealClip = GeneratePigSquealClip();
                    return _pigSquealClip;

                default:
                    if (_humanScreamClip == null) _humanScreamClip = GenerateHumanScreamClip();
                    return _humanScreamClip;
            }
        }

        private static AudioClip GenerateLaserShootClip()
        {
            const float duration = 0.16f;
            var sampleCount = Mathf.RoundToInt(SampleRate * duration);
            var samples = new float[sampleCount];

            var phase = 0f;
            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / SampleRate;
                var progress = t / duration;

                var freq = Mathf.Lerp(1250f, 220f, Mathf.Pow(progress, 0.6f));
                phase += 2f * Mathf.PI * freq / SampleRate;

                var env = Mathf.Exp(-progress * 7f);
                var wave = Mathf.Sin(phase) + 0.25f * Mathf.Sin(phase * 2f);

                samples[i] = Mathf.Clamp(wave * env * 0.7f, -1f, 1f);
            }

            var clip = AudioClip.Create("Laser_Shoot", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateBoingShrinkClip()
        {
            const float duration = 0.42f;
            var sampleCount = Mathf.RoundToInt(SampleRate * duration);
            var samples = new float[sampleCount];

            var phase = 0f;
            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / SampleRate;
                var progress = t / duration;

                var baseFreq = Mathf.Lerp(480f, 130f, progress * progress);
                var vibratoFreq = Mathf.Lerp(22f, 11f, progress);
                var vibratoAmp = Mathf.Lerp(65f, 15f, progress);
                var currentFreq = baseFreq + Mathf.Sin(2f * Mathf.PI * vibratoFreq * t) * vibratoAmp;

                phase += 2f * Mathf.PI * currentFreq / SampleRate;

                var attack = Mathf.Clamp01(t / 0.03f);
                var decay = Mathf.Pow(1f - progress, 1.4f);
                var env = attack * decay;

                var wave = Mathf.Sin(phase) + 0.15f * Mathf.Sin(phase * 3f);
                samples[i] = Mathf.Clamp(wave * env * 0.75f, -1f, 1f);
            }

            var clip = AudioClip.Create("Boing_Shrink", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateCapturePopClip()
        {
            const float duration = 0.18f;
            var sampleCount = Mathf.RoundToInt(SampleRate * duration);
            var samples = new float[sampleCount];

            var phase = 0f;
            var chimePhase = 0f;

            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / SampleRate;

                // Snappy downward pop in the first 25ms
                var popWave = 0f;
                if (t < 0.04f)
                {
                    var popT = t / 0.04f;
                    var popFreq = Mathf.Lerp(900f, 90f, popT);
                    phase += 2f * Mathf.PI * popFreq / SampleRate;
                    var popEnv = 1f - popT;
                    popWave = Mathf.Sin(phase) * popEnv * 0.8f;
                }

                // Resonant bubble ring
                var chimeFreq = 587.33f; // D5
                chimePhase += 2f * Mathf.PI * chimeFreq / SampleRate;
                var chimeEnv = Mathf.Exp(-t * 18f);
                var chimeWave = Mathf.Sin(chimePhase) * chimeEnv * 0.6f;

                samples[i] = Mathf.Clamp(popWave + chimeWave, -1f, 1f);
            }

            var clip = AudioClip.Create("Capture_Pop", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateCaptureSuccessClip()
        {
            const float duration = 0.32f;
            var sampleCount = Mathf.RoundToInt(SampleRate * duration);
            var samples = new float[sampleCount];

            var phase = 0f;
            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / SampleRate;
                var isSecondNote = t >= 0.12f;

                var freq = isSecondNote ? 783.99f : 523.25f; // G5 : C5
                var noteTime = isSecondNote ? (t - 0.12f) : t;
                var noteDuration = isSecondNote ? 0.20f : 0.12f;

                phase += 2f * Mathf.PI * freq / SampleRate;

                var attack = Mathf.Clamp01(noteTime / 0.015f);
                var decay = Mathf.Exp(-noteTime * 12f);
                var env = attack * decay;

                var wave = Mathf.Sin(phase) + 0.2f * Mathf.Sin(phase * 2f);
                samples[i] = Mathf.Clamp(wave * env * 0.65f, -1f, 1f);
            }

            var clip = AudioClip.Create("Capture_Success", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateCaptureErrorClip()
        {
            const float duration = 0.35f;
            var sampleCount = Mathf.RoundToInt(SampleRate * duration);
            var samples = new float[sampleCount];

            var phase1 = 0f;
            var phase2 = 0f;

            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / SampleRate;
                var progress = t / duration;

                var freq1 = Mathf.Lerp(190f, 120f, progress);
                var freq2 = Mathf.Lerp(205f, 128f, progress);

                phase1 += 2f * Mathf.PI * freq1 / SampleRate;
                phase2 += 2f * Mathf.PI * freq2 / SampleRate;

                var attack = Mathf.Clamp01(t / 0.02f);
                var decay = 1f - Mathf.Pow(progress, 0.7f);
                var env = attack * decay;

                // Dissonant square-ish buzz
                var wave1 = Mathf.Sin(phase1) > 0f ? 0.6f : -0.6f;
                var wave2 = Mathf.Sin(phase2) > 0f ? 0.4f : -0.4f;

                samples[i] = Mathf.Clamp((wave1 + wave2) * env * 0.5f, -1f, 1f);
            }

            var clip = AudioClip.Create("Capture_Error", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateMissionCompleteClip()
        {
            const float duration = 0.65f;
            var sampleCount = Mathf.RoundToInt(SampleRate * duration);
            var samples = new float[sampleCount];

            var notes = new[] { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6
            const float step = 0.12f;

            var phase = 0f;
            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / SampleRate;
                var noteIndex = Mathf.Clamp(Mathf.FloorToInt(t / step), 0, notes.Length - 1);
                var noteTime = t - noteIndex * step;

                var freq = notes[noteIndex];
                phase += 2f * Mathf.PI * freq / SampleRate;

                var attack = Mathf.Clamp01(noteTime / 0.015f);
                var decay = Mathf.Exp(-noteTime * (noteIndex == notes.Length - 1 ? 7f : 14f));
                var env = attack * decay;

                var wave = Mathf.Sin(phase) + 0.25f * Mathf.Sin(phase * 2f);
                samples[i] = Mathf.Clamp(wave * env * 0.7f, -1f, 1f);
            }

            var clip = AudioClip.Create("Mission_Complete_Fanfare", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateHumanScreamClip()
        {
            const float duration = 0.65f;
            var sampleCount = Mathf.RoundToInt(SampleRate * duration);
            var samples = new float[sampleCount];

            var phase = 0f;
            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / SampleRate;
                var progress = t / duration;

                // Comedic high-to-low terrified vocal scream with panic vibrato
                var baseFreq = progress < 0.25f
                    ? Mathf.Lerp(520f, 680f, progress / 0.25f)
                    : Mathf.Lerp(680f, 290f, (progress - 0.25f) / 0.75f);

                var vibrato = Mathf.Sin(2f * Mathf.PI * 9f * t) * 35f;
                var freq = baseFreq + vibrato;

                phase += 2f * Mathf.PI * freq / SampleRate;

                var attack = Mathf.Clamp01(t / 0.04f);
                var decay = Mathf.Pow(1f - progress, 1.2f);
                var env = attack * decay;

                // Human formant harmonics
                var wave = Mathf.Sin(phase)
                         + 0.5f * Mathf.Sin(phase * 2f)
                         + 0.25f * Mathf.Sin(phase * 3f)
                         + 0.12f * Mathf.Sin(phase * 4f);

                samples[i] = Mathf.Clamp(wave * env * 0.6f, -1f, 1f);
            }

            var clip = AudioClip.Create("Voice_Human_Scream", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateCowMooClip()
        {
            const float duration = 0.85f;
            var sampleCount = Mathf.RoundToInt(SampleRate * duration);
            var samples = new float[sampleCount];

            var phase = 0f;
            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / SampleRate;
                var progress = t / duration;

                // Deep oscillating bovine moo: 120Hz swelling up to 148Hz then down to 98Hz
                var freq = progress < 0.35f
                    ? Mathf.Lerp(118f, 148f, progress / 0.35f)
                    : Mathf.Lerp(148f, 96f, (progress - 0.35f) / 0.65f);

                var mooWobble = Mathf.Sin(2f * Mathf.PI * 4.5f * t) * 8f;
                freq += mooWobble;

                phase += 2f * Mathf.PI * freq / SampleRate;

                var attack = Mathf.Clamp01(t / 0.12f);
                var decay = Mathf.Pow(1f - progress, 1.3f);
                var env = attack * decay;

                // Rich deep brassy harmonics
                var wave = Mathf.Sin(phase)
                         + 0.75f * Mathf.Sin(phase * 2f)
                         + 0.45f * Mathf.Sin(phase * 3f)
                         + 0.25f * Mathf.Sin(phase * 4f);

                samples[i] = Mathf.Clamp(wave * env * 0.65f, -1f, 1f);
            }

            var clip = AudioClip.Create("Voice_Cow_Moo", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateChickenCluckClip()
        {
            const float duration = 0.45f;
            var sampleCount = Mathf.RoundToInt(SampleRate * duration);
            var samples = new float[sampleCount];

            // 3 staccato cluck bursts
            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / SampleRate;
                const float cluckInterval = 0.14f;
                var cluckIndex = Mathf.FloorToInt(t / cluckInterval);
                var localT = t - cluckIndex * cluckInterval;

                var sampleVal = 0f;
                if (localT < 0.08f)
                {
                    var progress = localT / 0.08f;
                    var freq = Mathf.Lerp(820f, 1250f, progress);
                    var phase = 2f * Mathf.PI * freq * localT;

                    var env = Mathf.Sin(progress * Mathf.PI);
                    sampleVal = (Mathf.Sin(phase) + 0.3f * Mathf.Sin(phase * 2.5f)) * env;
                }

                samples[i] = Mathf.Clamp(sampleVal * 0.75f, -1f, 1f);
            }

            var clip = AudioClip.Create("Voice_Chicken_Cluck", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GeneratePigSquealClip()
        {
            const float duration = 0.55f;
            var sampleCount = Mathf.RoundToInt(SampleRate * duration);
            var samples = new float[sampleCount];

            var phase = 0f;
            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / SampleRate;
                var progress = t / duration;

                // Piercing comic high squeal with fast frantic vibrato
                var baseFreq = Mathf.Lerp(780f, 1380f, Mathf.Sin(progress * Mathf.PI));
                var vibrato = Mathf.Sin(2f * Mathf.PI * 16f * t) * 90f;
                var freq = baseFreq + vibrato;

                phase += 2f * Mathf.PI * freq / SampleRate;

                var attack = Mathf.Clamp01(t / 0.03f);
                var decay = Mathf.Pow(1f - progress, 1.1f);
                var env = attack * decay;

                var wave = Mathf.Sin(phase) + 0.4f * Mathf.Sin(phase * 2f);
                samples[i] = Mathf.Clamp(wave * env * 0.7f, -1f, 1f);
            }

            var clip = AudioClip.Create("Voice_Pig_Squeal", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
