using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x, y;
    [SerializeField] private GameObject _highlight;
    private GameObject _character;
    private Character _characterData;
    public bool isOccupied = false;

    public bool movable = false;

    public bool attackable = false;

    public GameObject Character
    {
        get { return _character; }
        set { _character = value; }
    }

    public Character CharacterData
    {
        get { return _characterData; }
        set { _characterData = value; }
    }

    void Start()
    {

    }

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

    Tile findTileWithCharacterSelected()
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in allTiles)
        {
            if (tile.CharacterData != null && tile.CharacterData.selected)
            {
                return tile;
            }
        }
        return null;
    }

    void moveUnit(Tile tileOrigin, Tile tileDestination)
    {

        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager != null)
        {
            Character characterToMove = null;
            if (gridManager._army1.Contains(tileOrigin.CharacterData))
            {
                characterToMove = gridManager._army1.Find(c => c == tileOrigin.CharacterData);
            }
            else if (gridManager._army2.Contains(tileOrigin.CharacterData))
            {
                characterToMove = gridManager._army2.Find(c => c == tileOrigin.CharacterData);
            }
            Debug.Log("Character to move: " + characterToMove.name);
        }

        if (tileDestination.movable)
            tileDestination.Character = tileOrigin.Character;
        tileDestination.CharacterData = tileOrigin.CharacterData;
        tileDestination.isOccupied = true;
        tileOrigin.Character = null;
        tileOrigin.CharacterData = null;
        tileOrigin.isOccupied = false;
        tileDestination.CharacterData.hasMoved = true;
        tileDestination.CharacterData.selected = false;
        RemoveFilters(tileDestination);
        RemoveFilters(tileOrigin);

        // Update the character's position
        if (tileDestination.Character != null)
        {
            tileDestination.Character.transform.position = new Vector3(tileDestination.x, tileDestination.y, tileDestination.Character.transform.position.z);
        }

        // Reset all tiles to default state
        Tile[] allTiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in allTiles)
        {
            tile.movable = false;
            tile.attackable = false;
            RemoveFilters(tile);
        }
    }



void OnMouseDown()
{
    Debug.Log("Mouse clicked tile.");
    Debug.Log($"Tile position: x = {x}, y = {y}");

    Tile tileOriginMovement = findTileWithCharacterSelected();
    if (tileOriginMovement != null)
    {
        Debug.Log("The unit is on the tile x=" + tileOriginMovement.x + " y=" + tileOriginMovement.y);
        moveUnit(tileOriginMovement, this);
        return;
    }

    // Find all tiles and remove filters
    Tile[] allTiles = FindObjectsOfType<Tile>();
    foreach (Tile tile in allTiles)
    {
        if (tile.CharacterData != null)
        {
            tile.CharacterData.selected = false;
        }
        RemoveFilters(tile);
        tile.movable = false;
        tile.attackable = false;
    }

    if (_character != null)
    {
        ApplyGrayscale();
        showMovementRange(this, _characterData);
    }
}

void showMovementRange(Tile tile, Character _characterData)
{
    // if (_characterData.hasMoved)
    // {
    //     return;
    // }
    _characterData.selected = true;
    if (_character != null)
    {
        int movement = _characterData.movement;
        int range = _characterData.range;
        int x = tile.x;
        int y = tile.y;

        for (int i = 0; i <= movement; i++)
        {
            for (int j = -movement + i; j <= movement - i; j++)
            {
                if (true)
                {
                    GameObject foundTile = GameObject.Find($"Tile {x + i} {y + j}");
                    if (foundTile != null)
                    {
                        ApplyTileBlue(foundTile.GetComponent<Tile>());
                    }
                }
                if (true)
                {
                    GameObject foundTile = GameObject.Find($"Tile {x - i} {y + j}");
                    if (foundTile != null)
                    {
                        ApplyTileBlue(foundTile.GetComponent<Tile>());
                    }
                }

                if (true)
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
                if (true)
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
                GameObject foundTile = GameObject.Find($"Tile {x + j} {y + movement + i}");
                if (foundTile != null)
                {
                    ApplyTileRed(foundTile.GetComponent<Tile>());
                }
                foundTile = GameObject.Find($"Tile {x + j} {y - movement - i}");
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
        tile.movable = true;
    }
    else if (tile.Character != null)
    {
        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager != null)
        {
            if ((gridManager._army1.Contains(tile.CharacterData) && gridManager._army1.Contains(CharacterData)) ||
                (gridManager._army2.Contains(tile.CharacterData) && gridManager._army2.Contains(CharacterData)))
            {
                Renderer renderer = tile.Character.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material material = renderer.material;
                    material.color = Color.green;
                }
            }
            else
            {
                Renderer renderer = tile.Character.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material material = renderer.material;
                    material.color = Color.red;
                }
                tile.attackable = true;
            }
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
        tile.attackable = true;
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

