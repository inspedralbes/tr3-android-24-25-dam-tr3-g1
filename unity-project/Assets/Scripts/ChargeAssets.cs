using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ChargeAssetsAssetBundleLoader : MonoBehaviour
{
    // URL donde está el AssetBundle
    public string assetBundleURL = "http://lordgrids.dam.inspedralbes.cat:4000/AssetBundles";
    public string prefabName = "LL";

    // Cargar el AssetBundle al iniciar
    void Start()
    {
        StartCoroutine(LoadAssetBundle(assetBundleURL + "/" + prefabName));
    }

    // Función para cargar el AssetBundle desde la URL
    IEnumerator LoadAssetBundle(string url)
    {
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url);
        yield return www.SendWebRequest();

        // Verificar si hubo error en la carga
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al cargar AssetBundle: " + www.error);
        }
        else
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);

            // Intentar cargar el prefab del AssetBundle
            if (bundle.isStreamedSceneAssetBundle)
            {
                Debug.LogError("Este AssetBundle es un AssetBundle de escenas, no se pueden cargar prefabs.");
            }
            else
            {
                // Cargar el prefab desde el AssetBundle
                GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);

                if (prefab != null)
                {
                    // Instanciar el prefab cargado en la escena
                    Instantiate(prefab);
                    Debug.Log("Prefab cargado exitosamente: " + prefabName);
                }
                else
                {
                    Debug.LogError("No se encontró el prefab con el nombre: " + prefabName);
                }
            }

            // Descartar el AssetBundle para liberar memoria
            bundle.Unload(false);
        }
    }
}
