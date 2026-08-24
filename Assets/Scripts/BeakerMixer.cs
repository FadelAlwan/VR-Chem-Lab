using UnityEngine;
using TMPro;
using System.Collections.Generic;

// Attach to the Beaker. Cups (CupPourController.cs) call AddChemical() on this when poured.
// Supports MULTIPLE possible reactions in a single beaker: define a list of Recipes
// (e.g. A+B=Green, D+E=Red) and the beaker figures out which one matches whatever
// two chemicals the student pours in, regardless of order.
public class BeakerMixer : MonoBehaviour
{
    [System.Serializable]
    public class ReactionRecipe
    {
        public string chemicalIdA;
        public string chemicalIdB;
        public Color resultColor;
        public string label; // optional, just for your own reference in the Inspector
    }

    [Header("Possible Reactions This Beaker Can Produce")]
    public ReactionRecipe[] recipes;

    [Header("Visual")]
    public Renderer liquidRenderer;
    public Color emptyColor = Color.clear;
    public Color invalidMixColor = new Color(0.5f, 0.5f, 0.5f); // shown if the two chemicals don't match any recipe

    [Header("Feedback")]
    public GameObject successPanel;
    public TextMeshProUGUI errorText; // shows "Wrong combination!" clearly on screen
    public float errorDuration = 2.5f;

    private readonly List<(string id, Color color)> pouredChemicals = new List<(string, Color)>();
    private bool isComplete = false;

    public bool IsComplete => isComplete;
    public float CurrentLevel { get; private set; } = 0f;

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
        SetLevel(0f, 0f, emptyColor);
    }

    // Called by CupPourController once a pour finishes.
    public void AddChemical(string id, Color chemicalColor)
    {
        if (isComplete) return;

        // Same chemical poured twice in a row — treat as a mistake.
        foreach (var poured in pouredChemicals)
        {
            if (poured.id == id)
            {
                ShowError($"You already added {id}. Try a different chemical.");
                return;
            }
        }

        pouredChemicals.Add((id, chemicalColor));

        if (pouredChemicals.Count == 1)
        {
            // Preview the first chemical's own color while waiting for the second.
            SetColor(chemicalColor);
            return;
        }

        if (pouredChemicals.Count >= 2)
        {
            string firstId = pouredChemicals[0].id;
            string secondId = pouredChemicals[1].id;
            ReactionRecipe match = FindMatchingRecipe(firstId, secondId);
            if (match != null)
            {
                CompleteReaction(match.resultColor);
            }
            else
            {
                // The two chemicals poured don't form any known reaction.
                SetColor(invalidMixColor);
                Debug.Log($"No known reaction between '{firstId}' and '{secondId}'.");
                ShowError($"Incorrect reaction! {firstId} and {secondId} don't react — check the chart on the wall.");

                // Reset so the student can immediately try a correct pair again.
                pouredChemicals.Clear();
                Invoke(nameof(ResetVisualAfterMistake), errorDuration);
            }
        }
    }

    void ResetVisualAfterMistake()
    {
        if (!isComplete)
        {
            SetColor(emptyColor);
            SetLevel(0f, 0f, emptyColor);
        }
    }

    ReactionRecipe FindMatchingRecipe(string idOne, string idTwo)
    {
        foreach (var recipe in recipes)
        {
            bool matchesForward = recipe.chemicalIdA == idOne && recipe.chemicalIdB == idTwo;
            bool matchesReverse = recipe.chemicalIdA == idTwo && recipe.chemicalIdB == idOne;
            if (matchesForward || matchesReverse)
            {
                return recipe;
            }
        }
        return null;
    }

    void CompleteReaction(Color resultColor)
    {
        isComplete = true;
        SetColor(resultColor);
        SetLevel(1f, 0f, resultColor); // ensure the level visually reads as "full"
        Debug.Log("Reaction complete — new compound formed.");

        if (successPanel != null) successPanel.SetActive(true);

        ProcedureManager.Instance?.ReportEvent("ReactionComplete");
    }

    void ShowError(string message)
    {
        if (errorText == null) return;
        errorText.text = message;
        errorText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideError));
        Invoke(nameof(HideError), errorDuration);
    }

    void HideError()
    {
        if (errorText != null) errorText.gameObject.SetActive(false);
    }

    void SetColor(Color c)
    {
        if (liquidRenderer != null)
        {
            liquidRenderer.material.SetColor("_BaseColor", c);
        }
    }

    // Called by CupPourController every frame during a pour to visually raise the
    // liquid level AND tint it toward the chemical's color as it fills.
    public void SetLevel(float baseLevel, float frac, Color previewColor)
    {
        if (liquidRenderer == null) return;

        const float perCupShare = 0.5f;
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

    // Call to let the player redo the experiment with fresh chemicals.
    public void ResetBeaker()
    {
        pouredChemicals.Clear();
        isComplete = false;
        SetColor(emptyColor);
        SetLevel(0f, 0f, emptyColor);
        if (successPanel != null) successPanel.SetActive(false);
    }
}
