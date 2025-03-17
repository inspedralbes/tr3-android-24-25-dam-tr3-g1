using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Animator _animator;
    bool isMovingLeft = false;
    bool isMovingRight = false;
    bool isMovingUp = false;
    bool isMovingDown = false;
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
