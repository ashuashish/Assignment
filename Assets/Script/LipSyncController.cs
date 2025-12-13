using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LipSyncController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LipSyncEngine engine;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dialogueClip;

    [TextArea(3, 6)]
    public string dialogText;

    [Header("Animator")]                          
    [SerializeField] private Animator animator;  
    [SerializeField] private string talkingParam = "Talking"; 

    void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    public void PlayDialogue()
    {
        if (!engine || !dialogueClip) return;


        if (!InteractionGate.Instance.TryLock())
            return;

        audioSource.clip = dialogueClip;
        audioSource.Play();

        engine.StartLipSync(dialogText, audioSource);
        animator.SetBool("Talking", true);
    }

    public void StopDialogue()
    {
        animator.SetBool("Talking", false);
        audioSource.Stop();
        engine.StopLipSync();
    }
}
