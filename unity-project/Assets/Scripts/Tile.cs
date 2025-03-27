using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Newtonsoft.Json;

public class Tile : MonoBehaviour
{
    public int x, y;
    [SerializeField] private GameObject _highlight;
    private GameObject _character;
    private Character _characterData;
    public bool isOccupied = false;

    public bool movable = false;

    public bool attackable = false;
    private int? _userId;
    public int userId
    {
        get
        {
            if (!_userId.HasValue)
            {
                _userId = UserManager.Instance?.CurrentUser.id ?? 0;
            }
            return _userId.Value;
        }
    }

    private TurnManager _turnManager;
    public TurnManager turnManager
    {
        get
        {
            if (_turnManager == null)
            {
                _turnManager = TurnManager.Instance;
            }
            return _turnManager;
        }
    }

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

    public void moveUnit(Tile tileOrigin, Tile tileDestination)
    {
        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager != null)
        {
            Character characterToMove = null;
            if (turnManager.player1.army.Contains(tileOrigin.CharacterData))
            {
                characterToMove = turnManager.player1.army.Find(c => c == tileOrigin.CharacterData);
            }
            else if (turnManager.player2.army.Contains(tileOrigin.CharacterData))
            {
                characterToMove = turnManager.player2.army.Find(c => c == tileOrigin.CharacterData);
            }
            Debug.Log("Character to move: " + characterToMove.name);
            characterToMove.transform.SetParent(tileDestination.transform);
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
            unityMovesInAPath(tileDestination);
        }

        // Reset all tiles to default state
        Tile[] allTiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in allTiles)
        {
            tile.movable = false;
            tile.attackable = false;
            RemoveFilters(tile); 
        }

        WebSocketManager.MoveData moveData = new WebSocketManager.MoveData
        {
            origin = new WebSocketManager.Position { x = tileOrigin.x, y = tileOrigin.y },
            destination = new WebSocketManager.Position { x = tileDestination.x, y = tileDestination.y },
            userId = userId
        };

        // Envia l'objecte MoveData
        WebSocketManager.Instance.SendMove(moveData);
    }

    void unityMovesInAPath(Tile tileDestination)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3 startPosition = tileDestination.Character.transform.position;
        Vector3 endPosition = new Vector3(tileDestination.x, tileDestination.y, tileDestination.Character.transform.position.z);
        Vector3 aux = startPosition;
        // Move horizontally first
        path.Add(new Vector3(startPosition.x, startPosition.y, startPosition.z));
        while (startPosition.x != endPosition.x)
        {
            startPosition.x = Mathf.MoveTowards(startPosition.x, endPosition.x, 1);
            int auxX = Mathf.RoundToInt(startPosition.x);
            int auxY = Mathf.RoundToInt(startPosition.y);
            Debug.Log("Start position: " + startPosition.x);
            path.Add(new Vector3(startPosition.x, startPosition.y, startPosition.z));
        }
        Animator animator = tileDestination.Character.GetComponent<Animator>();
        if (aux.x < endPosition.x)
        {
            animator.SetBool("IsMovingRight", true);
        }
        else
        {
            animator.SetBool("IsMovingLeft", true);
        }
        if (path.Count > 1)
        {
            iTween.MoveTo(tileDestination.Character, iTween.Hash("path", path.ToArray(), "time", 1, "easetype", iTween.EaseType.linear, "oncomplete", "MoveVertical", "oncompletetarget", gameObject, "oncompleteparams", new object[] { tileDestination.Character, startPosition, endPosition, animator, tileDestination, aux }));
        }
        else
        {
            MoveVertical(new object[] { tileDestination.Character, startPosition, endPosition, animator, tileDestination, aux });
        }
    }

    void MoveVertical(object[] parameters)
    {
        GameObject character = (GameObject)parameters[0];
        Vector3 startPosition = (Vector3)parameters[1];
        Vector3 endPosition = (Vector3)parameters[2];
        Animator animator = (Animator)parameters[3];
        Tile tileDestination = (Tile)parameters[4];
        Vector3 aux = (Vector3)parameters[5];


        List<Vector3> path = new List<Vector3>();
        animator.SetBool("IsMovingRight", false);
        animator.SetBool("IsMovingLeft", false);

        path.Add(new Vector3(startPosition.x, startPosition.y, startPosition.z));
        while (startPosition.y != endPosition.y)
        {
            int auxX = Mathf.RoundToInt(startPosition.x);
            int auxY = Mathf.RoundToInt(startPosition.y);


            startPosition.y = Mathf.MoveTowards(startPosition.y, endPosition.y, 1);
            path.Add(new Vector3(startPosition.x, startPosition.y, startPosition.z));

        }
        if (tileDestination.Character != null)
        {
            if (path.Count > 1)
            {

                if (endPosition.y < aux.y)
                {
                    animator.SetBool("IsMovingDown", true);
                }
                else
                {
                    animator.SetBool("IsMovingUp", true);
                }

                if (path.Count > 0)
                {
                    iTween.MoveTo(tileDestination.Character, iTween.Hash("path", path.ToArray(), "time", 1, "easetype", iTween.EaseType.linear, "oncomplete", "OnMovementComplete", "oncompletetarget", gameObject, "oncompleteparams", animator));
                }

            }
        }
    }
    void OnMovementComplete(Animator animator)
    {
        animator.SetBool("IsMovingDown", false);
        animator.SetBool("IsMovingUp", false);
        animator.SetBool("IsMovingRight", false);
        animator.SetBool("IsMovingLeft", false);
    }
    void OnMouseDown()
    {
        Debug.Log("Mouse clicked tile.");
        Debug.Log($"Tile position: x = {x}, y = {y}");
        Debug.Log("Character data: " + JsonUtility.ToJson(_characterData, true));
        Debug.Log("Character: " + _character);
        Debug.Log("TurnManager: " + turnManager.player1.ToString());
        Debug.Log("TurnManager: " + turnManager.player2.ToString());

        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null) return;

        if (this.attackable && _characterData != null && !turnManager.player1.army.Contains(_characterData))
        {
            Attack();
            return;
        }
        else if (_characterData != null && !turnManager.player1.army.Contains(_characterData))
        {
            Debug.Log("Cannot control characters from army2.");
            return;
        }
        Tile tileOriginMovement = findTileWithCharacterSelected();
        if (tileOriginMovement != null && tileOriginMovement != this && this.movable)
        {
            if (Mathf.Abs(tileOriginMovement.x - this.x) <= 1 && Mathf.Abs(tileOriginMovement.y - this.y) <= 1 && this.attackable)
            {
                Debug.Log("The destination tile is adjacent. No need to move.");
                return;
            }

            Debug.Log("The unit is on the tile x=" + tileOriginMovement.x + " y=" + tileOriginMovement.y);
            moveUnit(tileOriginMovement, this);
            return;
        }

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
    void Attack()
    {
        Debug.Log("Attack action triggered!");

        GridManager gridManager = FindObjectOfType<GridManager>();

        Tile attackerTile = findTileWithCharacterSelected();

        // Validación del atacante
        if (attackerTile == null || attackerTile.CharacterData == null)
        {
            Debug.LogWarning("⚠️ No attacking character found.");
            return;
        }

        // Validación del objetivo
        if (this.CharacterData == null)
        {
            Debug.LogWarning("⚠️ No enemy character found on the target tile.");
            return;
        }

        // Verificar que el objetivo no es un aliado
        if (turnManager.player1.army.Contains(this.CharacterData))
        {
            Debug.LogWarning("⚠️ Cannot attack an allied unit.");
            return;
        }

        Debug.Log($"Attacker: {attackerTile.CharacterData.name} | Health: {attackerTile.CharacterData.actualHealth} | ATK: {attackerTile.CharacterData.atk}");
        Debug.Log($"Defender: {this.CharacterData.name} | Health: {this.CharacterData.actualHealth} | ATK: {this.CharacterData.atk}");

        int damage = attackerTile.CharacterData.atk;

        switch (attackerTile.CharacterData.weapon)
        {
            case "SWORD":
                Debug.Log("Sword attack!");
                damage = (int)(damage * this.CharacterData.vs_sword);
                break;
            case "SPEAR":
                Debug.Log("Spear attack!");
                damage = (int)(damage * this.CharacterData.vs_spear);
                break;
            case "AXE":
                Debug.Log("Axe attack!");
                damage = (int)(damage * this.CharacterData.vs_axe);
                break;
            case "BOW":
                Debug.Log("Bow attack!");
                damage = (int)(damage * this.CharacterData.vs_bow);
                break;
            case "MAGIC":
                Debug.Log("Magic attack!");
                damage = (int)(damage * this.CharacterData.vs_magic);
                break;
            default:
                Debug.Log("Rocky mode");
                break;
        }

        this.CharacterData.actualHealth -= damage;
        Debug.Log($"{attackerTile.CharacterData.name} attacked {this.CharacterData.name} for {damage} damage. Remaining health: {this.CharacterData.actualHealth}");

        if (Mathf.Abs(attackerTile.x - this.x) > 1 || Mathf.Abs(attackerTile.y - this.y) > 1)
        {
            Tile closestTile = FindClosestMovableTile(attackerTile, this);
            if (closestTile != null)
            {
                Debug.Log($"Moving to closest attack position at: {closestTile.x}, {closestTile.y}");
                moveUnit(attackerTile, closestTile);
                attackerTile = findTileWithCharacterSelected();
                StartCoroutine(AnimateAttackCoroutine(closestTile, this));
                return;
            }
        }

        AnimateAttack(attackerTile, this);
    }

    IEnumerator AnimateAttackCoroutine(Tile attackerTile, Tile targetTile)
    {
        Debug.Log("Waiting for movement to finish...");
        while (attackerTile.Character.GetComponent<iTween>() != null)
        {
            yield return null;
        }
        yield return new WaitForSeconds(1);
        Debug.Log("Movement finished. Starting attack animation...");
        AnimateAttack(attackerTile, targetTile);
    }

    void AnimateAttack(Tile attackerTile, Tile targetTile)
    {
        Debug.Log(JsonUtility.ToJson(attackerTile.CharacterData, true)); // Muestra toda la info del Tile
        Debug.Log(JsonUtility.ToJson(targetTile.CharacterData, true)); // Muestra toda la info del personaje

        if (attackerTile.Character != null)
        {
            Debug.Log("Attacker animation");
            Animator animator = attackerTile.CharacterData.GetComponent<Animator>();
            if (animator != null)
            {
                string attackDirection = "";
                if (attackerTile.x < targetTile.x)
                    attackDirection = "IsAttackingRight";
                else if (attackerTile.x > targetTile.x)
                    attackDirection = "IsAttackingLeft";
                else if (attackerTile.y > targetTile.y)
                    attackDirection = "IsAttackingDown";
                else
                    attackDirection = "IsAttackingUp";

                Debug.Log($"Attacking {attackDirection.Replace("IsAttacking", "").ToLower()}");
                animator.SetBool(attackDirection, true);
                StartCoroutine(ResetBoolAfterAnimation(animator, attackDirection));
            }
            else
            {
                Debug.LogWarning("⚠️ Animator not found.");
            }
        }

            attackerTile.CharacterData.selected = false;

        if (targetTile.CharacterData.actualHealth <= 0)
        {
            Debug.Log($"{targetTile.CharacterData.name} has been defeated!");
            if (targetTile.Character != null)
            {
                Destroy(targetTile.Character);
                targetTile.Character = null;
            }
            targetTile.CharacterData = null;
            targetTile.isOccupied = false;
        }
        Tile[] allTiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in allTiles)
        {
            tile.movable = false;
            tile.attackable = false;
            RemoveFilters(tile);
        }
    }
    IEnumerator ResetBoolAfterAnimation(Animator animator, string boolName)
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        animator.SetBool(boolName, false);
    }

    Tile FindClosestMovableTile(Tile attackerTile, Tile targetTile)
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
        Tile closestTile = null;
        float minDistance = float.MaxValue;

        foreach (Tile tile in allTiles)
        {
            if (!tile.isOccupied && tile.movable)
            {
                float distance = Vector2.Distance(new Vector2(tile.x, tile.y), new Vector2(targetTile.x, targetTile.y));
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTile = tile;
                }
            }
        }
        return closestTile;
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
            int range = _characterData.distance;
            int x = tile.x;
            int y = tile.y;
            tile.movable = false;
            tile.attackable = false;

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
                tile.movable = true;
                tile.attackable = false;
            }
        }
        else if (tile.Character != null)
        {
            GridManager gridManager = FindObjectOfType<GridManager>();
            if (gridManager != null)
            {
                if ((turnManager.player1.army.Contains(tile.CharacterData) && turnManager.player1.army.Contains(CharacterData)) ||
                    (turnManager.player2.army.Contains(tile.CharacterData) && turnManager.player2.army.Contains(CharacterData)))
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
                         tile.attackable = true;
                    }
                }
            }
        }
    }


    void ApplyTileRed(Tile tile)
    {
        tile.movable = false;
        if (tile.Character != null)
        {
            GridManager gridManager = FindObjectOfType<GridManager>();
            if (gridManager != null)
            {
                if ((turnManager.player1.army.Contains(tile.CharacterData) && turnManager.player1.army.Contains(CharacterData)) ||
                    (turnManager.player2.army.Contains(tile.CharacterData) && turnManager.player2.army.Contains(CharacterData)))
                {
                    // Same army, do not mark as attackable
                    tile.attackable = false;
                    return;
                }
            }
        }

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

