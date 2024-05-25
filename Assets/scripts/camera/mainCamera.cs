using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainCamera : MonoBehaviour
{
    public Transform player; // 주인공 캐릭터의 Transform

    public float mouseSensitivity = 5f; // 마우스 감도
    
    float verticalRotation = 0; // 상하 회전 값
    float rotationSpeed = 3f; // 회전 속도

    public Vector3 offset; // 플레이어와의 상대적인 위치

    void Start()
    {
        // 마우스 커서를 숨기고 화면 중앙에 고정
        Cursor.lockState = CursorLockMode.Locked;
        
        offset = new Vector3(0, 1.9f, -1.5f); // 카메라 위치 설정
    }

    void Update()
    {
        CameraRotation();
    }

    void CameraRotation() {
        // 마우스 입력 감지
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // x축은 회전 방향에 따라 플레이어를 중심으로 카메라 회전
        transform.RotateAround(player.position, Vector3.up, mouseX * rotationSpeed);

        // 플레이어를 바라보도록 회전
        transform.LookAt(player);

        // 카메라의 x축 회전을 고정
        Vector3 currentRotation = transform.localRotation.eulerAngles;
        
        // 카메라 회전
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -60f, 30f); // 수직 회전 각도 제한

        // y축 회전도를 마우스에 따라 영향 (유니티는 x와 y가 반대)
        currentRotation.x = verticalRotation;
        transform.localRotation = Quaternion.Euler(currentRotation);

        // 카메라를 플레이어 주위로 이동시키기
        Vector3 currentOffset  = transform.position - player.position;
        float clampedX = Mathf.Clamp(currentOffset.x, -1.1f, 1.1f);
        float clampedZ = Mathf.Clamp(currentOffset.z, -1.1f, 1.1f);
        float minX = Mathf.Clamp(currentOffset.x, 0.5f, -0.5f);
        float minZ = Mathf.Clamp(currentOffset.z, 0.5f, -0.5f);
        
        // 카메라가 플레이어를 바라보도록 설정 (플레이어를 중심으로 회전)
        if (currentOffset.x < 0.5 && currentOffset.z == 0.5) {
            transform.position = Vector3.Lerp(transform.position, new Vector3(minX, 1.9f, minZ) + player.position, Time.deltaTime * 10f);
        }
        
        // 카메라 따라가기
        else if (currentOffset.x != clampedX || currentOffset.z != clampedZ) {
            Vector3 pos = new Vector3(clampedX, 1.9f, clampedZ);
            transform.position = Vector3.Lerp(transform.position, pos + player.position, Time.deltaTime * 10f);
        }
    }
}