using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIUtils : MonoBehaviour
{
    private List<(string referenceName, Coroutine coroutine)> coroutinesByName = new List<(string, Coroutine)>();
    public void StopCoroutineByReference(string referenceName)
    {
        try
        {
            Coroutine coroutine = coroutinesByName.Find(c => c.referenceName == referenceName).coroutine;
            StopCoroutine(coroutine);
        }
        catch (Exception e)
        {
            Debug.LogError($"Coroutine with reference name {referenceName} not found. {e.Message}");
        }
    }
    public void AnimateIcon(string referenceName, VisualElement icon, List<Texture2D> frames, float delayBetweenFrames)
    {
        coroutinesByName.Add((referenceName, StartCoroutine(AnimateIconCoroutine(icon, frames, delayBetweenFrames))));
    }
    private IEnumerator AnimateIconCoroutine(VisualElement icon, List<Texture2D> frames, float delayBetweenFrames)
    {
        int frame = 0;
        while(true)
        {
            frame++;
            if (frame >= frames.Count)
            {
                frame = 0;
            }
            icon.style.backgroundImage = frames[frame];
            yield return new WaitForSeconds(delayBetweenFrames);
        }
    }
}
