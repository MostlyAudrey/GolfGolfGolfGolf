using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameWorldController : NetworkBehaviour
{
    public int current_level = 0;
    public GameObject[] LEVELS;
    public MainGameController MAIN_GAME_CONTROLLER;
    public CameraController CAMERA;

    public float count_down = 1f;
    private int score_at_last_level = 0;
    private bool start_countdown = false;
    // Start is called before the first frame update
    void Start()
    {
        MAIN_GAME_CONTROLLER = GameObject.Find("Main Game Controller").GetComponent<MainGameController>();
        CAMERA = GameObject.Find("Camera Controller").GetComponent<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {       
        //Check if all players are done with a level
        if (count_down <= 0)
        {
            current_level++;
            foreach (DictionaryEntry entity_entry in MAIN_GAME_CONTROLLER.players_hash)
            {
                GameObject player_object = (GameObject)(entity_entry.Value);
                player_object.GetComponent<PlayerScript>().gameObject.GetComponent<PlayerScript>().Rpc_next_level(get_start_transform_for_next_level(), get_goal_transform_for_next_level(), LEVELS.Length);
            }
            start_countdown = false;
            count_down = 1f;
        }
        else if (start_countdown == false)
        {
            bool all_players_are_done = (MAIN_GAME_CONTROLLER.players_hash.Count != 0);
            foreach (DictionaryEntry entity_entry in MAIN_GAME_CONTROLLER.players_hash)
            {
                GameObject player_object = (GameObject)(entity_entry.Value);
                if (player_object.GetComponent<PlayerScript>().play_state != PlayerScript.PLAY_STATE.in_the_hole)
                {
                    all_players_are_done = false;
                    continue;
                }
            }
            if (all_players_are_done)
                start_countdown = true;
        }
        else
            count_down -= Time.deltaTime;       
    }

    public Transform get_start_transform_for_next_level()
    {
        Transform parent_transform = LEVELS[current_level % LEVELS.Length].transform;

        for (int i = 0; i < parent_transform.childCount; i++)
        {
            if (parent_transform.GetChild(i).gameObject.tag == "Start")
                return parent_transform.GetChild(i);
        }
        return null;
    }

    public Transform get_goal_transform_for_next_level()
    {
        Transform parent_transform = LEVELS[current_level % LEVELS.Length].transform;

        for (int i = 0; i < parent_transform.childCount; i++)
        {
            if (parent_transform.GetChild(i).gameObject.name == "Goal")
                return parent_transform.GetChild(i);
        }
        return null;
    }

}
