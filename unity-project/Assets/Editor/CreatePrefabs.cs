using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class AutoAssetBundleCreator : MonoBehaviour
{
    [MenuItem("Tools/Generar AssetBundles Automáticos")]
    static void CrearAssetBundlesPorPrefab()
    {
        // Directorio donde se encuentran los prefabs
        string prefabDirectory = "Assets/Prefabs";

        // Directorio donde se guardarán los AssetBundles
        string assetBundleDirectory = "Assets/AssetBundles";

        // Crear el directorio de AssetBundles si no existe
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        // Obtener todos los prefabs en el directorio
        string[] prefabPaths = Directory.GetFiles(prefabDirectory, "*.prefab", SearchOption.AllDirectories);

        foreach (var prefabPath in prefabPaths)
        {
            // Asegurarnos de que estamos usando rutas de assets que comienzan desde "Assets"
            if (!prefabPath.StartsWith(Application.dataPath))
            {
                Debug.LogError($"Ruta inválida para prefab: {prefabPath}");
                continue;
            }

            // Convertir la ruta para usar barras normales
            string assetPath = "Assets" + prefabPath.Substring(Application.dataPath.Length).Replace("\\", "/");

            // Limpiar el nombre del prefab, eliminando caracteres no válidos
            string prefabName = Path.GetFileNameWithoutExtension(prefabPath);
            prefabName = CleanFileName(prefabName);

            // Cargar el prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                Debug.LogError($"No se pudo cargar el prefab en la ruta: {prefabPath}");
                continue;
            }

            // Obtener todas las dependencias del prefab (sprites, animadores, materiales, animaciones, etc.)
            string[] dependencias = AssetDatabase.GetDependencies(new string[] { assetPath });

            // Asignar cada dependencia al mismo AssetBundle
            string assetBundleName = prefabName.ToLower();  // El nombre del AssetBundle será el nombre del prefab

            foreach (string dependencia in dependencias)
            {
                // Excluir scripts (.cs)
                if (dependencia.EndsWith(".cs"))
                {
                    continue;
                }

                string dependenciaPath = dependencia.Replace("\\", "/");
                if (dependenciaPath.EndsWith(".meta")) continue;  // Ignorar archivos .meta

                // Asignar el nombre del AssetBundle a cada dependencia
                AssetImporter assetImporter = AssetImporter.GetAtPath(dependencia);
                if (assetImporter != null)
                {
                    assetImporter.assetBundleName = assetBundleName;
                    Debug.Log($"Asignado AssetBundle: {dependencia} -> {assetBundleName}");
                }
            }

            // Crear AssetBundle para el prefab y sus dependencias
            Debug.Log($"Creando AssetBundle para el prefab: {prefabName}");
        }

        // Construir todos los AssetBundles
        BuildAssetBundles(assetBundleDirectory);
    }

    // Método para limpiar el nombre de archivo, eliminando caracteres no válidos
    static string CleanFileName(string fileName)
    {
        // Reemplazar cualquier carácter no alfanumérico o especial por un guion bajo
        return Regex.Replace(fileName, @"[^a-zA-Z0-9_]", "_");
    }

    static void BuildAssetBundles(string assetBundleDirectory)
    {
        // Construir AssetBundles para todos los recursos asignados a AssetBundles
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        Debug.Log("AssetBundles creados correctamente.");
    }
}