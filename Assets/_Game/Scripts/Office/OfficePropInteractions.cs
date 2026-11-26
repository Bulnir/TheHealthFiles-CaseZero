using System.Collections;
using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Tactile micro-interactions for everyday office props (zero conflict with WP3!):
    /// - Desk Lamp: toggles directional SpotLight shining from the actual shade tip + emissive bulb glow
    /// - Coffee Mug: realistic soft wispy ParticleSystem steam + sip sound + witty detective quips
    /// - Damacana Water Cooler: glug-glug sound + rising air bubbles inside the jug + peer-reviewed hydration tip
    /// - Trash Can: rustle sound + hidden crumpled detective note from the previous investigator!
    /// </summary>
    public class OfficePropInteractions : MonoBehaviour
    {
        private Light deskLampLight;
        private Renderer lampBulbRenderer;
        private Transform waterBubbleAnchor;
        private AudioSource propAudio;
        private AudioClip clipLamp;
        private AudioClip clipSip;
        private AudioClip clipWater;
        private int coffeeIdx;
        private int waterIdx;
        private int trashIdx;
        private int windowIdx;

        private static readonly string[] WindowQuips =
        {
            "Investigator: \"What a beautiful day outside... almost makes me want to take a walk instead of reading privacy policies.\"",
            "Investigator: \"Five floors up, sunshine and drifting clouds. Not a bad view for Room 504!\"",
            "Investigator: \"Look at that clear sky — good reminder to rest your eyes away from the monitor once in a while.\""
        };

        private static readonly string[] CoffeeQuips =
        {
            "Investigator: \"Third espresso today... now I'm ready to read a 40-page privacy policy.\"",
            "Investigator: \"Still warm! Nothing pairs better with fact-checking than Italian coffee.\"",
            "Investigator: \"Caffeine level restored. Remember: Source, Evidence, Context!\""
        };

        private static readonly string[] WaterQuips =
        {
            "Investigator: \"Stay hydrated — actual medical advice, 100% peer-reviewed.\"",
            "Investigator: \"Glug, glug... refreshing! Even detectives need water breaks.\"",
            "Investigator: \"Cold tap still works! Better than trusting miracle detox teas online.\""
        };

        private static readonly string[] TrashNotes =
        {
            "Crumpled Sticky Note in Trash: \"Reminder to self — NEVER trust a health blog that has no author name!\"",
            "Crumpled Draft in Trash: \"Note: 4 million viral views ≠ clinical trial. Tell the interns!\"",
            "Crumpled Receipt in Trash: \"PoliMi Cafeteria: 3x Espresso, 1x Croissant. Case Zero fuel.\""
        };

        private void Awake()
        {
            propAudio = gameObject.AddComponent<AudioSource>();
            propAudio.playOnAwake = false;
            propAudio.spatialBlend = 0f;
            clipLamp = Resources.Load<AudioClip>("Audio/lamp_switch");
            clipSip = Resources.Load<AudioClip>("Audio/coffee_sip");
            clipWater = Resources.Load<AudioClip>("Audio/water_glug");
        }

        private void Start()
        {
            GameObject lampObj = GameObject.Find("DeskLampReadingLight");
            if (lampObj != null) deskLampLight = lampObj.GetComponent<Light>();

            GameObject bulbObj = GameObject.Find("DeskLampBulbGlow");
            if (bulbObj != null) lampBulbRenderer = bulbObj.GetComponent<Renderer>();

            GameObject mugObj = GameObject.Find("CoffeeMug");
            if (mugObj != null) AttachWispyCoffeeSteam(mugObj.transform);

            GameObject jugObj = GameObject.Find("Jug_WaterInside");
            if (jugObj != null) waterBubbleAnchor = jugObj.transform;

            InteractableObject.OnAnyObjectClicked += HandlePropClicked;
        }

        private void OnDestroy()
        {
            InteractableObject.OnAnyObjectClicked -= HandlePropClicked;
        }

        private void HandlePropClicked(InteractableObject obj)
        {
            if (obj == null) return;
            switch (obj.interactableId)
            {
                case "desk_lamp":
                    if (clipLamp != null) propAudio.PlayOneShot(clipLamp, 0.65f);
                    if (deskLampLight != null)
                    {
                        deskLampLight.enabled = !deskLampLight.enabled;
                        UpdateBulbVisual(deskLampLight.enabled);
                        MissionManager.Instance?.NotifyStatus(deskLampLight.enabled
                            ? "Desk lamp switched ON."
                            : "Desk lamp switched OFF.");
                    }
                    break;

                case "desk_coffee":
                    if (clipSip != null) propAudio.PlayOneShot(clipSip, 0.7f);
                    MissionManager.Instance?.NotifyStatus(CoffeeQuips[coffeeIdx % CoffeeQuips.Length]);
                    coffeeIdx++;
                    break;

                case "office_water_cooler":
                    if (clipWater != null) propAudio.PlayOneShot(clipWater, 0.65f);
                    if (waterBubbleAnchor != null) StartCoroutine(SpawnWaterBubblesRoutine());
                    MissionManager.Instance?.NotifyStatus(WaterQuips[waterIdx % WaterQuips.Length]);
                    waterIdx++;
                    break;

                case "desk_trash_can":
                    SoundManager.Instance?.Play("paper_rustle", 0.65f);
                    MissionManager.Instance?.NotifyStatus(TrashNotes[trashIdx % TrashNotes.Length]);
                    trashIdx++;
                    break;

                case "office_window":
                    SoundManager.Instance?.Play("ui_click", 0.35f);
                    MissionManager.Instance?.NotifyStatus(WindowQuips[windowIdx % WindowQuips.Length]);
                    windowIdx++;
                    break;
            }
        }

        private void UpdateBulbVisual(bool isOn)
        {
            if (lampBulbRenderer == null) return;
            Color c = isOn ? new Color(1f, 0.94f, 0.72f) : new Color(0.75f, 0.75f, 0.72f);
            Material mat = lampBulbRenderer.material;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", isOn ? new Color(1.6f, 1.3f, 0.7f) : Color.black);
            }
        }

        /// <summary>
        /// Creates a soft, realistic translucent wispy steam ParticleSystem rising gently from the coffee mug!
        /// </summary>
        private static void AttachWispyCoffeeSteam(Transform mug)
        {
            GameObject psObj = new GameObject("CoffeeWispySteam");
            psObj.transform.SetParent(mug, false);
            psObj.transform.localPosition = new Vector3(0f, 0.095f, 0f);

            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.035f, 0.065f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.060f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(1f, 0.99f, 0.96f, 0.22f);
            main.maxParticles = 24;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission;
            em.rateOverTime = 4.5f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.022f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.5f),
                new Keyframe(0.5f, 1.35f),
                new Keyframe(1f, 2.0f)
            ));

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.24f, 0.25f), new GradientAlphaKey(0.10f, 0.7f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = g;

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.025f;
            noise.frequency = 0.9f;

            // Soft radial Gaussian alpha texture for silky steam wisps (zero bubble edges!)
            Texture2D softTex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dx = (x - 31.5f) / 31.5f;
                    float dy = (y - 31.5f) / 31.5f;
                    float r2 = dx * dx + dy * dy;
                    float a = Mathf.Clamp01(Mathf.Exp(-r2 * 3.8f) * (1f - r2));
                    softTex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            softTex.Apply();

            Shader sh = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
            Material mat = new Material(sh);
            mat.mainTexture = softTex;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", softTex);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;

            var rend = ps.GetComponent<ParticleSystemRenderer>();
            rend.material = mat;
        }

        private IEnumerator SpawnWaterBubblesRoutine()
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 pos = waterBubbleAnchor.position + new Vector3(Random.Range(-0.04f, 0.04f), -0.07f, Random.Range(-0.04f, 0.04f));
                StartCoroutine(FloatBubbleRoutine(pos, 0.016f, 0.20f, 0.70f));
                yield return new WaitForSeconds(0.15f);
            }
        }

        private static IEnumerator FloatBubbleRoutine(Vector3 startPos, float size, float riseMeters, float duration)
        {
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(b.GetComponent<Collider>());
            b.transform.position = startPos;
            b.transform.localScale = Vector3.one * size;
            Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material m = new Material(sh);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", new Color(0.85f, 0.96f, 1f));
            b.GetComponent<Renderer>().material = m;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                b.transform.position = startPos + new Vector3(Mathf.Sin(t * 10f) * 0.008f, riseMeters * t, 0f);
                yield return null;
            }
            Destroy(b);
            Destroy(m);
        }
    }
}
