using System.Collections;
using UnityEngine;

public class ReactionSequence_BlendShapes : MonoBehaviour
{
    // =============================
    // REFERENCES
    // =============================
    [Header("Face Mesh")]
    [SerializeField] private SkinnedMeshRenderer headMesh;

    [Header("Body Animator")]
    [SerializeField] private Animator bodyAnimator;

    // =============================
    // BLENDSHAPE INDICES (Index-Based)
    // =============================
    [Header("Smile Blendshapes")]
    [SerializeField] private int smileLeft = 62;
    [SerializeField] private int smileRight = 63;

    [Header("Sad Blendshapes")]
    [SerializeField] private int sadLeft = 27;
    [SerializeField] private int sadRight = 28;
    [SerializeField] private int browSad = 17;

    // =============================
    // ANIMATOR TRIGGERS
    // =============================
    [Header("Animator Triggers")]
    [SerializeField] private string smileTrigger = "SmileGesture";
    [SerializeField] private string sadTrigger = "SadGesture";
    [SerializeField] private string resetTrigger = "ResetGesture";

    // =============================
    // TIMING
    // =============================
    [Header("Timings")]
    [SerializeField] private float blendDuration = 0.35f;
    [SerializeField] private float holdDuration = 2f;
    [SerializeField] private float gapBetweenReactions = 0.8f;

    // =============================
    // STATE
    // =============================
    private Coroutine activeSequence;
    private static readonly float MaxWeight = 100f;
    private static readonly float SmileMaxWeight = 50;
    // =============================
    // PUBLIC ENTRY POINT (UI Button)
    // =============================
    public void PlayReactionSequence()
    {
        if (activeSequence != null)
            return; // ignore extra clicks during playback


        if (!InteractionGate.Instance.TryLock())
            return;



        activeSequence = StartCoroutine(ReactionSequence());

        GetComponent<LipSyncController>().PlayDialogue();
    }

    // =============================
    // MAIN SEQUENCE
    // =============================
    private IEnumerator ReactionSequence()
    {
        yield return PlaySmile();
        yield return new WaitForSeconds(gapBetweenReactions);

        yield return PlaySad();
        yield return new WaitForSeconds(gapBetweenReactions);

        yield return PlaySmile();
        yield return new WaitForSeconds(gapBetweenReactions);

        yield return PlaySad();

        TriggerAnimator(resetTrigger);
        activeSequence = null;

        InteractionGate.Instance.Release();
    }

    // =============================
    // SMILE
    // =============================
    private IEnumerator PlaySmile()
    {
        TriggerAnimator(smileTrigger);

        yield return Blend(
            new[] { smileLeft, smileRight },
            0f, SmileMaxWeight, blendDuration);

        yield return new WaitForSeconds(holdDuration);

        yield return Blend(
            new[] { smileLeft, smileRight },
            SmileMaxWeight, 0f, blendDuration);
    }

    // =============================
    // SAD
    // =============================
    private IEnumerator PlaySad()
    {
        TriggerAnimator(sadTrigger);

        yield return Blend(
            new[] { sadLeft, sadRight, browSad },
            0f, MaxWeight, blendDuration);

        yield return new WaitForSeconds(holdDuration);

        yield return Blend(
            new[] { sadLeft, sadRight, browSad },
            MaxWeight, 0f, blendDuration);
    }

    // =============================
    // BLEND CORE (Optimized)
    // =============================
    private IEnumerator Blend(int[] indices, float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float weight = Mathf.Lerp(from, to, elapsed / duration);

            for (int i = 0; i < indices.Length; i++)
            {
                int idx = indices[i];
                if (idx >= 0)
                    headMesh.SetBlendShapeWeight(idx, weight);
            }
            yield return null;
        }

        // ensure final value
        for (int i = 0; i < indices.Length; i++)
        {
            int idx = indices[i];
            if (idx >= 0)
                headMesh.SetBlendShapeWeight(idx, to);
        }
    }

    // =============================
    // ANIMATOR HANDLING
    // =============================
    private void TriggerAnimator(string trigger)
    {
        if (!bodyAnimator) return;

        bodyAnimator.ResetTrigger(smileTrigger);
        bodyAnimator.ResetTrigger(sadTrigger);
        bodyAnimator.ResetTrigger(resetTrigger);

        bodyAnimator.SetTrigger(trigger);
    }
}
