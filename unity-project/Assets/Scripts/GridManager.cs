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

    void StartCharacter(int x, int y, int characterNumber)
    {
        if (_characterPrefab != null)
        {
            var character = Instantiate(_characterPrefab, new Vector3(x * _tileSize, y * _tileSize, -1), Quaternion.identity);
            character.name = $"Character {characterNumber}";
            var characterData = character.AddComponent<CharacterData>();
            characterData.Name = $"Character {characterNumber}";
            characterData.Atk = 10; 
            characterData.Movement = 2;
            characterData.HP = 100;
            characterData.Range = 3; 
            characterData.Type = "Warrior"; 
            var tile = GameObject.Find($"Tile {x} {y}");
            if (tile != null)
            {
                character.transform.SetParent(tile.transform);
                tile.GetComponent<Tile>().Character = character;
                tile.GetComponent<Tile>().CharacterData = characterData;
                tile.GetComponent<Tile>().isOccupied = true;
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
            StartCharacter(0, _height / 2 - 2 + i, i + 1); 
            }
            else
            {
            StartCharacter(_width - 1, _height / 2 - 2 + (i - half / 2), i + 1); 
            }
        }
    }

    void Update()
    {
        
    }
    void OnMouseDown(){
        if (Input.GetMouseButtonDown(0)) // Detecta clic izquierdo
        {
            
        }
    }
    public class CharacterData : MonoBehaviour
    {
        public string Name { get; set; }
        public int Atk { get; set; }
        public int Movement { get; set; }
        public int HP { get; set; }
        public int Range { get; set; }
        public string Type { get; set; }

        public bool usedMovement = false;
        public bool usedAction = false;

        public void Reset()
        {
            usedMovement = false;
            usedAction = false;
        }
        public void useMovement()
        {
            usedMovement = true;
        }
        public void useAction()
        {
            usedAction = true;
        }
    }
}
