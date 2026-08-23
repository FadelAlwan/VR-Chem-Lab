using UnityEngine;
using TMPro;

public class AssessmentManager : MonoBehaviour
{
    public static AssessmentManager Instance;
    public string[] expectedOrder;
    public int startingScore = 100;
    public int penaltyPerMistake = 15;
    public GameObject resultsPanel;
    public TextMeshProUGUI resultsText;

    private int currentIndex = 0;
    private int score;
    private int mistakes = 0;
    public bool AssessmentActive { get; private set; } = false;

    void Awake() { Instance = this; }

    public void StartAssessment()
    {
        currentIndex = 0; score = startingScore; mistakes = 0;
        AssessmentActive = true;
        if (resultsPanel != null) resultsPanel.SetActive(false);
    }

    public void ReportEvent(string eventId)
    {
        if (!AssessmentActive) return;
        if (currentIndex < expectedOrder.Length && expectedOrder[currentIndex] == eventId)
            currentIndex++;
        else
            RegisterMistake();
        if (currentIndex >= expectedOrder.Length) FinishAssessment();
    }

    void RegisterMistake()
    {
        mistakes++;
        score = Mathf.Max(0, score - penaltyPerMistake);
    }

    void FinishAssessment()
    {
        AssessmentActive = false;
        if (resultsPanel != null) resultsPanel.SetActive(true);
        if (resultsText != null)
            resultsText.text = $"Assessment Complete\nScore: {score}/100\nMistakes: {mistakes}";
    }
}