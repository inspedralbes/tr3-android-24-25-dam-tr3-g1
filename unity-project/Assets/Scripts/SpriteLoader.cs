using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class SpriteLoader : MonoBehaviour
{
    private string serverUrl = "http://localhost:4000/sprites"; // URL donde están los sprites

    void Start()
    {
        Debug.Log("Iniciando descarga de AssetBundles...");
        StartCoroutine(LoadSpritesFromServer()); // Inicia la descarga cuando el juego comienza
    }

    public IEnumerator LoadSpritesFromServer()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Error al obtener la lista de sprites: " + request.error);
                yield break;
            }

            string[] spritePaths = JsonUtility.FromJson<SpriteList>(request.downloadHandler.text).sprites;

            foreach (string path in spritePaths)
            {
                StartCoroutine(DownloadAndProcessSprite(path));
            }
        }
    }

    IEnumerator DownloadAndProcessSprite(string url)
    {
        using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            yield return request.SendWebRequest();

            Debug.Log($"Descargando {url}");

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ Error al descargar {url}: {request.error}");
                yield break;
            }

            UnityEngine.AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
            if (bundle == null)
            {
                Debug.LogError($"⚠️ No se pudo cargar el AssetBundle desde {url}");
                yield break;
            }

            string spriteName = Path.GetFileName(url);
            (int cols, int rows) = GetSpriteGrid(spriteName);

            ProcessSprite(bundle, spriteName, cols, rows);

            bundle.Unload(false);
        }
    }

    static (int cols, int rows) GetSpriteGrid(string fileName)
    {
        // Lógica para determinar columnas y filas dependiendo del archivo
        if (fileName == "backslash_128.png" || fileName == "backslash.png") return (13, 4);
        if (fileName == "halfslash_128.png" || fileName == "halfslash.png") return (6, 4);
        if (fileName == "slash_128.png" || fileName == "slash_oversize.png" || fileName == "slash_reverse_oversize.png" || fileName == "slash.png") return (6, 4);
        if (fileName == "thrust_oversize.png" || fileName == "thrust.png") return (8, 4);
        if (fileName == "tool_rod.png") return (13, 4);
        if (fileName == "tool_whip.png") return (8, 4);
        if (fileName == "walk_128.png" || fileName == "walk.png") return (9, 4);
        if (fileName == "wheelchair.png") return (2, 4);
        if (fileName == "climb.png") return (6, 1);
        if (fileName == "combat_idle.png") return (2, 4);
        if (fileName == "emote.png") return (3, 4);
        if (fileName == "hurt.png") return (1, 6);
        if (fileName == "idle.png") return (2, 4);
        if (fileName == "jump.png") return (5, 4);
        if (fileName == "run.png") return (8, 4);
        if (fileName == "shoot.png") return (13, 4);
        if (fileName == "sit.png") return (3, 4);
        if (fileName == "spellcast.png") return (7, 4);
        return (1, 1); // Valor por defecto
    }

    void ProcessSprite(UnityEngine.AssetBundle bundle, string spriteName, int cols, int rows)
    {
        Sprite sprite = bundle.LoadAsset<Sprite>(spriteName);
        if (sprite == null)
        {
            Debug.LogError($"❌ No se pudo cargar el sprite {spriteName}");
            return;
        }

        Debug.Log($"✅ Procesando {spriteName} con {cols} columnas y {rows} filas");
    }

    [System.Serializable]
    private class SpriteList
    {
        public string[] sprites;
    }
}