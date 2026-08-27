using UnityEngine;
using System.Collections;


public class CupPourController : MonoBehaviour, IInteractable
{
    private enum State { Idle, Held, MovingToPour, Pouring, Finished }

    [Header("Chemical Identity")]
    public string chemicalId = "A";
    public Color chemicalColor = Color.blue;

    [Header("Target")]
    public BeakerMixer targetBeaker;

    [Header("Holding (while carried)")]
    public Transform holdPoint; 

    [Header("Pour Position (fixed point above the beaker)")]
    public Transform pourPoint;   
    public float moveDuration = 0.6f;
    public float pourTiltAngle = 100f; 

    [Header("Pour Timing")]
    public float pourDuration = 2f;

    [Header("Visuals")]
    public Transform cupLiquidVisual;    
    public ParticleSystem pourParticles; 

    [Header("Requires Sitting")]
    public SitInteractable requiredSitZone; 

    private State state = State.Idle;


    private static bool AnyCupBusy = false;
    private Vector3 cupLiquidStartScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;

    void Start()
    {
        if (cupLiquidVisual != null)
        {
            cupLiquidStartScale = cupLiquidVisual.localScale;
        }

        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;
    }

    void Update()
    {

        if (state == State.Held && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(MoveToPourAndPour());
        }
    }

    public string GetPrompt()
    {
        if (requiredSitZone != null && !requiredSitZone.IsPlayerSeated)
        {
            return ""; 
        }

        if (AnyCupBusy && state == State.Idle)
        {
            return ""; 
        }

        switch (state)
        {
            case State.Idle: return "Press E to pick up";
            case State.Held: return "Press E to pour into beaker";
            case State.Finished: return "Empty";
            default: return ""; 
        }
    }

    public void Interact(GameObject player)
    {
        if (requiredSitZone != null && !requiredSitZone.IsPlayerSeated)
        {
            return; 
        }

        if (AnyCupBusy && state == State.Idle)
        {
            return; 
        }

        if (state == State.Idle)
        {
            PickUp();
        }

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
        AnyCupBusy = true;


        transform.SetParent(null);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

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
            main.startSize = 0.08f; 
            pourParticles.Play();
        }

        float elapsed = 0f;
        while (elapsed < pourDuration)
        {
            elapsed += Time.deltaTime;
            float frac = Mathf.Clamp01(elapsed / pourDuration);

            if (cupLiquidVisual != null)
            {
                cupLiquidVisual.localScale = new Vector3(
                    cupLiquidStartScale.x,
                    Mathf.Lerp(cupLiquidStartScale.y, 0f, frac),
                    cupLiquidStartScale.z);
            }


            if (targetBeaker != null)
            {
                targetBeaker.SetLevel(baseLevel, frac, chemicalColor);
            }

            yield return null;
        }

        if (pourParticles != null) pourParticles.Stop();


        if (targetBeaker != null)
        {
            targetBeaker.AddChemical(chemicalId, chemicalColor);
        }


        ProcedureManager.Instance?.ReportEvent($"CupPoured_{chemicalId}");

        state = State.Finished;
        AnyCupBusy = false;
        gameObject.SetActive(false);
    }


    public void ResetCup()
    {
        StopAllCoroutines();


        if (state == State.MovingToPour || state == State.Pouring)
        {
            AnyCupBusy = false;
        }

        gameObject.SetActive(true);
        state = State.Idle;

        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (cupLiquidVisual != null)
        {
            cupLiquidVisual.localScale = cupLiquidStartScale;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        if (pourParticles != null) pourParticles.Stop();
    }
}
