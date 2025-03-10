using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;
    [SerializeField] private GameObject _tilePrefab;

    [SerializeField] private Transform _cam;

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

        float tileSize = Camera.main.orthographicSize * 2 / _height;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x * tileSize, y * tileSize), Quaternion.identity);
                spawnedTile.transform.localScale = Vector3.one * tileSize;
                spawnedTile.name = $"Tile {x} {y}";

                var spriteRenderer = spawnedTile.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    if (x % 2 == 0)
                    {
                        spriteRenderer.color = (y % 2 == 0) ? Color.black : Color.white;
                    }
                    else
                    {
                        spriteRenderer.color = (y % 2 == 0) ? Color.white : Color.black;
                    }
                }
            }
        }

        if (_cam != null)
        {
            _cam.position = new Vector3((float)_width / 2 * tileSize - tileSize / 2, (float)_height / 2 * tileSize - tileSize / 2, -10);
        }
        else
        {
            Debug.LogWarning("Camera transform is not assigned.");
        }
    }

    void Start()
    {
        GenerateGrid();
    }

    void Update()
    {
        
    }
}
