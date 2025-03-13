using UnityEngine;


public class ArmyManager : MonoBehaviour
{
    CharacterList characterList = new CharacterList();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCharacter(Character character)
    {
        characterList.AddCharacter(character);
    }

    public bool checkIfCharacterIsInList(Character character)
    {
        return characterList.checkIfCharacterIsInList(character);
    }

}
