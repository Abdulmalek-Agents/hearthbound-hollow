using UnityEngine;
using UnityEngine.Rendering;

namespace LightMapFusion
{

    public static class RenderPipelineChecker
    {
        public static string CheckRenderPipeline()
        {
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                return "Built-in";
            }
            else if (GraphicsSettings.currentRenderPipeline.GetType().Name.Contains("UniversalRenderPipelineAsset"))
            {
                return "URP";
            }
            else if (GraphicsSettings.currentRenderPipeline.GetType().Name.Contains("HDRenderPipelineAsset"))
            {
                return "HDRP";
            }
            return "";
        }
    }
}
