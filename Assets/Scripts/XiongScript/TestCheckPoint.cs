using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TestCheckPoint : MonoBehaviour
{

    [SerializeField]
    public static List<GameObject> listOfCheckPoints;
    
    void Start() 
    {
        listOfCheckPoints = new List<GameObject>();
    }
}
