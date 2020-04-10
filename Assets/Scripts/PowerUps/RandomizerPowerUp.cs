using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RandomizerPowerUp: PowerUp
{
    public string name { get; set; }

    public float SPEED_POWER = 5f;
    public RandomizerPowerUp()
    {
        this.name = "Randomizer";
    }
    public void onUse(GameObject ball)
    {
        Debug.Log("Randomizing");
        ClientScene.localPlayer.gameObject.GetComponent<PlayerScript>().Cmd_randomize_all_other_clients_shape();
    }

}
