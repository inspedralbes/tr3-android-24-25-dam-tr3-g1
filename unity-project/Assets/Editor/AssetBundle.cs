using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class AssetBundleUploader : EditorWindow
{
    private static readonly string prefabFolderPath = "Assets/Prefabs";
    private static readonly string assetBundlePath = "Assets/AssetBundles";
    private static readonly string uploadUrl = "http://localhost:4000/AssetBundles";

    [MenuItem("Tools/Build & Upload AssetBundles")]
    public static void BuildAndUpload()
    {
        if (!Directory.Exists(prefabFolderPath))
        {
            Debug.LogError("No se ha encontrado la carpeta de Prefabs en: " + prefabFolderPath);
            return;
        }

        if (!Directory.Exists(assetBundlePath))
            Directory.CreateDirectory(assetBundlePath);

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolderPath });

        foreach (string guid in prefabGUIDs)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            string assetBundleName = Path.GetFileNameWithoutExtension(prefabPath);
            string outputPath = Path.Combine(assetBundlePath, assetBundleName);

            BuildSingleAssetBundle(new[] { prefabPath }, outputPath);
            UploadAssetBundle(outputPath);
        }
    }

    private static void BuildSingleAssetBundle(string[] assetPaths, string outputPath)
    {
        AssetBundleBuild bundleBuild = new AssetBundleBuild
        {
            assetBundleName = Path.GetFileName(outputPath),
            assetNames = assetPaths
        };

        BuildPipeline.BuildAssetBundles(assetBundlePath, new[] { bundleBuild }, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

        Debug.Log("AssetBundle generado: " + outputPath);
    }

    private static async void UploadAssetBundle(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("No se encontró el AssetBundle: " + filePath);
            return;
        }

        byte[] fileData = File.ReadAllBytes(filePath);

        using (HttpClient client = new HttpClient())
        using (MultipartFormDataContent form = new MultipartFormDataContent())
        using (ByteArrayContent fileContent = new ByteArrayContent(fileData))
        {
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            HttpResponseMessage response = await client.PostAsync(uploadUrl, form);

            if (response.IsSuccessStatusCode)
            {
                Debug.Log("AssetBundle subido correctamente: " + filePath);
            }
            else
            {
                Debug.LogError("❌ Error al subir el AssetBundle: " + response.ReasonPhrase);
            }
        }
    }
}
