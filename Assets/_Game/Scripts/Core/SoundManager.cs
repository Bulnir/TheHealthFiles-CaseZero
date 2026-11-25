using System.Collections.Generic;
using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// Shared lightweight audio manager loading procedurally generated or imported WAV/OGG/MP3 clips
    /// from Assets/_Game/Resources/Audio/.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource sfxSource;
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        private static readonly string[] ClipNames =
        {
            "phone_ring",
            "ui_click",
            "drawer_slide",
            "paper_rustle",
            "evidence_unlock",
            "objection_sting",
            "wrong_buzz"
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            sfxSource = gameObject.GetComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;

            foreach (string clipName in ClipNames)
            {
                AudioClip clip = Resources.Load<AudioClip>("Audio/" + clipName);
                if (clip != null)
                {
                    clips[clipName] = clip;
                }
            }
        }

        public void Play(string clipName, float volume = 0.85f)
        {
            if (sfxSource == null || string.IsNullOrEmpty(clipName)) return;
            if (clips.TryGetValue(clipName, out AudioClip clip) && clip != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }
    }
}
