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
    public void attackUnitSocket(Tile tileOrigin, Tile tileDestination, Tile closestTile)
    {
        Debug.Log("AttackUnitSocket called");
        Debug.Log("SEXo1", tileOrigin.CharacterData);
        Debug.Log("SEXo2", tileDestination.CharacterData);
        Tile attackerTile = GameObject.Find($"Tile {tileOrigin.x} {tileOrigin.y}").GetComponent<Tile>();
        Tile targetTile = GameObject.Find($"Tile {tileDestination.x} {tileDestination.y}").GetComponent<Tile>();
        Tile closest = GameObject.Find($"Tile {closestTile.x} {closestTile.y}").GetComponent<Tile>();
        if (tileOrigin == null || tileDestination == null)
        {
            Debug.LogWarning("Invalid tiles provided for attack.");
            return;
        }

        if (tileOrigin.CharacterData == null || tileDestination.CharacterData == null)
        {
            Debug.LogWarning("One or both tiles do not have characters.");
            return;
        }

        // if (checkIfCharacterIsInTheSameArmyAsThePlayer(tileDestination.CharacterData, userId))
        // {
        //     Debug.LogWarning("Cannot attack an allied unit.");
        //     return;
        // }

        int damage = tileOrigin.CharacterData.atk;

        switch (tileOrigin.CharacterData.weapon)
        {
            case "SWORD":
                damage = (int)(damage * tileDestination.CharacterData.vs_sword);
                break;
            case "SPEAR":
                damage = (int)(damage * tileDestination.CharacterData.vs_spear);
                break;
            case "AXE":
                damage = (int)(damage * tileDestination.CharacterData.vs_axe);
                break;
            case "BOW":
                damage = (int)(damage * tileDestination.CharacterData.vs_bow);
                break;
            case "MAGIC":
                damage = (int)(damage * tileDestination.CharacterData.vs_magic);
                break;
            default:
                break;
        }

        tileDestination.CharacterData.actualHealth -= damage;

        Debug.Log($"{tileOrigin.CharacterData.name} attacked {tileDestination.CharacterData.name} for {damage} damage. Remaining health: {tileDestination.CharacterData.actualHealth}");
        if (Mathf.Abs(attackerTile.x - tileDestination.x) > 1 || Mathf.Abs(attackerTile.y - tileDestination.y) > 1)
        {
            Debug.Log("attackerTile:" + attackerTile.CharacterData.name);
            Debug.Log("tileDestination:", tileDestination.CharacterData);


            if (closestTile != null)
            {
                Debug.Log($"Moving to closest attack position at: {closestTile.x}, {closestTile.y}");
                moveUnitSocket(attackerTile, closestTile);
                if (closestTile.Character != null && targetTile.Character != null)
                {
                    StartCoroutine(AnimateAttackCoroutine(closestTile, targetTile));
                }
                else
                {
                    Debug.LogWarning("Cannot animate attack: attacker or target character is null.");
                }
            }
            else
            {
                StartCoroutine(AnimateAttackCoroutine(attackerTile, targetTile));
            }
        }

    }

    public void moveUnitSocket(Tile tileOrigin, Tile tileDestination)
    {
        Debug.Log("MoveUnitSocket called");


        tileDestination.Character = tileOrigin.Character;
        tileDestination.CharacterData = tileOrigin.CharacterData;
        tileDestination.isOccupied = true;
        tileOrigin.Character = null;
        tileOrigin.CharacterData = null;
        tileOrigin.isOccupied = false;

        if (tileDestination.CharacterData != null)
        {
            tileDestination.CharacterData.selected = false;
            unityMovesInAPath(tileDestination);
        }
        else
        {
            Debug.LogWarning("CharacterData is null. Cannot execute unityMovesInAPath.");
        }

    }

    int countCharactersFromArmy(int army)
    {
        int count = 0;
        Character[] characters = FindObjectsOfType<Character>();
        List<int> characterIds = new List<int>();
        if (army == 1)
        {
            foreach (Character character in characters)
            {
                Debug.Log("Character ID: " + character.internalId);
                if (character.internalId < 4 && characterIds.Contains(character.internalId) == false)
                {
                    characterIds.Add(character.internalId);
                    Debug.Log("Character ID: " + character.internalId);
                    count++;
                }

            }
        }
        else
        {
            foreach (Character character in characters)
            {
                if (character.internalId > 3 && characterIds.Contains(character.internalId) == false)
                {
                    characterIds.Add(character.internalId);
                    Debug.Log("Character ID: " + character.internalId);
                    count++;
                }
            }
        }
        return count;
    }
    public void moveUnit(Tile tileOrigin, Tile tileDestination, bool fromAttack)
    {
        if (tileOrigin.CharacterData != null && tileOrigin.CharacterData.hasMoved)
        {
            Debug.LogWarning("This character has already moved and cannot move again.");
            return;
        }

        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager != null)
        {
            Character characterToMove = null;
            if (checkIfCharacterIsInTheSameArmyAsThePlayer(tileOrigin.CharacterData, turnManager.player1.id))
            {
                characterToMove = turnManager.player1.army[tileOrigin.CharacterData.internalId];
            }
            else if (checkIfCharacterIsInTheSameArmyAsThePlayer(tileOrigin.CharacterData, turnManager.player2.id))
            {
                characterToMove = turnManager.player2.army[tileOrigin.CharacterData.internalId - 4];
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

        tileDestination.CharacterData.selected = false;
        tileDestination.CharacterData.hasMoved = true;

        // Find the character in the armies and set hasMoved to true
        if (checkIfCharacterIsInTheSameArmyAsThePlayer(tileDestination.CharacterData, turnManager.player1.id))
        {
            turnManager.player1.army[tileDestination.CharacterData.internalId].hasMoved = true;
        }
        else if (checkIfCharacterIsInTheSameArmyAsThePlayer(tileDestination.CharacterData, turnManager.player2.id))
        {
            turnManager.player2.army[tileDestination.CharacterData.internalId - 4].hasMoved = true;
        }
        RemoveFilters(tileDestination);
        RemoveFilters(tileOrigin);
        if (tileDestination.CharacterData.internalId > 3)
        {
            int movedCharacters = 0;
            Character[] characters = FindObjectsOfType<Character>();
            foreach (Character character in turnManager.player2.army)
            {
                foreach (Character character2 in characters)
                {
                    if (character2.hasMoved && character2.internalId == character.internalId)
                    {
                        movedCharacters++;
                        break;
                    }
                }

            }
            if (movedCharacters == countCharactersFromArmy(2))
            {
                turnManager.ChangeTurn();
                turnManager.player1.army[0].hasMoved = false;
                turnManager.player1.army[1].hasMoved = false;
                turnManager.player1.army[2].hasMoved = false;
                turnManager.player1.army[3].hasMoved = false;
                turnManager.player2.army[0].hasMoved = false;
                turnManager.player2.army[1].hasMoved = false;
                turnManager.player2.army[2].hasMoved = false;
                turnManager.player2.army[3].hasMoved = false;
                foreach (Tile tile in FindObjectsOfType<Tile>())
                {
                    if (tile.CharacterData != null)
                    {
                        tile.CharacterData.hasMoved = false;
                    }
                }
            }
        }
        else
        {
            int movedCharacters = 0;
            Character[] characters = FindObjectsOfType<Character>();
            foreach (Character character in turnManager.player1.army)
            {
                foreach (Character character2 in characters)
                {
                    if (character2.hasMoved && character2.internalId == character.internalId)
                    {
                        Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                        movedCharacters++;
                        break;
                    }
                }

            }
            Debug.Log("movedCharacters: " + movedCharacters + "Must be: " + countCharactersFromArmy(1));
            if (movedCharacters == countCharactersFromArmy(1))
            {
                turnManager.ChangeTurn();
                turnManager.player1.army[0].hasMoved = false;
                turnManager.player1.army[1].hasMoved = false;
                turnManager.player1.army[2].hasMoved = false;
                turnManager.player1.army[3].hasMoved = false;
                turnManager.player2.army[0].hasMoved = false;
                turnManager.player2.army[1].hasMoved = false;
                turnManager.player2.army[2].hasMoved = false;
                turnManager.player2.army[3].hasMoved = false;
                foreach (Tile tile in FindObjectsOfType<Tile>())
                {
                    if (tile.CharacterData != null)
                    {
                        tile.CharacterData.hasMoved = false;
                    }
                }
            }

        }

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
        if (!fromAttack)
        {
            WebSocketManager.Instance.SendMove(moveData);
        }

    }

    void unityMovesInAPath(Tile tileDestination)
    {
        Debug.Log("Unity moves in a path");
        Debug.Log("Tile destination: " + tileDestination.x + ", " + tileDestination.y);
        List<Vector3> path = new List<Vector3>();
        Vector3 startPosition = tileDestination.Character.transform.position;
        Vector3 endPosition = new Vector3(tileDestination.x, tileDestination.y, tileDestination.Character.transform.position.z);
        Vector3 aux = startPosition;
        // Play walk SFX in a loop
        PlayAudioManager audioManager = FindObjectOfType<PlayAudioManager>();
        if (audioManager != null)
        {
            audioManager.SFXSource.loop = true;
            audioManager.PlayWalkSFX();
        }
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
            animator.SetTrigger("IsMovingRight");
        }
        else
        {
            animator.SetTrigger("IsMovingLeft");
        }
        if (path.Count > 1)
        {
            Debug.Log("Moving horizontally to: " + endPosition.x + ", " + endPosition.y);
            iTween.MoveTo(tileDestination.Character, iTween.Hash("path", path.ToArray(), "time", 1, "easetype", iTween.EaseType.linear, "oncomplete", "MoveVertical", "oncompletetarget", gameObject, "oncompleteparams", new object[] { tileDestination.Character, startPosition, endPosition, animator, tileDestination, aux }));
        }
        else
        {
            MoveVertical(new object[] { tileDestination.Character, startPosition, endPosition, animator, tileDestination, aux });
        }
        // Stop walk SFX after movement
        if (audioManager != null)
        {
            audioManager.SFXSource.loop = false;
            audioManager.SFXSource.Stop();
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
        animator.SetTrigger("IsIdle");
        // Play walk SFX in a loop
        PlayAudioManager audioManager = FindObjectOfType<PlayAudioManager>();
        if (audioManager != null)
        {
            audioManager.SFXSource.loop = true;
            audioManager.PlayWalkSFX();
        }
        //animator.SetTrigger("IsMovingRight");
        //animator.SetTrigger("IsMovingLeft");

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
                    animator.SetTrigger("IsMovingDown");
                }
                else
                {
                    animator.SetTrigger("IsMovingUp");
                }

                if (path.Count > 0)
                {
                    iTween.MoveTo(tileDestination.Character, iTween.Hash("path", path.ToArray(), "time", 1, "easetype", iTween.EaseType.linear, "oncomplete", "OnMovementComplete", "oncompletetarget", gameObject, "oncompleteparams", animator));
                }

            }
        }
        // Stop walk SFX after movement
        if (audioManager != null)
        {
            audioManager.SFXSource.loop = false;
            audioManager.SFXSource.Stop();
        }
    }
    void OnMovementComplete(Animator animator)
    {
        animator.SetTrigger("IsIdle");
    }
    void OnMouseDown()
    {
        if ((turnManager.turn == 1 && UserManager.Instance.CurrentUser.id == turnManager.player1.id) || (turnManager.turn == 2 && UserManager.Instance.CurrentUser.id == turnManager.player2.id))
        {
            Debug.Log("Turn is correct, proceeding with action." + turnManager.turn);


            Debug.Log("Mouse clicked tile.");
            Debug.Log($"Tile position: x = {x}, y = {y}");

            GridManager gridManager = FindObjectOfType<GridManager>();
            if (gridManager == null) return;
            Debug.Log(turnManager.player1.army[0].internalId);
            Debug.Log(_characterData != null ? _characterData.internalId : "No character data");
            int userId = UserManager.Instance.CurrentUser.id;
            if (userId == turnManager.player1.id)
            {
                if (this.attackable && _characterData != null && !checkIfCharacterIsInTheSameArmyAsThePlayer(_characterData, turnManager.player1.id))
                {
                    Debug.Log("Attack action triggered!");
                    Attack();
                    return;
                }
                else if (_characterData != null && !checkIfCharacterIsInTheSameArmyAsThePlayer(_characterData, turnManager.player1.id))
                {
                    if (turnManager.player1.id == userId)
                    {
                        Debug.Log("Cannot control characters from army1.");
                    }
                    else if (turnManager.player2.id == userId)
                    {
                        Debug.Log("Cannot control characters from army2.");
                    }
                    return;
                }
            }
            else
            {
                if (this.attackable && _characterData != null && !checkIfCharacterIsInTheSameArmyAsThePlayer(_characterData, turnManager.player2.id))
                {
                    Debug.Log("Attack action triggered!");
                    Attack();
                    return;
                }
                else if (_characterData != null && !checkIfCharacterIsInTheSameArmyAsThePlayer(_characterData, turnManager.player2.id))
                {
                    if (turnManager.player1.id == userId)
                    {
                        Debug.Log("Cannot control characters from army1.");
                    }
                    else if (turnManager.player2.id == userId)
                    {
                        Debug.Log("Cannot control characters from army2.");
                    }
                    return;
                }
            }
            Tile tileOriginMovement = findTileWithCharacterSelected();
            if (tileOriginMovement != null && tileOriginMovement != this && this.movable)
            {
                if (Mathf.Abs(tileOriginMovement.x - this.x) <= 1 && Mathf.Abs(tileOriginMovement.y - this.y) <= 1)
                {
                    Debug.Log("The destination tile is adjacent. No need to move.");
                    return;
                }

                Debug.Log("The unit is on the tile x=" + tileOriginMovement.x + " y=" + tileOriginMovement.y);
                moveUnit(tileOriginMovement, this, false);
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
                Debug.Log("Character selected: " + _character.name);
                ApplyGrayscale();
                showMovementRange(this, _characterData);
            }
        }
        else
        {
            Debug.Log("It's not your turn or you are not the player.");
            if (turnManager.turn == 1 && turnManager.player1 == UserManager.Instance.CurrentUser)
            {
                Debug.Log("It's your turn, but you are not the player.");
            }
            else if (turnManager.turn == 2 && turnManager.player2 == UserManager.Instance.CurrentUser)
            {
                Debug.Log("It's your turn, but you are not the player.");
            }
            else
            {
                Debug.Log("It's not your turn. " + turnManager.turn + " current turn " + (UserManager.Instance.CurrentUser.id == turnManager.player1.id));
            }
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
        // attackerTile.CharacterData.hasAttacked = true;
        
        Debug.Log($"{attackerTile.CharacterData.name} attacked {this.CharacterData.name} for {damage} damage. Remaining health: {this.CharacterData.actualHealth}");
        if (Mathf.Abs(attackerTile.x - this.x) > 1 || Mathf.Abs(attackerTile.y - this.y) > 1)
        {
            WebSocketManager.Instance.SendAttack(new WebSocketManager.AttackData
            {
                origin = new WebSocketManager.Position { x = attackerTile.x, y = attackerTile.y },
                destination = new WebSocketManager.Position { x = this.x, y = this.y },
                closest = new WebSocketManager.Position { x = FindClosestMovableTile(attackerTile, this).x, y = FindClosestMovableTile(attackerTile, this).y },
                userId = userId
            });
        }
        else
        {
            attackerTile.CharacterData.hasMoved = true;
        if (attackerTile.CharacterData.internalId > 3)
        {
            int movedCharacters = 0;
            Character[] characters = FindObjectsOfType<Character>();
            foreach (Character character in turnManager.player2.army)
            {
                foreach (Character character2 in characters)
                {
                    if (character2.hasMoved && character2.internalId == character.internalId)
                    {
                        movedCharacters++;
                        break;
                    }
                }

            }
            if (movedCharacters == countCharactersFromArmy(2))
            {
                turnManager.ChangeTurn();
                turnManager.player1.army[0].hasMoved = false;
                turnManager.player1.army[1].hasMoved = false;
                turnManager.player1.army[2].hasMoved = false;
                turnManager.player1.army[3].hasMoved = false;
                turnManager.player2.army[0].hasMoved = false;
                turnManager.player2.army[1].hasMoved = false;
                turnManager.player2.army[2].hasMoved = false;
                turnManager.player2.army[3].hasMoved = false;
                foreach (Tile tile in FindObjectsOfType<Tile>())
                {
                    if (tile.CharacterData != null)
                    {
                        tile.CharacterData.hasMoved = false;
                    }
                }
            }
        }
        else
        {
            int movedCharacters = 0;
            Character[] characters = FindObjectsOfType<Character>();
            foreach (Character character in turnManager.player1.army)
            {
                foreach (Character character2 in characters)
                {
                    if (character2.hasMoved && character2.internalId == character.internalId)
                    {
                        Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                        movedCharacters++;
                        break;
                    }
                }

            }
            Debug.Log("movedCharacters: " + movedCharacters);
            if (movedCharacters == countCharactersFromArmy(1))
            {
                turnManager.ChangeTurn();
                turnManager.player1.army[0].hasMoved = false;
                turnManager.player1.army[1].hasMoved = false;
                turnManager.player1.army[2].hasMoved = false;
                turnManager.player1.army[3].hasMoved = false;
                turnManager.player2.army[0].hasMoved = false;
                turnManager.player2.army[1].hasMoved = false;
                turnManager.player2.army[2].hasMoved = false;
                turnManager.player2.army[3].hasMoved = false;
                foreach (Tile tile in FindObjectsOfType<Tile>())
                {
                    if (tile.CharacterData != null)
                    {
                        tile.CharacterData.hasMoved = false;
                    }
                }
            }

        }

            WebSocketManager.Instance.SendAttack(new WebSocketManager.AttackData
            {
                origin = new WebSocketManager.Position { x = attackerTile.x, y = attackerTile.y },
                destination = new WebSocketManager.Position { x = this.x, y = this.y },
                closest = null,
                userId = userId
            });
        }

        if (Mathf.Abs(attackerTile.x - this.x) > 1 || Mathf.Abs(attackerTile.y - this.y) > 1)
        {
            Tile closestTile = FindClosestMovableTile(attackerTile, this);
            if (closestTile != null)
            {
                Debug.Log($"Moving to closest attack position at: {closestTile.x}, {closestTile.y}");
                moveUnit(attackerTile, closestTile, true);
                attackerTile= closestTile;
            }
        }

        if (attackerTile.Character != null)
        {
            Animator animator = attackerTile.Character.GetComponent<Animator>();
            if (animator != null)
            {
                string attackDirection = "";
                if (attackerTile.x < this.x)
                    attackDirection = "IsAttackingRight";
                else if (attackerTile.x > this.x)
                    attackDirection = "IsAttackingLeft";
                else if (attackerTile.y > this.y)
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

        if (this.CharacterData.actualHealth <= 0)
        {
            Debug.Log($"{this.CharacterData.name} has been defeated!");
            if (checkIfArmyIsDead(this.CharacterData))
            {
                Debug.Log("Army is dead, ending game.");
                WebSocketManager.Instance.SendEndGame(UserManager.Instance.CurrentUser.id);

            }
            if (this.Character != null)
            {
                Destroy(this.Character);
                this.Character = null;
            }
            this.CharacterData = null;
            this.isOccupied = false;
        }


    }
    bool checkIfArmyIsDead(Character character)
    {
        if (character == null)
        {
            Debug.Log("No character found.");
            return false;
        }
        Debug.Log("Character Id: " + character.id);
        Debug.Log("Character: " + character.internalId);
        Character[] characters = FindObjectsOfType<Character>();
        int charCount = 0;
        foreach (Character c in characters)
        {
            if (character.internalId > 4)
            {
                if (c.internalId > 3 && c.internalId < 8 && c.actualHealth > 0)
                {
                    charCount++;
                }
            }
            else
            {
                if (c.internalId < 4 && c.internalId > -1 && c.actualHealth > 0)
                {
                    charCount++;

                }
            }


        }
        return charCount > 4 ? false : true;
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
        // Debug.Log("Attacker: " + attackerTile.CharacterData.name);
        // Debug.Log("Target: " + targetTile.CharacterData.name);
        AnimateAttack(attackerTile, targetTile);
    }

    void AnimateAttack(Tile attackerTile, Tile targetTile)
    {
        Debug.Log("AnimateAttack called");
        Debug.Log(JsonUtility.ToJson(attackerTile.CharacterData, true)); // Muestra toda la info del Tile
        Debug.Log(JsonUtility.ToJson(targetTile.CharacterData, true)); // Muestra toda la info del personaje

        targetTile = GameObject.Find("Tile " + targetTile.x + " " + targetTile.y).GetComponent<Tile>();
        Debug.Log("TargetTile: " + targetTile.x + " " + targetTile.y);
        //        Debug.Log("TargetTile: " + targetTile.CharacterData.name);
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
                animator.SetTrigger(attackDirection);
                // Play attack SFX
                PlayAudioManager audioManager = FindObjectOfType<PlayAudioManager>();
                if (audioManager != null)
                {
                    audioManager.PlayAttackSFX();
                }
                else
                {
                    Debug.LogWarning("⚠️ PlayAudioManager not found.");
                }
                StartCoroutine(ResetBoolAfterAnimation(animator, attackDirection));
            }
            else
            {
                Debug.LogWarning("⚠️ Animator not found.");
            }
        }

        attackerTile.CharacterData.selected = false;
        // Debug.Log(JsonConvert.SerializeObject(targetTile.CharacterData, Formatting.Indented));
        if (targetTile.CharacterData.actualHealth <= 0)
        {
            Debug.Log($"{targetTile.CharacterData.name} has been defeated!");
            if (targetTile.Character != null)
            {
                Animator animator = targetTile.CharacterData.GetComponent<Animator>();
                animator.SetTrigger("IsDead");
                // Play death SFX
                PlayAudioManager audioManager = FindObjectOfType<PlayAudioManager>();
                if (audioManager != null)
                {
                    audioManager.PlayDeathSFX();
                }
                else
                {
                    Debug.LogWarning("⚠️ PlayAudioManager not found.");
                }
                if (checkIfArmyIsDead(this.CharacterData) && 
                    ((turnManager.turn == 1 && UserManager.Instance.CurrentUser.id == turnManager.player1.id) || 
                     (turnManager.turn == 2 && UserManager.Instance.CurrentUser.id == turnManager.player2.id)))
                {
                    Debug.Log("Army is dead, ending game.");
                    WebSocketManager.Instance.SendEndGame(UserManager.Instance.CurrentUser.id);
                }
                StartCoroutine(WaitAndDestroy(targetTile.Character, 1));
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
    private IEnumerator WaitAndDestroy(GameObject character, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Destroy(character);
    }
    IEnumerator ResetBoolAfterAnimation(Animator animator, string boolName)
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        animator.SetTrigger("IsIdle");
    }

    Tile FindClosestMovableTile(Tile attackerTile, Tile targetTile)
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
        Debug.Log("BMC" + allTiles.Length);
        Tile closestTile = null;
        float minDistance = float.MaxValue;
        Debug.Log("AAAAAAAAAAAAAAAAAAAA");
        Debug.Log("minDistance:" + minDistance);
        Debug.Log("targetTile.x: " + targetTile.x);
        Debug.Log("targetTile.y: " + targetTile.y);
        Debug.Log("attackerTile.x: " + attackerTile.x);
        Debug.Log("attackerTile.y: " + attackerTile.y);
        Debug.Log("AttackerTile: " + attackerTile.CharacterData.name);
        Debug.Log("TargetTile: " + targetTile.CharacterData.name);
        foreach (Tile tile in allTiles)
        {
            if (!tile.isOccupied && tile.movable)
            {
                float distance = Vector2.Distance(new Vector2(tile.x, tile.y), new Vector2(targetTile.x, targetTile.y));
                Debug.Log("Distance: " + distance);

                if (distance < minDistance)
                {
                    Debug.Log("This is the tile" + tile.x + " " + tile.y);
                    minDistance = distance;
                    closestTile = tile;
                }
            }

        }
        // Debug.Log("MMMMMMMMM", closestTile.CharacterData);
        // Debug.Log($"AAAAAAAAAAAAAAAaa Closest tile found at: {closestTile.x}, {closestTile.y}");
        return closestTile;
    }

    void showMovementRange(Tile tile, Character _characterData)
    {
        Debug.Log("Te enseño de que va la movida");
        Debug.Log("Character data: " + _characterData.hasMoved);
        if (_characterData.hasMoved)
        {
            return;
        }

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
                            Debug.Log("Found tile: " + foundTile.name);
                            ApplyTileBlue(foundTile.GetComponent<Tile>());
                        }
                    }
                    if (true)
                    {
                        GameObject foundTile = GameObject.Find($"Tile {x - i} {y + j}");
                        if (foundTile != null)
                        {
                            Debug.Log("Found tile: " + foundTile.name);
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
        Debug.Log("Applying blue tile");
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
                int userId = UserManager.Instance.CurrentUser.id;
                if (tile.CharacterData != null && CharacterData != null && checkIfCharacterIsInTheSameArmyAsThePlayer(tile.CharacterData, userId) && checkIfCharacterIsInTheSameArmyAsThePlayer(CharacterData, userId))
                {
                    Debug.Log("Same army, do not mark as attackable");
                    Renderer renderer = tile.Character.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material material = renderer.material;
                        material.color = Color.green;
                    }
                }
                else
                {
                    Debug.Log("Different army, mark as attackable");
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
                if (checkIfCharacterIsInTheSameArmyAsThePlayer(tile.CharacterData, userId) && !checkIfCharacterIsInTheSameArmyAsThePlayer(CharacterData, userId))
                {
                    Debug.Log("Same army, do not mark as attackable");
                    // Same army, do not mark as attackable
                    tile.attackable = false;
                    return;
                }
            }
        }

        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            Debug.Log("Applying red tile");
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
    bool checkIfCharacterIsInTheSameArmyAsThePlayer(Character character, int userId)
    {
        if (character == null)
        {
            Debug.Log("No character found.");
            return false;
        }
        Debug.Log("Character Id: " + character.id);
        Debug.Log("Character: " + character.internalId);
        if (userId == TurnManager.Instance.player1.id && character.internalId < 4)
        {
            Debug.Log("Se supone que eres el jugador 1 y el personaje está en el ejército 1");
            return TurnManager.Instance.player1.army[character.internalId].id == character.id && turnManager.player1.army[character.internalId].actualHealth == character.actualHealth;
        }
        else
        {
            if (userId == TurnManager.Instance.player2.id && character.internalId > 3)
            {
                Debug.Log("Se supone que eres el jugador 2 y el personaje está en el ejército 2");
                return TurnManager.Instance.player2.army[character.internalId - 4].id == character.id && turnManager.player2.army[character.internalId - 4].actualHealth == character.actualHealth;
            }
            else
            {
                Debug.Log("No eres el dueño del personaje");
                return false;
            }
        }
    }
}