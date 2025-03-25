using UnityEngine;
using System.Collections.Generic;
using System;

public class User
{
    public int id;
    public string username;
    public string password;
    public string email;
    public List<Character> army;
    public int elo;
    public int victories;
    public int defeats;

    public override string ToString()
    {
        string armyString = string.Join(", ", army);
        return $"User: {username}, Email: {email}, ELO: {elo}, Victories: {victories}, Defeats: {defeats}, Army: [{armyString}]";
    }
}
