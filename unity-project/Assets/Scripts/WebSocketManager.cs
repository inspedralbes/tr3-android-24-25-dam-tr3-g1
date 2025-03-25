using UnityEngine;
using NativeWebSocket;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Newtonsoft.Json; // Add this line

public class WebSocketManager : MonoBehaviour
{
    public static WebSocketManager Instance { get; private set; }
    private WebSocket websocket;
    private string serverUrl = "ws://localhost:4000";
    private string currentRoom; // Field to store the current room

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

    public async Task SendMove(string move)
    {
        // Crea un missatge JSON per enviar un moviment i l'envia
        string message = $"{{\"type\": \"makeMove\", \"room\": \"{currentRoom}\", \"move\": \"{move}\"}}";
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

    [System.Serializable]
    public class MatchFoundMessage
    {
        public string type;
        public string room;
        public List<User> players;
        public List<List<CharacterData>> armies;
    }

    void ProcessMessage(byte[] data)
    {
        // Convert the received data to a string
        string message = Encoding.UTF8.GetString(data);
        Debug.Log($"📩 Rebut: {message}");

        // Process the received message
        if (message.Contains("\"matchFound\""))
        {
            Debug.Log("🎮 Partida trobada!");
            Debug.Log($"💬 {message}");
            MatchFoundMessage matchFoundMessage = JsonConvert.DeserializeObject<MatchFoundMessage>(message);
            Debug.Log($"💬 {matchFoundMessage}");
            if (matchFoundMessage != null && matchFoundMessage.players != null)
            {
                TurnManager.Instance.player1 = matchFoundMessage.players[0];
                TurnManager.Instance.player2 = matchFoundMessage.players[1];
                TurnManager.Instance.player1.army = matchFoundMessage.armies[0].ConvertAll(data => ConvertToCharacter(data)); 
                TurnManager.Instance.player2.army = matchFoundMessage.armies[1].ConvertAll(data => ConvertToCharacter(data)); 
                currentRoom = matchFoundMessage.room; 

                // Debug logs to verify the armies
                Debug.Log($"Player 1 Army: {JsonConvert.SerializeObject(TurnManager.Instance.player1.army, Formatting.Indented)}");
                Debug.Log($"Player 2 Army: {JsonConvert.SerializeObject(TurnManager.Instance.player2.army, Formatting.Indented)}");
            }
            Debug.Log($"👤 Jugador 1: {TurnManager.Instance.player1.ToString()}");
            Debug.Log($"👤 Jugador 2: {TurnManager.Instance.player2.ToString()}");

            SceneManager.LoadScene("PlayScene");
        }
        else if (message.Contains("\"opponentMove\""))
        {
            Debug.Log("🏹 L'oponent ha fet un moviment!");
            var moveData = JsonConvert.DeserializeObject<MoveData>(message);
            if (moveData != null)
            {
                Tile tileOrigin = FindTile(moveData.origin.x, moveData.origin.y);
                Tile tileDestination = FindTile(moveData.destination.x, moveData.destination.y);
                if (tileOrigin != null && tileDestination != null)
                {
                    tileOrigin.moveUnit(tileOrigin, tileDestination);
                }
            }
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

    private Tile FindTile(int x, int y)
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in allTiles)
        {
            if (tile.x == x && tile.y == y)
            {
                return tile;
            }
        }
        return null;
    }

    private Character ConvertToCharacter(CharacterData data)
    {
        // Create a new Character instance and copy the data from CharacterData
        Character character = new GameObject().AddComponent<Character>();
        character.id = data.id;
        character.name = data.name;
        character.weapon = data.weapon;
        character.vs_sword = data.vs_sword;
        character.vs_spear = data.vs_spear;
        character.vs_axe = data.vs_axe;
        character.vs_bow = data.vs_bow;
        character.vs_magic = data.vs_magic;
        character.distance = data.distance;
        character.winged = data.winged;
        character.sprite = data.sprite;
        character.icon = data.icon;
        character.atk = data.atk;
        character.movement = data.movement;
        character.health = data.health;
        character.actualHealth = data.actualHealth;
        character.price = data.price;
        character.hasMoved = data.hasMoved;
        character.selected = data.selected;
        return character;
    }

    public class MoveData
    {
        public Position origin { get; set; }
        public Position destination { get; set; }
        public int userId { get; set; }
    }

    public class Position
    {
        public int x { get; set; }
        public int y { get; set; }
    }
}