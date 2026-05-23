
using UnityEngine;


namespace LightMapFusion
{
    public class VolumePlane : Volume
    {

        [HideInInspector]
        public Vector3 planeDirection = new Vector3(0, 1, 1);



        public override void UpdateVolume()
        {

            DissolveController data = transform.GetComponent<DissolveController>();
            if (data)
            {
                foreach (Material m in dissolveVolMaterials)
                {
                    if (m != null)
                    {

                        m.SetFloat("_MaskSelector", 0);
                        planeDirection = transform.up;
                        m.SetVector("_GizmoCenter", transform.position);
                        m.SetVector("_PlaneDirection", planeDirection);
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
