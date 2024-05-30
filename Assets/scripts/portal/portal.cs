using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portal : MonoBehaviour
{
    public GameObject destination; // 이동할 포탈
    private bool isPlayerIn = false; // 플레이어가 포탈 안에 있는지
    private GameObject player; // 충돌한 플레이어
    private GameObject sideTextUI; // 텍스트 UI

    void Start() {
        GameObject[] findUI = GameObject.FindGameObjectsWithTag("sideTextUI");
        foreach (GameObject ui in findUI)
        {
            sideTextUI = ui;
        }
    }
    
    void Update()
    {
        // 플레이어가 포탈1 근처에 있을 때 'E' 키를 누르면 포탈2로 이동
        if (isPlayerIn && Input.GetKeyDown(KeyCode.E))
        {
            // player.GetComponent<knight>().Hit(damage);
            Transform playerTransform = player.transform;
            playerTransform.position = destination.transform.position;
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        GameObject collidedObject = collision.gameObject;

        // e키를 안누르면 이동안함
        if (collidedObject.CompareTag("Player")) {
            isPlayerIn = true;
            player = collidedObject;

            // sideTextUI 텍스트 페이드
            sideTextUI.GetComponent<text>().TextViewFix("[E]키를 눌러 포탈이동");
        }
    }

    void OnTriggerExit(Collider collision)
    {
        GameObject collidedObject = collision.gameObject;

        if (collidedObject.CompareTag("Player")) {
            isPlayerIn = false;
            player = null;

            // sideTextUI 텍스트 페이드 아웃
            sideTextUI.GetComponent<text>().TextViewFixEnd();
        }
    }
}