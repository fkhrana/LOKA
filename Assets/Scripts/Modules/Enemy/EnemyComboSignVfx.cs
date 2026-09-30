using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyGestureCommand))]
public class EnemyComboSignVfx : MonoBehaviour
{
    [SerializeField] private GameObject comboSignVfx;

    private EnemyGestureCommand enemyGestureCommand;

    private void Awake()
    {
        enemyGestureCommand = GetComponent<EnemyGestureCommand>();

        if (comboSignVfx == null)
            return;

        comboSignVfx.SetActive(false);
        SetComboSignSortingOrder();
    }

    private void LateUpdate()
    {
        bool shouldShow = comboSignVfx != null
            && PowerManager.IsComboActive
            && EnemyGestureCommand.HasOtherActiveEnemyWithin(
                enemyGestureCommand,
                PowerManager.ActiveComboRadius
            );

        SetComboSignVisible(shouldShow);
    }

    private void OnDisable()
    {
        SetComboSignVisible(false);
    }

    private void SetComboSignVisible(bool visible)
    {
        if (comboSignVfx == null || comboSignVfx.activeSelf == visible)
            return;

        if (visible)
        {
            comboSignVfx.SetActive(true);
            foreach (ParticleSystem particles in comboSignVfx.GetComponentsInChildren<ParticleSystem>(true))
                particles.Play(true);
        }
        else
        {
            foreach (ParticleSystem particles in comboSignVfx.GetComponentsInChildren<ParticleSystem>(true))
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            comboSignVfx.SetActive(false);
        }
    }

    private void SetComboSignSortingOrder()
    {
        Renderer[] comboRenderers = comboSignVfx.GetComponentsInChildren<Renderer>(true);
        if (comboRenderers.Length == 0)
            return;

        Renderer[] enemyRenderers = GetComponentsInChildren<Renderer>(true);
        int sortingOrder = int.MinValue;
        int sortingLayerId = 0;

        foreach (Renderer enemyRenderer in enemyRenderers)
        {
            if (enemyRenderer == null || enemyRenderer.transform.IsChildOf(comboSignVfx.transform))
                continue;

            if (enemyRenderer.sortingOrder > sortingOrder)
            {
                sortingOrder = enemyRenderer.sortingOrder;
                sortingLayerId = enemyRenderer.sortingLayerID;
            }
        }

        if (sortingOrder == int.MinValue)
            return;

        foreach (Renderer comboRenderer in comboRenderers)
        {
            if (comboRenderer == null)
                continue;

            comboRenderer.sortingLayerID = sortingLayerId;
            comboRenderer.sortingOrder = sortingOrder + 1;
        }
    }
}