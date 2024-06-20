using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance = null;
    
    public List<Vector3> cameraPoint;
    public GameObject lookAt;
    
    [Range(0, 1)]
    [SerializeField] private float t;

    private bool cameraFunction = true; // 카메라 보기 수정 
    public bool playerTurn = true; // true : 1플레이어, false : 2플레이어

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
        
        UpdateCameraMove();
    }

    private void UpdateCameraMove()
    {
        transform.position = Vector3.Slerp(cameraPoint[cameraFunction ? 0 : 2], cameraPoint[cameraFunction ? 1 : 3], t);
        transform.LookAt(lookAt.transform);
    }

    public void ChangeCameraFunction()
    {
        cameraFunction = !cameraFunction;
        UpdateCameraMove();
    }

    public void StartCameraMove()
    {
        Chessboard.Instance.isMove = false;
        StartCoroutine(CameraMove());
    }
    
    private IEnumerator CameraMove()
    {
        float targetT = playerTurn ? 1f : 0f;
        float duration = 1f;

        float elapsedTime = 0f; // 경과 시간
        
        

        while (elapsedTime < duration)
        {
            
            UpdateCameraMove();
            
            yield return null;

            // 경과 시간 업데이트
            elapsedTime += Time.deltaTime;

            // t 값을 보간하여 부드럽게 이동
            t = Mathf.Lerp(playerTurn ? 0f : 1f, targetT, elapsedTime / duration);
        }

        UpdateCameraMove();
        Chessboard.Instance.isMove = true;
        playerTurn = !playerTurn;
        t = targetT;
    }
}