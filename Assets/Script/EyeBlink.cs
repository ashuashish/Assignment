using System.Collections;
using UnityEngine;

public class EyeBlink : MonoBehaviour
{
    [Header("Face Mesh")]
    public SkinnedMeshRenderer headMesh;

    [Header("Eye Blink Blendshape Index")]
    public int eyeBlinkLeft = 0;   // put correct index
    public int eyeBlinkRight = 1;  // put correct index

    [Header("Blink Settings")]
    public float blinkCloseTime = 0.06f;
    public float blinkOpenTime = 0.08f;
    public float blinkHoldTime = 0.03f;

    [Header("Auto Blink")]
    public bool autoBlink = true;
    public Vector2 blinkInterval = new Vector2(2.5f, 5f);

    void Start()
    {
        if (autoBlink)
            StartCoroutine(AutoBlink());
    }

    // ============================
    // AUTO BLINK
    // ============================
    IEnumerator AutoBlink()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(blinkInterval.x, blinkInterval.y));
            yield return Blink();
        }
    }

    // ============================
    // MANUAL CALL (Button / Event)
    // ============================
    public void BlinkOnce()
    {
        StopAllCoroutines();
        StartCoroutine(Blink());
    }

    // ============================
    // BLINK LOGIC (BOTH EYES TOGETHER)
    // ============================
    IEnumerator Blink()
    {
        // Close
        yield return BlendEyes(0, 100, blinkCloseTime);

        yield return new WaitForSeconds(blinkHoldTime);

        // Open
        yield return BlendEyes(100, 0, blinkOpenTime);
    }

    IEnumerator BlendEyes(float from, float to, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float value = Mathf.Lerp(from, to, t);

            headMesh.SetBlendShapeWeight(eyeBlinkLeft, value);
            headMesh.SetBlendShapeWeight(eyeBlinkRight, value);

            yield return null;
        }
    }
}
