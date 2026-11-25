using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Gently drifts exterior clouds across the sky outside the panoramic office window.
    /// </summary>
    public class CloudDrift : MonoBehaviour
    {
        [SerializeField] private float speedZ = 0.35f;
        [SerializeField] private float minZ = -26f;
        [SerializeField] private float maxZ = 14f;

        private void Update()
        {
            Vector3 p = transform.localPosition;
            p.z += speedZ * Time.deltaTime;
            if (p.z > maxZ) p.z = minZ;
            transform.localPosition = p;
        }
    }
}
