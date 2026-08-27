using UnityEngine;

public class DistillationBeaker : MonoBehaviour
{
    [Header("Visual")]
    public Renderer liquidRenderer;
    public Color emptyColor = Color.clear;

    private float liquidBaseScaleY, liquidBasePosY;

    void Start()
    {
        if (liquidRenderer != null)
        {
            liquidBaseScaleY = liquidRenderer.transform.localScale.y;
            liquidBasePosY = liquidRenderer.transform.localPosition.y;
        }
        SetColor(emptyColor);
        SetLevel(0f);
    }

    public void SetPartialLevel(float t, Color color)
    {
        SetColor(color);
        SetLevel(t);
    }

    public void ReceiveFraction(string substanceName, Color color)
    {
        SetColor(color);
        SetLevel(1f);
    }

    void SetColor(Color c)
    {
        if (liquidRenderer != null) liquidRenderer.material.SetColor("_BaseColor", c);
    }

    void SetLevel(float t)
    {
        if (liquidRenderer == null) return;
        Transform lt = liquidRenderer.transform;
        float minScaleY = liquidBaseScaleY * 0.05f;
        float targetScaleY = Mathf.Lerp(minScaleY, liquidBaseScaleY, Mathf.Clamp01(t));
        Vector3 scale = lt.localScale;
        lt.localScale = new Vector3(scale.x, targetScaleY, scale.z);
        float scaleDelta = liquidBaseScaleY - targetScaleY;
        lt.localPosition = new Vector3(lt.localPosition.x, liquidBasePosY - (scaleDelta / 2f), lt.localPosition.z);
    }

    public void ResetBeaker()
    {
        SetColor(emptyColor);
        SetLevel(0f);
    }
}
