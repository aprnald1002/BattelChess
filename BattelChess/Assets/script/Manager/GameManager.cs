using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    public TextMeshProUGUI resultText;
    public Canvas winnerCanvas;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CameraManager.Instance.ChangeCameraFunction();
        }
    }

    public void GameEnd(string winTeam)
    {
        winnerCanvas.enabled = true;
        resultText.text = winTeam + "Team Win";
    }
}
