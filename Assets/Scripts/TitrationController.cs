using UnityEngine;
using TMPro;


public class TitrationController : MonoBehaviour, IInteractable
{
    [Header("Beaker Liquid Visual")]
    public Renderer liquidRenderer;     
    public Color startColor = Color.clear;     
    public Color endColor = new Color(1f, 0.7f, 0.8f); 

    [Header("Titration Settings")]
    [Range(0f, 1f)] public float equivalencePoint = 0.5f; 
    [Range(0f, 1f)] public float acceptableTolerance = 0.05f; 
    public float fillSpeed = 0.15f; 

    [Header("UI")]
    public TextMeshProUGUI volumeText; 

    [Header("Feedback")]
    public GameObject overshootWarning; 

    private float amountAdded = 0f;
    private bool isAdding = false;
    private bool hasFinished = false;

    public string GetPrompt()
    {
        if (hasFinished) return "Titration complete";
        return "Hold E to add acid, release to stop";
    }

    public void Interact(GameObject player)
    {
        if (!SafetyGearManager.Instance.TryDoRiskyAction())
        {
            return;
        }


        if (!isAdding && !hasFinished)
        {
            StartAdding();
        }
        else if (isAdding)
        {
            StopAdding();
        }
    }

    void Update()
    {
        if (isAdding && !hasFinished)
        {
            amountAdded += fillSpeed * Time.deltaTime;
            amountAdded = Mathf.Clamp01(amountAdded);
            UpdateVisuals();


            if (amountAdded >= 1f)
            {
                StopAdding();
                FinishTitration();
            }
        }
    }

    void StartAdding()
    {
        isAdding = true;
    }

    void StopAdding()
    {
        isAdding = false;
        FinishTitration();
    }

    void UpdateVisuals()
    {
        if (liquidRenderer != null)
        {

            float t = Mathf.Clamp01((amountAdded - (equivalencePoint - acceptableTolerance)) /
                                     (acceptableTolerance * 2f));
            liquidRenderer.material.color = Color.Lerp(startColor, endColor, t);
        }

        if (volumeText != null)
        {
            volumeText.text = $"Volume added: {(amountAdded * 100f):F0}%";
        }
    }

    void FinishTitration()
    {
        hasFinished = true;
        float error = Mathf.Abs(amountAdded - equivalencePoint);
        bool success = error <= acceptableTolerance;

        if (success)
        {
            Debug.Log($"Titration correct! Stopped at {amountAdded:F2}, target was {equivalencePoint:F2}.");
            ProcedureManager.Instance?.ReportEvent("TitrationCorrect");
        }
        else
        {
            Debug.Log($"Titration missed. Stopped at {amountAdded:F2}, target was {equivalencePoint:F2}, error {error:F2}.");
            if (overshootWarning != null)
            {
                overshootWarning.SetActive(true);
                Invoke(nameof(HideOvershootWarning), 2.5f);
            }
            ProcedureManager.Instance?.ReportEvent("TitrationMistake");
        }
    }

    void HideOvershootWarning()
    {
        if (overshootWarning != null) overshootWarning.SetActive(false);
    }

    public void ResetTitration()
    {
        amountAdded = 0f;
        isAdding = false;
        hasFinished = false;
        UpdateVisuals();
    }
}
