using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace LightMapFusion
{
    [Serializable]
    public class LightmapFusionControlClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField]
        private LightMapFusionControlBehaviour BlendControl = new LightMapFusionControlBehaviour();
        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<LightMapFusionControlBehaviour>.Create(graph, BlendControl);
        }
    }
}

