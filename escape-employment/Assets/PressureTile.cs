using UnityEngine;

public class PressureTile : MonoBehaviour
{
    public bool IsPressed { get; private set; }

    void OnTriggerEnter(Collider other)
    {
        
            IsPressed = true;
    }

    void OnTriggerExit(Collider other)
    {
        
            IsPressed = false;
    }
}
