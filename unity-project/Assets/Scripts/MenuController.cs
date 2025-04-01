using UnityEngine;
using UnityEngine.SceneManagement; // Necessari per carregar escenes
using UnityEngine.UIElements; // Necessari per treballar amb UI Toolkit

public class MenuController : MonoBehaviour
{
    // Referències als elements UI
    private Button playButton;
    private Button optionsButton;

    private Button shopButton;
    private Button logoutButton;

    void OnEnable()
    {
        // Obtenim el root visual del document i accedim als botons
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

        // Obtenim els botons per les seves id (assignades en el document UXML)
        playButton = rootVisualElement.Q<Button>("play");
        optionsButton = rootVisualElement.Q<Button>("options");
        logoutButton = rootVisualElement.Q<Button>("logout");
        shopButton = rootVisualElement.Q<Button>("Botiga");

        // Assignem els esdeveniments als botons
        playButton.clicked += OnPlayButtonClicked;
        optionsButton.clicked += OnOptionsButtonClicked;
        logoutButton.clicked += OnLogoutButtonClicked;
        shopButton.clicked += OnShopButtonClicked;
    }

    // Canvia a l'escena PlayScene
    async void OnPlayButtonClicked()
    {
        // Uneix l'usuari a la cua
        await WebSocketManager.Instance.JoinQueue(UserManager.Instance.CurrentUser.id);

        // Canvia a l'escena PlayScene
        SceneManager.LoadScene("QueueScene");
    }

    // Canvia a l'escena OptionsScene
    void OnOptionsButtonClicked()
    {
        SceneManager.LoadScene("ArmyScene");
    }

    // Tanca el joc
    void OnLogoutButtonClicked()
    {
        // Per tancar el joc, només funciona en una construcció (no en el mode d'edició)
        Application.Quit();

        // En el mode editor de Unity, aquest codi no tanca el joc, així que afegim una línia per fer-ho només quan estigui construït
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    void OnShopButtonClicked()
    {
        SceneManager.LoadScene("ShopScene");
    }
}
