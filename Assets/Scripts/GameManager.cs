using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    bool isAlerted = false;

    public Enemy[] allEnemies;

    public PlayerControl player;

    public Transform[] allNodePoints;

    // Debug


    private void Awake()
    {
        player.SetGameManager(this);
        GameObject GO_player = player.gameObject;
        for (int i = 0; i < allEnemies.Length; i++)
        {
            allEnemies[i].Init(this, GO_player);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        isAlerted = false;

        for(int i = 0; i < allEnemies.Length; i++)
        {
            // Check enemy state
            if(allEnemies[i].GetState() == Enemy.State.chasing)
            {
                isAlerted = true;
            }
        }
    }

    public bool IsEnemyAlert()
    {
        return isAlerted;
    }
}
