using UnityEngine;

public class BarrelStartTrigger : MonoBehaviour
{
    private Barrel barrel;

    private void Awake()
    {
        barrel = GetComponentInParent<Barrel>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            barrel.NotifyPlayerEntered();
    }
}
