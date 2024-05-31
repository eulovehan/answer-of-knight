using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class introUnit : MonoBehaviour
{
    public float moveSpeed = 5f;  // 플레이어의 이동 속도

    void Update()
    {
        // 입력을 받아 이동 벡터를 설정합니다.
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        // 이동 벡터를 계산합니다.
        Vector3 moveVector = new Vector3(moveX, moveY, 0) * moveSpeed * Time.deltaTime;

        // Transform을 사용하여 오브젝트를 이동시킵니다.
        transform.Translate(moveVector);

        // 엔터입력시 다음 씬으로
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Enter key was pressed!");
            SceneManager.LoadScene("game");
        }
    }
}
