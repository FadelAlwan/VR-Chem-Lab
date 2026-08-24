using UnityEngine;

// Attach to any furniture the player should be able to "sit" at (table, chair, stool).
// Needs a Collider on this object (or a child) set to layer Interactable + Is Trigger.
public class SitInteractable : MonoBehaviour, IInteractable
{
    [Header("Sit Position")]
    public Transform sitPoint; // empty child positioned where the player should stand while "sitting"

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
    public GameObject stationUI; // e.g. DistillationUI (thermometer + temperature text)

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

        // Disable the CharacterController briefly to allow a direct position set
        // (CharacterController normally resists being moved by transform alone).
        if (seatedController != null) seatedController.enabled = false;
        player.transform.position = sitPoint.position;
        if (seatedController != null) seatedController.enabled = true;

        seatedFPController.SetSitting(true);
        isOccupied = true;

        // Show this station's specific UI (e.g. thermometer for distillation).
        if (stationUI != null) stationUI.SetActive(true);

        // Switch the on-screen instructions to this specific experiment's steps.
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

        // Hide this station's specific UI again.
        if (stationUI != null) stationUI.SetActive(false);

        // Revert the on-screen instructions back to a neutral state.
        if (ProcedureManager.Instance != null)
        {
            ProcedureManager.Instance.LoadSteps(new ProcedureManager.ProcedureStep[0], standUpMessage);
        }

        // Reset the beaker so the experiment can be redone fresh next time.
        if (beakerToReset != null)
        {
            beakerToReset.ResetBeaker();
        }

        // Reset the distillation station (flask, temperature, flame) if this
        // sit zone belongs to a distillation-type experiment.
        if (distillationToReset != null)
        {
            distillationToReset.ResetDistillation();
        }
        if (heatSourceToReset != null)
        {
            heatSourceToReset.ResetSource();
        }

        // Bring back any cups that were used (poured/hidden) so the experiment
        // can be redone from scratch.
        if (cupsToReset != null)
        {
            foreach (var cup in cupsToReset)
            {
                if (cup != null) cup.ResetCup();
            }
        }
    }
}
