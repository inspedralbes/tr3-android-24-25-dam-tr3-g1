#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class AnimationGenerator : MonoBehaviour
{
    [MenuItem("Tools/Generate Attack Animations")]
    static void GenerateAnimations()
    {
        string baseFolder = "Assets/Sprites/";
        string outputFolder = "Assets/Animations/";

        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        string[] animationTypes = { "slash", "thrust", "backslash", "halfslash" };

        foreach (string characterFolder in Directory.GetDirectories(baseFolder))
        {
            string standardPath = Path.Combine(characterFolder, "standard");
            string customPath = Path.Combine(characterFolder, "custom");

            ProcessAnimationFolder(standardPath, outputFolder, animationTypes, "");
            ProcessAnimationFolder(customPath, outputFolder, animationTypes, "custom_");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void ProcessAnimationFolder(string folderPath, string outputFolder, string[] animationTypes, string prefix)
    {
        if (!Directory.Exists(folderPath)) return;

        foreach (string animType in animationTypes)
        {
            string spriteSheetPath = Path.Combine(folderPath, animType + ".png");
            if (!File.Exists(spriteSheetPath))
            {
                Debug.LogWarning("No se encontr� la animaci�n: " + spriteSheetPath);
                continue;
            }

            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath).OfType<Sprite>().ToArray();
            if (sprites.Length == 0)
            {
                Debug.LogError("No se encontraron sprites en: " + spriteSheetPath);
                continue;
            }

            int framesPerAnimation = sprites.Length / 4;

            CreateAnimationClip(sprites, outputFolder, prefix + animType + "_Up", 0, framesPerAnimation);
            CreateAnimationClip(sprites, outputFolder, prefix + animType + "_Down", framesPerAnimation, framesPerAnimation * 2);
            CreateAnimationClip(sprites, outputFolder, prefix + animType + "_Left", framesPerAnimation * 2, framesPerAnimation * 3);
            CreateAnimationClip(sprites, outputFolder, prefix + animType + "_Right", framesPerAnimation * 3, sprites.Length);
        }
    }

    static void CreateAnimationClip(Sprite[] sprites, string folderPath, string animationName, int start, int end)
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 10;

        EditorCurveBinding spriteBinding = new EditorCurveBinding();
        spriteBinding.type = typeof(SpriteRenderer);
        spriteBinding.path = "";
        spriteBinding.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[end - start];
        for (int i = start; i < end; i++)
        {
            keyframes[i - start] = new ObjectReferenceKeyframe { time = (i - start) / 10f, value = sprites[i] };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

        AssetDatabase.CreateAsset(clip, folderPath + animationName + ".anim");
    }
}
#endif