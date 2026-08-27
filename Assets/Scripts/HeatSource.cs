using UnityEngine;


public class HeatSource : MonoBehaviour, IInteractable
{
    public SimpleDistillation distillation;

    [Header("Flame Visual")]
    public GameObject flameEffect;

    [Header("Requires Sitting")]
    public SitInteractable requiredSitZone; 

    private bool isOn = false;

    public string GetPrompt()
    {
        if (requiredSitZone != null && !requiredSitZone.IsPlayerSeated)
        {
            return ""; 
        }
        return isOn ? "" : "Press E to light the lamp";
    }

    public void Interact(GameObject player)
    {
        if (requiredSitZone != null && !requiredSitZone.IsPlayerSeated)
        {
            return; 
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
