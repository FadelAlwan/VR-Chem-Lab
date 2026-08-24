using UnityEngine;

// The heat source (spirit lamp/burner) under the mixture flask. Player interacts
// to light it; temperature then rises automatically over time.
public class HeatSource : MonoBehaviour, IInteractable
{
    public SimpleDistillation distillation;

    [Header("Flame Visual")]
    public GameObject flameEffect;

    [Header("Requires Sitting")]
    public SitInteractable requiredSitZone; // the player must be seated here to light this

    private bool isOn = false;

    public string GetPrompt()
    {
        if (requiredSitZone != null && !requiredSitZone.IsPlayerSeated)
        {
            return ""; // no prompt at all if the player isn't seated at this station
        }
        return isOn ? "" : "Press E to light the lamp";
    }

    public void Interact(GameObject player)
    {
        if (requiredSitZone != null && !requiredSitZone.IsPlayerSeated)
        {
            return; // ignore interaction entirely if not seated
        }

        if (isOn) return;
        if (!SafetyGearManager.Instance.TryDoRiskyAction()) return;

        isOn = true;
        if (distillation != null) distillation.StartHeating();
        if (flameEffect != null) flameEffect.SetActive(true);

        ProcedureManager.Instance?.ReportEvent("LampLit");
    }

    public void ResetSource()
    {
        isOn = false;
        if (flameEffect != null) flameEffect.SetActive(false);
    }
}
