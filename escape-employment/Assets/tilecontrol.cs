using UnityEngine;

public class TileController : MonoBehaviour
{
    [SerializeField] private pressureTile w1;
    [SerializeField] private pressureTile w2;

    void Update()
    {
        if (w1.IsPressed && w2.IsPressed)
        {
            Debug.Log("Both tiles are pressed!");
            // Your activation logic here
        }
    }
}
