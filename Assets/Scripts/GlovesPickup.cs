using UnityEngine;

public class GlovesPickup : MonoBehaviour, IInteractable
{
    public string GetPrompt() => "Press E to put on gloves";
    public void Interact(GameObject player)
    {
        SafetyGearManager.Instance.EquipGloves();
        ProcedureManager.Instance?.ReportEvent("GlovesEquipped");
        gameObject.SetActive(false);
    }
}