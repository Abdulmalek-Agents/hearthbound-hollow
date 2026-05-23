using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace LightMapFusion
{
    public enum SelectorMask
    {
        Plane = 0,
        Sphere = 1,
        Box = 2
    }

    public class DissolveController : MonoBehaviour
    {
        Color GizmoColor = Color.magenta;
        public Volume volumeGizmo;
        public List<Shader> toolShaders;

        [Range(0, 1)]
        public float noise;

        [Range(0, 1)]
        public float edgeThickness;

        [Range(0, 10)]
        public float noiseScale;

        public Vector3 noiseScrollSpeed;

        [Range(0, 1)]
        public float alphaThreshold;

        public bool invert;

        public int id = 0;

        [ColorUsageAttribute(true, true), SerializeField]
        public Color edgeColor;

        public SelectorMask selectorMask = new SelectorMask();

        private void Update()
        {
            if (volumeGizmo != null)
            {
                volumeGizmo.UpdateVolume();
            }
        }

#if UNITY_EDITOR

        public void AssigMaterialtoVolumes()
        {
            if (volumeGizmo)
            {
                volumeGizmo.dissolveVolMaterials.Clear();
                Renderer[] renderers = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
                foreach (Renderer r in renderers)
                {
                    for (int i = 0; i < toolShaders.Count; i++)
                    {
                        if (r.sharedMaterial.shader.name == toolShaders[i].name)
                        {
                            Material m = r.sharedMaterial;

                            if ((int)m.GetFloat("_vID") == id)
                            {

                                volumeGizmo.AddMaterialToVolume(m);
                            }
                        }
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = GizmoColor;

            if (!volumeGizmo)
            {
                return;
            }
            if (volumeGizmo.typeID != selectorMask)
            {
                switch (volumeGizmo.typeID)
                {

                    case SelectorMask.Plane:
                        DestroyImmediate(transform.GetComponent<VolumePlane>());
                        break;
                    case SelectorMask.Sphere:
                        DestroyImmediate(transform.GetComponent<VolumeSphere>());
                        break;
                    case SelectorMask.Box:
                        DestroyImmediate(transform.GetComponent<VolumeBox>());
                        break;

                }
                volumeGizmo = null;
                switch (selectorMask)
                {
                    case SelectorMask.Plane:
                        volumeGizmo = transform.AddComponent<VolumePlane>();

                        volumeGizmo.typeID = selectorMask;
                        break;
                    case SelectorMask.Sphere:
                        volumeGizmo = transform.AddComponent<VolumeSphere>();
                        volumeGizmo.typeID = selectorMask;

                        break;
                    case SelectorMask.Box:
                        volumeGizmo = transform.AddComponent<VolumeBox>();
                        volumeGizmo.typeID = selectorMask;

                        break;
                }
                AssigMaterialtoVolumes();
            }
            volumeGizmo.UpdateVolume();
            Gizmos.matrix = Matrix4x4.identity;

            switch (volumeGizmo)
            {
                case VolumeBox:
                    VolumeBox vB = volumeGizmo as VolumeBox;
                    //Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1,1,1));
                    //Gizmos.matrix = rotationMatrix;
                    Gizmos.DrawWireCube(transform.position, transform.localScale);
                    break;
                case VolumeSphere:
                    VolumeSphere vS = volumeGizmo as VolumeSphere;
                    float scale = vS.transform.localScale.x;
                    vS.transform.localScale.Set(scale, scale, scale);
                    //GizmoType.Pickable;
                    vS.radius = vS.transform.localScale.x * 0.5f;
                    Gizmos.DrawWireSphere(vS.transform.position, vS.radius);
                    break;
                case VolumePlane:

                    Matrix4x4 rotationMatrix = Matrix4x4.identity;
                    rotationMatrix.SetTRS(transform.position, transform.rotation, transform.localScale);
                    //Matrix4x4 pp = Matrix4x4.Translate(transform.position);
                    //Matrix4x4 sc= Matrix4x4.Scale(transform.localScale);
                    //rotationMatrix.
                    Gizmos.matrix = rotationMatrix;

                    Gizmos.DrawWireCube(
                      new Vector3(),
                        new Vector3(1, 0, 1)
                    );
                    Gizmos.DrawLine(new Vector3(), new Vector3(0, transform.localScale.y, 0));
                    break;
            }
            Gizmos.matrix = Matrix4x4.identity;
        }
        /*
            private void OnDrawGizmosSelected()
            {
                Gizmos.color = new Color(1f, 0.3f, 0f);

                if (!volumeGizmo)
                {
                    return;
                }
                volumeGizmo.UpdateVolume();
                Gizmos.matrix = Matrix4x4.identity;

                switch (volumeGizmo)
                {
                    case VolumeBox:
                        VolumeBox vB = volumeGizmo as VolumeBox;
                        //Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1,1,1));
                        //Gizmos.matrix = rotationMatrix;
                        Gizmos.DrawWireCube(transform.position, transform.localScale);
                        break;
                    case VolumeSphere:
                        VolumeSphere vS = volumeGizmo as VolumeSphere;
                        float scale = vS.transform.localScale.x;
                        vS.transform.localScale.Set(scale, scale, scale);
                        vS.radius = vS.transform.localScale.x * 0.5f;
                        Gizmos.DrawWireSphere(vS.transform.position, vS.radius);
                        break;
                    case VolumePlane:

                        //Matrix4x4 rotationMatrix = Matrix4x4.Rotate(transform.rotation);
                        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(transform.rotation); 
                        Vector3 p = transform.position;
                        Matrix4x4 pp = Matrix4x4.Translate(transform.position);


                        //rotationMatrix.SetTRS(transform.position, transform.rotation, transform.localScale);

                        Gizmos.matrix = rotationMatrix * pp;
                        //transform.position.Set(transform.position.x,0, transform.position.z);
                        Gizmos.DrawWireCube(
                            new Vector3(),
                            new Vector3(1,0,1)
                        );

                        Gizmos.DrawLine(new Vector3(), new Vector3(0, transform.localScale.y, 0));
                        break;
                }
                Gizmos.matrix = Matrix4x4.identity;
            }
            */

#endif
    }
}
