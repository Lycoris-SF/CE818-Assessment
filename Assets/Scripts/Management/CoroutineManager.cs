using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    private Coroutine activeCoroutine = null;
    private bool canBeOverWrite = true;
    private float remainingTime = 0;

    public void StartCoroutine(IEnumerator coroutine, bool canbeoverwrite = true, bool canBeInterrupted = true)
    {
        if (activeCoroutine != null && !canbeoverwrite && !canBeInterrupted)
        {
            return;
        }

        StopCoroutine();

        activeCoroutine = base.StartCoroutine(coroutine);
        canBeOverWrite = canbeoverwrite;
    }

    public void StopCoroutine()
    {
        if (activeCoroutine != null)
        {
            base.StopCoroutine(activeCoroutine);
            activeCoroutine = null;
            remainingTime = 0;
        }
    }

    public IEnumerator RunWithTimer(float duration)
    {
        float remainingTime = duration;

        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            Debug.Log("Remaining: " + remainingTime);
            yield return null;
        }

        Debug.Log("Complete");
    }

    public float GetRemainingTime()
    {
        return remainingTime;
    }
}
