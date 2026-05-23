using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Simple helper that shows a tooltip when the mouse hovers this UI element.
    /// Requires a Tooltip instance present in the scene.
    /// </summary>
    public class ToolTipInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea]
        public string mainText;
        public string costText;

        public void OnPointerEnter(PointerEventData eventData)
        {
            Tooltip.ShowTooltipStatic(mainText, costText);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Tooltip.HideTooltipStatic();
        }
    }
}
