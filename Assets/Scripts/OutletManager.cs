using UnityEngine;

public enum OutletConnectionType
{
    In,
    Out,
}

public class OutletManager : MonoBehaviour
{
    public OutletConnectionType? pendingConnectionType = null;

    public static OutletManager Instance
    {
        get; private set;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one Player instance");
        }
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
