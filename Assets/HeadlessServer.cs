using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HeadlessServer : MonoBehaviour {

	public static HeadlessServer Instance { set; get; }

	public GameObject serverPrefab;

	// Use this for initialization
	void Start () {
	
		try
		{
			Server s = Instantiate(serverPrefab).GetComponent<Server>();
			s.Init();
		}
		catch (Exception e)
		{
			Debug.Log (e.Message);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
