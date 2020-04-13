using System;
using Mirror;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    void FixedUpdate()
    {
        if (ClientScene.localPlayer)
        {
            Vector3 curr_pos = ClientScene.localPlayer.gameObject.GetComponent<PlayerScript>().CAMERA_OBJ.transform.position;

            this.gameObject.transform.position = curr_pos;
            this.gameObject.transform.rotation = ClientScene.localPlayer.gameObject.GetComponent<PlayerScript>().CAMERA_OBJ.transform.rotation;
        }
    }
}

