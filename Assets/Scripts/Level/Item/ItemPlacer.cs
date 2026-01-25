using UnityEngine;

public class ItemPlacer : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;

    [Range(0f, 1f)]
    [SerializeField] private float spawnChance = 0.35f;

    public void Place(Transform spawnPointsRoot, Transform instancesRoot)
    {
        if (!itemPrefab || !spawnPointsRoot || !instancesRoot) return;

        var points = spawnPointsRoot.GetComponentsInChildren<ItemSpawnPoint>(includeInactive: false);

        foreach (var sp in points)
        {
            if (Random.value > spawnChance) continue;

            var o = Instantiate(itemPrefab, sp.transform.position, Quaternion.identity);
            o.transform.SetParent(instancesRoot, worldPositionStays: true);
        }
    }
}
