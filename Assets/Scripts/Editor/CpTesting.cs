#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
[CustomEditor(typeof(TestCheckPoint))]
public class CpTesting : Editor
{
    public GameObject player;

    /// <summary>
    /// used to move player to a certain check point in the game to test levels/puzzles
    /// calls the hudcanvas resume function to allow player to move
    /// sets the player's position equal to the checkpionts position
    /// </summary>
    public override void OnInspectorGUI() 
    {   
        //name checkpoints correctly in heirarchy
        if(GUILayout.Button("Checkpoint one"))
        {
            //UnityEngine.Debug.Log(GameObject.Find("[GUI]").transform.Find("[HUDCanvas]"));
            
            GameObject.Find("[GUI]").transform.Find("[HUDCanvas]").GetComponent<HUDCanvas>().Resume();
            player = GameObject.FindWithTag("Player");
            
            if(GameObject.Find("CheckPoint1") == null)
            {
                UnityEngine.Debug.Log("CheckPoint1 does not exist in Hierarchy");
            }else {
                player.transform.position = GameObject.Find("CheckPoint1").transform.position;
            }
            
            // UnityEngine.Debug.Log(GameObject.Find("CheckPoint1").transform.position);
            // UnityEngine.Debug.Log("this is player's" + player.transform.position);
        }

        if(GUILayout.Button("Checkpoint Two"))
        {
            GameObject.Find("[GUI]").transform.Find("[HUDCanvas]").GetComponent<HUDCanvas>().Resume();
            player = GameObject.FindWithTag("Player");
            
            if(GameObject.Find("CheckPoint2") == null)
            {
                UnityEngine.Debug.Log("CheckPoint2 does not exist in Hierarchy");
            }else {
                player.transform.position = GameObject.Find("CheckPoint2").transform.position;
            }
        }
        
        if(GUILayout.Button("Checkpoint Three"))
        {
            GameObject.Find("[GUI]").transform.Find("[HUDCanvas]").GetComponent<HUDCanvas>().Resume();
            player = GameObject.FindWithTag("Player");
            
            if(GameObject.Find("CheckPoint3") == null)
            {
                UnityEngine.Debug.Log("CheckPoint3 does not exist in Hierarchy");
            }else {
                player.transform.position = GameObject.Find("CheckPoint3").transform.position;
            }
        }

        if(GUILayout.Button("Checkpoint Four"))
        {
            GameObject.Find("[GUI]").transform.Find("[HUDCanvas]").GetComponent<HUDCanvas>().Resume();
            player = GameObject.FindWithTag("Player");
            
            if(GameObject.Find("CheckPoint4") == null)
            {
                UnityEngine.Debug.Log("CheckPoint4 does not exist in Hierarchy");
            }else {
                player.transform.position = GameObject.Find("CheckPoint4").transform.position;
            }
        }
    }
}
#endif
