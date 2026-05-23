using TMPro;
using UnityEngine;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Simple singleton tooltip used by ToolTipInfo to display talent info near the mouse cursor.
    /// </summary>
    public class Tooltip : MonoBehaviour
    {
        private static Tooltip _instance;

        [SerializeField] private Camera mainCamera;
        [SerializeField] private Canvas toolTipCanvas;
        [SerializeField] private RectTransform canvasRectTransform;
        [SerializeField] private TMP_Text tooltipMainText;
        [SerializeField] private TMP_Text tooltipCostText;
        [SerializeField] private RectTransform backgroundRectTransform;

        private RectTransform _rectTransform;
        private string _tooltipString;
        private string _tooltipCostString;
        private bool _isScreenSpaceCamera;

        private void Awake()
        {
            _instance = this;
            _rectTransform = GetComponent<RectTransform>();

            if (toolTipCanvas != null)
                _isScreenSpaceCamera = toolTipCanvas.renderMode == RenderMode.ScreenSpaceCamera;

            HideTooltip();
        }

        private void Update()
        {
            SetText();

            if (_isScreenSpaceCamera)
            {
                if (mainCamera == null)
                    return;

                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = 10f;
                Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
                transform.position = worldMousePosition;
                return;
            }

            if (canvasRectTransform == null || _rectTransform == null)
                return;

            Vector2 anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x;

            if (backgroundRectTransform != null)
            {
                if (anchoredPosition.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width)
                    anchoredPosition.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width;

                if (anchoredPosition.y + backgroundRectTransform.rect.height > canvasRectTransform.rect.height)
                    anchoredPosition.y = canvasRectTransform.rect.height - backgroundRectTransform.rect.height;
            }

            _rectTransform.anchoredPosition = anchoredPosition;
        }

        private void ShowTooltip(string tooltipString, string costString = "")
        {
            _tooltipString = tooltipString;
            _tooltipCostString = costString;

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Update();
        }

        private void SetText()
        {
            if (tooltipMainText != null)
            {
                tooltipMainText.SetText(_tooltipString);
                tooltipMainText.ForceMeshUpdate();
            }

            if (tooltipCostText != null)
            {
                tooltipCostText.SetText(_tooltipCostString);
                tooltipCostText.ForceMeshUpdate();
            }

            if (backgroundRectTransform != null && tooltipMainText != null)
            {
                Vector2 textSize = tooltipMainText.GetRenderedValues(false);
                Vector2 paddingSize = new Vector2(18f, 12f);
                backgroundRectTransform.sizeDelta = textSize + paddingSize;
            }
        }

        private void HideTooltip()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows the tooltip with the given main text and optional cost text.
        /// </summary>
        public static void ShowTooltipStatic(string tooltipString, string additionalString = "")
        {
            if (_instance == null)
                return;

            _instance.ShowTooltip(tooltipString, additionalString);
        }

        /// <summary>
        /// Hides the currently visible tooltip, if any.
        /// </summary>
        public static void HideTooltipStatic()
        {
            if (_instance == null)
                return;

            _instance.HideTooltip();
        }
    }
}
