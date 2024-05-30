using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class key : MonoBehaviour
{
    private GameObject sideTextUI; // side 텍스트 UI

    void Start()
    {
        // Rigidbody를 가져옵니다.
        Rigidbody rb = GetComponent<Rigidbody>();

        // Mesh Collider를 가져옵니다.
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        // Rigidbody가 없으면 추가합니다.
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Mesh Collider가 없으면 추가합니다.
        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        // Mesh Collider를 Convex로 설정합니다.
        meshCollider.convex = true;

        // Rigidbody의 Kinematic을 비활성화하여 중력이 적용되도록 합니다.
        rb.isKinematic = false;

        // text ui set
        GameObject[] findUI = GameObject.FindGameObjectsWithTag("sideTextUI");
        foreach (GameObject ui in findUI)
        {
            sideTextUI = ui;
        }
    }
    
    // 충돌이 시작될 때 호출되는 메서드
    void OnCollisionEnter(Collision collision)
    {
        // 충돌한 물체의 태그가 "Player"인지 확인
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject); // 충돌한 물체 파괴

            // 열쇠 획득 메시지
            sideTextUI.GetComponent<text>().TextView("성문의 열쇠 X1", 1.5f);
        }
    }
}
