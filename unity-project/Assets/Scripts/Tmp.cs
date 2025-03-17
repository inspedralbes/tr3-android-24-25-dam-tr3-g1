using UnityEngine;

public class Tmp : MonoBehaviour
{

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Detecta clic izquierdo
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 0f; // Si estás en 2D, puedes dejarlo en 0
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Vector3Int wPos = new Vector3Int((int)worldPosition.x, (int)worldPosition.y, 0);
            Debug.Log("World Position: " + wPos);
        }
    }
}
