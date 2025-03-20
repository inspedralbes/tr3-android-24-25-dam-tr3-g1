using UnityEngine;
using NativeWebSocket;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class WebSocketManager : MonoBehaviour
{
    public static WebSocketManager Instance { get; private set; }
    private WebSocket websocket;
    private string serverUrl = "ws://localhost:4000";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        // Inicialitza el WebSocket amb l'URL del servidor
        websocket = new WebSocket(serverUrl);

        // Defineix els esdeveniments del WebSocket
        websocket.OnOpen += () => Debug.Log("🔗 Connectat al servidor WebSocket!");
        websocket.OnMessage += (bytes) =>
        {
            Debug.Log("📩 Missatge rebut!!!!!!");
            ProcessMessage(bytes);
        };
        websocket.OnError += (err) => Debug.LogError($"⚠️ Error: {err}");
        websocket.OnClose += (code) => Debug.Log($"🔌 Desconnectat (Codi: {code})");

        // Connecta al servidor WebSocket i uneix l'usuari a la cua
        await websocket.Connect();
    }
    void Update()
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
    #endif
    }

    public async Task JoinQueue(int userId) 
    {
        // Crea un missatge JSON per unir-se a la cua i l'envia
        string message = $"{{\"type\": \"joinQueue\", \"userId\": {userId}}}";
        await SendMessage(message);
    }

    public async Task SendMove(string move, string room)
    {
        // Crea un missatge JSON per enviar un moviment i l'envia
        string message = $"{{\"type\": \"makeMove\", \"room\": \"{room}\", \"move\": \"{move}\"}}";
        await SendMessage(message);
    }

    public async Task SendMessage(string message)
    {
        // Envia un missatge de text si el WebSocket està obert
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(message);
        }
    }

    void ProcessMessage(byte[] data)
    {
        // Converteix les dades rebudes a una cadena de text
        string message = Encoding.UTF8.GetString(data);
        Debug.Log($"📩 Rebut: {message}");

        // Processa el missatge rebut
        if (message.Contains("\"matchFound\""))
        {
            Debug.Log("🎮 Partida trobada!");
            Debug.Log($"💬 {message}");
            SceneManager.LoadScene("PlayScene"); // Canvia a l'escena PlayScene
        }
        else if (message.Contains("\"opponentMove\""))
        {
            Debug.Log("🏹 L'oponent ha fet un moviment!");
        }
        else if (message.Contains("\"gameOver\""))
        {
            Debug.Log("🏁 Partida acabada!");
        }
        else if (message.Contains("\"queueJoined\""))
        {
            Debug.Log("👥 Usuari afegit a la cua!");
        }
    }

    async void OnApplicationQuit()
    {
        Debug.Log("SocketManager::OnApplicationQuit");
        // Tanca el WebSocket quan l'aplicació es tanca
        if (websocket != null)
        {
            await websocket.Close();
        }
    }
}
