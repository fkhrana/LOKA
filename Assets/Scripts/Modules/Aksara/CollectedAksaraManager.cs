using System.Collections.Generic;
using UnityEngine;

public class CollectedAksaraManager : MonoBehaviour
{
    public static CollectedAksaraManager Instance { get; private set; }

    private HashSet<GestureShape> droppedThisWave = new HashSet<GestureShape>();
    private List<AksaraData> collectedThisWave = new List<AksaraData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool TryRegisterDrop(GestureShape shape)
    {
        if (droppedThisWave.Contains(shape))
        {
            Debug.Log($"[CollectedAksaraManager] Drop for {shape} already registered this wave.");
            return false;
        }
        droppedThisWave.Add(shape);
        Debug.Log($"[CollectedAksaraManager] Registered drop for {shape}.");
        return true;
    }

    public void RegisterCollect(AksaraData aksaraData)
    {
        if (aksaraData == null)
            return;

        collectedThisWave.Add(aksaraData);
        Debug.Log($"[CollectedAksaraManager] Collected aksara: {aksaraData.AksaraName}");
    }

    public void ResetWave()
    {
        droppedThisWave.Clear();
        collectedThisWave.Clear();
        Debug.Log("[CollectedAksaraManager] Wave reset.");
    }

    public IReadOnlyList<AksaraData> CollectedAksara => collectedThisWave.AsReadOnly();
}
