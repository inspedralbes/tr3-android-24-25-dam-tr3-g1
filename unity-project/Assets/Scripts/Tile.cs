using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private GameObject _highlight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseEnter()
    {
        Debug.Log("Mouse entered tile.");
        _highlight.SetActive(true);
    }

    void OnMouseExit()
    {
        Debug.Log("Mouse exited tile.");
        _highlight.SetActive(false);
    }
    void OnMouseDown()
    {
        Debug.Log("Mouse clicked tile.");
    }
}
