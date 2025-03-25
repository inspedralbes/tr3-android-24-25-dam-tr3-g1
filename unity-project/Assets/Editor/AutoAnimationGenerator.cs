using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class AutoAnimationGenerator : MonoBehaviour
{
    [MenuItem("Tools/Generate Animations Automatically")]
    static void GenerateAnimations()
    {
        string baseFolder = "Assets/Sprites/";

        foreach (string characterFolder in Directory.GetDirectories(baseFolder))
        {
            string standardPath = Path.Combine(characterFolder, "standard");
            string customPath = Path.Combine(characterFolder, "custom");

            // Procesar animaciones para cada carpeta
            ProcessAnimationFolder(standardPath);
            ProcessAnimationFolder(customPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void ProcessAnimationFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        string[] spriteFiles = Directory.GetFiles(folderPath, "*.png");

        foreach (string spriteFile in spriteFiles)
        {
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteFile).OfType<Sprite>().ToArray();

            if (sprites.Length > 0)
            {
                // Obtener el número de columnas y filas basándonos en las dimensiones del sprite
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(spriteFile);
                if (texture != null)
                {
                    int cols = texture.width / 64; // Consideramos 64x64 como el tamaño del recorte
                    int rows = texture.height / 64;

                    // Si tenemos suficientes filas y columnas para generar animaciones, procedemos
                    if (cols >= 1 && rows >= 1)
                    {
                        // Crear animaciones para 4 direcciones si hay suficientes sprites
                        int framesPerDirection = sprites.Length / 4;

                        if (framesPerDirection > 0)
                        {
                            CreateAnimationClip(sprites, folderPath, "Up", 0, framesPerDirection);
                            CreateAnimationClip(sprites, folderPath, "Down", framesPerDirection, framesPerDirection * 2);
                            CreateAnimationClip(sprites, folderPath, "Left", framesPerDirection * 2, framesPerDirection * 3);
                            CreateAnimationClip(sprites, folderPath, "Right", framesPerDirection * 3, sprites.Length);
                        }
                        else
                        {
                            // Si no hay suficientes sprites, generar solo una animación (en una dirección)
                            CreateAnimationClip(sprites, folderPath, "Idle", 0, sprites.Length);
                        }
                    }
                }
            }
        }
    }

    static void CreateAnimationClip(Sprite[] sprites, string folderPath, string animationName, int start, int end)
    {
        // Crear un clip de animación
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 10;

        // Definir la propiedad para la animación del sprite
        EditorCurveBinding spriteBinding = new EditorCurveBinding();
        spriteBinding.type = typeof(SpriteRenderer);
        spriteBinding.path = "";
        spriteBinding.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[end - start];
        for (int i = start; i < end; i++)
        {
            keyframes[i - start] = new ObjectReferenceKeyframe { time = (i - start) / 10f, value = sprites[i] };
        }

        // Asignar las keyframes a la animación
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

        // Guardar la animación en la carpeta correspondiente
        string animationPath = Path.Combine(folderPath, animationName + ".anim");
        AssetDatabase.CreateAsset(clip, animationPath);
        Debug.Log($"Animación generada: {animationPath}");
    }
}
