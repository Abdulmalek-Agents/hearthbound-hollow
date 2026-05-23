using System;
using UnityEngine;
using UnityEngine.Playables;



namespace LightMapFusion
{

    [Serializable]
    public class LightMapFusionControlBehaviour : PlayableBehaviour
    {
        [SerializeField]
        [Range(0, 1)]
        public float blendValueX = 0f;


        [SerializeField]
        [Range(0, 1)]
        public float blendValueY = 1f;
        [Range(0, 1)]
        [SerializeField]
        public float set_intensity_1 = 1f;
        [Range(0, 1)]
        [SerializeField]
        public float set_intensity_2 = 1f;
        [Range(0, 1)]
        [SerializeField]
        public float set_intensity_3 = 1f;
        [Range(0, 1)]
        [SerializeField]
        public float set_intensity_4 = 1f;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var tool = playerData as LightMapFusion;
            base.ProcessFrame(playable, info, playerData);

            if (tool == null || tool.LightmapAtlas.Count == 0)
            {
                return;
            }
            tool.blendValueX = blendValueX;
            tool.blendValueY = blendValueY;

            tool.Set1Intensity = set_intensity_1;
            tool.Set2Intensity = set_intensity_2;
            tool.Set3Intensity = set_intensity_3;
            tool.Set4Intensity = set_intensity_4;
        }
    }
}




