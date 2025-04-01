using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class AutoAnimationGenerator : MonoBehaviour
{
    [MenuItem("Tools/Generate Animations Automatically")]
    static void GenerateAnimations()
    {
        Debug.Log("Iniciando la generación de animaciones...");
        string baseFolder = "Assets/Sprites/";

        foreach (string characterFolder in Directory.GetDirectories(baseFolder))
        {
            Debug.Log($"Procesando carpeta: {characterFolder}");
            ProcessAnimationFolder(characterFolder);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generación de animaciones completada.");
    }

    static void ProcessAnimationFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        string[] subFolders = { "standard", "custom" };

        foreach (string subFolder in subFolders)
        {
            string subFolderPath = Path.Combine(folderPath, subFolder);
            if (!Directory.Exists(subFolderPath)) continue;

            string[] spriteFiles = Directory.GetFiles(subFolderPath, "*.png");

            foreach (string spriteFile in spriteFiles)
            {
                string fileName = Path.GetFileName(spriteFile);
                Debug.Log($"Procesando archivo de sprite: {fileName} en carpeta: {subFolderPath}");
                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteFile).OfType<Sprite>().ToArray();

                if (sprites.Length == 0)
                {
                    Debug.LogWarning($"No se encontraron sprites en el archivo: {fileName}");
                    continue;
                }
                //Custom
                if (fileName.Contains("backslash_128.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_Up_Left", 0, 6);
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_Up_Right", 7, 12);
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_Left_Left", 13, 19);
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_Left_Right", 20, 26);
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_Down_Left", 27, 32);
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_Down_Right", 34, 40);
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_Right_Left", 41, 47);
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_Right_Right", 48, 53);

                }
                else if (fileName.Contains("halfslash_128.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "halfslash_Up", 0, 5);
                    CreateAnimationClip(sprites, subFolderPath, "halfslash_Left", 6, 11);
                    CreateAnimationClip(sprites, subFolderPath, "halfslash_Down", 12, 17);
                    CreateAnimationClip(sprites, subFolderPath, "halfslash_Right", 18, 23);
                }
                else if (fileName.Contains("slash_128.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "slash_Up", 0, 5);
                    CreateAnimationClip(sprites, subFolderPath, "slash_Left", 6, 11);
                    CreateAnimationClip(sprites, subFolderPath, "slash_Down", 12, 17);
                    CreateAnimationClip(sprites, subFolderPath, "slash_Right", 18, 23);
                }
                else if (fileName.Contains("slash_oversize.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "slash_oversize_Up", 0, 5);
                    CreateAnimationClip(sprites, subFolderPath, "slash_oversize_Left", 6, 11);
                    CreateAnimationClip(sprites, subFolderPath, "slash_oversize_Down", 12, 17);
                    CreateAnimationClip(sprites, subFolderPath, "slash_oversize_Right", 18, 23);
                }
                else if (fileName.Contains("slash_reverse_oversize.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "slash_reverse_oversize_Up", 0, 5);
                    CreateAnimationClip(sprites, subFolderPath, "slash_reverse_oversize_Left", 6, 11);
                    CreateAnimationClip(sprites, subFolderPath, "slash_reverse_oversize_Down", 12, 17);
                    CreateAnimationClip(sprites, subFolderPath, "slash_reverse_oversize_Right", 18, 23);
                }
                else if (fileName.Contains("thrust_oversize.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "thrust_oversize_Up", 0, 7);
                    CreateAnimationClip(sprites, subFolderPath, "thrust_oversize_Left", 8, 15);
                    CreateAnimationClip(sprites, subFolderPath, "thrust_oversize_Down", 16, 23);
                    CreateAnimationClip(sprites, subFolderPath, "thrust_oversize_Right", 24, 31);
                }
                else if (fileName.Contains("tool_rod.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "tool_rod_Up", 0, 12);
                    CreateAnimationClip(sprites, subFolderPath, "tool_rod_Left", 13, 25);
                    CreateAnimationClip(sprites, subFolderPath, "tool_rod_Down", 26, 38);
                    CreateAnimationClip(sprites, subFolderPath, "tool_rod_Right", 39, 51);
                }
                else if (fileName.Contains("tool_whip.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "tool_whip_Up", 0, 7);
                    CreateAnimationClip(sprites, subFolderPath, "tool_whip_Left", 8, 15);
                    CreateAnimationClip(sprites, subFolderPath, "tool_whip_Down", 16, 23);
                    CreateAnimationClip(sprites, subFolderPath, "tool_whip_Right", 24, 31);
                }
                else if (fileName.Contains("walk_128.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "walk_Up", 0, 8);
                    CreateAnimationClip(sprites, subFolderPath, "walk_Left", 9, 17);
                    CreateAnimationClip(sprites, subFolderPath, "walk_Down", 18, 26);
                    CreateAnimationClip(sprites, subFolderPath, "walk_Right", 27, 35);
                }
                else if (fileName.Contains("wheelchair.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "wheelchair_Up", 0, 1);
                    CreateAnimationClip(sprites, subFolderPath, "wheelchair_Left", 2, 3);
                    CreateAnimationClip(sprites, subFolderPath, "wheelchair_Down", 4, 5);
                    CreateAnimationClip(sprites, subFolderPath, "wheelchair_Right", 6, 7);
                }
                else if (fileName.Contains("idle_custom.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "idle_custom", 0, 1);
                }
                //Standard
                else if (fileName.Contains("backslash.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_standard_Up_Left", 0, 6);
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_standard_Up_Right", 7, 12);
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_standard_Left_Left", 13, 19);
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_standard_Left_Right", 20, 26);
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_standard_Down_Left", 27, 33);
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_standard_Down_Right", 34, 40);
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_standard_Right_Left", 41, 47);
                    CreateAnimationClip(sprites, subFolderPath, "blackslash_standard_Right_Right", 48, 53);

                }
                else if (fileName.Contains("climb.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "climb_standard", 0, 5);

                }
                else if (fileName.Contains("combat_idle.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "combat_idle_standard_Up", 0, 1);
                    CreateAnimationClip(sprites, subFolderPath, "combat_idle_standard_Left", 2, 3);
                    CreateAnimationClip(sprites, subFolderPath, "combat_idle_standard_Down", 4, 5);
                    CreateAnimationClip(sprites, subFolderPath, "combat_idle_standard_Right", 6, 7);
                }
                else if (fileName.Contains("emote.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "emote_standard_Up", 0, 2);
                    CreateAnimationClip(sprites, subFolderPath, "emote_standard_Left", 3, 5);
                    CreateAnimationClip(sprites, subFolderPath, "emote_standard_Down", 6, 8);
                    CreateAnimationClip(sprites, subFolderPath, "emote_standard_Right", 9, 11);
                }
                else if (fileName.Contains("halfslash.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "halfslash_standard_Up", 0, 5);
                    CreateAnimationClip(sprites, subFolderPath, "halfslash_standard_Left", 6, 11);
                    CreateAnimationClip(sprites, subFolderPath, "halfslash_standard_Down", 12, 17);
                    CreateAnimationClip(sprites, subFolderPath, "halfslash_standard_Right", 18, 23);
                }
                else if (fileName.Contains("hurt.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "hurt_standard", 0, 5);

                }
                else if (fileName.Contains("idle.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "idle_standard_Up", 0, 1);
                    CreateAnimationClip(sprites, subFolderPath, "idle_standard_Left", 2, 3);
                    CreateAnimationClip(sprites, subFolderPath, "idle_standard_Down", 4, 5);
                    CreateAnimationClip(sprites, subFolderPath, "idle_standard_Right", 6, 7);
                }
                else if (fileName.Contains("jump.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "jump_standard_Up", 0, 4);
                    CreateAnimationClip(sprites, subFolderPath, "jump_standard_Left", 5, 9);
                    CreateAnimationClip(sprites, subFolderPath, "jump_standard_Down", 10, 14);
                    CreateAnimationClip(sprites, subFolderPath, "jump_standard_Right", 15, 19);
                }
                else if (fileName.Contains("run.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "run_standard_Up", 0, 7);
                    CreateAnimationClip(sprites, subFolderPath, "run_standard_Left", 8, 15);
                    CreateAnimationClip(sprites, subFolderPath, "run_standard_Down", 16, 23);
                    CreateAnimationClip(sprites, subFolderPath, "run_standard_Right", 24, 31);
                }
                else if (fileName.Contains("shoot.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "shoot_standard_Up", 0, 12);
                    CreateAnimationClip(sprites, subFolderPath, "shoot_standard_Left", 13, 25);
                    CreateAnimationClip(sprites, subFolderPath, "shoot_standard_Down", 26, 38);
                    CreateAnimationClip(sprites, subFolderPath, "shoot_standard_Right", 39, 51);
                }
                else if (fileName.Contains("slash.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "slash_standard_Up", 0, 5);
                    CreateAnimationClip(sprites, subFolderPath, "slash_standard_Left", 6, 11);
                    CreateAnimationClip(sprites, subFolderPath, "slash_standard_Down", 12, 17);
                    CreateAnimationClip(sprites, subFolderPath, "shoot_standard_Right", 18, 23);
                }

                else if (fileName.Contains("spellcast.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "spellcast_standard_Up", 0, 6);
                    CreateAnimationClip(sprites, subFolderPath, "spellcast_standard_Left", 7, 13);
                    CreateAnimationClip(sprites, subFolderPath, "spellcast_standard_Down", 14, 20);
                    CreateAnimationClip(sprites, subFolderPath, "spellcast_standard_Right", 21, 27);
                }
                else if (fileName.Contains("thrust.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "thrust_standard_Up", 0, 7);
                    CreateAnimationClip(sprites, subFolderPath, "thrust_standard_Left", 8, 15);
                    CreateAnimationClip(sprites, subFolderPath, "thrust_standard_Down", 16, 23);
                    CreateAnimationClip(sprites, subFolderPath, "thrust_standard_Right", 24, 31);
                }
                else if (fileName.Contains("walk.png"))
                {
                    CreateAnimationClip(sprites, subFolderPath, "walk_standard_Up", 0, 8);
                    CreateAnimationClip(sprites, subFolderPath, "walk_standard_Left", 9, 17);
                    CreateAnimationClip(sprites, subFolderPath, "walk_standard_Down", 18, 26);
                    CreateAnimationClip(sprites, subFolderPath, "walk_standard_Right", 27, 35);
                }
            }
        }
    }

    static void CreateAnimationClip(Sprite[] sprites, string folderPath, string animationName, int start, int end)
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

        string animationPath = Path.Combine(folderPath, animationName + ".anim");

        AssetDatabase.CreateAsset(clip, animationPath);
        Debug.Log($"Animación generada: {animationPath}");
    }
}

