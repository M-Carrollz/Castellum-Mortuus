using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuController : MonoBehaviour
{
    static int currentSceneIndex = 0;

    public GameObject[] panels;

    // Start is called before the first frame update
    void Start()
    {
        //ChangeToPanelIndex(0);
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void QuitApp()
    {
        Debug.Log("yay");
        Application.Quit();
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// This will Reload the current Scene.
    /// </summary>
    /// <param name="hardReset">If hardReset is true. Then the GameManager will not remeber if the player has hit a checkpoint</param>
    public void ReloadCurrentScene(bool hardReset)
    {
        if(hardReset)
        {
            GameManager.checkPointActivated = false;
        }
        SceneManager.LoadScene(currentSceneIndex);
    }

    public void ChangeToPanelIndex(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == index)
            {
                panels[i].SetActive(true);
            }
            else
            {
                panels[i].SetActive(false);
            }

        }
    }

    public void TurnOffAllPanels()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(false);
        }
    }

    public void ChangeResolutionTo1280()
    {
        Debug.Log(Screen.width);
        //Resolution r = Screen.currentResolution;
        Screen.SetResolution(1280, 720, true);
        Debug.Log(Screen.width);
    }

    public void ChangeResolutionTo1600()
    {
        Debug.Log(Screen.width);
        //Resolution r = Screen.currentResolution;
        Screen.SetResolution(1600, 900, true);
        Debug.Log(Screen.width);
    }
}
