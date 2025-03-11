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
        }
        else
        {
            Debug.LogWarning("Character prefab is not assigned.");
        }
    }

    void Start()
    {
        GenerateGrid();
        for (int i = _height/2-4; i < _height/2+4; i++)
        {
            if(i<_height/2){
                StartCharacter(0, i, i + 1);
            }
                // Place characters on the first column
            else
            StartCharacter(i, 0, i + 1); // Place characters on the first row
        }
    }

    void Update()
    {
        
    }
}
