
using UnityEngine.Playables;


namespace LightMapFusion
{

    public class LightMapFusionControlMixer : PlayableBehaviour
    {
        private LightMapFusion tool;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            tool = playerData as LightMapFusion;
            if (tool == null)
            {
                return;
            }

            int inputCount = playable.GetInputCount();

            float blendValueX = 0f;
            float blendValueY = 0f;
            float totalWeigt = 0f;



            float set_intensity_1 = 0f;
            float set_intensity_2 = 0f;
            float set_intensity_3 = 0f;
            float set_intensity_4 = 0f;
            float totalWeigtIntensity = 0f;

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<LightMapFusionControlBehaviour> inputPlayable =
                    (ScriptPlayable<LightMapFusionControlBehaviour>)playable.GetInput(i);
                LightMapFusionControlBehaviour behaviour = inputPlayable.GetBehaviour();

                blendValueX += behaviour.blendValueX * inputWeight;
                blendValueY += behaviour.blendValueY * inputWeight;
                totalWeigt += inputWeight;

                set_intensity_1 += behaviour.set_intensity_1 * inputWeight;
                set_intensity_2 += behaviour.set_intensity_2 * inputWeight;
                set_intensity_3 += behaviour.set_intensity_3 * inputWeight;
                set_intensity_4 += behaviour.set_intensity_4 * inputWeight;
                totalWeigtIntensity += inputWeight;
            }

            tool.blendValueX = blendValueX;
            tool.blendValueY = blendValueY;
            tool.Set1Intensity = set_intensity_1;
            tool.Set2Intensity = set_intensity_2;
            tool.Set3Intensity = set_intensity_3;
            tool.Set4Intensity = set_intensity_4;

            tool.BlendLightMaps();
            tool.BlendSky();
            tool.BlendFog();
            tool.UpdateReflectionProbe();
        }
    }
}
