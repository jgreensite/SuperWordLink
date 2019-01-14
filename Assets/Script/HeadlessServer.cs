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
            //Create the Host's server first
            var s = Instantiate(serverPrefab);
            s.GetComponent<Server>().Init();
            s.GetComponent<GameBoardState>().Init();
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