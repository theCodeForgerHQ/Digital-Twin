using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using UnityEngine.InputSystem.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class ControlPanelUI : MonoBehaviour
{
    public GameObject linearPiston;
    public GameObject rotaryCylinder;
    public GameObject verticalPiston;

    private readonly Vector3 reverseEnd = new Vector3(0, 0, 0);
    private readonly Vector3 forwardEnd = new Vector3(0, 0, (float)4.5);
    private readonly Vector3 downEnd = new Vector3(0, 0, 0);
    private readonly Vector3 upEnd = new Vector3(0, (float)2.5, 0);

    private readonly Vector3 rotDownEnd = new Vector3((float)-3.052145, (float)6.6, (float)2.095987);
    private readonly Vector3 rotUpEnd = new Vector3((float)-3.052145, (float)9.3, (float)2.095987);

    private readonly Vector3 rotAntiEndEuler = new Vector3(-90, 360, 0);
    private readonly Vector3 rotEndEuler = new Vector3(-90, 180, 0);

    private string serverIP = "192.168.56.60";
    private int serverPort = 4210;

    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    Canvas runtimeCanvas;
    GameObject panel;
    GameObject toggleButton;

    void Start()
    {
        udpClient = new UdpClient();
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

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
        CreateButtonLayout(panel);
        panel.SetActive(false);
    }

    GameObject CreatePanel(Canvas canvas)
    {
        var go = new GameObject("ControlPanel");
        go.transform.SetParent(canvas.transform, false);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.08f, 0.08f, 0.08f, 0.96f);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -64);
        rt.sizeDelta = new Vector2(420, 250);

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
        tt.text = "Controls Panel";
        tt.alignment = TextAnchor.MiddleCenter;
        tt.fontSize = 14;
        tt.color = Color.white;

        return go;
    }

    void CreateToggleButton(Canvas canvas)
    {
        toggleButton = new GameObject("ToggleControlPanel");
        toggleButton.transform.SetParent(canvas.transform, false);

        var img = toggleButton.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f);

        var btn = toggleButton.AddComponent<Button>();

        var rt = toggleButton.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -20);
        rt.sizeDelta = new Vector2(180, 36);

        var txtGo = new GameObject("Text");
        txtGo.transform.SetParent(toggleButton.transform, false);

        var t = txtGo.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = "Toggle Controls Panel";
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
            SendUDP("Toggle Controls Panel");
        });
    }

    void CreateButtonLayout(GameObject panel)
    {
        float buttonWidth = 120;
        float buttonHeight = 40;
        float padding = 10;

        var panelRectTransform = panel.GetComponent<RectTransform>();
        Vector2 panelSize = panelRectTransform.sizeDelta;

        float totalButtonHeight = (buttonHeight + padding) * 3;
        float panelHeight = panelSize.y;

        float initialY = (panelHeight - totalButtonHeight) * 0.5f;

        CreateButton(panel, "Start", new Vector2(-(buttonWidth + padding), initialY), buttonWidth, buttonHeight);
        CreateButton(panel, "Stop", new Vector2(0, initialY), buttonWidth, buttonHeight);
        CreateButton(panel, "Reset", new Vector2(buttonWidth + padding, initialY), buttonWidth, buttonHeight);

        float yOffset = -(buttonHeight + padding);

        CreateButton(panel, "Forward", new Vector2(-(buttonWidth + padding), initialY + yOffset), buttonWidth, buttonHeight);
        CreateButton(panel, "Anticlockwise", new Vector2(0, initialY + yOffset), buttonWidth, buttonHeight);
        CreateButton(panel, "Up", new Vector2(buttonWidth + padding, initialY + yOffset), buttonWidth, buttonHeight);

        yOffset -= (buttonHeight + padding);

        CreateButton(panel, "Reverse", new Vector2(-(buttonWidth + padding), initialY + yOffset), buttonWidth, buttonHeight);
        CreateButton(panel, "Clockwise", new Vector2(0, initialY + yOffset), buttonWidth, buttonHeight);
        CreateButton(panel, "Down", new Vector2(buttonWidth + padding, initialY + yOffset), buttonWidth, buttonHeight);
    }

    void CreateButton(GameObject parent, string label, Vector2 position, float width, float height)
    {
        GameObject buttonObj = new GameObject(label);
        buttonObj.transform.SetParent(parent.transform, false);

        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f);

        var rectTransform = buttonObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(width, height);
        rectTransform.anchoredPosition = position;

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        Text buttonText = textObj.AddComponent<Text>();
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.text = label;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        buttonText.fontSize = 14;

        var textRectTransform = textObj.GetComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.offsetMin = Vector2.zero;
        textRectTransform.offsetMax = Vector2.zero;

        button.onClick.AddListener(() =>
        {
            SendUDP(label);

            switch (label)
            {
                case "Start":
                    PLCSequence.isPlaying = true;
                    break;
                case "Stop":
                    PLCSequence.isPlaying = false;
                    break;
                case "Forward":
                    if (!PLCSequence.isPlaying)
                    {
                        StopAllCoroutines();
                        StartCoroutine(MoveObject(linearPiston, forwardEnd, PLCSequence.duration / PLCSequence.forwardSpeed));
                    }
                    break;
                case "Reverse":
                    if (!PLCSequence.isPlaying)
                    {
                        StopAllCoroutines();
                        StartCoroutine(MoveObject(linearPiston, reverseEnd, PLCSequence.duration / PLCSequence.reverseSpeed));
                    }
                    break;
                case "Up":
                    if (!PLCSequence.isPlaying)
                    {
                        StopAllCoroutines();
                        StartCoroutine(MoveObject(verticalPiston, upEnd, PLCSequence.duration / PLCSequence.upSpeed));
                        StartCoroutine(MoveObject(rotaryCylinder, rotUpEnd, PLCSequence.duration / PLCSequence.upSpeed));
                    }
                    break;
                case "Down":
                    if (!PLCSequence.isPlaying)
                    {
                        StopAllCoroutines();
                        StartCoroutine(MoveObject(verticalPiston, downEnd, PLCSequence.duration / PLCSequence.downSpeed));
                        StartCoroutine(MoveObject(rotaryCylinder, rotDownEnd, PLCSequence.duration / PLCSequence.downSpeed));
                    }
                    break;
                case "Clockwise":
                    if (!PLCSequence.isPlaying)
                    {
                        StopAllCoroutines();
                        StartCoroutine(RotateObject(rotaryCylinder, rotEndEuler, PLCSequence.duration / PLCSequence.clockwiseSpeed));
                    }
                    break;
                case "Anticlockwise":
                    if (!PLCSequence.isPlaying)
                    {
                        StopAllCoroutines();
                        StartCoroutine(RotateObject(rotaryCylinder, rotAntiEndEuler, PLCSequence.duration / PLCSequence.anticlockwiseSpeed));
                    }
                    break;
                case "Reset":
                    if (!PLCSequence.isPlaying)
                    {
                        StopAllCoroutines();
                        StartCoroutine(MoveObject(linearPiston, reverseEnd, PLCSequence.duration / PLCSequence.reverseSpeed));
                        StartCoroutine(MoveObject(verticalPiston, downEnd, PLCSequence.duration / PLCSequence.downSpeed));
                        StartCoroutine(MoveObject(rotaryCylinder, rotDownEnd, PLCSequence.duration / PLCSequence.downSpeed));
                        StartCoroutine(RotateObject(rotaryCylinder, rotAntiEndEuler, PLCSequence.duration / PLCSequence.anticlockwiseSpeed));
                    }
                    break;
            }
        });
    }

    void SendUDP(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, remoteEndPoint);
    }

    IEnumerator MoveObject(GameObject obj, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = obj.transform.position;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            obj.transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = targetPosition;
    }

    IEnumerator RotateObject(GameObject obj, Vector3 targetRotation, float duration)
    {
        Quaternion startRotation = obj.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(targetRotation);
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            obj.transform.rotation = Quaternion.Slerp(startRotation, endRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.rotation = endRotation;
    }

    private void OnApplicationQuit()
    {
        udpClient.Close();
        udpClient = null;
    }
}
