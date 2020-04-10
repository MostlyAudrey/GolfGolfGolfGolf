using System;
using Mirror;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 DEFAULT_POS = new Vector3( 5, 2.5f, 0 );
    public Quaternion DEFAULT_ROT = new Quaternion( 0, 0, 0, 0);
    public float maxCharacterDistanceApart = 15;
    public float cameraHeightClosest = 1f;
    public float cameraHeightFarthest = 3f;
    public float heightLook;

    public float cameraDistanceClosest = 3f;
    public float cameraDistanceFarthest = 6f;

    public AnimationCurve cameraDistanceFunc;

    //private GameWorldController game_controller;
    private Vector3 last_position = Vector3.zero;
    //private Transform testTransform1;


    public float dampTime = 4f;


    public void Awake()
    {
       // game_controller = GameObject.Find("Game World Controller").GetComponent<GameWorldController>();
    }

    /*    public void reset_camera()
        {
            Debug.Log(ClientScene.localPlayer.gameObject.GetComponent<PlayerScript>().BALL.transform.position);
            Debug.Log(DEFAULT_POS);
            Debug.Log(ClientScene.localPlayer.gameObject.GetComponent<PlayerScript>().BALL.transform.position + DEFAULT_POS);
            this.gameObject.transform.position = ClientScene.localPlayer.gameObject.GetComponent<PlayerScript>().BALL.transform.position + DEFAULT_POS;
            //this.gameObject.transform.rotation = DEFAULT_ROT;
            last_position = ClientScene.localPlayer.gameObject.GetComponent<PlayerScript>().BALL.transform.position;
        }*/

    void LateUpdate()
    {
        
        /*if (ClientScene.localPlayer)
        {
            _handle_right_click();
            _handle_ws();
            _handle_ad();
        }*/
    }

    void FixedUpdate()
    {
        if (ClientScene.localPlayer)
        {
            Vector3 curr_pos = ClientScene.localPlayer.gameObject.GetComponent<PlayerScript>().CAMERA_OBJ.transform.position;

            
            this.gameObject.transform.position = curr_pos;
            this.gameObject.transform.rotation = ClientScene.localPlayer.gameObject.GetComponent<PlayerScript>().CAMERA_OBJ.transform.rotation;
            last_position = curr_pos;

            /*
            if ( curr_pos != last_position) {
                this.gameObject.transform.Translate(curr_pos - last_position, Space.World );
                //this.gameObject.transform.rotation = ClientScene.localPlayer.gameObject.GetComponent<PlayerScript>().CAMERA_OBJ.transform.rotation;
                last_position = curr_pos;
            }*/
        }
    }

  /*  void LateUpdate()
    {
        Transform char0 = game_controller.get_goal_transform_for_next_level();
        Transform char1 = ClientScene.localPlayer.gameObject.GetComponent<PlayerScript>().BALL.transform;

        Vector3 direction = char1.position - char0.position;
        Vector3 midPoint = char0.position + (direction / 2);

        Vector3 currPosition = transform.position;

        Vector3 dirLeft = Vector3.Cross(direction, Vector3.up).normalized;
        Vector3 dirRight = Vector3.Cross(-direction, Vector3.up).normalized;

        // These are related to my game where the camera distance is related to the distance the players are apart from one another
        float charsDistanceNorm = Vector3.Distance(char1.position, char0.position) / maxCharacterDistanceApart;
        float cameraDistance = Mathf.Lerp(cameraDistanceClosest, cameraDistanceFarthest, cameraDistanceFunc.Evaluate(charsDistanceNorm));
        float cameraHeight = Mathf.Lerp(cameraHeightClosest, cameraHeightFarthest, cameraDistanceFunc.Evaluate(charsDistanceNorm));
         
        Vector3 desiredPosition;
        if (((midPoint + dirLeft) - transform.position).sqrMagnitude < ((midPoint + dirRight) - transform.position).sqrMagnitude)
        {
           // testTransform1.transform.position = midPoint + dirLeft;
            desiredPosition = midPoint + dirLeft * cameraDistance + Vector3.up * cameraHeight;
        }
        else
        {
          //  testTransform1.transform.position = midPoint + dirRight;
            desiredPosition = midPoint + dirRight * cameraDistance + Vector3.up * cameraHeight;
        }
        // Either just set the newPosition to desiredPosition or Lerp it with a dampTime to smooth it out
        Vector3 newPosition = Vector3.Lerp(currPosition, desiredPosition, dampTime * Time.deltaTime);

        Vector3 cameraCheckDirection = newPosition - (midPoint + cameraHeightClosest * Vector3.up);

        RaycastHit cameraHit;
        if (Physics.Raycast(midPoint + cameraHeightClosest * Vector3.up, cameraCheckDirection, out cameraHit, cameraCheckDirection.magnitude))
        {
            newPosition = cameraHit.point + cameraCheckDirection.normalized * 0.2f;
        }

        transform.position = newPosition;
        transform.LookAt(midPoint + Vector3.up * heightLook);
    }*/


    /*
       
        }*/
}
