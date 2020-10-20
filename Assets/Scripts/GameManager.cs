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
