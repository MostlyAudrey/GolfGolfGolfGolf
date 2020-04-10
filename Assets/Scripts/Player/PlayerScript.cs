using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerScript : NetworkBehaviour
{
    public GameObject CAMERA_OBJ;
    public GameObject ROTATOR;
    public GameObject CLUB_MODEL;
    public GameObject GOLF_CLUB;
    public GameObject BALL;
    public GameObject POINTER;
    public Canvas SCORE_CARD;
    public String PLAYER_NAME = "Host";
    public Color BALL_COLOR;
    public float TIME_TILL_DEATH = 2.5f;
    public float THRUST_MULTIPLIER = 3.0f;
    public float ROTATE_STRENGTH = 15.0f;
    public float MAX_STRENGTH = 40;
    public float STRENGTH_THRESHOLD = 1;
    public float POINTER_LENGTH = 2.5e-03f;
    public float HIT_TIMER = .05f;
    public float BALL_STOPPING_SPEED = .5f;
    public PLAY_STATE play_state = PLAY_STATE.waiting_for_player;
    public PowerUp power_up;
    public int score = -1;
    public int current_level;
    public int[] scores;
    public GameObject managers;

    private Canvas POWER_UP_CANVAS;
    private Text LIST_OF_PLAYERS;
    private Text SCORE_TEXT;
    private Text POWER_UP_TEXT;
    private Vector3 camera_start;
    private float strength = 0.0f;
    private float left_mouse_x = 0.0f;
    private float left_mouse_y = 0.0f;
    private float death_zone_timer = 0.0f;
    public Vector3 last_ball_pos;
    public Vector3 last_valid_position;
    private float timer = 1.0f;
    private float last_strength = 0.0f;
    public bool in_death_zone = false;
    private bool left_mouse_clicked = false;
    private bool left_arrow_clicked = false;
    private bool right_arrow_clicked = false;
    private bool moving_club = false;
    private bool add_impulse = false;
    private float rest_timer = 0f;
    private Rigidbody ball_rb;
    public float actual_time_swinging = 0f;
    private bool right_mouse_clicked = false;
    private bool w_clicked = false;
    private bool a_clicked = false;
    private bool s_clicked = false;
    private bool d_clicked = false;
    private bool r_clicked = false;
    private bool tab_clicked = false;
    private float right_mouse_x = 0.0f;
    private float right_mouse_y = 0.0f;

    private float last_camera_angle_x = 0.0f;
    private float last_camera_angle_y = 0.0f;

    // Sabin Kim: (using_fireproof : bool) : true if player currently using fireproof powerup
    private bool using_fireproof = false;

    public enum PLAY_STATE
    {
        waiting_for_player,
        hitting_ball,
        ball_rolling,
        in_the_hole
    }

    // Start is called before the first frame update
    void Start()
    {
        camera_start = CAMERA_OBJ.transform.localPosition;
        managers = GameObject.Find("Game Play Managers");
        POINTER.transform.localScale = new Vector3(5 * POINTER_LENGTH, 1, POINTER_LENGTH);
        POINTER.transform.localPosition = new Vector3(POINTER.transform.localScale.x * 5, BALL.transform.localScale.y / -2.15f, 0);
        ROTATOR.transform.localRotation = Quaternion.identity;
        if (hasAuthority && managers)
            Cmd_next_level();
    }

    // Update is called once per frame
    void Update()
    {
        if (managers == null)
            return;
        if (ball_rb == false)
        {
            ball_rb = BALL.GetComponent<Rigidbody>();
            ball_rb.maxAngularVelocity = 100000;
        }
        BALL.GetComponent<MeshRenderer>().material.color = BALL_COLOR;
        if (!base.hasAuthority)
            return;
        //Tell all other clients your game state
        Cmd_publish_game_state(this.play_state, this.scores);
        _handle_tab();
        _handle_r();
        _handle_right_click();
        _handle_ws();
        _handle_ad();
        _display_powerup();
        switch (play_state)
        {
            case PLAY_STATE.waiting_for_player:
                if (in_death_zone)
                {
                    _resetOnDeath();
                    break;
                }
                if (using_fireproof && !(power_up is FireProofPowerUp))
                {
                    using_fireproof = false;
                }
                ball_rb.velocity = Vector3.zero;
                _handle_left_click();
                _handle_arrow_keys();
                break;
            case PLAY_STATE.hitting_ball:
                if (timer > 0)
                {
                    GOLF_CLUB.transform.Rotate(0, -1 * last_strength * Time.deltaTime / HIT_TIMER, 0);
                    timer = timer - Time.deltaTime;
                }
                else
                {
                    rest_timer = 0f;
                    last_ball_pos = BALL.transform.position;
                    add_impulse = true;
                    play_state = PLAY_STATE.ball_rolling;
                    moving_club = true;
                }
                break;
            case PLAY_STATE.ball_rolling:
                foreach (GameObject ball in GameObject.FindGameObjectsWithTag("ball"))
                {
                    if (ball != BALL)
                        Physics.IgnoreCollision(BALL.GetComponent<SphereCollider>(), ball.GetComponent<SphereCollider>(), false);
                }
                if (moving_club == false && in_death_zone && death_zone_timer <= 0)
                {
                    _resetOnDeath();
                    break;
                }
                if (in_death_zone)
                    death_zone_timer -= Time.deltaTime;
                Vector3 diff_ball_pos = BALL.transform.position - last_ball_pos;
                Vector3 cam_pos = CAMERA_OBJ.transform.position;
                CAMERA_OBJ.transform.position = cam_pos + diff_ball_pos;
                _handle_space_bar();
                if (timer > -1 * HIT_TIMER)
                {
                    actual_time_swinging += Time.deltaTime;
                    GOLF_CLUB.transform.Rotate(0, -1 * last_strength * Time.deltaTime / HIT_TIMER, 0);
                    timer = timer - Time.deltaTime;
                }
                else if (timer > -4 * HIT_TIMER)
                {
                    timer = timer - Time.deltaTime;
                }
                else if (moving_club)
                {
                    moving_club = false;
                    Cmd_disable_golf_club();
                }
                else if (rest_timer > 2.0f)
                {
                    actual_time_swinging = 0f;
                    ball_rb.velocity = Vector3.zero;
                    Cmd_next_turn();
                }
                else if (ball_rb.velocity.magnitude < BALL_STOPPING_SPEED && ball_rb.velocity.y < 0.1)
                {
                    rest_timer = rest_timer + Time.deltaTime;
                }
                else
                {
                    rest_timer = 0f;
                }
                last_ball_pos = BALL.transform.position;
                break;
            case PLAY_STATE.in_the_hole:
                if (!(power_up is FireProofPowerUp))
                {
                    using_fireproof = false;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        if (!base.hasAuthority)
            return;
        if (add_impulse == true)
        {
            float thrust = last_strength * THRUST_MULTIPLIER;
            ball_rb.AddForce(ROTATOR.transform.right * thrust);
            add_impulse = false;
        }
    }

    private void _next_turn(Transform end, bool set_direction = true)
    {
        ROTATOR.SetActive(true);
        GOLF_CLUB.transform.localRotation = Quaternion.Euler(0, 90, 0);
        last_strength = 0;
        if (set_direction)
        {
            ROTATOR.transform.localPosition = BALL.transform.localPosition;
            ROTATOR.transform.LookAt(end);
            ROTATOR.transform.Rotate(0, -90, 0);
            ROTATOR.transform.rotation = Quaternion.Euler(0, ROTATOR.transform.rotation.eulerAngles.y, 0);
        }
        last_valid_position = BALL.transform.position;
        play_state = PLAY_STATE.waiting_for_player;
    }

    public void next_level(Transform start, Transform end, int level_count)
    {
        ROTATOR.SetActive(true);
        if (play_state == PLAY_STATE.in_the_hole || scores.Length == 0)
        {
            foreach (GameObject ball in GameObject.FindGameObjectsWithTag("ball"))
            {
                if (ball != BALL)
                    Physics.IgnoreCollision(BALL.GetComponent<SphereCollider>(), ball.GetComponent<SphereCollider>(), true);
            }
            if (scores.Length == 0)
            {
                scores = new int[level_count];
                current_level = 0;
            }
            else
            {
                current_level++;
            }
        }
        this.transform.position = start.position + new Vector3(0, .05f, 0);
        this.transform.localRotation = start.rotation;
        ROTATOR.transform.localPosition = Vector3.zero;
        ROTATOR.transform.localRotation = Quaternion.Euler(0, 0, 0);
        BALL.transform.position = start.position + new Vector3(0, .05f, 0);
        CAMERA_OBJ.transform.localPosition = BALL.transform.localPosition + camera_start;
        _next_turn(end, false);
    }

    private void _handle_arrow_keys()
    {
        if (left_arrow_clicked)
        {
            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                left_arrow_clicked = false;
            }
            else
            {
                ROTATOR.transform.Rotate(Vector3.up * ROTATE_STRENGTH * Time.deltaTime);
            }
        }
        else if (right_arrow_clicked)
        {
            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                right_arrow_clicked = false;
            }
            else
            {
                ROTATOR.transform.Rotate(Vector3.down * ROTATE_STRENGTH * Time.deltaTime);
            }
        }
        if (!right_arrow_clicked && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            left_arrow_clicked = true;
        }

        if (!left_arrow_clicked && Input.GetKeyDown(KeyCode.RightArrow))
        {
            right_arrow_clicked = true;
        }
    }
    private void _handle_left_click()
    {
        if (Input.GetMouseButtonDown(0))
        {
            left_mouse_clicked = true;
            left_mouse_x = Input.mousePosition.x;
            left_mouse_y = Input.mousePosition.y;
        }
        else if (Input.GetMouseButtonUp(0) && left_mouse_clicked)
        {
            if (last_strength > STRENGTH_THRESHOLD)
            {
                this.play_state = PLAY_STATE.hitting_ball;
                scores[current_level]++;
                timer = HIT_TIMER;
            }
            left_mouse_clicked = false;
            left_mouse_x = 0;
            left_mouse_y = 0;
        }
        else if (left_mouse_clicked)
        {
            float strength = Math.Min(Math.Abs(Input.mousePosition.y - left_mouse_y) / 15, MAX_STRENGTH);
            POINTER.transform.localScale = new Vector3(strength * POINTER_LENGTH, 1, POINTER_LENGTH);
            POINTER.transform.localPosition = new Vector3(POINTER.transform.localScale.x * 5, BALL.transform.localScale.y / -2.1f, 0);
            GOLF_CLUB.transform.Rotate(0, strength - last_strength, 0);

            last_strength = strength;
        }
    }
    private void _handle_right_click()
    {
        if (Input.GetMouseButtonDown(1))
        {
            right_mouse_clicked = true;
            right_mouse_x = Input.mousePosition.x;
            right_mouse_y = Input.mousePosition.y;
        }
        else if (Input.GetMouseButtonUp(1) && right_mouse_clicked)
        {
            right_mouse_clicked = false;
            right_mouse_x = 0;
            right_mouse_y = 0;
            last_camera_angle_x = 0f;
            last_camera_angle_y = 0f;
        }
        else if (right_mouse_clicked)
        {
            float curr_x_angle = (Input.mousePosition.x - right_mouse_x) / 10;
            float curr_y_angle = (Input.mousePosition.y - right_mouse_y) / -10;
            CAMERA_OBJ.transform.Rotate(curr_y_angle - last_camera_angle_y, curr_x_angle - last_camera_angle_x, 0, Space.Self);
            CAMERA_OBJ.transform.rotation = Quaternion.Euler(CAMERA_OBJ.transform.rotation.eulerAngles.x, CAMERA_OBJ.transform.rotation.eulerAngles.y, 0);

            last_camera_angle_x = curr_x_angle;
            last_camera_angle_y = curr_y_angle;
        }
    }
    private void _handle_r()
    {
        if (r_clicked)
        {
            if (Input.GetKeyUp(KeyCode.R))
            {
                Debug.Log("Resetting!!!");
                r_clicked = false;
                //RESET LEVEL
                this.score++;
                Cmd_next_level();
            }
        } 
        else if (Input.GetKeyDown(KeyCode.R))
        {
            r_clicked = true;
        }
    }
    private void _handle_tab()
    {
        if (SCORE_CARD == null || LIST_OF_PLAYERS == null || SCORE_TEXT == null)
        {
            SCORE_CARD = GameObject.Find("ScoreCard").GetComponent<Canvas>();
            LIST_OF_PLAYERS = GameObject.Find("ListOfPlayers").GetComponent<Text>();
            SCORE_TEXT = GameObject.Find("ScoreText").GetComponent<Text>();
        }
        else
        {
            if (tab_clicked)
            {
                if (Input.GetKeyUp(KeyCode.Tab))
                {
                    tab_clicked = false;
                }
                SCORE_CARD.enabled = true;
                string player_text = "";
                string score_text = "| Level 1 | Level 2 | Level 3 |\n-------------------------------------\n";
                foreach (GameObject client in GameObject.FindGameObjectsWithTag("Client"))
                {
                    PlayerScript player = client.GetComponent<PlayerScript>();
                    player_text += player.PLAYER_NAME + ":\n";
                    score_text += "|";
                    foreach (int score in player.scores)
                    {
                        int spaces = 6 - score / 10;
                        for (int i = 0; i < spaces; i++)
                            score_text += " ";
                        score_text += score.ToString();
                        for (int i = 0; i < spaces; i++)
                            score_text += " ";
                        score_text += "|";
                    }
                    score_text += "\n";
                }
                LIST_OF_PLAYERS.text = player_text;
                SCORE_TEXT.text = score_text;
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                tab_clicked = true;
            }
            else
                SCORE_CARD.enabled = false;
        }
    }
    private void _handle_ws()
    {
        if (w_clicked)
        {
            if (Input.GetKeyUp(KeyCode.W))
            {
                w_clicked = false;
            }
            else
            {
                CAMERA_OBJ.transform.Translate(2 * Vector3.forward * Time.deltaTime);
            }
        }
        else if (s_clicked)
        {
            if (Input.GetKeyUp(KeyCode.S))
            {
                s_clicked = false;
            }
            else
            {
                CAMERA_OBJ.transform.Translate(-2f * Vector3.forward * Time.deltaTime);
            }
        }
        if (!s_clicked && Input.GetKeyDown(KeyCode.W))
        {
            w_clicked = true;
        }

        if (!w_clicked && Input.GetKeyDown(KeyCode.S))
        {
            s_clicked = true;
        }
    }
    private void _handle_ad()
    {
        if (a_clicked)
        {
            if (Input.GetKeyUp(KeyCode.A))
            {
                a_clicked = false;
            }
            else
            {
                CAMERA_OBJ.transform.Translate(-2 * Vector3.right * Time.deltaTime);
            }
        }
        else if (d_clicked)
        {
            if (Input.GetKeyUp(KeyCode.D))
            {
                d_clicked = false;
            }
            else
            {
                CAMERA_OBJ.transform.Translate(2 * Vector3.right * Time.deltaTime);
            }
        }
        if (!d_clicked && Input.GetKeyDown(KeyCode.A))
        {
            a_clicked = true;
        }

        if (!a_clicked && Input.GetKeyDown(KeyCode.D))
        {
            d_clicked = true;
        }
    }
    public void reachedGoal() { play_state = PLAY_STATE.in_the_hole; }
    public void _handle_space_bar()
    {
        if (this.power_up != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Using Powerup: " + power_up.name);
                this.power_up.onUse(BALL);
                if (this.power_up is FireProofPowerUp)
                {
                    using_fireproof = true;
                }
                this.power_up = null;
            }
        }
    }
    public void pickedUpPowerUp(PowerUp power_up) { this.power_up = power_up; }
    private void _resetOnDeath()
    {
        Debug.Log("resetting from death");
        ball_rb.velocity = Vector3.zero;
        CAMERA_OBJ.transform.Translate(last_valid_position - BALL.transform.position);
        BALL.transform.position = last_valid_position;
        in_death_zone = false;
        Cmd_next_turn();
    }

    private void _display_powerup()
    {
        if (POWER_UP_CANVAS == false || POWER_UP_TEXT == false)
        {
            POWER_UP_CANVAS = GameObject.Find("PowerUpCanvas").GetComponent<Canvas>();
            POWER_UP_TEXT = GameObject.Find("PowerUpName").GetComponent<Text>();
        }
        if ( this.power_up != null )
        {
            POWER_UP_CANVAS.enabled = true;
            POWER_UP_TEXT.text = "Current Power Up:\n" + this.power_up.name;
        }
        else
            POWER_UP_CANVAS.enabled = false;
    }
    public void enterDeathZone()
    {
        in_death_zone = true;
        death_zone_timer = TIME_TILL_DEATH;
    }
    public void exitDeathZone() { in_death_zone = false; }

    // Sabin Kim: getter for bool using_fireproof
    public bool isUsingFireProof() { return using_fireproof; }


    /*************************************************
     * Command functions are called by the player with authority
     * They call the ClientRpc method of all other clients
     * This is how all information has to be passed from 1 client to another
     *************************************************/
    [Command]
    public void Cmd_set_name_and_color(string name, Color color) { Rpc_set_name_and_color(name, color); }

    [ClientRpc]
    void Rpc_set_name_and_color(string name, Color color) { PLAYER_NAME = name; BALL_COLOR = color; BALL.GetComponent<MeshRenderer>().material.color = BALL_COLOR; }

    [Command]
    void Cmd_disable_golf_club() { Rpc_disable_golf_club(); }

    [ClientRpc]
    void Rpc_disable_golf_club() { ROTATOR.SetActive(false); }

    [Command]
    void Cmd_next_turn() { Rpc_next_turn(GameObject.Find("Game World Controller").GetComponent<GameWorldController>().get_goal_transform_for_next_level()); }

    [ClientRpc]
    void Rpc_next_turn(Transform end) { this._next_turn(end); }

    [Command]
    public void Cmd_next_level() 
    {
        Rpc_next_level(
            GameObject.Find("Game World Controller").GetComponent<GameWorldController>().get_start_transform_for_next_level(),
            GameObject.Find("Game World Controller").GetComponent<GameWorldController>().get_goal_transform_for_next_level(),
            GameObject.Find("Game World Controller").GetComponent<GameWorldController>().LEVELS.Length
        );
    }

    [ClientRpc]
    public void Rpc_next_level(Transform start, Transform end, int level_count) {
        Debug.Log("Next level");
        this.next_level(start, end, level_count); 
    }

    [Command]
    public void Cmd_publish_game_state( PLAY_STATE play_state, int[] scores ) { Rpc_publish_game_state(play_state, scores); }

    [ClientRpc]
    public void Rpc_publish_game_state(PLAY_STATE play_state, int[] scores)
    {
        if (!base.hasAuthority)
        {
            this.play_state = play_state;
            this.scores = scores;
        }
    }
    
    [Command]
    public void Cmd_randomize_all_other_clients_shape() { Rpc_randomize_all_other_clients_shape(); }

    [ClientRpc]
    void Rpc_randomize_all_other_clients_shape() 
    {
        foreach (GameObject ball in GameObject.FindGameObjectsWithTag("ball"))
        {
            if (ball != BALL)
                Debug.Log("I change you");
        }
    }
}
