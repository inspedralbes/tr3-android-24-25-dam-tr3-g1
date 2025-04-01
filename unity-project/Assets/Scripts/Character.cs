using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading;

[Serializable]
public class Character : MonoBehaviour
{
    public int id;
    public string name;
    public string weapon;
    public float vs_sword;
    public float vs_spear;
    public float vs_axe;
    public float vs_bow;
    public float vs_magic;
    public int distance;
    public bool winged;
    public string sprite;
    public string icon;
    public int atk;
    public int movement;
    public int health;
    public int actualHealth;
    public int price;
    public bool hasMoved;
    public bool selected;
    public int internalId;

    [JsonIgnore]
    public CancellationToken destroyCancellationToken;
}