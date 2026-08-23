using UnityEngine;
using System.Collections;

// Replaces ChemicalCup.cs. Flow:
// 1) Press E on cup (idle) -> picks up to hand (holdPoint), same as before.
// 2) Press E again while held -> cup smoothly moves + tilts to a fixed pour position
//    above the beaker, then pours gradually: cup liquid visually shrinks, beaker
//    liquid visually rises, a particle stream plays between them.
// 3) When pour finishes, the actual chemical is registered with BeakerMixer.
public class CupPourController : MonoBehaviour, IInteractable
{
    private enum State { Idle, Held, MovingToPour, Pouring, Finished }

    [Header("Chemical Identity")]
    public string chemicalId = "A";
    public Color chemicalColor = Color.blue;

    [Header("Target")]
    public BeakerMixer targetBeaker;

    [Header("Holding (while carried)")]
    public Transform holdPoint; // camera-relative hand position, same as before

    [Header("Pour Position (fixed point above the beaker)")]
    public Transform pourPoint;      // empty GameObject positioned above the beaker
    public float moveDuration = 0.6f;
    public float pourTiltAngle = 100f; // degrees to tilt on local X axis while pouring

    [Header("Pour Timing")]
    public float pourDuration = 2f;

    [Header("Visuals")]
    public Transform cupLiquidVisual;    // child mesh inside the cup representing its own liquid (optional)
    public ParticleSystem pourParticles; // child Particle System at the cup's spout/rim (optional)

    private State state = State.Idle;
    private Vector3 cupLiquidStartScale;

    void Start()
    {
        if (cupLiquidVisual != null)
        {
            cupLiquidStartScale = cupLiquidVisual.localScale;
        }
    }

    void Update()
    {
        // The collider gets disabled after pickup (so it stops blocking the raycast),
        // which means PlayerInteraction can no longer detect it to call Interact().
        // So once held, listen for the key directly instead.
        if (state == State.Held && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(MoveToPourAndPour());
        }
    }

    public string GetPrompt()
    {
        switch (state)
        {
            case State.Idle: return "Press E to pick up";
            case State.Held: return "Press E to pour into beaker";
            case State.Finished: return "Empty";
            default: return ""; // no prompt while animating/pouring
        }
    }

    public void Interact(GameObject player)
    {
        if (state == State.Idle)
        {
            PickUp();
        }
        // Held state is now handled directly in Update() via key press,
        // since the collider is disabled once picked up.
    }

    void PickUp()
    {
        if (!SafetyGearManager.Instance.TryDoRiskyAction())
        {
            return;
        }

        state = State.Held;
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    IEnumerator MoveToPourAndPour()
    {
        state = State.MovingToPour;

        // Detach from the hand so we can move in world space toward the fixed pour point.
        transform.SetParent(null);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        // Tilt sideways (local Z axis) so the rim faces downward toward the beaker,
        // rather than tipping forward/backward (X axis).
        Quaternion tiltedRot = pourPoint.rotation * Quaternion.Euler(0f, 0f, pourTiltAngle);

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float frac = Mathf.Clamp01(t / moveDuration);
            transform.position = Vector3.Lerp(startPos, pourPoint.position, frac);
            transform.rotation = Quaternion.Slerp(startRot, tiltedRot, frac);
            yield return null;
        }

        transform.position = pourPoint.position;
        transform.rotation = tiltedRot;

        yield return StartCoroutine(PourGradually());
    }

    IEnumerator PourGradually()
    {
        state = State.Pouring;

        float baseLevel = targetBeaker != null ? targetBeaker.CurrentLevel : 0f;

        if (pourParticles != null)
        {
            var main = pourParticles.main;
            main.startColor = chemicalColor;
            main.startSize = 0.08f; // bigger, more visible droplets — tweak as needed
            pourParticles.Play();
        }

        float elapsed = 0f;
        while (elapsed < pourDuration)
        {
            elapsed += Time.deltaTime;
            float frac = Mathf.Clamp01(elapsed / pourDuration);

            // Cup's own liquid visually shrinks as it empties.
            if (cupLiquidVisual != null)
            {
                cupLiquidVisual.localScale = new Vector3(
                    cupLiquidStartScale.x,
                    Mathf.Lerp(cupLiquidStartScale.y, 0f, frac),
                    cupLiquidStartScale.z);
            }

            // Beaker's liquid visually rises AND tints toward this chemical's color
            // as it fills, building on top of whatever was already poured before
            // (so a second cup doesn't visually erase the first one's liquid).
            if (targetBeaker != null)
            {
                targetBeaker.SetLevel(baseLevel, frac, chemicalColor);
            }

            yield return null;
        }

        if (pourParticles != null) pourParticles.Stop();

        // Register the actual chemical now that the pour animation is complete.
        if (targetBeaker != null)
        {
            targetBeaker.AddChemical(chemicalId, chemicalColor);
        }

        // Report this specific cup's pour as its own step event (in addition to
        // whatever BeakerMixer reports for the final reaction).
        ProcedureManager.Instance?.ReportEvent($"CupPoured_{chemicalId}");
        AssessmentManager.Instance?.ReportEvent($"CupPoured_{chemicalId}");

        state = State.Finished;
        gameObject.SetActive(false);
    }
}
