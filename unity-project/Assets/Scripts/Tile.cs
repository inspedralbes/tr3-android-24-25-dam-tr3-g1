using UnityEngine;

public class Tile : MonoBehaviour

[SerializedField] private int _highlight;

{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onMouseEnter()
    {
        Debug.Log("Mouse entered tile.");
        _highlight.SetActive(true);
    }

    void onMouseExit()
    {
        Debug.Log("Mouse exited tile.");
        _highlight.SetActive(false);
    }
    
}
