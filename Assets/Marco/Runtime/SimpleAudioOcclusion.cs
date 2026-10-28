using UnityEngine;

namespace LoQueCallaste
{
    /// <summary>Optional line-of-sight muffling. Not a simulation of acoustic propagation.</summary>
    [RequireComponent(typeof(AudioSource), typeof(AudioLowPassFilter))]
    public sealed class SimpleAudioOcclusion : MonoBehaviour
    {
        public Transform listener;
        public LayerMask obstacles = ~0;
        [Range(200, 12000)] public float blockedCutoff = 1800;
        public bool IsOccluded { get; private set; }
        private AudioLowPassFilter filter;
        private readonly RaycastHit[] hits = new RaycastHit[16];
        private float nextCheck, originalCutoff;
        private bool originalEnabled;
        private void Awake()
        {
            filter = GetComponent<AudioLowPassFilter>();
            originalCutoff = filter.cutoffFrequency; originalEnabled = filter.enabled;
        }
        private void OnEnable() { if (filter) filter.enabled = true; }
        private void OnDisable()
        {
            if (filter) { filter.cutoffFrequency = originalCutoff; filter.enabled = originalEnabled; }
            IsOccluded = false;
        }
        private void Update()
        {
            if (Time.time >= nextCheck) { RefreshOcclusion(); nextCheck = Time.time + .2f; }
            if (filter) filter.cutoffFrequency = Mathf.MoveTowards(filter.cutoffFrequency, IsOccluded ? blockedCutoff : originalCutoff, Time.deltaTime * 28000);
        }
        public void RefreshOcclusion()
        {
            IsOccluded = false;
            if (!listener)
            {
                var found = FindFirstObjectByType<AudioListener>();
                if (found && found.isActiveAndEnabled) listener = found.transform;
            }
            if (!listener) return;
            Vector3 path = listener.position - transform.position;
            if (path.sqrMagnitude < .01f) return;
            int count = Physics.RaycastNonAlloc(transform.position, path.normalized, hits, path.magnitude, obstacles, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var obstacle = hits[i].transform;
                if (obstacle == transform || obstacle.IsChildOf(transform) || listener.IsChildOf(obstacle)) continue;
                IsOccluded = true; break;
            }
        }
    }
}
