using System.Collections;
using UnityEngine;

public class PLCSequence : MonoBehaviour
{
    public Transform forwardPiston;
    public Transform upCylinder;
    public Transform rotator;

    private readonly Vector3 forwardStart = new Vector3(0, 0, 0);
    private readonly Vector3 forwardEnd = new Vector3(0, 0, 4.5f);

    private readonly Vector3 upStart = new Vector3(0, 0, 0);
    private readonly Vector3 upEnd = new Vector3(0, 2.5f, 0);

    private readonly Vector3 rotUpStart = new Vector3(-3.052145f, 6.6f, 2.095987f);
    private readonly Vector3 rotUpEnd = new Vector3(-3.052145f, 9.3f, 2.095987f);

    private readonly Vector3 rotStartEuler = new Vector3(-90, 360, 0);
    private readonly Vector3 rotEndEuler = new Vector3(-90, 180, 0);

    public Rigidbody carriedObject;
    public Transform objectSpawnPoint;
    public Rigidbody objectPrefab;

    public static float forwardSpeed = 15f;
    public static float upSpeed = 15f;
    public static float anticlockwiseSpeed = 15f;
    public static float reverseSpeed = 15f;
    public static float downSpeed = 15f;
    public static float clockwiseSpeed = 15f;
    public static readonly float duration = 15f;
    public static bool isPlaying = false;

    private Coroutine sequenceCoroutine;

    void Awake()
    {
        if (forwardPiston) forwardPiston.localPosition = forwardStart;
        if (upCylinder) upCylinder.localPosition = upStart;
        if (rotator)
        {
            rotator.localPosition = rotUpStart;
            rotator.localRotation = Quaternion.Euler(rotStartEuler);
        }
    }

    void Start()
    {
        sequenceCoroutine = StartCoroutine(RunSequence());
    }

    void GrabObject()
    {
        if (!carriedObject) return;
        carriedObject.isKinematic = true;
        carriedObject.transform.SetParent(rotator);
    }

    void DropObject()
    {
        if (!carriedObject) return;
        carriedObject.transform.SetParent(null);
        carriedObject.isKinematic = false;
        Destroy(carriedObject.gameObject, 5f);
    }

    void SpawnNewObject()
    {
        Rigidbody obj = Instantiate(objectPrefab, objectSpawnPoint.position, Quaternion.Euler(90f, 0, 0));
        carriedObject = obj;
    }

    void HeatOnly(Transform active, float speed, string label, bool reverse)
    {
        SetHeat(forwardPiston, forwardPiston == active, speed, "linear piston", reverse);
        SetHeat(upCylinder, upCylinder == active, speed, "vertical cylinder", reverse);
        SetHeat(rotator, rotator == active, speed, "rotator", reverse);
    }

    void SetHeat(Transform t, bool active, float speed, string label, bool reverse)
    {
        if (!t) return;
        HeatEmitter h = t.GetComponent<HeatEmitter>();
        if (!h) return;
        h.SetSpeedMultiplier(label, reverse);
        h.SetActive(active, active ? speed : 0f);
    }

    IEnumerator RunSequence()
    {
        yield return new WaitUntil(() => isPlaying);
        bool objectSpawned = false;

        while (true)
        {
            yield return new WaitUntil(() => isPlaying);

            if (!objectSpawned)
            {
                SpawnNewObject();
                objectSpawned = true;
            }

            HeatOnly(forwardPiston, forwardSpeed, "linear piston", false);
            yield return MoveLocalPosition(forwardPiston, forwardStart, forwardEnd, duration / forwardSpeed);

            HeatOnly(upCylinder, upSpeed, "vertical cylinder", false);
            Coroutine up1 = StartCoroutine(MoveLocalPosition(upCylinder, upStart, upEnd, duration / upSpeed));
            Coroutine rot1 = StartCoroutine(MoveLocalPosition(rotator, rotUpStart, rotUpEnd, duration / upSpeed));
            yield return up1;
            yield return rot1;

            HeatOnly(rotator, anticlockwiseSpeed, "rotator", false);
            yield return RotateLocalEuler(rotator, rotStartEuler, rotEndEuler, duration / anticlockwiseSpeed);

            HeatOnly(upCylinder, downSpeed, "vertical cylinder", true);
            Coroutine up2 = StartCoroutine(MoveLocalPosition(upCylinder, upEnd, upStart, duration / downSpeed));
            Coroutine rot2 = StartCoroutine(MoveLocalPosition(rotator, rotUpEnd, rotUpStart, duration / downSpeed));
            yield return up2;
            yield return rot2;

            GrabObject();

            HeatOnly(forwardPiston, reverseSpeed, "linear piston", true);
            yield return MoveLocalPosition(forwardPiston, forwardEnd, forwardStart, duration / reverseSpeed);

            HeatOnly(upCylinder, upSpeed, "vertical cylinder", false);
            Coroutine up3 = StartCoroutine(MoveLocalPosition(upCylinder, upStart, upEnd, duration / upSpeed));
            Coroutine rot3 = StartCoroutine(MoveLocalPosition(rotator, rotUpStart, rotUpEnd, duration / upSpeed));
            yield return up3;
            yield return rot3;

            HeatOnly(rotator, clockwiseSpeed, "rotator", true);
            yield return RotateLocalEuler(rotator, rotEndEuler, rotStartEuler, duration / clockwiseSpeed);

            HeatOnly(upCylinder, downSpeed, "vertical cylinder", true);
            Coroutine up4 = StartCoroutine(MoveLocalPosition(upCylinder, upEnd, upStart, duration / downSpeed));
            Coroutine rot4 = StartCoroutine(MoveLocalPosition(rotator, rotUpEnd, rotUpStart, duration / downSpeed));
            yield return up4;
            yield return rot4;

            DropObject();

            SetHeat(forwardPiston, false, 0, "linear piston", false);
            SetHeat(upCylinder, false, 0, "vertical cylinder", false);
            SetHeat(rotator, false, 0, "rotator", false);

            if (!isPlaying)
                objectSpawned = false;
            else
                SpawnNewObject();
        }
    }

    IEnumerator MoveLocalPosition(Transform t, Vector3 start, Vector3 end, float d)
    {
        if (!t) yield break;
        float e = 0f;
        t.localPosition = start;
        while (e < d)
        {
            e += Time.deltaTime;
            t.localPosition = Vector3.Lerp(start, end, e / d);
            yield return null;
        }
        t.localPosition = end;
    }

    IEnumerator RotateLocalEuler(Transform t, Vector3 start, Vector3 end, float d)
    {
        if (!t) yield break;
        Quaternion a = Quaternion.Euler(start);
        Quaternion b = Quaternion.Euler(end);
        float e = 0f;
        t.localRotation = a;
        while (e < d)
        {
            e += Time.deltaTime;
            t.localRotation = Quaternion.Slerp(a, b, e / d);
            yield return null;
        }
        t.localRotation = b;
    }

    public void StopSequence()
    {
        isPlaying = false;

        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        SetHeat(forwardPiston, false, 0, "linear piston", false);
        SetHeat(upCylinder, false, 0, "vertical cylinder", false);
        SetHeat(rotator, false, 0, "rotator", false);
    }
}
