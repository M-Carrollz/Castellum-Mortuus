using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    GameManager gameManager;
    Transform playerTransform;
    SphereCollider playerCollider;
    Collider goalCollider;

    // Start is called before the first frame update
    void Start()
    {
        goalCollider = GetComponent<BoxCollider>();
        playerTransform = gameManager.player.transform;
        playerCollider = gameManager.player.gameObject.GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(gameManager.gameState == GameManager.GameState.running && IsPlayerInside())
        {
            gameManager.PlayerWin();
        }
    }

    private bool IsPlayerInside()
    {
        Vector3 difference = playerTransform.position - goalCollider.ClosestPoint(playerTransform.position);
        return (difference.magnitude < playerCollider.radius);
    }

    public void SetGameManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }
}
