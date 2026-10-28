using UnityEngine;

namespace LoQueCallaste.Hud
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class HudSafeArea : MonoBehaviour
    {
        private Rect previous;
        private Vector2Int screen;
        private void OnEnable() { Apply(); }
        private void Update()
        {
            if (previous != Screen.safeArea || screen.x != Screen.width || screen.y != Screen.height) Apply();
        }
        private void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            previous = Screen.safeArea; screen = new Vector2Int(Screen.width, Screen.height);
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(previous.xMin / Screen.width, previous.yMin / Screen.height);
            rect.anchorMax = new Vector2(previous.xMax / Screen.width, previous.yMax / Screen.height);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
