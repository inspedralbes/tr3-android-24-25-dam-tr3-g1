using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private GameObject _characterPrefab;

    [SerializeField] private Transform _cam;

    private float _tileSize;

    public TurnManager turnManager;


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
                spawnedTile.GetComponent<Tile>().turnManager = turnManager;
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

    void StartCharacter(int x, int y, int characterNumber, List<Character> armyManager)
    {
        if (_characterPrefab == null)
        {
            Debug.LogError("Character prefab is not assigned.");
            return;
        }

        if (armyManager == null || characterNumber < 0 || characterNumber >= armyManager.Count)
        {
            Debug.Log("Army Manager: " + armyManager);
            Debug.Log("Character Number: " + characterNumber);
            Debug.LogError("Invalid army manager or character number.");
            return;
        }

        var characterData = armyManager[characterNumber];
        if (characterData == null)
        {
            Debug.LogError("Character data is null.");
            return;
        }

        var character = Instantiate(_characterPrefab, new Vector3(x * _tileSize, y * _tileSize, -1), Quaternion.identity);
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
        characterComponent.range = characterData.range;
        characterComponent.hasMoved = characterData.hasMoved;
        characterComponent.selected = characterData.selected;

        var tile = GameObject.Find($"Tile {x} {y}");
        if (tile != null)
        {
            var tileComponent = tile.GetComponent<Tile>();
            tileComponent.Character = character;
            tileComponent.CharacterData = characterComponent;
            tileComponent.isOccupied = true;
            Debug.LogWarning("Character for army " + armyManager + " created at " + x + " " + y);
        }
        else
        {
            Debug.LogWarning($"Tile {x} {y} not found.");
        }
    }

    void Start()
    {
        turnManager = TurnManager.Instance;
        Debug.Log("TurnManager: " + turnManager.player1.army);
        Debug.Log("TurnManager: " + turnManager.player2.army);
        GenerateGrid();
        int half = (_height / 2 + 4) - (_height / 2 - 4);
        for (int i = 0; i < half; i++)
        {
            if (i < half / 2)
            {
                StartCharacter(0, _height / 2 - 2 + i, i, turnManager.player1.army);
            }
            else
            {
                StartCharacter(_width - 1, _height / 2 - 2 + (i - half / 2), i, turnManager.player2.army);
            }
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
