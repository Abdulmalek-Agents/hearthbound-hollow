
using System.Collections.Generic;
using UnityEngine;

namespace LightMapFusion
{
    public class Volume : MonoBehaviour
    {

        [HideInInspector]
        public SelectorMask typeID;


        [SerializeField]
        public List<Material> dissolveVolMaterials = new List<Material>();





        public virtual void UpdateVolume() { }

        public void AddMaterialToVolume(Material m)
        {

            if (this is VolumeSphere)
            {
                typeID = SelectorMask.Sphere;
            }
            else if (this is VolumeBox)
            {
                typeID = SelectorMask.Box;
            }
            else
            {
                typeID = SelectorMask.Plane;
            }
            if (dissolveVolMaterials.Count == 0)
            {
                dissolveVolMaterials = new List<Material>
            {
                m
            };
                return;
            }
            else
            {

                foreach (Material mat in dissolveVolMaterials)
                {
                    bool exist = false;
                    if (mat == m)
                    {
                        exist = true;

                    }
                    if (!exist)
                    {

                        //if ((SelectorMask)m.GetFloat("_MaskSelector") == typeID )
                        //{
                        dissolveVolMaterials.Add(m);
                        return;
                        //}
                    }
                }
            }
        }
    }
}
