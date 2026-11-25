using System.Collections.Generic;
using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// Shared lightweight audio manager loading procedurally generated or imported WAV clips
    /// from Assets/_Game/Resources/Audio/. Supports pitch-varied indie character voice blips!
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource sfxSource;
        private AudioSource voiceSource;
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        public bool IsMuted { get; private set; }
        public float MasterVolume { get; private set; } = 1f;
        public event System.Action OnAudioSettingsChanged;

        private static readonly string[] ClipNames =
        {
            "phone_ring",
            "ui_click",
            "drawer_slide",
            "paper_rustle",
            "evidence_unlock",
            "objection_sting",
            "wrong_buzz",
            "voice_supervisor",
            "voice_alex",
            "voice_sofia",
            "voice_daniel"
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;

            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
            voiceSource.spatialBlend = 0f;

            foreach (string clipName in ClipNames)
            {
                AudioClip clip = Resources.Load<AudioClip>("Audio/" + clipName);
                if (clip != null)
                {
                    clips[clipName] = clip;
                }
            }
        }

        public void SetMuted(bool muted)
        {
            IsMuted = muted;
            if (sfxSource != null) sfxSource.mute = IsMuted;
            if (voiceSource != null) voiceSource.mute = IsMuted;
            AudioListener.volume = IsMuted ? 0f : MasterVolume;
            OnAudioSettingsChanged?.Invoke();
        }

        public void ToggleMute()
        {
            SetMuted(!IsMuted);
        }

        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);
            if (!IsMuted)
            {
                AudioListener.volume = MasterVolume;
            }
            OnAudioSettingsChanged?.Invoke();
        }

        public void Play(string clipName, float volume = 0.85f)
        {
            if (IsMuted || sfxSource == null || string.IsNullOrEmpty(clipName)) return;
            if (clips.TryGetValue(clipName, out AudioClip clip) && clip != null)
            {
                sfxSource.PlayOneShot(clip, volume * MasterVolume);
            }
        }

        /// <summary>
        /// Plays a short character voice mumble blip with slight random pitch variation (Ace Attorney / Undertale style).
        /// </summary>
        public void PlayVoiceBlip(string characterKey, float volume = 0.42f)
        {
            if (IsMuted || voiceSource == null) return;
            string clipName = "voice_" + (string.IsNullOrEmpty(characterKey) ? "supervisor" : characterKey.ToLowerInvariant());
            if (!clips.TryGetValue(clipName, out AudioClip clip))
            {
                clips.TryGetValue("voice_supervisor", out clip);
            }
            if (clip == null) return;

            voiceSource.pitch = Random.Range(0.92f, 1.10f);
            voiceSource.PlayOneShot(clip, volume * MasterVolume);
        }

        public void StopVoice()
        {
            if (voiceSource != null)
            {
                voiceSource.Stop();
            }
        }
    }
}
