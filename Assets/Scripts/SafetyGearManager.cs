using UnityEngine;
using TMPro;

// Central place to track whether the student is wearing required safety gear.
// Attach ONE of these to the Player. Other scripts (like PourController) check
// SafetyGearManager.Instance.IsFullyEquipped before allowing risky actions.
public class SafetyGearManager : MonoBehaviour
{
    public static SafetyGearManager Instance;

    [Header("Current Gear State")]
    public bool hasGoggles = false;
    public bool hasGloves = false;
    public bool hasLabCoat = false;

    [Header("Feedback")]
    public TextMeshProUGUI warningText; // shows exactly what's missing
    public float warningDuration = 2.5f;

    void Awake()
    {
        Instance = this;
    }

    public bool IsFullyEquipped => hasGoggles && hasGloves && hasLabCoat;

    public void EquipGoggles()
    {
        hasGoggles = true;
    }

    public void EquipGloves()
    {
        hasGloves = true;
    }

    public void EquipLabCoat()
    {
        hasLabCoat = true;
    }

    // Call this before any dangerous action (pouring acid, lighting a flame, etc.)
    public bool TryDoRiskyAction()
    {
        if (IsFullyEquipped) return true;

        ShowWarning(BuildMissingGearMessage());
        return false;
    }

    string BuildMissingGearMessage()
    {
        // Collect exactly which items are missing, in a fixed, readable order.
        System.Collections.Generic.List<string> missing = new System.Collections.Generic.List<string>();
        if (!hasLabCoat) missing.Add("lab coat");
        if (!hasGoggles) missing.Add("safety goggles");
        if (!hasGloves) missing.Add("gloves");

        if (missing.Count == 1)
        {
            return $"You need to put on your {missing[0]} first!";
        }
        if (missing.Count == 2)
        {
            return $"You need to put on your {missing[0]} and {missing[1]} first!";
        }
        return $"You need to put on your {missing[0]}, {missing[1]}, and {missing[2]} first!";
    }

    void ShowWarning(string message)
    {
        if (warningText == null) return;
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideWarning));
        Invoke(nameof(HideWarning), warningDuration);
    }

    void HideWarning()
    {
        if (warningText != null) warningText.gameObject.SetActive(false);
    }
}
