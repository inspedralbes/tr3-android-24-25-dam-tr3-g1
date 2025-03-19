using UnityEngine;
using NativeWebSocket;
using System;
using System.Text;
using System.Threading.Tasks;

public class WebSocketManager : MonoBehaviour
{
    private WebSocket websocket;
    private string serverUrl = "ws://localhost:4000"; 

    async void Start()
    {
        // Inicialitza el WebSocket amb l'URL del servidor
        websocket = new WebSocket(serverUrl);

        // Defineix els esdeveniments del WebSocket
        websocket.OnOpen += () => Debug.Log("🔗 Connectat al servidor WebSocket!");
        websocket.OnMessage += (bytes) => ProcessMessage(bytes);
        websocket.OnError += (err) => Debug.LogError($"⚠️ Error: {err}");
        websocket.OnClose += (code) => Debug.Log($"🔌 Desconnectat (Codi: {code})");

        // Connecta al servidor WebSocket i uneix l'usuari a la cua
        await websocket.Connect();
        await JoinQueue(UserManager.Instance.CurrentUser.id); // Exemple d'usuari
    }

    async Task JoinQueue(int userId)
    {
        // Crea un missatge JSON per unir-se a la cua i l'envia
        string message = $"{{\"type\": \"joinQueue\", \"userId\": {userId}}}";
        await SendMessage(message);
    }

    async Task SendMove(string move, string room)
    {
        // Crea un missatge JSON per enviar un moviment i l'envia
        string message = $"{{\"type\": \"makeMove\", \"room\": \"{room}\", \"move\": \"{move}\"}}";
        await SendMessage(message);
    }

    async Task SendMessage(string message)
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
        }
        else if (message.Contains("\"opponentMove\""))
        {
            Debug.Log("🏹 L'oponent ha fet un moviment!");
        }
    }

    async void OnApplicationQuit()
    {
        // Tanca el WebSocket quan l'aplicació es tanca
        if (websocket != null)
        {
            await websocket.Close();
        }
    }
}
