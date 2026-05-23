
using UnityEngine;
using UnityEngine.Playables;

using UnityEngine.Timeline;

namespace LightMapFusion
{

    [TrackBindingType(typeof(LightMapFusion))]
    [TrackClipType(typeof(LightmapFusionControlClip))]
    public class LightmapFusionControlTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(
            PlayableGraph graph,
            GameObject go,
            int inputCount
        )
        {
            return ScriptPlayable<LightMapFusionControlMixer>.Create(graph, inputCount);
        }
    }

}
