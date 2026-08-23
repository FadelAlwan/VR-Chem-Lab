using UnityEngine;

// Attach to the Beaker. Cups (ChemicalCup.cs) call AddChemical() on this when poured.
// Mixing both chemicals produces a new color — a simple, clear stand-in for "a reaction happened".
public class BeakerMixer : MonoBehaviour
{
    [Header("Visual")]
    public Renderer liquidRenderer;
    public Color emptyColor = Color.clear;
    public Color resultColor = new Color(0.3f, 0.8f, 0.3f); // green = "reaction complete"

    [Header("Feedback")]
    public GameObject successPanel;      // shown when both chemicals are mixed correctly
    public GameObject wrongOrderWarning; // shown if the same cup is poured twice

    private bool hasChemicalA = false;
    private bool hasChemicalB = false;

    public bool IsComplete => hasChemicalA && hasChemicalB;

    private float liquidBaseScaleY;
    private float liquidBasePosY;

    void Start()
    {
        if (liquidRenderer != null)
        {
            liquidBaseScaleY = liquidRenderer.transform.localScale.y;
            liquidBasePosY = liquidRenderer.transform.localPosition.y;
        }
        SetColor(emptyColor);
        SetLevel(0f, 0f, emptyColor); // start visually empty
    }

    // id should be "A" or "B" — each cup passes its own id.
    public void AddChemical(string id, Color chemicalColor)
    {
        if (IsComplete) return; // already finished, ignore further pours

        if (id == "A")
        {
            if (hasChemicalA)
            {
                ShowWrongOrderWarning();
                return;
            }
            hasChemicalA = true;
        }
        else if (id == "B")
        {
            if (hasChemicalB)
            {
                ShowWrongOrderWarning();
                return;
            }
            hasChemicalB = true;
        }

        // Show the single chemical's color first if it's the only one added so far.
        if (!IsComplete)
        {
            SetColor(chemicalColor);
        }
        else
        {
            CompleteReaction();
        }
    }

    void CompleteReaction()
    {
        SetColor(resultColor);
        Debug.Log("Reaction complete — new compound formed.");

        if (successPanel != null) successPanel.SetActive(true);

        ProcedureManager.Instance?.ReportEvent("ReactionComplete");
        AssessmentManager.Instance?.ReportEvent("ReactionComplete");
    }

    void ShowWrongOrderWarning()
    {
        Debug.Log("That chemical was already added.");
        if (wrongOrderWarning != null)
        {
            wrongOrderWarning.SetActive(true);
            Invoke(nameof(HideWarning), 2f);
        }
        AssessmentManager.Instance?.ReportEvent("MixMistake");
    }

    void HideWarning()
    {
        if (wrongOrderWarning != null) wrongOrderWarning.SetActive(false);
    }

    void SetColor(Color c)
    {
        if (liquidRenderer != null)
        {
            liquidRenderer.material.SetColor("_BaseColor", c);
        }
    }

    public float CurrentLevel { get; private set; } = 0f; // 0 = empty, 1 = completely full (both chemicals)

    // Called by CupPourController every frame during a pour to visually raise the
    // liquid level AND tint it toward the chemical's color as it fills.
    // baseLevel = the level already filled by previous pours (0 for the first cup,
    // ~0.5 for the second cup). frac = 0→1 progress of THIS pour. Each cup fills a
    // fixed share (0.5, assuming two cups total) on top of the base level.
    public void SetLevel(float baseLevel, float frac, Color previewColor)
    {
        if (liquidRenderer == null) return;

        const float perCupShare = 0.5f; // each of the 2 cups fills half the beaker
        float t = Mathf.Clamp01(baseLevel + frac * perCupShare);
        CurrentLevel = t;

        Transform lt = liquidRenderer.transform;
        float minScaleY = liquidBaseScaleY * 0.05f;
        float targetScaleY = Mathf.Lerp(minScaleY, liquidBaseScaleY, t);

        Vector3 scale = lt.localScale;
        lt.localScale = new Vector3(scale.x, targetScaleY, scale.z);

        float scaleDelta = liquidBaseScaleY - targetScaleY;
        lt.localPosition = new Vector3(lt.localPosition.x, liquidBasePosY - (scaleDelta / 2f), lt.localPosition.z);

        SetColor(previewColor);
    }

    // Call to let the player redo the experiment.
    public void ResetBeaker()
    {
        hasChemicalA = false;
        hasChemicalB = false;
        SetColor(emptyColor);
        SetLevel(0f, 0f, emptyColor);
        if (successPanel != null) successPanel.SetActive(false);
    }
}
