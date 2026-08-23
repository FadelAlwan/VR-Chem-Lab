using UnityEngine;

public class SafetyGearManager : MonoBehaviour
{
    public static SafetyGearManager Instance;
    public bool hasGoggles = false;
    public bool hasGloves = false;
    public GameObject warningPopup;

    void Awake() { Instance = this; }

    public bool IsFullyEquipped => hasGoggles && hasGloves;

    public void EquipGoggles() { hasGoggles = true; }
    public void EquipGloves() { hasGloves = true; }

    public bool TryDoRiskyAction()
    {
        if (IsFullyEquipped) return true;
        ShowWarning();
        return false;
    }

    void ShowWarning()
    {
        if (warningPopup == null) return;
        warningPopup.SetActive(true);
        Invoke(nameof(HideWarning), 2f);
    }

    void HideWarning() { warningPopup.SetActive(false); }
}