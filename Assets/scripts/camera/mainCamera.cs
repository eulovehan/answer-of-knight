using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainCamera : MonoBehaviour
{
    public Transform player; // 주인공 캐릭터의 Transform

    public float mouseSensitivity = 5f; // 마우스 감도
    
    public bool isLockOn = false; // 카메라가 적을 바라보는 중인지
    
    float verticalRotation = 0; // 상하 회전 값
    float rotationSpeed = 3f; // 회전 속도

    public Vector3 offset; // 플레이어와의 상대적인 위치
 
    private float lockOnRange = 8f; // 락온 범위
    public GameObject currentTarget; // 현재 락온된 적

    void Start()
    {
        // 마우스 커서를 숨기고 화면 중앙에 고정
        Cursor.lockState = CursorLockMode.Locked;
        
        offset = new Vector3(0, 1.9f, -1.5f); // 카메라 위치 설정
    }

    void LateUpdate()
    {
        if (!isLockOn) {
            CameraRotation();
        }

        else {
            Lockon();
        }

        // q키를 눌러 락온
        if (Input.GetKeyDown(KeyCode.Q)) {
            FindClosestEnemy();
        }
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
            transform.position = Vector3.Lerp(transform.position, pos + player.position, Time.deltaTime * 3f);
        }
    }

    // 락온
    void FindClosestEnemy() {
        // 락온 초기화
        if (currentTarget != null) {
            currentTarget.GetComponent<knight>().isLockOn = false;
        }
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = lockOnRange;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentTarget = enemy;
            }
        }

        if (currentTarget != null)
        {
            isLockOn = !isLockOn;
            currentTarget.GetComponent<knight>().isLockOn = isLockOn;
        }

        else
        {
            currentTarget = null;
            isLockOn = false;
        }
    }

    void Lockon() {
        // 적의 전방 벡터
        Vector3 enemyForward = currentTarget.transform.forward;

        // 적의 전방 벡터를 기준으로 플레이어의 뒤쪽 위치 계산
        Vector3 cameraPosition = player.position + enemyForward * 2f;
        cameraPosition.y = player.position.y + 1.9f;

        // 카메라 위치 및 회전 설정
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, cameraPosition, 10f);
        transform.position = smoothedPosition;

        // 락온된 적을 바라보도록 회전
        transform.LookAt(currentTarget.transform);

        // 10m 이상 떨어지면 락온 해제
        if (Vector3.Distance(transform.position, currentTarget.transform.position) > 15f) {
            isLockOn = false;
            currentTarget.GetComponent<knight>().isLockOn = false;
        }
    }
}