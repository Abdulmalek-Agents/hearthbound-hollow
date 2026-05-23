
using UnityEngine;


namespace LightMapFusion
{
    public class VolumeBox : Volume
    {
        [HideInInspector]
        public Vector3 boxSize = new Vector3(1, 1, 1);

        public override void UpdateVolume()
        {
            DissolveController data = transform.GetComponent<DissolveController>();
            if (data)
            {
                foreach (Material m in dissolveVolMaterials)
                {
                    m.SetFloat("_MaskSelector", 2);
                    m.SetVector("_BoxSize", transform.localScale);
                    m.SetVector("_GizmoCenter", transform.position);
                    m.SetFloat("_Invert", System.Convert.ToSingle(data.invert));
                    m.SetColor("_EdgeColor", data.edgeColor);
                    m.SetFloat("_Dissolve", data.noise);
                    m.SetFloat("_EdgeThickness", data.edgeThickness);
                    m.SetFloat("_NoiseScale", data.noiseScale);
                    m.SetVector("_NoiseScrollSpeed", data.noiseScrollSpeed);
                    m.SetFloat("_AlphaThreshold", data.alphaThreshold);
                    //m.SetVector("_BoxRotation", transform.eulerAngles);
                }
            }
        }
    }
}
