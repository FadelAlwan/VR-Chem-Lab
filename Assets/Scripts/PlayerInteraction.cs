using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float interactRange = 2.5f;
    public LayerMask interactableLayer;
    public TextMeshProUGUI promptText;

    [Header("Player Root (drag the Player GameObject that has CharacterController)")]
    public GameObject playerRoot; 

    private IInteractable currentTarget;

    void Update()
    {
        HandleRaycast();
        if (currentTarget != null && Input.GetKeyDown(KeyCode.E))
        {
            GameObject target = playerRoot != null ? playerRoot : gameObject;
            currentTarget.Interact(target);
        }
    }

    void HandleRaycast()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentTarget = interactable;
                promptText.text = interactable.GetPrompt();
                promptText.gameObject.SetActive(true);
                return;
            }
        }
        currentTarget = null;
        promptText.gameObject.SetActive(false);
    }
}

public interface IInteractable
{
    string GetPrompt();
    void Interact(GameObject player);
}