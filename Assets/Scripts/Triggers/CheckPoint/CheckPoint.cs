using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    Collider trigger;

    public Transform player;

    SphereCollider playerCollider;

    public Transform spawnPosition;

    GameManager gameManager;

    bool isTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        trigger = GetComponent<BoxCollider>();
        playerCollider = player.gameObject.GetComponent<SphereCollider>();
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isTriggered && IsPlayerInside())
        {
            isTriggered = true;
            gameManager.HitCheckPoint(this);
        }
    }

    private bool IsPlayerInside()
    {
        Vector3 difference = player.position - trigger.ClosestPoint(player.position);
        return (difference.magnitude < playerCollider.radius);
    }
}
