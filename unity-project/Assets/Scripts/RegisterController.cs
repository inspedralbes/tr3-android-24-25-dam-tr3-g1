using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


public class RegisterController : MonoBehaviour
{
    private TextField usernameField;
    private TextField emailField;
    private TextField passwordField;
    private Button loginButton;

    private Button registerButton;

    private const string registerUrl = "http://localhost:4000/newUSer";
    
    private const string odooUrl = "http://localhost:4001/create-client";
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        // Obté l'arrel del document UI Toolkit
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Busca els elements pel seu nom definit a l'UXML
        usernameField = root.Q<TextField>("username");
        emailField = root.Q<TextField>("email");
        passwordField = root.Q<TextField>("password");
        loginButton = root.Q<Button>("goToLogin");
        registerButton = root.Q<Button>("register");

        // Assigna l'event al botó
        loginButton.clicked += OnLoginButtonClicked;
        registerButton.clicked += OnRegisterButtonClicked;
    }

    private void OnLoginButtonClicked()
    {

        SceneManager.LoadScene("LoginScene");
    }
    private void OnRegisterButtonClicked()
    {
        // Captura el text introduït per l'usuari
        string email = emailField.value;
        string password = passwordField.value;
        string username = usernameField.value;

        // Comença la petició HTTP asíncrona
        StartCoroutine(RegisterRequest(email, password, username));
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDisable()
    {
        // Elimina l'event per evitar problemes de referències després de desactivar el script
        loginButton.clicked -= OnLoginButtonClicked;
    }


    private IEnumerator RegisterOnOdoo(string email, string username, System.Action<int> callback)
    {
        // Crea un objecte JSON amb el username i email
        string jsonData = $"{{\"name\":\"{username}\",\"email\":\"{email}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData); // Converteix el JSON en bytes

        using (UnityWebRequest request = new UnityWebRequest(odooUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);  // Envia el cos de la petició
            request.downloadHandler = new DownloadHandlerBuffer(); // Gestiona la resposta
            request.SetRequestHeader("Content-Type", "application/json"); // Defineix el format JSON

            // Envia la petició i espera la resposta
            yield return request.SendWebRequest();

            // Comprova si la petició ha tingut èxit
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Resposta del servidor Odoo: " + responseText);

                // Deserialitza la resposta JSON per obtenir l'ID
                List<OdooResponse> responseList = JsonConvert.DeserializeObject<List<OdooResponse>>(responseText);
                if (responseList.Count > 0)
                {
                    callback(responseList[0].id);
                }
                else
                {
                    callback(0); // Retorna 0 si no hi ha dades
                }
            }
            else
            {
                Debug.LogError("Error a la connexió amb Odoo: " + request.error);
                callback(0); // Retorna 0 en cas d'error
            }
        }
    }

    [System.Serializable]
    private class OdooResponse
    {
        public int id;
    }
    [System.Serializable]
    private class OdooResponseWrapper
    {
        public OdooResponse[] responses;
    }

    private IEnumerator RegisterRequest(string email, string password, string username)
    {
        // Crea un objecte JSON amb el username i password
        int id = 0;
        yield return StartCoroutine(RegisterOnOdoo(email, username, result => id = result));
        string jsonData = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"username\":\"{username}\", \"id\":\"{id}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData); // Converteix el JSON en bytes

        // Configura la petició HTTP amb el mètode POST
        using (UnityWebRequest request = new UnityWebRequest(registerUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);  // Envia el cos de la petició
            request.downloadHandler = new DownloadHandlerBuffer(); // Gestiona la resposta
            request.SetRequestHeader("Content-Type", "application/json"); // Defineix el format JSON

            // Envia la petició i espera la resposta
            yield return request.SendWebRequest();

            // Comprova si la petició ha tingut èxit
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Resposta del servidor: " + responseText);

                // Deserialitza la resposta JSON a un objecte User
                User user = JsonUtility.FromJson<User>(responseText);

                // Comprova si la resposta conté l'usuari amb l'email introduït
                if (user != null && user.email == email)
                {
                    // Desa la informació de l'usuari
                    UserManager.Instance.SetUser(user);

                    SceneManager.LoadScene("MenuScene"); // Canvia a l'escena del menú
                }
                else
                {
                    Debug.LogError("Usuari no trobat.");
                }
            }
            else
            {
                Debug.LogError("Error a la connexió: " + request.error);
            }
        }
    }
}
