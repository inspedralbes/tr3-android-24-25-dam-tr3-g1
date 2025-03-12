using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x, y;
    [SerializeField] private GameObject _highlight;
    private GameObject _character;
    private object _characterData;
    public bool isOccupied = false;

    public GameObject Character
    {
        get { return _character; }
        set { _character = value; }
    }

    public object CharacterData
    {
        get { return _characterData; }
        set { _characterData = value; }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseEnter()
    {
        Debug.Log("Mouse entered tile.");
        if (_highlight != null)
        {
            _highlight.SetActive(true);
        }
    }

    void OnMouseExit()
    {
        Debug.Log("Mouse exited tile.");
        if (_highlight != null)
        {
            _highlight.SetActive(false);
        }
    }

    void OnMouseDown()
    {
        Debug.Log("Mouse clicked tile.");
        Debug.Log($"Tile position: x = {x}, y = {y}");

        // Find all tiles and remove filters
        Tile[] allTiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in allTiles)
        {
            RemoveFilters(tile);
        }

        if (_character != null)
        {
            ApplyGrayscale();
            showMovementRange(this);
        }

    }
    void showMovementRange(Tile tile)
    {
        if (_character != null)
        {
            int movement = (int)_characterData.GetType().GetProperty("Movement").GetValue(_characterData, null);
            int range = (int)_characterData.GetType().GetProperty("Range").GetValue(_characterData, null);
            int x = tile.x;
            int y = tile.y;

            for (int i = 0; i <= movement; i++)
            {
                for (int j = -movement + i; j <= movement - i; j++)
                {
                    if (x + i < 10 && y + j < 19 && y + j >= 0)
                    {
                        GameObject foundTile = GameObject.Find($"Tile {x + i} {y + j}");
                        if (foundTile != null)
                        {
                            ApplyTileBlue(foundTile.GetComponent<Tile>());
                        }
                    }
                    if (x - i >= 0 && y + j < 19 && y + j >= 0)
                    {
                        GameObject foundTile = GameObject.Find($"Tile {x - i} {y + j}");
                        if (foundTile != null)
                        {
                            ApplyTileBlue(foundTile.GetComponent<Tile>());
                        }
                    }

                    if (x + i + 1 < 10 && y + j < 19 && y + j >= 0)
                    {
                        for (int k = 1; k <= range; k++)
                        {
                            GameObject foundTile = GameObject.Find($"Tile {x + i + k} {y + j}");
                            if (foundTile != null)
                            {
                                ApplyTileRed(foundTile.GetComponent<Tile>());
                            }
                        }

                    }
                    if (x - i - 1 >= 0 && y + j < 19 && y + j >= 0)
                    {
                        for (int k = 1; k <= range; k++)
                        {
                            GameObject foundTile = GameObject.Find($"Tile {x - i - k} {y + j}");
                            if (foundTile != null)
                            {
                                ApplyTileRed(foundTile.GetComponent<Tile>());
                            }
                        }
                    }


                }

            }
            for (int i = 1; i <= range; i++)
            {
                for (int j = -range + i; j <= range - i; j++)
                {
                    GameObject foundTile = GameObject.Find($"Tile {x+j} {y + movement + i}");
                    if (foundTile != null)
                    {
                        ApplyTileRed(foundTile.GetComponent<Tile>());
                    }
                    foundTile = GameObject.Find($"Tile {x+j} {y - movement - i}");
                    if (foundTile != null)
                    {
                        ApplyTileRed(foundTile.GetComponent<Tile>());
                    }
                }

            }


        }
    }
    void ApplyGrayscale()
    {
        if (_character != null)
        {
            Renderer renderer = _character.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = renderer.material;
                material.color = Color.gray;
            }
        }
    }

    void ApplyTileBlue(Tile tile)
    {
        if (!tile.isOccupied)
        {
            Renderer renderer = tile.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = renderer.material;
                material.color = Color.blue;
            }
        }

    }


    void ApplyTileRed(Tile tile)
    {

        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = renderer.material;
            material.color = Color.red;
        }

    }
    public void RemoveFilters(Tile tile)
    {
        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = renderer.material;
            material.color = Color.white; // Reset to default color
            if (tile.Character != null)
            {
                Renderer characterRenderer = tile.Character.GetComponent<Renderer>();
                if (characterRenderer != null)
                {
                    Material characterMaterial = characterRenderer.material;
                    characterMaterial.color = Color.white; // Reset character to default color
                }
            }
        }
    }
}
