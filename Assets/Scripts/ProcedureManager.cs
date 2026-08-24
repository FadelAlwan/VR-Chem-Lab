using UnityEngine;
using TMPro;

// Drives Guided Practice mode: shows the current step's instruction, and only
// advances when that step's action actually happens (called by other scripts).
// Attach to an empty GameObject called "ProcedureManager" in the scene.
//
// Supports two phases:
// 1) Onboarding steps (set in the Inspector) — e.g. put on goggles, put on gloves.
//    When finished, shows postCompletionMessage (e.g. "Choose one of the experiments")
//    instead of hiding, since GuidedModeActive stays off after that.
// 2) Per-experiment steps — call LoadSteps() (e.g. from SitInteractable when the
//    player sits at a station) to switch to that experiment's own instructions.
public class ProcedureManager : MonoBehaviour
{
    public static ProcedureManager Instance;

    [System.Serializable]
    public class ProcedureStep
    {
        public string instruction;   // shown to the student, e.g. "Put on your safety goggles"
        public string requiredEvent; // an id this step waits for, e.g. "GogglesEquipped"
    }

    [Header("Onboarding Steps — set these in the Inspector")]
    public ProcedureStep[] steps;

    [Header("Message shown once these steps are all complete")]
    [TextArea] public string postCompletionMessage = "Choose one of the experiments";

    [Header("UI")]
    public TextMeshProUGUI instructionText;
    public GameObject completionPanel;

    private int currentStepIndex = 0;
    public bool GuidedModeActive { get; private set; } = true;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShowCurrentStep();
    }

    void ShowCurrentStep()
    {
        if (currentStepIndex >= steps.Length)
        {
            if (!string.IsNullOrEmpty(postCompletionMessage))
            {
                instructionText.gameObject.SetActive(true);
                instructionText.text = postCompletionMessage;
            }
            else
            {
                instructionText.gameObject.SetActive(false);
            }

            if (completionPanel != null) completionPanel.SetActive(true);
            return;
        }

        instructionText.gameObject.SetActive(true);
        instructionText.text = $"Step {currentStepIndex + 1}/{steps.Length}: {steps[currentStepIndex].instruction}";
    }

    // Other scripts call this when the student does something (e.g. GogglesPickup
    // calls ProcedureManager.Instance.ReportEvent("GogglesEquipped") after equipping).
    public void ReportEvent(string eventId)
    {
        if (!GuidedModeActive) return;
        if (currentStepIndex >= steps.Length) return;

        if (steps[currentStepIndex].requiredEvent == eventId)
        {
            currentStepIndex++;
            ShowCurrentStep();
        }
        // if the event doesn't match the current required step, it's simply ignored —
        // this is what makes it "guided": the student can't skip ahead out of order.
    }

    // Switches to a new set of steps — e.g. called by SitInteractable when the
    // player sits at a specific experiment station, so its instructions take over.
    public void LoadSteps(ProcedureStep[] newSteps, string newPostCompletionMessage = "")
    {
        steps = newSteps;
        postCompletionMessage = newPostCompletionMessage;
        currentStepIndex = 0;
        GuidedModeActive = true;
        if (completionPanel != null) completionPanel.SetActive(false);
        ShowCurrentStep();
    }

    public void SetGuidedMode(bool active)
    {
        GuidedModeActive = active;
    }
}
