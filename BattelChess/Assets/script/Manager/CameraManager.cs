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

    public Vector3 cameraSetUp;
    
    [Range(0, 1)]
    [SerializeField] private float t;

    private bool cameraFunction = true; // 카메라 보기 수정 
    private bool isCameraFunction = false;
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            SetCamera();
        }
    }

    private void SetCamera()
    {
        isCameraFunction = !isCameraFunction;
        if (isCameraFunction)
        {
            transform.position = cameraSetUp;
            transform.LookAt(lookAt.transform);
        }
        else
        {
            transform.position = cameraPoint[playerTurn ? cameraFunction ? 2 : 0 : cameraFunction ? 3 : 1 ];
            transform.LookAt(lookAt.transform);
        }
    }

    private void UpdateCameraMove()
    {
        transform.position = Vector3.Slerp(cameraPoint[cameraFunction ? 2 : 0], cameraPoint[cameraFunction ? 3 : 1], t);
        transform.LookAt(lookAt.transform);
    }

    public void ChangeCameraFunction()
    {
        cameraFunction = !cameraFunction;
        UpdateCameraMove();
    }

    public void StartCameraMove()
    {
        playerTurn = !playerTurn;
        if (isCameraFunction)
            return;
        Chessboard.Instance.isMove = false;
        StartCoroutine(CameraMove());
    }
    
    private IEnumerator CameraMove()
    {
        float targetT = playerTurn ? 0f : 1f;
        float duration = 1f;

        float elapsedTime = 0f; // 경과 시간
        
        while (elapsedTime < duration)
        {
            
            UpdateCameraMove();
            
            yield return null;

            // 경과 시간 업데이트
            elapsedTime += Time.deltaTime;

            // t 값을 보간하여 부드럽게 이동
            t = Mathf.Lerp(playerTurn ? 1f : 0f, targetT, elapsedTime / duration);
        }

        UpdateCameraMove();
        Chessboard.Instance.isMove = true;
        t = targetT;
    }
}