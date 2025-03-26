using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEngine.U2D.IK;

public class AutoSpriteGridProcessor : EditorWindow
{
    private const string targetFolder = "Assets/Sprites"; // Carpeta base
    private const int pixelsPerUnit = 64; // Pixels Per Unit deseado
    private const int gridCellSize = 64; // Tamaño de la celda para "standard" (64x64)

    [MenuItem("Tools/Auto Slice (Grid) Sprites")]
    static void ProcessSprites()
    {
        // Buscar todas las carpetas dentro de "Sprites", incluidas las subcarpetas
        string[] allFolders = AssetDatabase.GetAllAssetPaths()
                                            .Where(path => Directory.Exists(path) && path.StartsWith("Assets/Sprites"))
                                            .Distinct()
                                            .ToArray();

        int processedFolders = 0;
        int processedSprites = 0;

        Debug.Log("🔍 Buscando carpetas dentro de: " + targetFolder);

        foreach (string folder in allFolders)
        {
            Debug.Log($"📁 Carpeta detectada: {folder}");

            // Procesar carpeta "standard" y "custom" usando el sistema de cell count
            if (folder.EndsWith("/standard") || folder.EndsWith("/custom"))
            {
                Debug.Log($"✅ Procesando carpeta: {folder}");
                processedSprites += ProcessFolder(folder);
                GenerateCombinedSprite(folder);
                processedFolders++;
            }
        }

        AssetDatabase.Refresh();

        Debug.Log($"✅ Proceso finalizado. Carpetas procesadas: {processedFolders}, Sprites modificados: {processedSprites}");
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
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(walkSpritePath).OfType<Sprite>().ToArray();

        if (sprites.Length < 20)
        {
            Debug.LogError("❌ No se encontraron suficientes sprites en walk_128.png.");
            return;
        }

        // Obtener los sprites
        Sprite sprite18 = sprites[21];
        Sprite sprite19 = sprites[25];

        int combinedWidth = (int)Mathf.Max(sprite18.rect.width, sprite19.rect.width);
        int combinedHeight = (int)(sprite18.rect.height + sprite19.rect.height);

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
            (int)sprite18.rect.width,
            (int)sprite18.rect.height,
            sprite18.texture.GetPixels(
                (int)sprite18.rect.x,
                (int)sprite18.rect.y,
                (int)sprite18.rect.width,
                (int)sprite18.rect.height
            )
        );

        // Copiar el segundo sprite (parte inferior)
        combinedTexture.SetPixels(
            0,
            (int)sprite18.rect.height,
            (int)sprite19.rect.width,
            (int)sprite19.rect.height,
            sprite19.texture.GetPixels(
                (int)sprite19.rect.x,
                (int)sprite19.rect.y,
                (int)sprite19.rect.width,
                (int)sprite19.rect.height
            )
        );

        combinedTexture.Apply();

        byte[] bytes = combinedTexture.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(folder, "idle_custom.png"), bytes);

        ProcessCustomSprite(Path.Combine(folder, "idle_custom.png"), 1, 2);

        AssetDatabase.Refresh();
        Debug.Log("✅ Nuevo sprite combinado generado: idle_custom.png");
    }
}