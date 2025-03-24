
using System;
using UnityEngine;
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
    public float atk;
    public int movement;
    public int health;
    public int actualHealth;
    public int price;
    public bool hasMoved;
    public bool selected;
}
// public Character(int id, string name, string weapon, int vs_sword, int vs_spear, int vs_axe, int vs_bow, int vs_magic, int distance, bool winged, string sprite, string icon, int atk, int movement, int health, int price)
// {
//     this.id = id;
//     this.name = name;
//     this.weapon = weapon;
//     this.vs_sword = vs_sword;
//     this.vs_spear = vs_spear;
//     this.vs_axe = vs_axe;
//     this.vs_bow = vs_bow;
//     this.vs_magic = vs_magic;
//     this.distance = distance;
//     this.winged = winged;
//     this.sprite = sprite;
//     this.icon = icon;
//     this.atk = atk;
//     this.movement = movement;
//     this.health = health;
//     this.price = price;
//     this.hasMoved = false;
//     this.selected = false;
// }