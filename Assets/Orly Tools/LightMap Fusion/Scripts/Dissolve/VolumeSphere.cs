using UnityEngine;

namespace LightMapFusion
{
    public class VolumeSphere : Volume
    {
        [HideInInspector]
        public float radius = 1;

        public override void UpdateVolume()
        {
            DissolveController data = transform.GetComponent<DissolveController>();
            if (data)
            {
                foreach (Material m in dissolveVolMaterials)
                {
                    if (m != null)
                    {
                        m.SetFloat("_MaskSelector", 1);
                        m.SetFloat("_SphereRadius", transform.localScale.x * 0.5f);
                        m.SetVector("_GizmoCenter", transform.position);
                        m.SetFloat("_Invert", System.Convert.ToSingle(data.invert));
                        m.SetColor("_EdgeColor", data.edgeColor);
                        m.SetFloat("_Dissolve", data.noise);
                        m.SetFloat("_EdgeThickness", data.edgeThickness);
                        m.SetFloat("_NoiseScale", data.noiseScale);
                        m.SetVector("_NoiseScrollSpeed", data.noiseScrollSpeed);
                        m.SetFloat("_AlphaThreshold", data.alphaThreshold);

                    }
                    else
                    {
                        dissolveVolMaterials.Remove(m);
                    }
                }
            }
        }
    }
}
