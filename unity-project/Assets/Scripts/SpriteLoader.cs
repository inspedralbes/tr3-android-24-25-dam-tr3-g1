﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;
using UnityEditor.Animations;
using System.Linq;

public class SpriteLoader : MonoBehaviour
{
    private string spriteServerUrl = "http://localhost:4000/sprites";
    private string assetBundlePath = "Assets/AssetBundles/";
    //private string assetBundleUrl = "http://localhost:4000/assetbundles/";
    //private string localCachePath = Application.persistentDataPath + "/AssetBundles/";

    void Start()
    {
        StartCoroutine(LoadSpritesFromServer());
    }

    IEnumerator LoadSpritesFromServer()
    {
        Debug.Log("🔄 Cargando sprites...");
        using (UnityWebRequest request = UnityWebRequest.Get(spriteServerUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Error al obtener la lista de sprites: " + request.error);
                yield break;
            }

            Debug.Log(request.downloadHandler.text);

            SpriteListWrapper spriteListWrapper = JsonUtility.FromJson<SpriteListWrapper>(request.downloadHandler.text);
            if (spriteListWrapper == null || spriteListWrapper.spriteLists == null)
            {
                Debug.LogError("❌ Error al parsear la lista de sprites.");
                yield break;
            }

            foreach (var spriteList in spriteListWrapper.spriteLists)
            {
                string[] spritePaths = spriteList.rutas;
                Dictionary<string, List<string>> folderSprites = new Dictionary<string, List<string>>();

                string folderName = spriteList.name;
                CreateFolderSprites(folderName);
                CreateFolderAnimation(folderName);

                foreach (string spritePath in spritePaths)
                {
                    yield return StartCoroutine(DownloadSprite(spritePath, folderName));
                }
                string folderPath = Path.Combine("Assets/Sprites", folderName);
                GenerateCombinedSprite(folderPath);
                ProcessFolder(folderPath);
                ProcessAnimationFolder(folderPath, folderName);
            }
        }
    }

    IEnumerator DownloadSprite(string url, string folderName)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Error al descargar el sprite: " + request.error);
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            byte[] bytes = texture.EncodeToPNG();
            string fileName = Path.GetFileName(url);
            string filePath = Path.Combine("Assets/Sprites", folderName, fileName);

            File.WriteAllBytes(filePath, bytes);
            Debug.Log($"✅ Sprite descargado y guardado en: {filePath}");
        }
    }

    static int ProcessFolder(string folder)
    {
        int processedSprites = 0;

        string[] files = AssetDatabase.FindAssets("t:Texture2D", new[] { folder })
                            .Select(AssetDatabase.GUIDToAssetPath)
                            .Where(filePath => filePath.EndsWith(".png"))
                            .ToArray();

        if (files.Length == 0)
            Debug.Log($"⚠️ No se encontraron imágenes en: {folder}");

        foreach (string file in files)
        {
            Debug.Log($"🎨 Sprite encontrado: {file}");

            // Lógica especial de recorte para ciertos archivos en "custom"
            if (file.EndsWith("backslash_128.png"))
            {
                ProcessCustomSprite(file, 13, 4); // Recorte personalizado (13 columnas x 4 filas)
            }
            else if (file.EndsWith("halfslash_128.png"))
            {
                ProcessCustomSprite(file, 6, 4); // Recorte personalizado (6 columnas x 4 filas)
            }
            else if (file.EndsWith("slash_128.png"))
            {
                ProcessCustomSprite(file, 6, 4); // Recorte personalizado (6 columnas x 4 filas)
            }
            else if (file.EndsWith("slash_oversize.png"))
            {
                ProcessCustomSprite(file, 6, 4); // Recorte personalizado (6 columnas x 4 filas)
            }
            else if (file.EndsWith("slash_reverse_oversize.png"))
            {
                ProcessCustomSprite(file, 6, 4); // Recorte personalizado (6 columnas x 4 filas)
            }
            else if (file.EndsWith("thrust_oversize.png"))
            {
                ProcessCustomSprite(file, 8, 4); // Recorte personalizado (8 columnas x 4 filas)
            }
            else if (file.EndsWith("tool_rod.png"))
            {
                ProcessCustomSprite(file, 13, 4); // Recorte personalizado (13 columnas x 4 filas)
            }
            else if (file.EndsWith("tool_whip.png"))
            {
                ProcessCustomSprite(file, 8, 4); // Recorte personalizado (8 columnas x 4 filas)
            }
            else if (file.EndsWith("walk_128.png"))
            {
                ProcessCustomSprite(file, 9, 4); // Recorte personalizado (9 columnas x 4 filas)
            }
            else if (file.EndsWith("wheelchair.png"))
            {
                ProcessCustomSprite(file, 2, 4); // Recorte personalizado (2 columnas x 4 filas)
            }
            // Lógica especial de recorte para ciertos archivos en "standard"
            else if (file.EndsWith("backslash.png"))
            {
                ProcessCustomSprite(file, 13, 4); // Recorte personalizado (13 columnas x 4 filas)
            }
            else if (file.EndsWith("climb.png"))
            {
                ProcessCustomSprite(file, 6, 1); // Recorte personalizado (6 columnas x 1 filas)
            }
            else if (file.EndsWith("combat_idle.png"))
            {
                ProcessCustomSprite(file, 2, 4); // Recorte personalizado (2 columnas x 4 filas)
            }
            else if (file.EndsWith("emote.png"))
            {
                ProcessCustomSprite(file, 3, 4); // Recorte personalizado (3 columnas x 4 filas)
            }
            else if (file.EndsWith("halfslash.png"))
            {
                ProcessCustomSprite(file, 6, 4); // Recorte personalizado (6 columnas x 4 filas)
            }
            else if (file.EndsWith("hurt.png"))
            {
                ProcessCustomSprite(file, 1, 6); // Recorte personalizado (1 columnas x 6 filas)
            }
            else if (file.EndsWith("idle.png"))
            {
                ProcessCustomSprite(file, 2, 4); // Recorte personalizado (2 columnas x 4 filas)
            }
            else if (file.EndsWith("jump.png"))
            {
                ProcessCustomSprite(file, 5, 4); // Recorte personalizado (5 columnas x 4 filas)
            }
            else if (file.EndsWith("run.png"))
            {
                ProcessCustomSprite(file, 8, 4); // Recorte personalizado (8 columnas x 4 filas)
            }
            else if (file.EndsWith("shoot.png"))
            {
                ProcessCustomSprite(file, 13, 4); // Recorte personalizado (2 columnas x 4 filas)
            }
            else if (file.EndsWith("sit.png"))
            {
                ProcessCustomSprite(file, 3, 4); // Recorte personalizado (3 columnas x 4 filas)
            }
            else if (file.EndsWith("slash.png"))
            {
                ProcessCustomSprite(file, 6, 4); // Recorte personalizado (6 columnas x 4 filas)
            }
            else if (file.EndsWith("spellcast.png"))
            {
                ProcessCustomSprite(file, 7, 4); // Recorte personalizado (7 columnas x 4 filas)
            }
            else if (file.EndsWith("thrust.png"))
            {
                ProcessCustomSprite(file, 8, 4); // Recorte personalizado (8 columnas x 4 filas)
            }
            else if (file.EndsWith("walk.png"))
            {
                ProcessCustomSprite(file, 9, 4); // Recorte personalizado (2 columnas x 4 filas)
            }
            processedSprites++;
        }

        return processedSprites;
    }

    // Recorte personalizado (para "standard" y "custom")
    static void ProcessCustomSprite(string assetPath, int cols, int rows)
    {
        ProcessSprite(assetPath, cols, rows);
    }

    // Recorte estándar o personalizado
    static void ProcessSprite(string assetPath, int cols, int rows)
    {

        int pixelsPerUnit = 64;

        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (importer == null)
        {
            Debug.LogWarning($"⚠️ No se pudo cargar el sprite: {assetPath}");
            return;
        }

        Debug.Log($"🛠️ Modificando sprite: {assetPath}");

        // Aplicar configuración
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple; // Modo múltiple para recorte en grilla
        importer.spritePixelsPerUnit = pixelsPerUnit;

        // Establecer el recorte con el número de columnas y filas específicos
        importer.spritesheet = GenerateGridSlices(importer, cols, rows);

        // Forzar reimportación
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        Debug.Log($"✅ Sprite modificado y reimportado: {assetPath}");
    }

    // Recorte utilizando el número de columnas y filas específicos
    static SpriteMetaData[] GenerateGridSlices(TextureImporter importer, int cols, int rows)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(importer.assetPath);
        if (texture == null)
        {
            Debug.LogError($"❌ No se pudo cargar la textura: {importer.assetPath}");
            return new SpriteMetaData[0];
        }

        Debug.Log($"📏 Grid aplicado: {cols} columnas, {rows} filas en {importer.assetPath}");

        SpriteMetaData[] sprites = new SpriteMetaData[cols * rows];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int index = y * cols + x;
                sprites[index] = new SpriteMetaData
                {
                    rect = new Rect(x * (texture.width / cols), texture.height - (y + 1) * (texture.height / rows), texture.width / cols, texture.height / rows),
                    name = Path.GetFileNameWithoutExtension(importer.assetPath) + "_" + index,
                    alignment = (int)SpriteAlignment.Center
                };
            }
        }

        return sprites;
    }

    static void GenerateCombinedSprite(string folder)
    {
        AssetDatabase.Refresh();

        string[] spritePaths = AssetDatabase.FindAssets("t:Texture2D", new[] { folder })
                                            .Select(AssetDatabase.GUIDToAssetPath)
                                            .Where(filePath => filePath.EndsWith("walk_128.png"))
                                            .ToArray();

        if (spritePaths.Length == 0)
        {
            Debug.LogWarning("⚠️ No se encontró el sprite walk_128.png.");
            return;
        }

        string walkSpritePath = spritePaths[0];
        TextureImporter importer = AssetImporter.GetAtPath(walkSpritePath) as TextureImporter;

        if (importer == null)
        {
            Debug.LogError($"❌ No se pudo cargar el importador de la textura: {walkSpritePath}");
            return;
        }

        // Asegurarse de que la textura sea legible
        importer.isReadable = true;
        importer.SaveAndReimport();

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(walkSpritePath).OfType<Sprite>().ToArray();

        if (sprites.Length < 20)
        {
            Debug.LogError("❌ No se encontraron suficientes sprites en walk_128.png.");
            return;
        }

        // Obtener los sprites
        Sprite sprite21 = sprites[21];
        Sprite sprite25 = sprites[25];

        int combinedWidth = (int)Mathf.Max(sprite21.rect.width, sprite25.rect.width);
        int combinedHeight = (int)(sprite21.rect.height + sprite25.rect.height);

        Texture2D combinedTexture = new Texture2D(combinedWidth, combinedHeight, TextureFormat.RGBA32, false);

        // Rellenar con transparente primero
        Color[] transparentPixels = new Color[combinedWidth * combinedHeight];
        for (int i = 0; i < transparentPixels.Length; i++)
        {
            transparentPixels[i] = Color.clear;
        }
        combinedTexture.SetPixels(transparentPixels);

        // Copiar el primer sprite (parte superior)
        combinedTexture.SetPixels(
            0,
            0,
            (int)sprite21.rect.width,
            (int)sprite21.rect.height,
            sprite21.texture.GetPixels(
                (int)sprite21.rect.x,
                (int)sprite21.rect.y,
                (int)sprite21.rect.width,
                (int)sprite21.rect.height
            )
        );

        // Copiar el segundo sprite (parte inferior)
        combinedTexture.SetPixels(
            0,
            (int)sprite21.rect.height,
            (int)sprite25.rect.width,
            (int)sprite25.rect.height,
            sprite25.texture.GetPixels(
                (int)sprite25.rect.x,
                (int)sprite25.rect.y,
                (int)sprite25.rect.width,
                (int)sprite25.rect.height
            )
        );

        combinedTexture.Apply();

        byte[] bytes = combinedTexture.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(folder, "idle_custom.png"), bytes);

        ProcessCustomSprite(Path.Combine(folder, "idle_custom.png"), 1, 2);

        AssetDatabase.Refresh();
        Debug.Log("✅ Nuevo sprite combinado generado: idle_custom.png");
    }

    static void ProcessAnimationFolder(string folderPath, string folderName)
    {
        if (!Directory.Exists(folderPath)) return;

            string[] spriteFiles = Directory.GetFiles(folderPath, "*.png");

            foreach (string spriteFile in spriteFiles)
            {
                string fileName = Path.GetFileName(spriteFile);
                Debug.Log($"Procesando archivo de sprite: {fileName} en carpeta: {folderPath}");
                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteFile).OfType<Sprite>().ToArray();

                if (sprites.Length == 0)
                {
                    Debug.LogWarning($"No se encontraron sprites en el archivo: {fileName}");
                    continue;
                }
                //Custom
                if (fileName.Contains("backslash_128.png"))
                {
                    CreateAnimationClip(sprites, folderName, "blackslash_Up_Left", 0, 6);
                    CreateAnimationClip(sprites, folderName, "blackslash_Up_Right", 7, 12);
                    CreateAnimationClip(sprites, folderName, "blackslash_Left_Left", 13, 19);
                    CreateAnimationClip(sprites, folderName, "blackslash_Left_Right", 20, 26);
                    CreateAnimationClip(sprites, folderName, "blackslash_Down_Left", 27, 32);
                    CreateAnimationClip(sprites, folderName, "blackslash_Down_Right", 34, 40);
                    CreateAnimationClip(sprites, folderName, "blackslash_Right_Left", 41, 47);
                    CreateAnimationClip(sprites, folderName, "blackslash_Right_Right", 48, 53);

                }
                else if (fileName.Contains("halfslash_128.png"))
                {
                    CreateAnimationClip(sprites, folderName, "halfslash_Up", 0, 5);
                    CreateAnimationClip(sprites, folderName, "halfslash_Left", 6, 11);
                    CreateAnimationClip(sprites, folderName, "halfslash_Down", 12, 17);
                    CreateAnimationClip(sprites, folderName, "halfslash_Right", 18, 23);
                }
                else if (fileName.Contains("slash_128.png"))
                {
                    CreateAnimationClip(sprites, folderName, "slash_Up", 0, 5);
                    CreateAnimationClip(sprites, folderName, "slash_Left", 6, 11);
                    CreateAnimationClip(sprites, folderName, "slash_Down", 12, 17);
                    CreateAnimationClip(sprites, folderName, "slash_Right", 18, 23);
                }
                else if (fileName.Contains("slash_oversize.png"))
                {
                    CreateAnimationClip(sprites, folderName, "slash_oversize_Up", 0, 5);
                    CreateAnimationClip(sprites, folderName, "slash_oversize_Left", 6, 11);
                    CreateAnimationClip(sprites, folderName, "slash_oversize_Down", 12, 17);
                    CreateAnimationClip(sprites, folderName, "slash_oversize_Right", 18, 23);
                }
                else if (fileName.Contains("slash_reverse_oversize.png"))
                {
                    CreateAnimationClip(sprites, folderName, "slash_reverse_oversize_Up", 0, 5);
                    CreateAnimationClip(sprites, folderName, "slash_reverse_oversize_Left", 6, 11);
                    CreateAnimationClip(sprites, folderName, "slash_reverse_oversize_Down", 12, 17);
                    CreateAnimationClip(sprites, folderName, "slash_reverse_oversize_Right", 18, 23);
                }
                else if (fileName.Contains("thrust_oversize.png"))
                {
                    CreateAnimationClip(sprites, folderName, "thrust_oversize_Up", 0, 7);
                    CreateAnimationClip(sprites, folderName, "thrust_oversize_Left", 8, 15);
                    CreateAnimationClip(sprites, folderName, "thrust_oversize_Down", 16, 23);
                    CreateAnimationClip(sprites, folderName, "thrust_oversize_Right", 24, 31);
                }
                else if (fileName.Contains("tool_rod.png"))
                {
                    CreateAnimationClip(sprites, folderName, "tool_rod_Up", 0, 12);
                    CreateAnimationClip(sprites, folderName, "tool_rod_Left", 13, 25);
                    CreateAnimationClip(sprites, folderName, "tool_rod_Down", 26, 38);
                    CreateAnimationClip(sprites, folderName, "tool_rod_Right", 39, 51);
                }
                else if (fileName.Contains("tool_whip.png"))
                {
                    CreateAnimationClip(sprites, folderName, "tool_whip_Up", 0, 7);
                    CreateAnimationClip(sprites, folderName, "tool_whip_Left", 8, 15);
                    CreateAnimationClip(sprites, folderName, "tool_whip_Down", 16, 23);
                    CreateAnimationClip(sprites, folderName, "tool_whip_Right", 24, 31);
                }
                else if (fileName.Contains("walk_128.png"))
                {
                    CreateAnimationClip(sprites, folderName, "walk_Up", 0, 8);
                    CreateAnimationClip(sprites, folderName, "walk_Left", 9, 17);
                    CreateAnimationClip(sprites, folderName, "walk_Down", 18, 26);
                    CreateAnimationClip(sprites, folderName, "walk_Right", 27, 35);
                }
                else if (fileName.Contains("wheelchair.png"))
                {
                    CreateAnimationClip(sprites, folderName, "wheelchair_Up", 0, 1);
                    CreateAnimationClip(sprites, folderName, "wheelchair_Left", 2, 3);
                    CreateAnimationClip(sprites, folderName, "wheelchair_Down", 4, 5);
                    CreateAnimationClip(sprites, folderName, "wheelchair_Right", 6, 7);
                }
                else if (fileName.Contains("idle_custom.png"))
                {
                    CreateAnimationClip(sprites, folderName, "idle_custom", 0, 1);
                }
                //Standard
                else if (fileName.Contains("backslash.png"))
                {
                    CreateAnimationClip(sprites, folderName, "blackslash_standard_Up_Left", 0, 6);
                    CreateAnimationClip(sprites, folderName, "blackslash_standard_Up_Right", 7, 12);
                    CreateAnimationClip(sprites, folderName, "blackslash_standard_Left_Left", 13, 19);
                    CreateAnimationClip(sprites, folderName, "blackslash_standard_Left_Right", 20, 26);
                    CreateAnimationClip(sprites, folderName, "blackslash_standard_Down_Left", 27, 33);
                    CreateAnimationClip(sprites, folderName, "blackslash_standard_Down_Right", 34, 40);
                    CreateAnimationClip(sprites, folderName, "blackslash_standard_Right_Left", 41, 47);
                    CreateAnimationClip(sprites, folderName, "blackslash_standard_Right_Right", 48, 53);

                }
                else if (fileName.Contains("climb.png"))
                {
                    CreateAnimationClip(sprites, folderName, "climb_standard", 0, 5);

                }
                else if (fileName.Contains("combat_idle.png"))
                {
                    CreateAnimationClip(sprites, folderName, "combat_idle_standard_Up", 0, 1);
                    CreateAnimationClip(sprites, folderName, "combat_idle_standard_Left", 2, 3);
                    CreateAnimationClip(sprites, folderName, "combat_idle_standard_Down", 4, 5);
                    CreateAnimationClip(sprites, folderName, "combat_idle_standard_Right", 6, 7);
                }
                else if (fileName.Contains("emote.png"))
                {
                    CreateAnimationClip(sprites, folderName, "emote_standard_Up", 0, 2);
                    CreateAnimationClip(sprites, folderName, "emote_standard_Left", 3, 5);
                    CreateAnimationClip(sprites, folderName, "emote_standard_Down", 6, 8);
                    CreateAnimationClip(sprites, folderName, "emote_standard_Right", 9, 11);
                }
                else if (fileName.Contains("halfslash.png"))
                {
                    CreateAnimationClip(sprites, folderName, "halfslash_standard_Up", 0, 5);
                    CreateAnimationClip(sprites, folderName, "halfslash_standard_Left", 6, 11);
                    CreateAnimationClip(sprites, folderName, "halfslash_standard_Down", 12, 17);
                    CreateAnimationClip(sprites, folderName, "halfslash_standard_Right", 18, 23);
                }
                else if (fileName.Contains("hurt.png"))
                {
                    CreateAnimationClip(sprites, folderName, "hurt_standard", 0, 5);

                }
                else if (fileName.Contains("idle.png"))
                {
                    CreateAnimationClip(sprites, folderName, "idle_standard_Up", 0, 1);
                    CreateAnimationClip(sprites, folderName, "idle_standard_Left", 2, 3);
                    CreateAnimationClip(sprites, folderName, "idle_standard_Down", 4, 5);
                    CreateAnimationClip(sprites, folderName, "idle_standard_Right", 6, 7);
                }
                else if (fileName.Contains("jump.png"))
                {
                    CreateAnimationClip(sprites, folderName, "jump_standard_Up", 0, 4);
                    CreateAnimationClip(sprites, folderName, "jump_standard_Left", 5, 9);
                    CreateAnimationClip(sprites, folderName, "jump_standard_Down", 10, 14);
                    CreateAnimationClip(sprites, folderName, "jump_standard_Right", 15, 19);
                }
                else if (fileName.Contains("run.png"))
                {
                    CreateAnimationClip(sprites, folderName, "run_standard_Up", 0, 7);
                    CreateAnimationClip(sprites, folderName, "run_standard_Left", 8, 15);
                    CreateAnimationClip(sprites, folderName, "run_standard_Down", 16, 23);
                    CreateAnimationClip(sprites, folderName, "run_standard_Right", 24, 31);
                }
                else if (fileName.Contains("shoot.png"))
                {
                    CreateAnimationClip(sprites, folderName, "shoot_standard_Up", 0, 12);
                    CreateAnimationClip(sprites, folderName, "shoot_standard_Left", 13, 25);
                    CreateAnimationClip(sprites, folderName, "shoot_standard_Down", 26, 38);
                    CreateAnimationClip(sprites, folderName, "shoot_standard_Right", 39, 51);
                }
                else if (fileName.Contains("slash.png"))
                {
                    CreateAnimationClip(sprites, folderName, "slash_standard_Up", 0, 5);
                    CreateAnimationClip(sprites, folderName, "slash_standard_Left", 6, 11);
                    CreateAnimationClip(sprites, folderName, "slash_standard_Down", 12, 17);
                    CreateAnimationClip(sprites, folderName, "shoot_standard_Right", 18, 23);
                }

                else if (fileName.Contains("spellcast.png"))
                {
                    CreateAnimationClip(sprites, folderName, "spellcast_standard_Up", 0, 6);
                    CreateAnimationClip(sprites, folderName, "spellcast_standard_Left", 7, 13);
                    CreateAnimationClip(sprites, folderName, "spellcast_standard_Down", 14, 20);
                    CreateAnimationClip(sprites, folderName, "spellcast_standard_Right", 21, 27);
                }
                else if (fileName.Contains("thrust.png"))
                {
                    CreateAnimationClip(sprites, folderName, "thrust_standard_Up", 0, 7);
                    CreateAnimationClip(sprites, folderName, "thrust_standard_Left", 8, 15);
                    CreateAnimationClip(sprites, folderName, "thrust_standard_Down", 16, 23);
                    CreateAnimationClip(sprites, folderName, "thrust_standard_Right", 24, 31);
                }
                else if (fileName.Contains("walk.png"))
                {
                    CreateAnimationClip(sprites, folderName, "walk_standard_Up", 0, 8);
                    CreateAnimationClip(sprites, folderName, "walk_standard_Left", 9, 17);
                    CreateAnimationClip(sprites, folderName, "walk_standard_Down", 18, 26);
                    CreateAnimationClip(sprites, folderName, "walk_standard_Right", 27, 35);
                }
            }
    }
    static void CreateAnimationClip(Sprite[] sprites, string folderName, string animationName, int start, int end)
    {
        // Asegurarse de que los índices estén dentro de los límites del array
        start = Mathf.Clamp(start, 0, sprites.Length - 1);
        end = Mathf.Clamp(end, 0, sprites.Length - 1);

        // Verifica que el rango no sea inválido
        if (start >= end)
        {
            Debug.LogWarning($"Rango inválido para la animación: {animationName} ({start}-{end})");
            return;
        }

        AnimationClip clip = new AnimationClip();
        clip.frameRate = 12; // 12 fotogramas por segundo

        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[end - start + 1];
        for (int i = start; i <= end; i++)
        {
            keyframes[i - start] = new ObjectReferenceKeyframe { time = (i - start) / clip.frameRate, value = sprites[i] };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

        string animationPath = Path.Combine("Assets/Animations", folderName, animationName + ".anim");

        animationPath = CheckAndCorrectPath(animationPath);

        Debug.Log($"Creando animación: {animationPath}");

        AssetDatabase.CreateAsset(clip, animationPath);
        Debug.Log($"Animación generada: {animationPath}");
    }

    public static string CheckAndCorrectPath(string originalPath)
    {
        // Reemplazar las barras invertidas por barras diagonales
        string correctedPath = originalPath.Replace("\\", "/");

        // Verificar si el archivo existe
        if (File.Exists(correctedPath))
        {
            Debug.Log("✅ El archivo existe en la ruta: " + correctedPath);
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró el archivo en la ruta: " + correctedPath);
        }

        return correctedPath;
    }



    void CreateFolderPrefab(string folderName)
    {
        string folderPath = $"Assets/Prefabs/{folderName}";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }

    void CreateFolderSprites(string folderName)
    {
        string folderPath = $"Assets/Sprites/{folderName}";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }
    void CreateFolderAnimation(string folderName)
    {
        string folderPath = $"Assets/Animation/{folderName}";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }


    [System.Serializable]
    private class SpriteList
    {
        public string name;
        public string[] rutas;
    }

    [System.Serializable]
    private class SpriteListWrapper
    {
        public SpriteList[] spriteLists;
    }
}