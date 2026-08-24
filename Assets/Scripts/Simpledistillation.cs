using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// Simple Distillation: ONE mixture (two substances) in a flask, heated.
// The LOWER-boiling-point substance evaporates and gradually transfers
// (no particle visual — just the flask/beaker colors and levels changing
// smoothly over time) into a single receiving beaker. The HIGHER-boiling-
// point substance stays behind in the flask, at a small remaining amount.
public class SimpleDistillation : MonoBehaviour
{
    [System.Serializable]
    public class Substance
    {
        public string substanceName;
        public float boilingPointC;
        [Range(0f, 1f)] public float mixtureShare = 0.5f;
        public Color liquidColor;
    }

    [Header("The two substances in the flask (order doesn't matter, sorted automatically)")]
    public Substance substanceA;
    public Substance substanceB;

    [Header("Heating")]
    public float heatingRatePerSecond = 15f;
    public float maxTemperature = 120f;

    [Header("Flask Visual (shrinks as the lighter substance evaporates)")]
    public Renderer flaskLiquidRenderer;
    [Range(0f, 1f)] public float remainingFlaskAmount = 0.15f; // small amount left behind, e.g. 15%

    [Header("Receiving Beaker (single)")]
    public DistillationBeaker receivingBeaker;

    [Header("Transfer Animation")]
    public float transferDuration = 4f; // how long the color/level change takes

    [Header("UI")]
    public TextMeshProUGUI temperatureText;
    public TextMeshProUGUI hintText;

    [Header("Thermometer Slider (optional visual)")]
    public Slider thermometerSlider; // Min=20, Max=maxTemperature, Direction=Bottom to Top
    public Image thermometerFill;    // the Slider's Fill image, tints color-blue-to-red
    public Color coldColor = new Color(0.3f, 0.5f, 0.9f);
    public Color hotColor = new Color(0.9f, 0.2f, 0.2f);

    private float currentTemp = 20f;
    private bool isHeating = false;
    private bool hasStartedTransfer = false;
    private Substance lowerBoiling, higherBoiling;
    private float flaskBaseScaleY, flaskBasePosY;
    private float flaskRemainderScaleY;

    void Start()
    {
        if (substanceA.boilingPointC <= substanceB.boilingPointC)
        {
            lowerBoiling = substanceA;
            higherBoiling = substanceB;
        }
        else
        {
            lowerBoiling = substanceB;
            higherBoiling = substanceA;
        }

        if (flaskLiquidRenderer != null)
        {
            flaskBaseScaleY = flaskLiquidRenderer.transform.localScale.y;
            flaskBasePosY = flaskLiquidRenderer.transform.localPosition.y;
            flaskRemainderScaleY = flaskBaseScaleY * remainingFlaskAmount;
            SetFlaskColor(Color.Lerp(lowerBoiling.liquidColor, higherBoiling.liquidColor, 0.5f));
        }

        UpdateTemperatureText();
        UpdateHint();
    }

    void Update()
    {
        if (!isHeating || hasStartedTransfer) return;

        currentTemp = Mathf.Min(currentTemp + heatingRatePerSecond * Time.deltaTime, maxTemperature);
        UpdateTemperatureText();

        if (currentTemp >= lowerBoiling.boilingPointC)
        {
            hasStartedTransfer = true;
            isHeating = false;
            StartCoroutine(TransferOverTime());
        }
    }

    public void StartHeating()
    {
        isHeating = true;
    }

    IEnumerator TransferOverTime()
    {
        if (hintText != null) hintText.text = $"Collecting {lowerBoiling.substanceName}...";

        float t = 0f;
        while (t < transferDuration)
        {
            t += Time.deltaTime;
            float frac = Mathf.Clamp01(t / transferDuration);

            ShrinkFlaskGradually(frac);

            if (receivingBeaker != null)
            {
                receivingBeaker.SetPartialLevel(frac, lowerBoiling.liquidColor);
            }

            yield return null;
        }

        if (receivingBeaker != null)
        {
            receivingBeaker.ReceiveFraction(lowerBoiling.substanceName, lowerBoiling.liquidColor);
        }
        SetFlaskColor(higherBoiling.liquidColor);

        if (hintText != null)
        {
            hintText.text = $"{lowerBoiling.substanceName} collected! {higherBoiling.substanceName} remains in the flask.";
        }

        Debug.Log($"{lowerBoiling.substanceName} distilled into the beaker. {higherBoiling.substanceName} remains in the flask.");
        ProcedureManager.Instance?.ReportEvent("DistillationComplete");
    }

    void ShrinkFlaskGradually(float frac)
    {
        if (flaskLiquidRenderer == null) return;
        Transform ft = flaskLiquidRenderer.transform;

        float currentScaleY = Mathf.Lerp(flaskBaseScaleY, flaskRemainderScaleY, frac);

        // Scale only — no position offset. Many imported liquid meshes already
        // have their pivot at the BOTTOM (not the center), so scaling alone
        // keeps the liquid surface dropping naturally without sinking through
        // the glass. If your liquid mesh instead has a CENTER pivot and this
        // looks wrong (liquid floating above the bottom instead), let me know
        // and we'll add the position compensation back for your specific case.
        ft.localScale = new Vector3(ft.localScale.x, currentScaleY, ft.localScale.z);
    }

    void SetFlaskColor(Color c)
    {
        if (flaskLiquidRenderer != null)
        {
            flaskLiquidRenderer.material.SetColor("_BaseColor", c);
        }
    }

    void UpdateTemperatureText()
    {
        if (temperatureText != null)
        {
            temperatureText.text = $"{currentTemp:F0}°C";
        }

        if (thermometerSlider != null)
        {
            thermometerSlider.value = currentTemp;
        }

        if (thermometerFill != null)
        {
            float t = Mathf.InverseLerp(20f, maxTemperature, currentTemp);
            thermometerFill.color = Color.Lerp(coldColor, hotColor, t);
        }
    }

    void UpdateHint()
    {
        if (hintText != null && !hasStartedTransfer)
        {
            hintText.text = $"Heat the flask — {lowerBoiling.substanceName} will evaporate at {lowerBoiling.boilingPointC}°C.";
        }
    }

    public void ResetDistillation()
    {
        StopAllCoroutines();
        isHeating = false;
        hasStartedTransfer = false;
        currentTemp = 20f;

        if (flaskLiquidRenderer != null)
        {
            flaskLiquidRenderer.transform.localScale = new Vector3(
                flaskLiquidRenderer.transform.localScale.x, flaskBaseScaleY, flaskLiquidRenderer.transform.localScale.z);
            flaskLiquidRenderer.transform.localPosition = new Vector3(
                flaskLiquidRenderer.transform.localPosition.x, flaskBasePosY, flaskLiquidRenderer.transform.localPosition.z);
            SetFlaskColor(Color.Lerp(lowerBoiling.liquidColor, higherBoiling.liquidColor, 0.5f));
        }
        if (receivingBeaker != null) receivingBeaker.ResetBeaker();
        UpdateTemperatureText();
        UpdateHint();
    }
}
