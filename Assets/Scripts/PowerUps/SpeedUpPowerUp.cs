using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUpPowerUp : PowerUp
{
    public string name { get; set; }

    public float SPEED_POWER = 15f;
    public SpeedUpPowerUp()
    {
        this.name = "Speed Up";
    }
    public void onUse(GameObject ball)
    {
        Vector3 ball_direction = ball.GetComponent<Rigidbody>().velocity / ball.GetComponent<Rigidbody>().velocity.magnitude;
        ball.GetComponent<Rigidbody>().AddForce( ball_direction * SPEED_POWER, ForceMode.Impulse);
    }

}
