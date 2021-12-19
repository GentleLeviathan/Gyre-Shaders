using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GentleShaders.Gyre
{
    [CanEditMultipleObjects]
    public class GyreElevenEditor : ShaderGUI
    {
        private bool instanced;
        private bool draw = true;
        private bool performedUpdateCheck;
        private bool updateReady;
        private Texture2D header;

        private MaterialProperty mainTex;
        private MaterialProperty cc;
        private MaterialProperty wearMask;
        private MaterialProperty normal;

        private MaterialProperty primaryColor;
        private MaterialProperty secondaryColor;
        private MaterialProperty uPrimaryColor;
        private MaterialProperty uSecondaryColor;
        private MaterialProperty uTertiaryColor;
        private MaterialProperty illuminationColor;

        private MaterialProperty metallic;
        private MaterialProperty roughness;

        private MaterialProperty edgeWearColor;
        private MaterialProperty dirtColor;
        private MaterialProperty grungeColor;

        private MaterialProperty edgeWearStrength;
        private MaterialProperty dirtStrength;
        private MaterialProperty grungeStrength;
        private MaterialProperty grungeMagnitude;

        private void GetProperties(MaterialProperty[] props)
        {
            mainTex = ShaderGUI.FindProperty("_MainTex", props);
            cc = ShaderGUI.FindProperty("_CC", props);
            wearMask = ShaderGUI.FindProperty("_WearMask", props);
            normal = ShaderGUI.FindProperty("_BumpMap", props);

            primaryColor = ShaderGUI.FindProperty("_Color", props);
            secondaryColor = ShaderGUI.FindProperty("_SecondaryColor", props);
            uPrimaryColor = ShaderGUI.FindProperty("_UPrimaryColor", props);
            uSecondaryColor = ShaderGUI.FindProperty("_USecondaryColor", props);
            uTertiaryColor = ShaderGUI.FindProperty("_UTertiaryColor", props);
            illuminationColor = ShaderGUI.FindProperty("_IllumColor", props);

            metallic = ShaderGUI.FindProperty("_Metallic", props);
            roughness = ShaderGUI.FindProperty("_Roughness", props);

            edgeWearColor = ShaderGUI.FindProperty("_EdgeWearColor", props);
            dirtColor = ShaderGUI.FindProperty("_DirtColor", props);
            grungeColor = ShaderGUI.FindProperty("_GrungeColor", props);

            edgeWearStrength = ShaderGUI.FindProperty("_EdgeWear", props);
            dirtStrength = ShaderGUI.FindProperty("_Dirt", props);
            grungeStrength = ShaderGUI.FindProperty("_Grunge", props);
            grungeMagnitude = ShaderGUI.FindProperty("_GrungeMagnitude", props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (!draw) { return; }
            #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            if (!performedUpdateCheck) { PerformUpdateCheck(); }
            #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            Material target = materialEditor.target as Material;

            try
            {
                GetProperties(properties);
            }
            catch (NullReferenceException e)
            {
                Debug.LogError("GyreElevenEditor: " + e.Message);
                draw = false;
                return;
            }

            DisplayGUI(materialEditor, target);
        }

        private void DisplayGUI(MaterialEditor editor, Material mat)
        {
            UpdateGUI();

            DrawMaterialSlot(mat);

            DrawHeader();

            EditorGUI.BeginChangeCheck();

            DrawTextures(editor, mat);

            DrawProperties(editor);

            EditorGUI.EndChangeCheck();

            ApplyKeywords(mat);
        }

        private async Task PerformUpdateCheck()
        {
            performedUpdateCheck = true;
            updateReady = await GyreUpdateChecker.CheckForUpdates();
        }

        private void UpdateGUI()
        {
            if (!updateReady) { return; }

            GUILayout.Label("An update is available!", EditorStyles.boldLabel);
            if (GUILayout.Button(new GUIContent("Open Repository", "Opens the Gyre Shaders GitHub Repository")))
            {
                GyreUpdateChecker.OpenRepository();
            }
        }

        private void DrawMaterialSlot(Material mat)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Editing Material: "));
            GUILayout.FlexibleSpace();
            EditorGUILayout.ObjectField(mat, typeof(Material), false);
            GUILayout.EndHorizontal();
        }

        private void GetHeaderImage()
        {
            header = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/GentleShaders/Gyre/Editor/Assets/Gyre_Eleven_Header.png", typeof(Texture2D)) ?? new Texture2D(1, 1);
        }

        private void DrawHeader()
        {
            if (!header) { GetHeaderImage(); }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(header, GUILayout.MinWidth(350), GUILayout.MaxWidth(525), GUILayout.MinHeight(40), GUILayout.MaxHeight(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawTextures(MaterialEditor editor, Material mat)
        {
            //Textures
            GUILayout.Space(4f);
            GUILayout.Label("Main Textures", EditorStyles.boldLabel);
            if (editor.TextureProperty(mainTex, "ASG Control (RGBA)", false)?.graphicsFormat == (UnityEngine.Experimental.Rendering.GraphicsFormat)87)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("DDS Texture Detected", "DDS textures are not supported by Unity's importer. It is strongly recommended to use the helper below to auto-fix."), EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reimport as PNG DXT5-Crunched"))
                {
                    mat.SetTexture("_MainTex", Gyre.Helpers.GyreDDSImporter.DDSTextureImport(mat.GetTexture("_MainTex")));
                }
                GUILayout.EndHorizontal();
                DrawDivider();
            }

            if (editor.TextureProperty(cc, "Color Control (RGB)", false)?.graphicsFormat == (UnityEngine.Experimental.Rendering.GraphicsFormat)87)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("DDS Texture Detected", "DDS textures are not supported by Unity's importer. It is strongly recommended to use the helper below to auto-fix."), EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reimport as PNG DXT5-Crunched"))
                {
                    mat.SetTexture("_CC", Gyre.Helpers.GyreDDSImporter.DDSTextureImport(mat.GetTexture("_CC")));
                }
                GUILayout.EndHorizontal();
                DrawDivider();
            }

            if (editor.TextureProperty(wearMask, "Wear Mask (RGB)", false)?.graphicsFormat == (UnityEngine.Experimental.Rendering.GraphicsFormat)87)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("DDS Texture Detected", "DDS textures are not supported by Unity's importer. It is strongly recommended to use the helper below to auto-fix."), EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reimport as PNG DXT5-Crunched"))
                {
                    mat.SetTexture("_WearMask", Gyre.Helpers.GyreDDSImporter.DDSTextureImport(mat.GetTexture("_WearMask")));
                }
                GUILayout.EndHorizontal();
                DrawDivider();
            }

            if (editor.TextureProperty(normal, "Normal Map (DirectX)", false)?.graphicsFormat == (UnityEngine.Experimental.Rendering.GraphicsFormat)87)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Error: DDS Normal Map Detected", "DDS textures are not* supported by Unity for normal maps, use the helper below to auto-fix. \n\n*DirectDraw textures not in the DXT5_NM/BC5 format are not supported by Unity."), EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reimport as PNG Normal Map"))
                {
                    mat.SetTexture("_BumpMap", Gyre.Helpers.GyreDDSImporter.DDSNormalMapImport(mat.GetTexture("_BumpMap")));
                }
                GUILayout.EndHorizontal();
                DrawDivider();
            }
            editor.TextureCompatibilityWarning(normal);
            editor.TextureScaleOffsetProperty(mainTex);
        }

        private void DrawProperties(MaterialEditor editor)
        {
            bool flag = (wearMask.textureValue);

            GUILayout.Space(8f);
            GUILayout.Label(new GUIContent("Main Properties", "Scalars for the calculated lighting values"), EditorStyles.boldLabel);
            editor.RangeProperty(metallic, "Metallic");
            editor.RangeProperty(roughness, "Roughness");
            DrawDivider();

            GUILayout.Space(8f);
            GUILayout.Label(new GUIContent("Main Colors", "These are the colors assigned to the primary zones."), EditorStyles.boldLabel);
            editor.ColorProperty(primaryColor, "Primary Color (ASG-R)");
            editor.ColorProperty(secondaryColor, "Secondary Color (ASG-G)");
            if (flag)
            {
                editor.ColorProperty(illuminationColor, "Illumination Color (HDR)");
            }
            DrawDivider();

            GUILayout.Space(8f);
            GUILayout.Label(new GUIContent("Undersuit Colors", "These are the colors assigned to the secondary zones."), EditorStyles.boldLabel);
            editor.ColorProperty(uPrimaryColor, "Undersuit Primary (CC-G)");
            editor.ColorProperty(uSecondaryColor, "Undersuit Secondary (CC-B)");
            if (flag)
            {
                editor.ColorProperty(uTertiaryColor, "Undersuit Tertiary (WM-R)");
            }
            DrawDivider();

            GUILayout.Space(8f);
            GUILayout.Label(new GUIContent("Edge Wear", "Controls for the 'dynamic' edge wear."), EditorStyles.boldLabel);
            editor.ColorProperty(edgeWearColor, "Edge Wear Color");
            editor.RangeProperty(edgeWearStrength, "Edge Wear Strength");
            DrawDivider();

            GUILayout.Space(8f);
            GUILayout.Label(new GUIContent("Dirt", "Controls for the 'dynamic' dirt effect."), EditorStyles.boldLabel);
            editor.ColorProperty(dirtColor, "Dirt Color");
            editor.RangeProperty(dirtStrength, "Dirt Strength");

            if (flag)
            {
                DrawDivider();
                GUILayout.Space(8f);
                GUILayout.Label(new GUIContent("Grunge", "Controls for the 'dynamic' grunge effect."), EditorStyles.boldLabel);
                editor.ColorProperty(grungeColor, "Grunge Color");
                editor.RangeProperty(grungeStrength, "Grunge Strength");
                editor.RangeProperty(grungeMagnitude, "Grunge Magnitude");
            }

            DrawDivider();
            GUILayout.Space(8f);
            instanced = editor.EnableInstancingField();

            GUILayout.Space(40f);
        }

        private void DrawDivider()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void ApplyKeywords(Material mat)
        {
            if (wearMask.textureValue)
            {
                mat.EnableKeyword("_WEARMASK");
            }
            else
            {
                mat.DisableKeyword("_WEARMASK");
            }

            mat.enableInstancing = instanced;
        }
    }
}
