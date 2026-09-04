using System.Collections.Generic;
using UnityEngine;

public class CollectedAksaraManager : MonoBehaviour
{
    public static CollectedAksaraManager Instance { get; private set; }

    private HashSet<GestureShape> droppedThisWave =
        new HashSet<GestureShape>();

    private List<AksaraData> collectedThisWave =
        new List<AksaraData>();

    [Header("Collect SFX")]
    [SerializeField] private bool useCollectSFX = true;
    [SerializeField] private string collectSFXName = "CollectAksara";

    [Range(0f, 1f)]
    [SerializeField] private float collectSFXVolume = 1f;

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
            Debug.Log(
                $"[CollectedAksaraManager] Drop for {shape} already registered this wave."
            );

            return false;
        }

        droppedThisWave.Add(shape);

        Debug.Log(
            $"[CollectedAksaraManager] Registered drop for {shape}."
        );

        return true;
    }

    public void RegisterCollect(AksaraData aksaraData)
    {
        if (aksaraData == null)
            return;

        if (IsCollected(aksaraData))
        {
            Debug.Log(
                $"[CollectedAksaraManager] Aksara already collected: {aksaraData.AksaraName}"
            );

            return;
        }

        collectedThisWave.Add(aksaraData);

        if (useCollectSFX && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                collectSFXName,
                collectSFXVolume
            );
        }

        Debug.Log(
            $"[CollectedAksaraManager] Collected aksara: {aksaraData.AksaraName}"
        );
    }

    public bool IsCollected(AksaraData aksaraData)
    {
        if (aksaraData == null)
            return false;

        foreach (var collectedAksara in collectedThisWave)
        {
            if (collectedAksara == aksaraData)
                return true;
        }

        return false;
    }

    public bool IsCollected(GestureShape shape)
    {
        foreach (var collectedAksara in collectedThisWave)
        {
            if (
                collectedAksara != null &&
                collectedAksara.GestureShape == shape
            )
            {
                return true;
            }
        }

        return false;
    }

    public void ResetWave()
    {
        droppedThisWave.Clear();
        collectedThisWave.Clear();

        Debug.Log(
            "[CollectedAksaraManager] Wave reset."
        );
    }

    public IReadOnlyList<AksaraData> CollectedAksara =>
        collectedThisWave.AsReadOnly();
}