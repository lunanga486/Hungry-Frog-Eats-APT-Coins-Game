using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceBoost : MonoBehaviour
{
    // Singleton instance
    public static ResourceBoost Instance { get; private set; }

    // Variable to store the resource boost value
    public int piggy = 0;
    public int doggy = 0;
    public int goaty = 0;
    public int turtle = 0;
    public int koala = 0;
    public int sheep = 0;
    public int duck = 0;
    public int heartBoost = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetBoostValue()
    {
        heartBoost = 0;
    }
}