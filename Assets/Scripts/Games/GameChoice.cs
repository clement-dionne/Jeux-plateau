using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameChoice : MonoBehaviour
{

    public GameObject[] AllGamePanel;

    public void Start()
    {
        foreach (GameObject GamePanel in AllGamePanel)
        {
            GamePanel.SetActive(false);
        }
    }
}
