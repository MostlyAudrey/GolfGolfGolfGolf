using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine;
using Mirror;

public class ConnectionScreenManager : MonoBehaviour
{
    public GameObject MAIN_GAME_CONTROLLER_OBJECT;
    public Text SERVER_INFO_TEXTBOX;
    public Text PLAYERS_CONNECTED_TEXTBOX;

    public Button[] COLOR_BUTTONS;
    public InputField PLAYER_NAME;
    public InputField CONNECTION_INFO;
    public GameObject JOIN_SCREEN_ELEMENTS;
    public GameObject CONNECTING_SCREEN_ELEMENTS;
    public GameObject LOBBY_SCREEN_ELEMENTS;
    public bool update_player_list = true;

    private string ip = "localhost";
    private ushort port = 7777;
    private string connection_address;
    private MainGameController MAIN_GAME_CONTROLLER;
    private Color selected_color;
    public string selected_name = "Host";

    private bool attempting_host_connection = false;
    
    public void Start()
    {
        
        StartCoroutine(CheckIP());
        if ( update_player_list == false)
        {
            JOIN_SCREEN_ELEMENTS.SetActive(true);
            CONNECTING_SCREEN_ELEMENTS.SetActive(false);
            LOBBY_SCREEN_ELEMENTS.SetActive(false);
        }
    }

    public void start_host()
    {
        MAIN_GAME_CONTROLLER_OBJECT = GameObject.Find("Main Game Controller");
        MAIN_GAME_CONTROLLER = MAIN_GAME_CONTROLLER_OBJECT.GetComponent<MainGameController>();
        selected_name = "Host";
        MAIN_GAME_CONTROLLER.set_name_and_color(selected_name, selected_color);
        MAIN_GAME_CONTROLLER.start_host();
        port = MAIN_GAME_CONTROLLER.get_port();
    }

    public void connect_to_host()
    {
        if (PLAYER_NAME.text == "")
            PLAYER_NAME.placeholder.GetComponent<Text>().text = "Name Cannot Be Blank";
        else
        {
            MAIN_GAME_CONTROLLER_OBJECT = GameObject.Find("Main Game Controller");
            MAIN_GAME_CONTROLLER = MAIN_GAME_CONTROLLER_OBJECT.GetComponent<MainGameController>();
            MAIN_GAME_CONTROLLER.set_name_and_color(selected_name, selected_color);
            connection_address = CONNECTION_INFO.text;
            if (connection_address == "")
                connection_address = ip;
            attempting_host_connection = true;
            JOIN_SCREEN_ELEMENTS.SetActive(false);
            MAIN_GAME_CONTROLLER.start_client(connection_address);
            CONNECTING_SCREEN_ELEMENTS.SetActive(true);
            SERVER_INFO_TEXTBOX.text = "Connecting to Host:\n" + connection_address;
        }
    }

    public void cancel_connect_to_host()
    {
        MAIN_GAME_CONTROLLER.stop_client();
        CONNECTING_SCREEN_ELEMENTS.SetActive(false);
        JOIN_SCREEN_ELEMENTS.SetActive(true);
    }

    public void stop_host() { MAIN_GAME_CONTROLLER.stop_host(); }

    IEnumerator CheckIP()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://checkip.dyndns.org");
        yield return www.SendWebRequest();
        if (www.isNetworkError)
        {
            Debug.Log(": Error: " + www.error);
        }
        else
        {
            string text = www.downloadHandler.text;
            text = text.Substring(text.IndexOf(":") + 1);
            text = text.Substring(0, text.IndexOf("<"));
            ip = text;
            if (update_player_list)
                SERVER_INFO_TEXTBOX.text = "Your IP Address:\n" + ip;
        }
    }

    void Update()
    {
        if ( update_player_list)
        {
            MAIN_GAME_CONTROLLER.refresh_player_list();
            string players_list = "";
            foreach (DictionaryEntry entity_entry in MAIN_GAME_CONTROLLER.players_hash)
            {
                GameObject networked_entity = (GameObject)(entity_entry.Value);
                string name = networked_entity.GetComponent<PlayerScript>().PLAYER_NAME;
                if (networked_entity == MAIN_GAME_CONTROLLER.local_player)
                    name = name + " (You)";
                players_list = players_list + name + "\n";
            };
            PLAYERS_CONNECTED_TEXTBOX.text = players_list;
        }
        else
        {
            if (attempting_host_connection && !NetworkClient.active)
            {
                attempting_host_connection = false;
                SERVER_INFO_TEXTBOX.text = "Error Connecting to Host:\n" + connection_address;
            }
            if (NetworkClient.isConnected && attempting_host_connection)
            {
                attempting_host_connection = false;
                CONNECTING_SCREEN_ELEMENTS.SetActive(false);
                LOBBY_SCREEN_ELEMENTS.SetActive(true);
                update_player_list = true;
            }
        }
    }

    public void start_game()
    {
        MAIN_GAME_CONTROLLER.ServerChangeScene("World 1");
    }

    public void save_preferences()
    {
        if (PLAYER_NAME.text == "")
            PLAYER_NAME.placeholder.GetComponent<Text>().text = "Name Cannot Be Blank";
        else
        {
            selected_name = PLAYER_NAME.text;
            MAIN_GAME_CONTROLLER.set_name_and_color(PLAYER_NAME.text, selected_color);
        }
            
    }

    public void save_green() { _save_color(COLOR_BUTTONS[0]); }
    public void save_cyan() { _save_color(COLOR_BUTTONS[1]); }
    public void save_blue() { _save_color(COLOR_BUTTONS[2]); }
    public void save_purple() { _save_color(COLOR_BUTTONS[3]); }
    public void save_magenta() { _save_color(COLOR_BUTTONS[4]); }
    public void save_red() { _save_color(COLOR_BUTTONS[5]); }
    public void save_orange() { _save_color(COLOR_BUTTONS[6]); }

    private void _save_color(Button button) { selected_color = button.colors.normalColor; }

}