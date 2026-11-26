using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Visually vibrates the 3D desk telephone and plays a periodic ring sound whenever
    /// a phone call is waiting for the player (Supervisor, Alex, or Sofia).
    /// </summary>
    public class RingingPhoneEffect : MonoBehaviour
    {
        [SerializeField] private float ringIntervalSeconds = 4.2f;
        [SerializeField] private float ringDurationSeconds = 1.0f;
        [SerializeField] private float shakeAngle = 6f;

        private Quaternion baseRotation;
        private float timer;
        private bool isRingingNow;
        private bool shouldRing;

        private void Awake()
        {
            baseRotation = transform.localRotation;
        }

        private void Start()
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionChanged += HandleMissionChanged;
                HandleMissionChanged(MissionManager.Instance.CurrentState, string.Empty);
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
            shouldRing = state == MissionState.Prologue_SupervisorCall
                      || state == MissionState.Alex_AnswerFirstCall
                      || state == MissionState.Alex_ObjectionCall
                      || state == MissionState.Sofia_Ready;

            if (shouldRing)
            {
                timer = ringIntervalSeconds - 0.6f; // Ring almost immediately when state starts
            }
            else
            {
                isRingingNow = false;
                transform.localRotation = baseRotation;
            }
        }

        private void Update()
        {
            if (!shouldRing || MainMenuOverlay.IsActive)
            {
                transform.localRotation = baseRotation;
                return;
            }

            timer += Time.deltaTime;
            if (timer >= ringIntervalSeconds)
            {
                timer = 0f;
                isRingingNow = true;
                SoundManager.Instance?.Play("phone_ring", 0.55f);
            }

            if (isRingingNow)
            {
                if (timer < ringDurationSeconds)
                {
                    float wobble = Mathf.Sin(Time.time * 48f) * shakeAngle;
                    transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, wobble);
                }
                else
                {
                    isRingingNow = false;
                    transform.localRotation = baseRotation;
                }
            }
        }
    }
}
