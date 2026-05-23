using UnityEngine;
using UnityEngine.UI;

namespace Microdetail.Demo
{
    public class DencitySliderDemo : MonoBehaviour
    {
        [SerializeField] private Layer layer;
        [SerializeField] private Slider slider;

        private void Start()
        {
            slider.onValueChanged.AddListener(x =>
                {
                    Settings.SetLayerEntriesPerUnitAreaScaler(layer, x);
                });
        }
    }
}