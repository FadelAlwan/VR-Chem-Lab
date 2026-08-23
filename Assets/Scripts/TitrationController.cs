using UnityEngine;
using TMPro;

// Attach this to the Beaker object at the titration station.
// Simulates titration WITHOUT real fluid physics: a simple "amount added" value
// drives both a UI progress readout and a color lerp on the beaker's liquid material.
//
// Player holds E (or clicks) while looking at the Burette to add acid gradually.
// The goal is to release/stop within an acceptable range around the hidden
// equivalence point — overshooting or stopping too early counts as a mistake.
public class TitrationController : MonoBehaviour, IInteractable
{
    [Header("Beaker Liquid Visual")]
    public Renderer liquidRenderer;      // the Renderer of the liquid part of the beaker mesh
    public Color startColor = Color.clear;      // colorless before reaching equivalence
    public Color endColor = new Color(1f, 0.7f, 0.8f); // light pink, like phenolphthalein past equivalence

    [Header("Titration Settings")]
    [Range(0f, 1f)] public float equivalencePoint = 0.5f; // hidden "correct" stop point (0-1)
    [Range(0f, 1f)] public float acceptableTolerance = 0.05f; // how close counts as "correct"
    public float fillSpeed = 0.15f; // how fast amount increases per second while holding

    [Header("UI")]
    public TextMeshProUGUI volumeText; // optional: shows "Volume added: 42%"

    [Header("Feedback")]
    public GameObject overshootWarning; // shown if student overshoots significantly

    private float amountAdded = 0f;
    private bool isAdding = false;
    private bool hasFinished = false;

    // ---- IInteractable ----
    public string GetPrompt()
    {
        if (hasFinished) return "Titration complete";
        return "Hold E to add acid, release to stop";
    }

    public void Interact(GameObject player)
    {
        // Safety check first — this is the same pattern used by PourableContainer.
        if (!SafetyGearManager.Instance.TryDoRiskyAction())
        {
            return;
        }

        // Single E press starts/stops adding; for a "hold" feel, call StartAdding/StopAdding
        // from Update() based on Input.GetKey instead — see note below.
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

            // Auto-stop and fail if it goes far past the equivalence point without release —
            // prevents the student from just holding forever.
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
            // Color lerps fastest right around the equivalence point to mimic an indicator's
            // sharp color change, rather than a flat linear blend across the whole range.
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
            AssessmentManager.Instance?.ReportEvent("TitrationCorrect");
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
            AssessmentManager.Instance?.ReportEvent("TitrationMistake");
        }
    }

    void HideOvershootWarning()
    {
        if (overshootWarning != null) overshootWarning.SetActive(false);
    }

    // Call this to let the player redo the experiment (e.g. from a "Retry" button).
    public void ResetTitration()
    {
        amountAdded = 0f;
        isAdding = false;
        hasFinished = false;
        UpdateVisuals();
    }
}
