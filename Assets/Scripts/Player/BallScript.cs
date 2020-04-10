using UnityEngine;
using System;
using Mirror;
public class BallScript : NetworkBehaviour
{
    public GameObject ENTITY;
    public float SPEED_UP_SPEED = 10f;
    public float FIRE_FORCE = 15f;
    public float coolDownTimer = 0;

    private void Start()
    {
        this.gameObject.tag = "ball";
    }

    private void Update()
    {
        if (coolDownTimer > 0)
        {
            coolDownTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log("collision: " + collider.gameObject.name + collider.gameObject.tag) ;
        if(collider.gameObject.tag == "goal")
        {
            Debug.Log("reached goal");
            ENTITY.GetComponent<PlayerScript>().reachedGoal();
        }
        else if (collider.gameObject.tag == "PowerUp")
        {
            Debug.Log("hit power up!");
            PowerUp powerUpType = collider.gameObject.GetComponent<PowerUpController>().GetPowerUp();
            collider.gameObject.GetComponent<PowerUpController>().PickUp();
            ENTITY.GetComponent<PlayerScript>().pickedUpPowerUp(powerUpType);
        }
        else if (collider.gameObject.tag == "SpeedUp")
        {
            Debug.Log("speeding up");
            Vector3 ball_direction = collider.gameObject.transform.rotation.eulerAngles / collider.gameObject.transform.rotation.eulerAngles.magnitude;
            GetComponent<Rigidbody>().AddForce( ball_direction * SPEED_UP_SPEED, ForceMode.Impulse);
        }
        else if (collider.gameObject.tag == "Fire")
        {
            if (ENTITY.GetComponent<PlayerScript>().isUsingFireProof())
            {
                return;
            }
            Vector3 originalDirection = GetComponent<Rigidbody>().velocity.normalized;

            float random = UnityEngine.Random.Range(-.15f, .15f);
            

            Vector3 randomDirection = new Vector3(originalDirection.z - random,
                                                  0,
                                                  -originalDirection.x + random);

            GetComponent<Rigidbody>().velocity = randomDirection * FIRE_FORCE;
            Debug.Log(originalDirection + " -> " + randomDirection * FIRE_FORCE);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Death")
        {
            Debug.Log("DEATH!");
            ENTITY.GetComponent<PlayerScript>().enterDeathZone();
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Death")
        {
            Debug.Log("DEATH!");
            ENTITY.GetComponent<PlayerScript>().exitDeathZone();
        }
    }
}
