using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class CreatePrefabs
{
    [MenuItem("Tools/Create Prefabs From Sprites")]
    public static void CreatePrefabsFromSprites()
    {
        string spritesBasePath = "Assets/Sprites"; // Carpeta base de los sprites
        string prefabsBasePath = "Assets/Prefabs"; // Carpeta base de los prefabs

        // Obtener todas las carpetas dentro de Sprites
        string[] spriteDirectories = Directory.GetDirectories(spritesBasePath);

        foreach (string spriteDir in spriteDirectories)
        {
            string folderName = Path.GetFileName(spriteDir);
            string standardAnimsPath = Path.Combine("Assets", folderName, "standard"); // Ruta a las animaciones estándar
            string customAnimsPath = Path.Combine("Assets", folderName, "custom"); // Ruta a las animaciones personalizadas
            string prefabsPath = Path.Combine(prefabsBasePath, folderName);

            if (!Directory.Exists(prefabsPath))
                Directory.CreateDirectory(prefabsPath);

            // Obtener las rutas de animación y asignarlas dependiendo de la carpeta
            string idleClipPath = Path.Combine(customAnimsPath, "idle_custom.anim");
            if (!File.Exists(idleClipPath))
            {
                idleClipPath = Path.Combine(standardAnimsPath, "idle_standard_Down.anim");
            }

            string walkUpClipPath = Path.Combine(customAnimsPath, "walk_Up.anim");
            if (!File.Exists(walkUpClipPath))
            {
                walkUpClipPath = Path.Combine(standardAnimsPath, "walk_standard_Up.anim");
            }

            string walkDownClipPath = Path.Combine(customAnimsPath, "walk_Down.anim");
            if (!File.Exists(walkDownClipPath))
            {
                walkDownClipPath = Path.Combine(standardAnimsPath, "walk_standard_Down.anim");
            }

            string walkLeftClipPath = Path.Combine(customAnimsPath, "walk_Left.anim");
            if (!File.Exists(walkLeftClipPath))
            {
                walkLeftClipPath = Path.Combine(standardAnimsPath, "walk_standard_Left.anim");
            }

            string walkRightClipPath = Path.Combine(customAnimsPath, "walk_Right.anim");
            if (!File.Exists(walkRightClipPath))
            {
                walkRightClipPath = Path.Combine(standardAnimsPath, "walk_standard_Right.anim");
            }

            string attackUpClipPath = Path.Combine(customAnimsPath, sprite.name + "_AttackUp.anim");
            if (!File.Exists(attackUpClipPath))
            {
                attackUpClipPath = Path.Combine(standardAnimsPath, sprite.name + "_AttackUp.anim");
            }

            string attackDownClipPath = Path.Combine(customAnimsPath, sprite.name + "_AttackDown.anim");
            if (!File.Exists(attackDownClipPath))
            {
                attackDownClipPath = Path.Combine(standardAnimsPath, sprite.name + "_AttackDown.anim");
            }

            string attackLeftClipPath = Path.Combine(customAnimsPath, sprite.name + "_AttackLeft.anim");
            if (!File.Exists(attackLeftClipPath))
            {
                attackLeftClipPath = Path.Combine(standardAnimsPath, sprite.name + "_AttackLeft.anim");
            }

            string attackRightClipPath = Path.Combine(customAnimsPath, sprite.name + "_AttackRight.anim");
            if (!File.Exists(attackRightClipPath))
            {
                attackRightClipPath = Path.Combine(standardAnimsPath, sprite.name + "_AttackRight.anim");
            }

            // Ahora asignamos las rutas a las animaciones en el diccionario
            var animationPaths = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Idle", idleClipPath },
                { "WalkUp", walkUpClipPath },
                { "WalkDown", walkDownClipPath },
                { "WalkLeft", walkLeftClipPath },
                { "WalkRight", walkRightClipPath },
                { "AttackUp", attackUpClipPath },
                { "AttackDown", attackDownClipPath },
                { "AttackLeft", attackLeftClipPath },
                { "AttackRight", attackRightClipPath }
            };

            // Obtener todos los sprites en la carpeta actual
            string[] spriteFiles = Directory.GetFiles(spriteDir, "*.png", SearchOption.AllDirectories);

            foreach (string spriteFile in spriteFiles)
            {
                string assetPath = spriteFile.Replace(Application.dataPath, "Assets");
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

                if (sprite != null)
                {
                    // Crear GameObject
                    GameObject newPrefab = new GameObject(sprite.name);
                    SpriteRenderer spriteRenderer = newPrefab.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = sprite;

                    // Añadir Animator
                    Animator animator = newPrefab.AddComponent<Animator>();
                    string controllerPath = Path.Combine(standardAnimsPath, sprite.name + "_Controller.controller");
                    AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                    animator.runtimeAnimatorController = animatorController;

                    // Crear estados y parámetros
                    AnimatorStateMachine rootStateMachine = animatorController.layers[0].stateMachine;
                    foreach (var entry in animationPaths)
                    {
                        string stateName = entry.Key;
                        string standardClipPath = Path.Combine(standardAnimsPath, sprite.name + "_" + stateName + ".anim");
                        string customClipPath = Path.Combine(customAnimsPath, sprite.name + "_" + stateName + ".anim");
                        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(customClipPath) ??
                                            AssetDatabase.LoadAssetAtPath<AnimationClip>(standardClipPath);

                        if (clip != null)
                        {
                            AnimatorState state = rootStateMachine.AddState(stateName);
                            state.motion = clip;
                        }
                        else
                        {
                            Debug.LogWarning($"No se encontró la animación en Standard ni en Custom: {stateName} para {sprite.name}");
                        }
                    }

                    // Agregar transitions desde AnyState con triggers
                    foreach (var entry in animationPaths)
                    {
                        AnimatorState targetState = FindState(rootStateMachine, entry.Key);
                        if (targetState != null)
                        {
                            AnimatorStateTransition transition = rootStateMachine.AddAnyStateTransition(targetState);
                            transition.AddCondition(AnimatorConditionMode.If, 0, entry.Key);
                            transition.hasExitTime = false;
                        }
                    }

                    // Guardar el GameObject como un prefab
                    string prefabPath = Path.Combine(prefabsPath, sprite.name + ".prefab");
                    PrefabUtility.SaveAsPrefabAsset(newPrefab, prefabPath);

                    // Destruir GameObject temporal
                    GameObject.DestroyImmediate(newPrefab);
                }
            }
        }

        // Refrescar la base de datos de assets
        AssetDatabase.Refresh();
    }

    private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
    {
        foreach (var state in stateMachine.states)
        {
            if (state.state.name == stateName)
            {
                return state.state;
            }
        }
        return null;
    }
}
