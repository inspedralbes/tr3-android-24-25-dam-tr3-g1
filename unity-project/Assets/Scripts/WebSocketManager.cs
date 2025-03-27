using UnityEngine;
using NativeWebSocket;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.Linq;

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
        Debug.Log($"📤 Enviat: {message}");
        await SendMessage(message);
    }

    public async Task SendMove(WebSocketManager.MoveData moveData)
    {
        // Crea un objecte per al missatge complet
        var messageObject = new
        {
            type = "makeMove",
            room = currentRoom,
            move = moveData // Inclou l'objecte MoveData directament
        };

        // Serialitza l'objecte complet a JSON
        string messageJson = JsonConvert.SerializeObject(messageObject);
        Debug.Log($"📤 Enviat: {messageJson}");

        // Envia el missatge serialitzat
        await SendMessage(messageJson);
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
        string message = Encoding.UTF8.GetString(data);
        Debug.Log($"📩 Rebut: {message}");

        if (message.Contains("\"matchFound\""))
        {
            Debug.Log("🎮 Partida trobada!");
            MatchFoundMessage matchFoundMessage = JsonConvert.DeserializeObject<MatchFoundMessage>(message);

            if (matchFoundMessage != null && matchFoundMessage.players != null)
            {
                TurnManager.Instance.player1 = matchFoundMessage.players[0];
                TurnManager.Instance.player2 = matchFoundMessage.players[1];

                TurnManager.Instance.player1.army = MapCharacterDataToCharacter(matchFoundMessage.armies[0]);
                TurnManager.Instance.player2.army = MapCharacterDataToCharacter(matchFoundMessage.armies[1]);

                TurnManager.Instance.SetArmyReady(); // Mark armies as ready

                currentRoom = matchFoundMessage.room;
            }

            Debug.Log($"👤 Jugador 1: {TurnManager.Instance.player1.ToString()}");
            Debug.Log($"👤 Jugador 2: {TurnManager.Instance.player2.ToString()}");

            SceneManager.LoadScene("PlayScene");
        }
        else if (message.Contains("\"opponentMove\""))
        {
            Debug.Log("🏹 L'oponent ha fet un moviment!");
            var opponentMoveMessage = JsonConvert.DeserializeObject<OpponentMoveMessage>(message);

            if (opponentMoveMessage == null || opponentMoveMessage.move == null)
            {
                Debug.LogError("❌ Error: opponentMoveMessage or move is null after deserialization.");
                return;
            }

            MoveData moveData = opponentMoveMessage.move;
            Debug.Log($"Movedata: {moveData.ToString()}");
            Debug.Log($"🏹 Moviment rebut: {moveData.origin.x}, {moveData.origin.y} -> {moveData.destination.x}, {moveData.destination.y}");

            Tile tileOrigin = FindTile(moveData.origin.x, moveData.origin.y);
            Tile tileDestination = FindTile(moveData.destination.x, moveData.destination.y);

            if (tileOrigin == null)
            {
                Debug.LogError($"❌ Error: Tile at origin ({moveData.origin.x}, {moveData.origin.y}) not found.");
            }

            if (tileDestination == null)
            {
                Debug.LogError($"❌ Error: Tile at destination ({moveData.destination.x}, {moveData.destination.y}) not found.");
            }

            if (tileOrigin != null && tileDestination != null)
            {
                tileOrigin.moveUnit(tileOrigin, tileDestination);
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

    private List<Character> MapCharacterDataToCharacter(List<CharacterData> characterDataList)
    {
        List<Character> characters = new List<Character>();

        foreach (var data in characterDataList)
        {
            // Instantiate a new GameObject for each character
            GameObject characterObject = new GameObject(data.name);
            Character character = characterObject.AddComponent<Character>();

            // Copy data from CharacterData to Character
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
            character.actualHealth = data.health; 
            character.price = data.price;

            characters.Add(character);
        }

        return characters;
    }

    [System.Serializable]
    public class MoveData
    {
        public Position origin;
        public Position destination;
        public int userId;
    }

    [System.Serializable]
    public class Position
    {
        public int x;
        public int y;
    }

    [System.Serializable]
    public class OpponentMoveMessage
    {
        public string type;
        public MoveData move;
    }
}