using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Story-driven Daylight Progression (Morning -> Midday -> Warm Golden Hour Sunset!)
    /// plus subtle, soothing in-game Office Ambience audio (automatically muted on the Main Menu!).
    /// </summary>
    public class OfficeAtmosphereController : MonoBehaviour
    {
        private Light sunLight;
        private Camera mainCam;
        private AudioSource ambienceSource;

        private Quaternion targetSunRot;
        private Color targetSunColor;
        private float targetSunIntensity;
        private Color targetSkyBg;
        private Color targetAmbientSky;

        private void Start()
        {
            mainCam = Camera.main;
            foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (l.type == LightType.Directional)
                {
                    sunLight = l;
                    break;
                }
            }

            // Subtle office room tone + distant city breeze + faint clock tick (plays only after Main Menu!)
            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.loop = true;
            ambienceSource.playOnAwake = false;
            ambienceSource.spatialBlend = 0f;
            ambienceSource.volume = 0f;
            AudioClip clip = Resources.Load<AudioClip>("Audio/office_ambience");
            if (clip != null)
            {
                ambienceSource.clip = clip;
                ambienceSource.Play();
            }

            ApplyLightingForState(MissionManager.Instance != null ? MissionManager.Instance.CurrentState : MissionState.Prologue_SupervisorCall);
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionChanged += HandleMissionChanged;
            }
        }

        private void OnDestroy()
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionChanged -= HandleMissionChanged;
            }
        }

        private void HandleMissionChanged(MissionState state, string _)
        {
            ApplyLightingForState(state);
        }

        private void ApplyLightingForState(MissionState state)
        {
            if (state <= MissionState.Alex_AnswerFirstCall)
            {
                // Fresh Bright Morning Daylight
                targetSunRot = Quaternion.Euler(40f, -125f, 0f);
                targetSunColor = new Color(1.00f, 0.97f, 0.90f);
                targetSunIntensity = 1.12f;
                targetSkyBg = new Color(0.55f, 0.76f, 0.95f);
                targetAmbientSky = new Color(0.52f, 0.58f, 0.68f);
            }
            else if (state <= MissionState.Alex_InspectBlogAndPolicy)
            {
                // Warm Crisp Afternoon
                targetSunRot = Quaternion.Euler(30f, -118f, 0f);
                targetSunColor = new Color(1.00f, 0.93f, 0.80f);
                targetSunIntensity = 1.08f;
                targetSkyBg = new Color(0.58f, 0.74f, 0.90f);
                targetAmbientSky = new Color(0.54f, 0.54f, 0.60f);
            }
            else
            {
                // Cinematic Golden Hour Sunset (Case 01 Climax / Sofia / Daniel!)
                targetSunRot = Quaternion.Euler(19f, -108f, 0f);
                targetSunColor = new Color(1.00f, 0.78f, 0.52f);
                targetSunIntensity = 1.02f;
                targetSkyBg = new Color(0.84f, 0.62f, 0.48f);
                targetAmbientSky = new Color(0.58f, 0.48f, 0.46f);
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime * 0.85f;
            if (sunLight != null)
            {
                sunLight.transform.rotation = Quaternion.Slerp(sunLight.transform.rotation, targetSunRot, dt);
                sunLight.color = Color.Lerp(sunLight.color, targetSunColor, dt);
                sunLight.intensity = Mathf.Lerp(sunLight.intensity, targetSunIntensity, dt);
            }
            if (mainCam != null)
            {
                mainCam.backgroundColor = Color.Lerp(mainCam.backgroundColor, targetSkyBg, dt);
            }
            RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, targetAmbientSky, dt);

            if (ambienceSource != null)
            {
                float targetVol = MainMenuOverlay.IsActive ? 0f : 0.24f;
                ambienceSource.volume = Mathf.MoveTowards(ambienceSource.volume, targetVol, Time.deltaTime * 0.4f);
            }
        }
    }
}
