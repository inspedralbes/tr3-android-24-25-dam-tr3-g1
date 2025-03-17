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

    public List<Character> _army1;
    public List<Character> _army2;


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

    void StartCharacter(int x, int y, int characterNumber, List<Character> armyManager)
    {
        if (_characterPrefab != null)
        {
            var character = Instantiate(_characterPrefab, new Vector3(x * _tileSize, y * _tileSize, -1), Quaternion.identity);
            character.name = $"Character {characterNumber}";
            var characterData = character.AddComponent<Character>();
            characterData.id = characterNumber;
            characterData.name = $"Character {characterNumber}";
            characterData.weapon = "Sword";
            characterData.vs_sword = 1.0f;
            characterData.vs_spear = 1.0f;
            characterData.vs_axe = 1.0f;
            characterData.vs_bow = 1.0f;
            characterData.vs_magic = 1.0f;
            characterData.winged = false;
            characterData.sprite = "character_sprite";
            characterData.icon = "character_icon";
            characterData.atk = 10;
            characterData.movement = 4;
            characterData.health = 100;
            characterData.actualHealth = 100;
            characterData.range = 1;
            characterData.hasMoved = false;
            var tile = GameObject.Find($"Tile {x} {y}");
            if (tile != null)
            {
                tile.GetComponent<Tile>().Character = character;
                tile.GetComponent<Tile>().CharacterData = characterData;
                tile.GetComponent<Tile>().isOccupied = true;
                Debug.LogWarning("Character for army " + armyManager + " created at " + x + " " + y);
                armyManager.Add(characterData);
            }
            else
            {
                Debug.LogWarning($"Tile {x} {y} not found.");
            }
        }
        else
        {
            Debug.LogWarning("Character prefab is not assigned.");
        }
    }

    void Start()
    {
   

        GenerateGrid();
        int half = (_height / 2 + 4) - (_height / 2 - 4);
        for (int i = 0; i < half; i++)
        {
            if (i < half / 2)
            {
                StartCharacter(0, _height / 2 - 2 + i, i + 1, _army1);
            }
            else
            {
                StartCharacter(_width - 1, _height / 2 - 2 + (i - half / 2), i + 1, _army2);
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
