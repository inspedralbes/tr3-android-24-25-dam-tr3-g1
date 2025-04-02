using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class WinSceneController : MonoBehaviour
{
    private void OnEnable()
    {
        // Obtenim el document UI
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        // Busquem el botó per classe
        Button menuButton = root.Q<Button>(className: "menu-button");
        
        if (menuButton != null)
        {
            // Afegim el listener per canviar d'escena
            menuButton.clicked += () => SceneManager.LoadScene("MenuScene");
        }
        else
        {
            Debug.LogError("No s'ha trobat el botó del menú!");
        }
    }
}
