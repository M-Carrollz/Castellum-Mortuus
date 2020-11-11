using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public bool showNodeConncetions = false;

    public LayerMask nodeMask;

    public LayerMask poiMask;

    public MenuController panelManager;

    bool isAlerted = false;

    Enemy[] allEnemies;

    public PlayerControl player;

    PatrolNode[] allNodePoints;

    public GameObject goalTrigger;

    public UnityEvent winEvent;

    public UnityEvent lossEvent;

    public UnityEvent pauseEvent;

    public enum GameState
    {
        running,
        paused,
        win,
        lose
    }

    public GameState gameState = GameState.running;

    float timeScale = 1;

    // Debug


    private void Awake()
    {
        Time.timeScale = timeScale;

        player.SetGameManager(this);
        GameObject GO_player = player.gameObject;

        allEnemies = FindObjectsOfType<Enemy>();
        
        for (int i = 0; i < allEnemies.Length; i++)
        {
            allEnemies[i].Init(this, GO_player);
        }

        allNodePoints = FindObjectsOfType<PatrolNode>();

        goalTrigger.GetComponent<GoalTrigger>().SetGameManager(this);
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

        // Temp input for pause
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(gameState == GameState.paused)
            {
                UnpauseGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public bool IsEnemyAlert()
    {
        return isAlerted;
    }

    public void PlayerLose()
    {
        Debug.Log("Losing");
        // player lose related things go here.
        PauseGame();
        gameState = GameState.lose;
        lossEvent.Invoke();
    }

    public void PlayerWin()
    {
        Debug.Log("winning");
        // player win related things go here
        PauseGame();
        gameState = GameState.win;
        winEvent.Invoke();
    }

    public void PauseGame()
    {
        // Pause game
        switch(gameState)
        {
            case GameState.running:
                Time.timeScale = 0;
                gameState = GameState.paused;
                // Turn on menu
                pauseEvent.Invoke();
                break;
            default:
                // no other game state should change the state to paused
                break;
        }
    }

    public void UnpauseGame()
    {
        switch (gameState)
        {
            case GameState.paused:
                // Start game running
                Time.timeScale = timeScale;
                gameState = GameState.running;
                // Turn off menu
                panelManager.TurnOffAllPanels();
                break;
            default:
                // no other game state should change the state to running
                break;
        }
    }

    private void OnDrawGizmos()
    {
        if(showNodeConncetions)
        {
            PatrolNode[] allNodePoints = FindObjectsOfType<PatrolNode>();

            foreach(PatrolNode node in allNodePoints)
            {
                node.DebugDrawConnections(Color.green);
            }
        }
    }
}
