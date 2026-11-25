using System.Collections.Generic;
using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Continuously drifts distant clouds from off-screen right (z = -38) to behind the tall skyscrapers (z = -3),
    /// enforcing a minimum spacing distance so clouds never overlap/bunch, and subtly varying each cloud's
    /// puff silhouette, altitude, depth, and size on every pass!
    /// </summary>
    public class CloudDrift : MonoBehaviour
    {
        private static readonly List<CloudDrift> ActiveClouds = new List<CloudDrift>();

        [SerializeField] private float speedZ = 0.78f;
        [SerializeField] private float minZ = -38f;
        [SerializeField] private float maxZ = -3f;
        [SerializeField] private float minY = 5.0f;
        [SerializeField] private float maxY = 11.4f;
        [SerializeField] private float minX = 19.5f;
        [SerializeField] private float maxX = 23.5f;
        [SerializeField] private float minSpacingZ = 6.2f;

        private float personalSpeed;
        private Vector3[] baseChildPos;
        private Vector3[] baseChildScale;

        private void OnEnable()
        {
            if (!ActiveClouds.Contains(this)) ActiveClouds.Add(this);
        }

        private void OnDisable()
        {
            ActiveClouds.Remove(this);
        }

        private void Awake()
        {
            personalSpeed = speedZ * Random.Range(0.90f, 1.12f);
            int n = transform.childCount;
            baseChildPos = new Vector3[n];
            baseChildScale = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                baseChildPos[i] = transform.GetChild(i).localPosition;
                baseChildScale[i] = transform.GetChild(i).localScale;
            }
            RandomizeShape();
        }

        private void Update()
        {
            Vector3 p = transform.localPosition;
            p.z += personalSpeed * Time.deltaTime;
            if (p.z > maxZ)
            {
                // Find a clean off-screen spawn Z that keeps at least minSpacingZ from every other cloud!
                float spawnZ = minZ;
                for (int attempt = 0; attempt < 12; attempt++)
                {
                    bool tooClose = false;
                    foreach (CloudDrift other in ActiveClouds)
                    {
                        if (other == this || other == null) continue;
                        if (Mathf.Abs(other.transform.localPosition.z - spawnZ) < minSpacingZ)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (!tooClose) break;
                    spawnZ -= minSpacingZ * 0.85f;
                }

                p.z = spawnZ;
                p.y = Random.Range(minY, maxY);
                p.x = Random.Range(minX, maxX);
                transform.localScale = Vector3.one * Random.Range(0.70f, 1.04f);
                RandomizeShape();
            }
            transform.localPosition = p;
        }

        private void RandomizeShape()
        {
            if (baseChildPos == null) return;
            for (int i = 0; i < transform.childCount && i < baseChildPos.Length; i++)
            {
                Transform child = transform.GetChild(i);
                child.localPosition = baseChildPos[i] + new Vector3(
                    Random.Range(-0.22f, 0.22f),
                    Random.Range(-0.18f, 0.28f),
                    Random.Range(-0.55f, 0.55f)
                );
                child.localScale = Vector3.Scale(baseChildScale[i], new Vector3(
                    Random.Range(0.82f, 1.22f),
                    Random.Range(0.78f, 1.28f),
                    Random.Range(0.80f, 1.35f)
                ));
            }
        }
    }
}
