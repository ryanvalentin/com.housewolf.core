using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to a GameObject to automatically ensure the item is pooled, and optionally recycle after
/// a given interval.
/// </summary>
public class ObjectPoolItem : MonoBehaviour
{
    [Tooltip("How many objects to create initially in this pool. For some object if you anticipate having at least a certain number, pass it here.")]
    public int InitialPoolSize = 0;

    [Tooltip("Whether to automatically recycle this item after AutoRecycleTime when enabled.")]
    public bool AutoRecycle = false;

    [Tooltip("How long to wait until automatically recycling this object when enabled.")]
    public float AutoRecycleTime = 1f;

    public void RecycleItem(float? overrideTime = null)
    {
        float time = overrideTime == null ? AutoRecycleTime : (float)overrideTime;

        StartCoroutine(RecycleRoutine(time));
    }

    private void OnEnable()
    {
        if (AutoRecycle)
            RecycleItem(AutoRecycleTime);
    }

    private IEnumerator RecycleRoutine(float time)
    {
        yield return new WaitForSeconds(time);

        gameObject.Recycle();
    }
}
