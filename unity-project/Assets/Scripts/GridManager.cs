using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Networking;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private GameObject _characterPrefab;

    [SerializeField] private Transform _cam;

    private float _tileSize;

    void GenerateGrid()
    {
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = (float)_width / (float)_height;

        if (screenRatio >= targetRatio)
        {
            Camera.main.orthographicSize = _height / 2f;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            Camera.main.orthographicSize = _height / 2f * differenceInSize;
        }

        _tileSize = Camera.main.orthographicSize * 2 / _height;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x * _tileSize, y * _tileSize, 0), Quaternion.identity);
                spawnedTile.transform.localScale = Vector3.one * _tileSize;
                spawnedTile.name = $"Tile {x} {y}";
                spawnedTile.GetComponent<Tile>().x = x;
                spawnedTile.GetComponent<Tile>().y = y;
                var spriteRenderer = spawnedTile.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    if (x % 2 == 0)
                    {
                        spriteRenderer.color = (y % 2 == 0) ? new Color(0.5f, 1.0f, 0.5f) : new Color(0.4f, 0.9f, 0.4f);
                    }
                    else
                    {
                        spriteRenderer.color = (y % 2 == 0) ? new Color(0.4f, 0.9f, 0.4f) : new Color(0.5f, 1.0f, 0.5f);
                    }
                }
            }
        }

        if (_cam != null)
        {
            _cam.position = new Vector3((float)_width / 2 * _tileSize - _tileSize / 2, (float)_height / 2 * _tileSize - _tileSize / 2, -10);
        }
        else
        {
            Debug.LogWarning("Camera transform is not assigned.");
        }
    }

    void StartCharacter(int x, int y, Character characterData, int army, int positionOnArray)
    {
        StartCoroutine(LoadCharacterPrefab(x, y, characterData, army, positionOnArray));
    }

    private IEnumerator LoadCharacterPrefab(int x, int y, Character characterData, int army, int positionOnArray)
    {
        Debug.Log($"info http://lordgrids.dam.inspedralbes.cat:4000/AssetBundles/{characterData.name}");
        string assetBundleURL = $"http://lordgrids.dam.inspedralbes.cat:4000/AssetBundles/{characterData.name}";
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleURL);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al cargar AssetBundle: " + www.error);
        }
        else
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            _characterPrefab = bundle.LoadAsset<GameObject>(characterData.name);

            if (_characterPrefab != null)
            {
                Debug.Log("Character prefab cargado exitosamente.");

                // Instantiate the character prefab at the specified position
                var character = Instantiate(_characterPrefab, new Vector3(x * _tileSize, y * _tileSize, -1), Quaternion.identity);
                character.name = characterData.name;

                // Add the Character component and populate its properties
                var characterComponent = character.AddComponent<Character>();
                characterComponent.id = characterData.id;
                characterComponent.name = characterData.name;
                characterComponent.weapon = characterData.weapon;
                characterComponent.vs_sword = characterData.vs_sword;
                characterComponent.vs_spear = characterData.vs_spear;
                characterComponent.vs_axe = characterData.vs_axe;
                characterComponent.vs_bow = characterData.vs_bow;
                characterComponent.vs_magic = characterData.vs_magic;
                characterComponent.winged = characterData.winged;
                characterComponent.sprite = characterData.sprite;
                characterComponent.icon = characterData.icon;
                characterComponent.atk = characterData.atk;
                characterComponent.movement = characterData.movement;
                characterComponent.health = characterData.health;
                characterComponent.actualHealth = characterData.actualHealth;
                characterComponent.distance = characterData.distance;
                characterComponent.hasMoved = characterData.hasMoved;

                // Find the tile at the specified position and assign the character to it
                var tile = GameObject.Find($"Tile {x} {y}");
                if (tile != null)
                {
                    var tileComponent = tile.GetComponent<Tile>();
                    tileComponent.Character = character;
                    if (army == 1)
                    {
                        characterComponent.internalId = positionOnArray;
                    }
                    else
                    {
                        characterComponent.internalId = positionOnArray + 4;
                    }
                    tileComponent.CharacterData = characterComponent;
                    tileComponent.isOccupied = true;

                    Debug.Log($"Character {characterData.name} created at {x}, {y}");
                }
                else
                {
                    Debug.LogWarning($"Tile {x} {y} not found.");
                }
            }
            else
            {
                Debug.LogError("No se encontr� el prefab CharacterTest en el AssetBundle.");
            }

            // Unload the AssetBundle to free memory
            bundle.Unload(false);
        }
    }

    void Start()
    {
        Debug.Log("TurnManager: " + TurnManager.Instance.player1.ToString());
        Debug.Log("TurnManager: " + TurnManager.Instance.player2.ToString());
        StartCoroutine(WaitForArmyReady());
    }

    private IEnumerator WaitForArmyReady()
    {
        while (!TurnManager.Instance.IsArmyReady)
        {
            yield return null; // Wait for the next frame
        }

        Debug.Log("TurnManager: " + TurnManager.Instance.player1.ToString());
        Debug.Log("TurnManager: " + TurnManager.Instance.player2.ToString());
        GenerateGrid();

        // Place player 1's army
        for (int i = 0; i < TurnManager.Instance.player1.army.Count; i++)
        {
            StartCharacter(0, _height / 2 - 2 + i, TurnManager.Instance.player1.army[i], 1, i);
            TurnManager.Instance.player1.army[i].internalId = i;
        }

        // Place player 2's army
        for (int i = 0; i < TurnManager.Instance.player2.army.Count; i++)
        {
            StartCharacter(_width - 1, _height / 2 - 2 + i, TurnManager.Instance.player2.army[i], 2, i);
            TurnManager.Instance.player2.army[i].internalId = i + 4;
        }
    }

    void Update()
    {

    }

    void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0)) // Detecta clic izquierdo
        {

        }
    }
}