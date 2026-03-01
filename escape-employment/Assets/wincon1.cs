using UnityEngine;

public class TileActivator : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

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

