using UnityEngine;
using TMPro;


public class ProcedureManager : MonoBehaviour
{
    public static ProcedureManager Instance;

    [System.Serializable]
    public class ProcedureStep
    {
        public string instruction;   
        public string requiredEvent; 
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

    public void ReportEvent(string eventId)
    {
        if (!GuidedModeActive) return;
        if (currentStepIndex >= steps.Length) return;

        if (steps[currentStepIndex].requiredEvent == eventId)
        {
            currentStepIndex++;
            ShowCurrentStep();
        }
    }

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
