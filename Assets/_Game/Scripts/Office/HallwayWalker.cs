using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Animates a stylized office coworker walking casually past the glass corridor outside Room 504
    /// every ~16-28 seconds (with swinging arms/legs), bringing the outer hallway to life!
    /// </summary>
    public class HallwayWalker : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 1.25f;
        [SerializeField] private float minX = -5.2f;
        [SerializeField] private float maxX = 5.2f;
        [SerializeField] private float corridorZ = 4.95f;
        [SerializeField] private float initialDelay = 6f;

        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;

        private bool isWalking;
        private int dir = 1; // +1 = left->right, -1 = right->left
        private float waitTimer;
        private float walkCycle;

        public void Configure(Transform lLeg, Transform rLeg, Transform lArm, Transform rArm, float startDelay, float zPos)
        {
            leftLeg = lLeg; rightLeg = rLeg; leftArm = lArm; rightArm = rArm;
            initialDelay = startDelay;
            corridorZ = zPos;
        }

        private void Start()
        {
            waitTimer = initialDelay;
            HideOffCorridor();
        }

        private void HideOffCorridor()
        {
            isWalking = false;
            transform.position = new Vector3(minX - 1.2f, 0f, corridorZ);
        }

        private void Update()
        {
            if (!isWalking)
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    StartWalk();
                }
                return;
            }

            Vector3 p = transform.position;
            p.x += dir * walkSpeed * Time.deltaTime;
            transform.position = p;

            walkCycle += Time.deltaTime * 6.5f;
            float swing = Mathf.Sin(walkCycle) * 24f;
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(0f, 0f, swing);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(0f, 0f, -swing);
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(0f, 0f, -swing * 0.65f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(0f, 0f, swing * 0.45f);

            // Subtle walking bob
            p.y = Mathf.Abs(Mathf.Sin(walkCycle)) * 0.025f;
            transform.position = p;

            if ((dir > 0 && p.x > maxX) || (dir < 0 && p.x < minX))
            {
                waitTimer = Random.Range(32f, 56f);
                HideOffCorridor();
            }
        }

        private void StartWalk()
        {
            dir = Random.value > 0.5f ? 1 : -1;
            float startX = dir > 0 ? minX : maxX;
            transform.position = new Vector3(startX, 0f, corridorZ);
            transform.rotation = Quaternion.Euler(0f, dir > 0 ? 0f : 180f, 0f);
            walkSpeed = Random.Range(1.10f, 1.48f);
            isWalking = true;
        }
    }
}
