using UnityEngine;

namespace LoQueCallaste
{
    public sealed class AmbientMotion : MonoBehaviour
    {
        public Light bulb;
        public float intensity = 1f;
        private void Update()
        {
            if (bulb) bulb.intensity = intensity * (0.85f + 0.15f * Mathf.PerlinNoise(Time.time * 8f, transform.position.z));
        }
    }
}
