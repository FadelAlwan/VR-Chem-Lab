using UnityEngine;

public class SitInteractable : MonoBehaviour, IInteractable
{
    [Header("Sit Position")]
    public Transform sitPoint; 

    [Header("Experiment Instructions (shown only while seated here)")]
    public ProcedureManager.ProcedureStep[] experimentSteps;
    [TextArea] public string experimentCompleteMessage = "Great job! You've completed this experiment.";

    [Header("Message shown after standing up")]
    [TextArea] public string standUpMessage = "Choose one of the experiments";

    [Header("Beaker to reset when standing up")]
    public BeakerMixer beakerToReset;

    [Header("Distillation station to reset when standing up (optional)")]
    public SimpleDistillation distillationToReset;
    public HeatSource heatSourceToReset;

    [Header("Station-specific UI (shown only while seated here)")]
    public GameObject stationUI; 

    [Header("Cups to reset when standing up (reappear at original position)")]
    public CupPourController[] cupsToReset;

    private bool isOccupied = false;
    private GameObject seatedPlayer;
    private CharacterController seatedController;
    private SimpleFirstPersonController seatedFPController;
    private Vector3 previousPosition;

    public bool IsPlayerSeated => isOccupied;

    public string GetPrompt()
    {
        return isOccupied ? "" : "Press E to sit";
    }

    public void Interact(GameObject player)
    {
        if (isOccupied) return;

        seatedPlayer = player;
        seatedController = player.GetComponent<CharacterController>();
        seatedFPController = player.GetComponent<SimpleFirstPersonController>();

        if (seatedFPController == null || sitPoint == null)
        {
            Debug.LogWarning("SitInteractable is missing sitPoint or the player's SimpleFirstPersonController.");
            return;
        }

        previousPosition = player.transform.position;


        if (seatedController != null) seatedController.enabled = false;
        player.transform.position = sitPoint.position;
        if (seatedController != null) seatedController.enabled = true;

        seatedFPController.SetSitting(true);
        isOccupied = true;

        if (stationUI != null) stationUI.SetActive(true);

        if (ProcedureManager.Instance != null && experimentSteps != null && experimentSteps.Length > 0)
        {
            ProcedureManager.Instance.LoadSteps(experimentSteps, experimentCompleteMessage);
        }
    }

    void Update()
    {
        if (isOccupied && Input.GetKeyDown(KeyCode.Space))
        {
            StandUp();
        }
    }

    void StandUp()
    {
        if (seatedFPController != null)
        {
            seatedFPController.SetSitting(false);
        }

        if (seatedController != null) seatedController.enabled = false;
        if (seatedPlayer != null) seatedPlayer.transform.position = previousPosition;
        if (seatedController != null) seatedController.enabled = true;

        isOccupied = false;
        seatedPlayer = null;
        seatedController = null;
        seatedFPController = null;

        if (stationUI != null) stationUI.SetActive(false);

        if (ProcedureManager.Instance != null)
        {
            ProcedureManager.Instance.LoadSteps(new ProcedureManager.ProcedureStep[0], standUpMessage);
        }

        if (beakerToReset != null)
        {
            beakerToReset.ResetBeaker();
        }

        if (distillationToReset != null)
        {
            distillationToReset.ResetDistillation();
        }
        if (heatSourceToReset != null)
        {
            heatSourceToReset.ResetSource();
        }

        if (cupsToReset != null)
        {
            foreach (var cup in cupsToReset)
            {
                if (cup != null) cup.ResetCup();
            }
        }
    }
}
