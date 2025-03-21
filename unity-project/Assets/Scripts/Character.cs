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
    public bool winged;
    public string sprite;
    public string icon;
    public int atk;
    public int movement;
    public int health;
    public int actualHealth;
    public int range;
    public bool hasMoved=false;
    public bool selected = false;

}