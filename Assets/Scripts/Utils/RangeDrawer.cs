using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RangeDrawer
{
    public static GameObject DrawCircle(Transform parent, float range,
     Color color, float width = 0.1f, int precision = 60)
    {
        GameObject circle = new GameObject("Circle");
        circle.transform.SetParent(parent);
        circle.transform.localPosition = Vector3.zero;
        LineRenderer lineRenderer = circle.AddComponent<LineRenderer>();

        lineRenderer.positionCount = precision;
        lineRenderer.useWorldSpace = false;

        for(int currentStep=0; currentStep<precision; currentStep++)
        {
            float circumferenceProgress = (float)currentStep/(precision-1);
 
            float currentRadian = circumferenceProgress * 2 * Mathf.PI;
            
            float xScaled = Mathf.Cos(currentRadian);
            float yScaled = Mathf.Sin(currentRadian);
 
            float x = range * xScaled;
            float y = range * yScaled;
            float z = 0;
 
            Vector3 currentPosition = new Vector3(x,y,z);
 
            lineRenderer.SetPosition(currentStep,currentPosition);
        }
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
        lineRenderer.material.color = color;
        
        return circle;
    }

}
