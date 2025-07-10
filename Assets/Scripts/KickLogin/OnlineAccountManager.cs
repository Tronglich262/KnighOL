using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class OnlineAccountManager : MonoBehaviour
{
    public Dictionary<string, PlayerRef> OnlineTokens = new Dictionary<string, PlayerRef>();

    public static OnlineAccountManager Instance;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
}
