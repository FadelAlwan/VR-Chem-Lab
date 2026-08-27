using UnityEngine;

public class LabCoatPickup : MonoBehaviour, IInteractable
{
    public string GetPrompt() => "Press E to put on lab coat";

    public void Interact(GameObject player)
    {
        SafetyGearManager.Instance.EquipLabCoat();
        ProcedureManager.Instance?.ReportEvent("LabCoatEquipped");
        gameObject.SetActive(false);
    }
}
