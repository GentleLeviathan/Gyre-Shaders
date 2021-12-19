using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GentleShaders.Gyre.Helpers
{
    public static class GyreDDSImporter
    {
        public static Texture2D DDSTextureImport(Texture texture)
        {
            try
            {
                Texture2D ddsTexture = (Texture2D)texture;

                string path = AssetDatabase.GetAssetPath(ddsTexture);

                IHVImageFormatImporter ddsImporter = (IHVImageFormatImporter)AssetImporter.GetAtPath(path);
                ddsImporter.isReadable = true;
                ddsImporter.SaveAndReimport();

                ddsTexture.requestedMipmapLevel = 0;
                byte[] pngTexture = ddsTexture.EncodeToPNG();

                File.WriteAllBytes(path.Replace(".dds", ".png"), pngTexture);

                AssetDatabase.Refresh();

                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path + ddsTexture.name.Replace(".dds", ".png"));
                importer.crunchedCompression = true;
                importer.streamingMipmaps = true;
                importer.SaveAndReimport();

                AssetDatabase.Refresh();

                return AssetDatabase.LoadAssetAtPath<Texture2D>(path + ddsTexture.name.Replace(".dds", ".png"));
            }
            catch (Exception e)
            {
                Debug.LogError("Gyre DDS Importer Helper - " + e);
                return null;
            }
        }

        public static Texture2D DDSNormalMapImport(Texture normal)
        {
            try
            {
                Texture2D ddsNormal = (Texture2D)normal;
                string path = AssetDatabase.GetAssetPath(ddsNormal);

                IHVImageFormatImporter ddsImporter = (IHVImageFormatImporter)AssetImporter.GetAtPath(path);
                ddsImporter.isReadable = true;
                ddsImporter.SaveAndReimport();

                ddsNormal.requestedMipmapLevel = 0;
                byte[] pngNormal = ddsNormal.EncodeToPNG();

                File.WriteAllBytes(path.Replace(".dds", ".png"), pngNormal);

                AssetDatabase.Refresh();

                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path.Replace(".dds", ".png"));

                importer.textureType = TextureImporterType.NormalMap;
                importer.crunchedCompression = true;
                importer.streamingMipmaps = true;
                importer.SaveAndReimport();

                AssetDatabase.Refresh();

                return AssetDatabase.LoadAssetAtPath<Texture2D>(path.Replace(".dds", ".png"));
            }
            catch (Exception e)
            {
                Debug.LogError("Gyre DDS Importer Helper - " + e);
                return null;
            }
        }
    }
}
