using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuController : MonoBehaviour
{
 

    public GameObject[] panels;

    // Start is called before the first frame update
    void Start()
    {
        ChangeToPanelIndex(0);
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
}
