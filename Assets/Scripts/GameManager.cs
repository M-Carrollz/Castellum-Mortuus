using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    bool isAlerted = false;

    public GameObject[] GO_allEnemies;
    Enemy[] allEnemies;

    public PlayerControl player;

    // Debug


    private void Awake()
    {
        allEnemies = new Enemy[GO_allEnemies.Length];
        for (int i = 0; i < allEnemies.Length; i++)
        {
            allEnemies[i] = GO_allEnemies[i].GetComponent<Enemy>();
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
