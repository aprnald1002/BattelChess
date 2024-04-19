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
    public float t;

    public bool cameraFun; // 카메라 보기 수정 
    public bool playerSeq; // true : 1플레이어, false : 2플레이어

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
        
        t = 0;

        playerSeq = false;
        cameraFun = true;
    }

    private void Update()
    {
        
        transform.LookAt(lookAt.transform);
        if (cameraFun)
            transform.position = Vector3.Slerp(cameraPoint[0], cameraPoint[1], t);
        else
            transform.position = Vector3.Slerp(cameraPoint[2], cameraPoint[3], t);
            

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartCoroutine(CameraMove());
        }
        
    }
    
    private IEnumerator CameraMove()
    {
        float targetT = playerSeq ? 1f : 0f;
        float duration = cameraFun ? 3f : 1f; // 이동 시간 설정

        float elapsedTime = 0f; // 경과 시간

        while (elapsedTime < duration)
        {

            yield return null;

            // 경과 시간 업데이트
            elapsedTime += Time.deltaTime;

            // t 값을 보간하여 부드럽게 이동
            t = Mathf.Lerp(playerSeq ? 0f : 1f, targetT, elapsedTime / duration);
        }
        playerSeq = !playerSeq;
        t = targetT;
    }
}