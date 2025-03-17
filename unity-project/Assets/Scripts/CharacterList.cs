using System;
using System.Collections.Generic;

[Serializable]
public class CharacterList
{
    public List<Character> characters;

    public bool checkIfCharacterIsInList(Character character)
    {
        return characters.Contains(character);
    }

    public void AddCharacter(Character character)
    {
        characters.Add(character);
    }
}