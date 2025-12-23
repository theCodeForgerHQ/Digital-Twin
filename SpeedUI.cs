using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.InputSystem.UI;

public class SpeedUI : MonoBehaviour
{
    public static bool showHeatmap = false;

    public MonoBehaviour targetScript;
    readonly string[,] layoutPairs = new string[3, 2]
    {
        { "upSpeed", "downSpeed" },
        { "clockwiseSpeed", "anticlockwiseSpeed" },
        { "forwardSpeed", "reverseSpeed" }
    };

    readonly string[,] displayPairs = new string[3, 2]
    {
        { "Up", "Down" },
        { "Clockwise", "Anticlockwise" },
        { "Forward", "Reverse" }
    };

    float minVal = 10f;
    float maxVal = 100f;
    private float duration = 15f;
    Text throughputText;
    Button heatmapToggleButton;

    Canvas runtimeCanvas;
    GameObject panel;
    GameObject toggleButton;

    void Start()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        runtimeCanvas = FindObjectOfType<Canvas>();
        if (runtimeCanvas == null)
        {
            var cgo = new GameObject("RuntimeCanvas");
            runtimeCanvas = cgo.AddComponent<Canvas>();
            runtimeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
        }

        panel = CreatePanel(runtimeCanvas);
        CreateToggleButton(runtimeCanvas);
        BuildKnobGrid(panel.GetComponent<RectTransform>());
        panel.SetActive(false);
    }

    GameObject CreatePanel(Canvas canvas)
    {
        var go = new GameObject("SpeedPanel");
        go.transform.SetParent(canvas.transform, false);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.08f, 0.08f, 0.08f, 0.96f);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -64);
        rt.sizeDelta = new Vector2(420, 400);

        var title = new GameObject("Title");
        title.transform.SetParent(go.transform, false);
        var trt = title.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0.5f, 1);
        trt.anchoredPosition = new Vector2(0, -8);
        trt.sizeDelta = new Vector2(0, 22);

        var tt = title.AddComponent<Text>();
        tt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tt.text = "Speed Controls";
        tt.alignment = TextAnchor.MiddleCenter;
        tt.fontSize = 14;
        tt.color = Color.white;

        var throughputGO = new GameObject("ThroughputText");
        throughputGO.transform.SetParent(go.transform, false);
        var ttr = throughputGO.AddComponent<RectTransform>();
        ttr.anchorMin = new Vector2(0.5f, 0);
        ttr.anchorMax = new Vector2(0.5f, 0);
        ttr.pivot = new Vector2(0.5f, 0);
        ttr.anchoredPosition = new Vector2(0, 92);
        ttr.sizeDelta = new Vector2(380, 22);

        throughputText = throughputGO.AddComponent<Text>();
        throughputText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        throughputText.fontSize = 14;
        throughputText.alignment = TextAnchor.MiddleLeft;
        throughputText.color = Color.white;
        throughputText.text = "Throughput = 0";

        CreateHeatmapToggleButton(go);

        return go;
    }

    void CreateToggleButton(Canvas canvas)
    {
        toggleButton = new GameObject("ToggleSpeedControl");
        toggleButton.transform.SetParent(canvas.transform, false);

        var img = toggleButton.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f);

        var btn = toggleButton.AddComponent<Button>();

        var rt = toggleButton.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -20);
        rt.sizeDelta = new Vector2(180, 36);

        var txtGo = new GameObject("Text");
        txtGo.transform.SetParent(toggleButton.transform, false);

        var t = txtGo.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = "Toggle Speed Control";
        t.fontSize = 14;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;

        var trt = txtGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        btn.onClick.AddListener(() =>
        {
            panel.SetActive(!panel.activeSelf);
            if (panel.activeSelf) ComputeAndDisplayThroughput();
        });
    }

    void CreateHeatmapToggleButton(GameObject panel)
    {
        var heatmapButton = new GameObject("HeatmapButton");
        heatmapButton.transform.SetParent(panel.transform, false);

        var img = heatmapButton.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f);

        heatmapToggleButton = heatmapButton.AddComponent<Button>();

        var rt = heatmapButton.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 20);
        rt.sizeDelta = new Vector2(390, 40);

        var txtGo = new GameObject("Text");
        txtGo.transform.SetParent(heatmapButton.transform, false);

        var t = txtGo.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = "Toggle Heatmap";
        t.fontSize = 14;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;

        var trt = txtGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        heatmapToggleButton.onClick.AddListener(() =>
        {
            showHeatmap = !showHeatmap;
            img.color = showHeatmap ? new Color(0.2f, 0.4f, 0.8f) : new Color(0.2f, 0.2f, 0.2f);
        });
    }

    void BuildKnobGrid(RectTransform panelRT)
    {
        var gridGO = new GameObject("KnobGrid");
        gridGO.transform.SetParent(panelRT, false);

        var grt = gridGO.AddComponent<RectTransform>();
        grt.anchorMin = new Vector2(0, 0);
        grt.anchorMax = new Vector2(1, 1);
        grt.offsetMin = new Vector2(12, 12);
        grt.offsetMax = new Vector2(-12, -36);

        var grid = gridGO.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.cellSize = new Vector2(120, 110);
        grid.spacing = new Vector2(14, 6);
        grid.childAlignment = TextAnchor.UpperCenter;

        for (int c = 0; c < 3; c++)
        {
            for (int r = 0; r < 2; r++)
            {
                string varName = layoutPairs[c, r];
                string disp = displayPairs[c, r];
                var knob = CreateKnob(disp, varName);
                knob.transform.SetParent(gridGO.transform, false);
            }
        }
    }

    GameObject CreateKnob(string labelStr, string varName)
    {
        var go = new GameObject(labelStr);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.12f, 0.12f, 0.12f);

        var txtGo = new GameObject("Label");
        txtGo.transform.SetParent(go.transform, false);
        var txt = txtGo.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = labelStr;
        txt.alignment = TextAnchor.UpperCenter;
        txt.fontSize = 12;
        txt.color = new Color(0.8f, 0.8f, 0.8f);

        var trt = txtGo.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0.5f, 1);
        trt.anchoredPosition = new Vector2(0, -6);
        trt.sizeDelta = new Vector2(0, 20);

        var valGo = new GameObject("ValueText");
        valGo.transform.SetParent(go.transform, false);
        var valTxt = valGo.AddComponent<Text>();
        valTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        valTxt.text = "0";
        valTxt.alignment = TextAnchor.MiddleCenter;
        valTxt.fontSize = 12;
        valTxt.color = Color.cyan;

        var vrt = valGo.GetComponent<RectTransform>();
        vrt.anchorMin = new Vector2(0, 0);
        vrt.anchorMax = new Vector2(1, 0);
        vrt.anchoredPosition = new Vector2(0, 10);
        vrt.sizeDelta = new Vector2(0, 20);

        var sliderGo = new GameObject("Slider");
        sliderGo.transform.SetParent(go.transform, false);
        var srt = sliderGo.AddComponent<RectTransform>();
        srt.anchorMin = new Vector2(0, 0.5f);
        srt.anchorMax = new Vector2(1, 0.5f);
        srt.anchoredPosition = new Vector2(0, -5);
        srt.sizeDelta = new Vector2(-20, 20);

        var bg = new GameObject("Background");
        bg.transform.SetParent(sliderGo.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f);

        var bgrt = bg.GetComponent<RectTransform>();
        bgrt.anchorMin = Vector2.zero;
        bgrt.anchorMax = Vector2.one;
        bgrt.sizeDelta = Vector2.zero;

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGo.transform, false);
        var fart = fillArea.AddComponent<RectTransform>();
        fart.anchorMin = new Vector2(0, 0.25f);
        fart.anchorMax = new Vector2(1, 0.75f);
        fart.sizeDelta = new Vector2(-10, 0);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fimg = fill.AddComponent<Image>();
        fimg.color = new Color(0.3f, 0.6f, 0.9f);

        var frt = fill.GetComponent<RectTransform>();
        frt.sizeDelta = Vector2.zero;

        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderGo.transform, false);
        var hart = handleArea.AddComponent<RectTransform>();
        hart.anchorMin = Vector2.zero;
        hart.anchorMax = Vector2.one;
        hart.sizeDelta = new Vector2(-10, 0);

        var handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        var himg = handle.AddComponent<Image>();
        himg.color = Color.white;

        var hrt = handle.GetComponent<RectTransform>();
        hrt.sizeDelta = new Vector2(20, 0);

        var slider = sliderGo.AddComponent<Slider>();
        slider.targetGraphic = himg;
        slider.fillRect = frt;
        slider.handleRect = hrt;
        slider.minValue = minVal;
        slider.maxValue = maxVal;
        slider.value = minVal;

        if (targetScript != null)
        {
            var field = targetScript.GetType().GetField(varName);
            if (field != null)
            {
                var val = field.GetValue(targetScript);
                if (val is float f) slider.value = f;
                else if (val is int i) slider.value = i;
            }
        }

        slider.onValueChanged.AddListener((v) =>
        {
            valTxt.text = v.ToString("F1");
            if (targetScript != null)
            {
                var field = targetScript.GetType().GetField(varName);
                if (field != null)
                {
                    if (field.FieldType == typeof(float)) field.SetValue(targetScript, v);
                    else if (field.FieldType == typeof(int)) field.SetValue(targetScript, (int)v);
                }
            }
            ComputeAndDisplayThroughput();
        });

        valTxt.text = slider.value.ToString("F1");

        return go;
    }

    float ReadTargetFloat(string variableName, float def = 0f)
    {
        if (targetScript == null) return def;
        var ty = targetScript.GetType();
        var f = ty.GetField(variableName);
        if (f != null && f.FieldType == typeof(float)) return (float)f.GetValue(targetScript);
        var p = ty.GetProperty(variableName);
        if (p != null && p.PropertyType == typeof(float) && p.CanRead) return (float)p.GetValue(targetScript);
        return def;
    }

    void ComputeAndDisplayThroughput()
    {
        if (targetScript == null || throughputText == null) return;

        float totalTime = 0f;
        totalTime += (duration / ReadTargetFloat("forwardSpeed", minVal));
        totalTime += (duration / ReadTargetFloat("upSpeed", minVal));
        totalTime += (duration / ReadTargetFloat("anticlockwiseSpeed", minVal));
        totalTime += (duration / ReadTargetFloat("downSpeed", minVal));
        totalTime += (duration / ReadTargetFloat("reverseSpeed", minVal));
        totalTime += (duration / ReadTargetFloat("upSpeed", minVal));
        totalTime += (duration / ReadTargetFloat("clockwiseSpeed", minVal));
        totalTime += (duration / ReadTargetFloat("downSpeed", minVal));
        float throughput = 0f;
        if (totalTime > 0f) throughput = 1f / totalTime * 60;

        throughputText.text = $"Throughput (Est): {throughput:F1} units/min";
    }

    float GetValue(string varName)
    {
        if (targetScript == null) return 0;
        var field = targetScript.GetType().GetField(varName);
        if (field != null)
        {
            var val = field.GetValue(targetScript);
            if (val is float f) return f;
            if (val is int i) return i;
        }
        return 0;
    }
}
