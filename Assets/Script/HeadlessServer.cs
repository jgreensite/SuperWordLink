using System;
using UnityEngine;

public class HeadlessServer : MonoBehaviour
{
    public GameObject serverPrefab;

    public static HeadlessServer Instance { set; get; }

    // Use this for initialization
    private void Start()
    {
        try
        {
            var s = Instantiate(serverPrefab).GetComponent<Server>();
            s.Init();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}