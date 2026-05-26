#if UNITY_EDITOR


using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace LightMapFusion
{
    public class TextureAtlasGenerator
    {
        public void GenerateTextureAtlas(List<Texture2D> lightmaps, int mode)
        {
            int offset = 0;
            for (int j = 0; j < lightmaps.Count; j += mode)
            {
                int atlasSize = lightmaps[j].width * 2;
                int subTextureWidth = atlasSize / 2;
                int subTextureHeight = atlasSize / 2;
                TextureFormat textureFormat = lightmaps[j].format;
                Texture2D atlasTexture = new Texture2D(
                    atlasSize,
                    atlasSize,
                    textureFormat,
                    true,
                    true,
                    true
                );
                for (int i = 0; i < mode; i++)
                {
                    int x = (i % 2) * subTextureWidth;
                    int y = (i / 2) * subTextureHeight;
                    Graphics.CopyTexture(
                        lightmaps[i + (offset * mode)],
                        0,
                        0,
                        0,
                        0,
                        subTextureWidth,
                        subTextureHeight,
                        atlasTexture,
                        0,
                        0,
                        x,
                        y
                    );
                }
                atlasTexture.Apply();
                byte[] atlasBytes = atlasTexture.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
                string scenePath = SceneManager.GetActiveScene().path;
                string folder = PathUtils.cleanScenePath(scenePath);
                string atlasFilePath = Path.Combine(
                    folder,
                    "Lightmap-" + offset + "_comp_light.exr"
                );
                File.WriteAllBytes(atlasFilePath, atlasBytes);
                AssetDatabase.Refresh();
                offset++;
            }
        }

        public void GeneratePreviewTextureAtlas(List<Texture2D> lightmaps, int index)
        {
            int offset = 0;
            for (int j = 0; j < lightmaps.Count; j++)
            {
                int atlasSize = lightmaps[j].width * 2;
                int subTextureWidth = atlasSize / 2;
                int subTextureHeight = atlasSize / 2;
                TextureFormat textureFormat = lightmaps[j].format;
                Texture2D atlasTexture = new Texture2D(
                    atlasSize,
                    atlasSize,
                    textureFormat,
                    true,
                    true,
                    true
                );
                //for (int i = 0; i < mode; i++)
                //{
                int x = (index % 2) * subTextureWidth;
                int y = (index / 2) * subTextureHeight;
                Graphics.CopyTexture(
                    lightmaps[j],
                    0,
                    0,
                    0,
                    0,
                    subTextureWidth,
                    subTextureHeight,
                    atlasTexture,
                    0,
                    0,
                    x,
                    y
                );
                //}
                atlasTexture.Apply();
                byte[] atlasBytes = atlasTexture.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
                string scenePath = SceneManager.GetActiveScene().path;
                string folder = PathUtils.cleanScenePath(scenePath);
                string atlasFilePath = Path.Combine(
                    folder,
                    "Lightmap-" + offset + "_comp_light.exr"
                );
                File.WriteAllBytes(atlasFilePath, atlasBytes);
                AssetDatabase.Refresh();
                offset++;
            }
        }

        


        public Texture2D InterpolateTexture(Texture2D texture, float lerpValueX, float lerpValueY)
        {
            int width = (int)(texture.width * 0.5);
            int height = (int)(texture.height * 0.5);
            Texture2D interpolatedTexture = new Texture2D(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color color0 = texture.GetPixel(x, y);
                    Color color1 = texture.GetPixel(x * 2, y);
                    Color color2 = texture.GetPixel(x * 2, y * 2);
                    Color color3 = texture.GetPixel(x, y * 2);
                    Color lerpedColor0 = Color.Lerp(color0, color1, lerpValueX);
                    Color lerpedColor1 = Color.Lerp(color2, color3, lerpValueX);
                    Color lerpedColorFinal = Color.Lerp(lerpedColor0, lerpedColor1, lerpValueY);
                    interpolatedTexture.SetPixel(x, y, lerpedColorFinal);
                }
            }
            interpolatedTexture.Apply();
            return interpolatedTexture;
        }


    }
}


#endif
