using UnityEngine;

public class GogglesPickup : MonoBehaviour, IInteractable
{
    public string GetPrompt() => "Press E to put on safety goggles";
    public void Interact(GameObject player)
    {
        SafetyGearManager.Instance.EquipGoggles();
        ProcedureManager.Instance?.ReportEvent("GogglesEquipped");
        gameObject.SetActive(false);
    }
}