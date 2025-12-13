using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class LipSyncEngine : MonoBehaviour
{
    [System.Serializable]
    private class PhonemeClip
    {
        public float start;
        public int blendIndex;
        public float weight;
    }

    [Header("Meshes")]
    [SerializeField] private SkinnedMeshRenderer headMesh;
    [SerializeField] private SkinnedMeshRenderer teethMesh;

    [Header("Tuning")]
    [SerializeField] private float smoothSpeed = 18f;
    [SerializeField] private float phonemeWindow = 0.12f;



    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string talkingParam = "Talking";

    private readonly List<PhonemeClip> timeline = new();
    private readonly Dictionary<string, int> phonemeMap = new();

    private AudioSource audioSource;
    private float timer;
    private int timelineCursor;
    private bool playing;
   
    // =============================
    // PUBLIC API
    // =============================
    public void StartLipSync(string text, AudioSource source)
    {
        if (!headMesh || source == null) return;

        


        audioSource = source;
        timer = 0f;
        timelineCursor = 0;
        playing = true;

        InitBlendshapeMap();
        GenerateTimeline(text);
        AnimationTrigger(true);
    }

   void AnimationTrigger(bool Isactive)
    {
       animator.SetBool(talkingParam, Isactive);
    }

    public void StopLipSync()
    {
       
        playing = false;
        ResetAllBlendshapes();
        AnimationTrigger(false);
        InteractionGate.Instance.Release();
    }

    // =============================
    // UPDATE
    // =============================
    void Update()
    {
        if (!playing || audioSource == null) return;

        timer += Time.deltaTime;

        ApplyActivePhoneme();

        if (timer >= audioSource.clip.length)
        {
            StopLipSync();
        }
    }

    // =============================
    // CORE OPTIMIZATION
    // =============================
    void ApplyActivePhoneme()
    {
        if (timeline.Count == 0) return;

        // Move cursor forward only (O(1) amortized)
        while (timelineCursor < timeline.Count - 1 &&
               timer >= timeline[timelineCursor + 1].start)
        {
            timelineCursor++;
        }

        PhonemeClip clip = timeline[timelineCursor];

        // fade all to zero (only mapped phonemes)
        foreach (var idx in phonemeMap.Values)
        {
            LerpBlend(idx, 0f);
        }

        if (timer <= clip.start + phonemeWindow)
        {
            LerpBlend(clip.blendIndex, clip.weight * 100f);
        }
    }

    void LerpBlend(int idx, float target)
    {
        if (idx < 0) return;

        float current = headMesh.GetBlendShapeWeight(idx);
        float val = Mathf.Lerp(current, target, Time.deltaTime * smoothSpeed);

        headMesh.SetBlendShapeWeight(idx, val);
        if (teethMesh) teethMesh.SetBlendShapeWeight(idx, val);
    }

    // =============================
    // BLENDSHAPE MAP
    // =============================
    void InitBlendshapeMap()
    {
        phonemeMap.Clear();

        Add("AA", "viseme_aa");
        Add("E", "viseme_E");
        Add("O", "viseme_O");
        Add("U", "viseme_U");
        Add("PP", "viseme_PP");
        Add("FF", "viseme_FF");
    }

    void Add(string key, string name)
    {
        int idx = headMesh.sharedMesh.GetBlendShapeIndex(name);
        phonemeMap[key] = idx;
    }

    // =============================
    // TIMELINE GENERATION
    // =============================
    void GenerateTimeline(string dialog)
    {
        timeline.Clear();
        if (string.IsNullOrWhiteSpace(dialog)) return;

        float time = 0f;
        string[] words = dialog.Split(' ');

        foreach (string w in words)
        {
            string clean = new string(w.Where(char.IsLetter).ToArray());
            if (string.IsNullOrEmpty(clean)) continue;

            float dur = Mathf.Max(0.25f, clean.Length * 0.05f);
            float step = dur / clean.Length;

            for (int i = 0; i < clean.Length; i++)
            {
                string p = GuessPhoneme(clean[i]);

                if (!phonemeMap.ContainsKey(p)) continue;

                timeline.Add(new PhonemeClip
                {
                    start = time + i * step,
                    blendIndex = phonemeMap[p],
                    weight = Random.Range(0.65f, 0.9f)
                });
            }

            time += dur;
        }
    }

    string GuessPhoneme(char c)
    {
        c = char.ToLower(c);
        switch (c)
        {
            // Vowels
            case 'a': return "AA";
            case 'e':
            case 'i': return "E";
            case 'o': return "O";
            case 'u': return "U";

            // Labials
            case 'f':
            case 'v': return "FF";
            case 'm':
            case 'b':
            case 'p': return "PP";

            // Dentals/Alveolars
            case 't':
            case 'd': return "TT";
            case 'n': return "NN";
            case 's':
            case 'z': return "SS";
            case 'l': return "LL";

            // Velars
            case 'k':
            case 'g': return "KK";
            case 'h': return "HH";

            // Glides
            case 'r': return "RR";
            case 'w': return "WW";
            case 'y': return "YY";

            // Default
            default: return "AA";
        }
    }

    void ResetAllBlendshapes()
    {
        foreach (var idx in phonemeMap.Values)
        {
            if (idx < 0) continue;
            headMesh.SetBlendShapeWeight(idx, 0f);
            if (teethMesh) teethMesh.SetBlendShapeWeight(idx, 0f);
        }
    }
}
