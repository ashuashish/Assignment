using UnityEngine;

public class InteractionGate : MonoBehaviour
{
    public static InteractionGate Instance { get; private set; }

    public bool IsBusy { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool TryLock()
    {
        if (IsBusy)
            return false;

        IsBusy = true;
        return true;
    }

    public void Release()
    {
        IsBusy = false;
    }
}
