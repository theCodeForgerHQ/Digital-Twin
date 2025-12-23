using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HeatEmitter : MonoBehaviour
{
    public float heatRate = 10f;
    public float coolingRate = 2f;
    public float maxHeat = 100f;
    public string targetComponent;

    public float heat;
    private bool active;
    private float speedMultiplier = 1f;
    private Renderer r;
    private Material m;
    private Gradient heatGradient;
    private float logTimer;
    private Color baseColor;

    void Awake()
    {
        r = GetComponent<Renderer>();
        m = new Material(r.sharedMaterial);
        r.material = m;
        baseColor = m.color;
    
        heatGradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0].color = m.color;
        colorKeys[0].time = 0f;
        colorKeys[1].color = Color.red;
        colorKeys[1].time = 1f;

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1f;
        alphaKeys[0].time = 0f;
        alphaKeys[1].alpha = 1f;
        alphaKeys[1].time = 1f;

        heatGradient.SetKeys(colorKeys, alphaKeys);
    }

    void Update()
    {
        if (active)
        {
            heat += heatRate * speedMultiplier * Time.deltaTime;
        }
        else
        {
            heat -= coolingRate * Time.deltaTime;
        }

        if (heat < 0f) heat = 0f;
        if (heat > maxHeat) heat = maxHeat;

        float heatNormalized = Mathf.Pow(heat / maxHeat, 2f);
        if (SpeedUI.showHeatmap)
        {
            m.color = heatGradient.Evaluate(heatNormalized);
        }
        else
        {
            m.color = baseColor;
        }

        logTimer += Time.deltaTime;
        if (logTimer >= 3f)
        {
            logTimer = 0f;
        }
    }

    public void SetActive(bool value, float speedMultiplier)
    {
        active = value;
        this.speedMultiplier = speedMultiplier;
    }

    public void SetSpeedMultiplier(string component, bool isReverse = false)
    {
        switch (component)
        {
            case "linear piston":
                speedMultiplier = isReverse ? PLCSequence.reverseSpeed : PLCSequence.forwardSpeed;
                break;
            case "vertical cylinder":
                speedMultiplier = isReverse ? PLCSequence.downSpeed : PLCSequence.upSpeed;
                break;
            case "rotator":
                speedMultiplier = isReverse ? PLCSequence.clockwiseSpeed : PLCSequence.anticlockwiseSpeed;
                break;
            default:
                speedMultiplier = 1f;
                break;
        }
    }
}
