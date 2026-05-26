using UnityEngine;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Simple mouse wheel zoom controller for a UI panel using a RectTransform scale.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIPanelZoom : MonoBehaviour
    {
        [SerializeField] private float zoomSpeed = 100f;
        [SerializeField] private float maxZoom = 3f;
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float smoothAmount = 15f;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            // Only react to input while the game is playing.
            if (!Application.isPlaying)
                return;

            if (_rectTransform == null)
                return;

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Approximately(scroll, 0f))
                return;

            float zoomFactor = 1f - scroll * zoomSpeed;
            var targetScale = new Vector3(zoomFactor, zoomFactor, 1f);

            _rectTransform.localScale = Vector3.Lerp(
                _rectTransform.localScale,
                targetScale,
                Time.deltaTime * smoothAmount);

            _rectTransform.localScale = new Vector3(
                Mathf.Clamp(_rectTransform.localScale.x, minZoom, maxZoom),
                Mathf.Clamp(_rectTransform.localScale.y, minZoom, maxZoom),
                1f);
        }
    }
}
