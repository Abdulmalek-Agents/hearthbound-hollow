#if UNITY_EDITOR


using UnityEditor;
using UnityEngine;


namespace LightMapFusion
{

    public class MyTextureImporter
    {
        public void ImportTexture(Texture2D asset)
        {
            TextureImporter importer = (TextureImporter)
                TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(asset));
            importer.isReadable = true;
            importer.textureType = TextureImporterType.Lightmap;

            TextureImporterSettings importerSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(importerSettings);
            importer.SetTextureSettings(importerSettings);
            //importer.maxTextureSize = 4096; // or whatever
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;

            //importer.textureFormat = TextureImporterFormat.RGBAFloat;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            //var androidOverrides = importer.GetPlatformTextureSettings("Android");
            //androidOverrides.overridden = true;
            //androidOverrides.format = TextureImporterFormat.RGBAHalf;
            //importer.SetPlatformTextureSettings(androidOverrides);
        }
    }
}

#endif
