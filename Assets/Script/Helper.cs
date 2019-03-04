using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class Helper : MonoBehaviour
{
    // This script finds all the objects in Scene, excluding Prefabs.
    public static List<GameObject> GetAllObjectsInScene()
    {
        List<GameObject> objectsInScene = new List<GameObject>();

        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                continue;

            if (!EditorUtility.IsPersistent(go.transform.root.gameObject))
                continue;

            objectsInScene.Add(go);
        }

        return objectsInScene;
    }

//    private static List<GameObject> GetAllObjectsInScene2()
//    {
//        List<GameObject> objectsInScene = new List();
//        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
//        {
//            if (go.hideFlags != HideFlags.None)
//                continue;
//            if (PrefabUtility.GetPrefabType(go) == PrefabType.Prefab ||
//                PrefabUtility.GetPrefabType(go) == PrefabType.ModelPrefab)
//                continue;
//            objectsInScene.Add(go);
//        }
//
//        return objectsInScene;
//    }
}
