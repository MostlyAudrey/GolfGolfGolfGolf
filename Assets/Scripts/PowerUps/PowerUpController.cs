using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpController : MonoBehaviour
{
    // Start is called before the first frame update
    public enum POWERUPS
    {
        JumpPowerUp,
        SpeedUpPowerUp,
        FireProofPowerUp,
        RandomizerPowerup
    }

    public POWERUPS[] powerTypes;
    public float RESET_TIME = 5f;

    private PowerUp[] powerUps { get; set; }
    private float timer = 0f;

    private void Start()
    {
        powerUps = new PowerUp[powerTypes.Length];
        for (int i = 0; i < powerTypes.Length; i++)
        {
            var objectType = Type.GetType(powerTypes[i].ToString());
            powerUps[i] = (PowerUp)Activator.CreateInstance(objectType);
        }
    }

    void Update()
    {
        transform.Rotate(0, 50 * Time.deltaTime, 0);
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                this.gameObject.GetComponent<MeshRenderer>().enabled = true;
                this.gameObject.GetComponent<BoxCollider>().enabled = true;
            }
        }
    }

    public void PickUp()
    {
        Debug.Log("Power up is picked up");
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
        this.gameObject.GetComponent<BoxCollider>().enabled = false;
        timer = RESET_TIME;
    }

    public PowerUp GetPowerUp()
    {
        return powerUps[UnityEngine.Random.Range(0, powerTypes.Length)];
    }
}