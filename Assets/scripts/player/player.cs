using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class player : MonoBehaviour
{
    public float Speed = 5f; // 이동 속도
    private Rigidbody rb;
    private Animator animator;
    private bool isMove = false;
    private bool isAttack = false;
    private int attackAction = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (rb == null)
        {
            Debug.Log("Rigidbody 컴포넌트가 할당되지 않았습니다!");
        }

        if (animator == null)
        {
            Debug.Log("Animator 컴포넌트가 할당되지 않았습니다!");
        }

        // 애니메이터에서 루트 모션 비활성화
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        // Y축 회전을 고정하여 캐릭터가 넘어지지 않게 함
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update() {
        isAttack = animator.GetBool("isAttack");
    }

    void LateUpdate()
    {
        Move();

        // 마우스 왼쪽 버튼이 클릭되면 공격
        if (Input.GetMouseButtonDown(0)) {
            Attack();
        }
    }

    void Move()
    {
        float moveX = Input.GetAxis("Horizontal"); // A, D 키 또는 좌우 화살표 입력
        float moveZ = Input.GetAxis("Vertical");   // W, S 키 또는 상하 화살표 입력

        // 이동안할 시 애니메이션 변경
        if (moveX == 0 && moveZ == 0) {
            MoveAnimate(false);        
            return;
        }

        // 공격시 이동불가
        if (isAttack) {
            Debug.Log("");
            return;
        }
        
        // 앞으로 이동
        if (moveZ > 0) {
            Quaternion playerRotation = Camera.main.transform.rotation;
            Quaternion targetRotation = Quaternion.Euler(0f, playerRotation.eulerAngles.y, 0f);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
        }
        
        // 뒤쪽 키를 누르면 (반복작업되면 안됨)
        if (Input.GetKeyDown(KeyCode.S)) {
            Quaternion playerRotation = Camera.main.transform.rotation;

            // 현재 플레이어의 Y축 회전값에 180도를 더한 후 회전
            transform.rotation = Quaternion.Euler(0f, playerRotation.eulerAngles.y + 180f, 0f);
        }

         // 왼쪽 키를 누르면
        if (moveX < 0)
        {
            // 현재 메인 카메라의 왼쪽 방향 벡터를 가져옴
            Vector3 cameraLeftDirection = -Camera.main.transform.right;

            // 카메라의 왼쪽 방향 벡터를 기준으로 플레이어를 회전
            Quaternion targetRotation = Quaternion.LookRotation(cameraLeftDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
        }
        
        // 오른쪽 키를 누르면
        if (moveX > 0)
        {
            // 현재 메인 카메라의 오른쪽 방향 벡터를 가져옴
            Vector3 cameraRightDirection = Camera.main.transform.right;

            // 카메라의 오른쪽 방향 벡터를 기준으로 플레이어를 회전
            Quaternion targetRotation = Quaternion.LookRotation(cameraRightDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        rb.MovePosition(transform.position + transform.forward * Speed * Time.deltaTime);
        MoveAnimate(true);
    }
    
    void MoveAnimate(bool setMove)
    {
        if (setMove == isMove) {
            return;
        }
        
        if (setMove == true) {
            float moveDuration = 0.05f;
            animator.SetBool("isMove", true);
            animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.deltaTime));
            animator.CrossFade("Move", moveDuration);
        }
        
        else {
            float moveDuration = 0.2f;
            animator.SetBool("isMove", false);
            animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.deltaTime));
            animator.CrossFade("Idle", moveDuration);
        }

        isMove = setMove;
    }

    void Attack() {
        if (isAttack) {
            return;
        }
        
        float attackDuration = 0.05f;
        
        isMove = false;
        animator.SetBool("isMove", false);
        animator.SetInteger("attackAction", attackAction);
        animator.SetBool("isAttack", true);
        
        animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, attackDuration * Time.deltaTime));

        if (attackAction == 0) {
            animator.CrossFade("Attack1", attackDuration);
            attackAction = 1;
            StartCoroutine(AttackMoveForwardCoroutine(0.6f, 0.3f));
        }

        else {
            animator.CrossFade("Attack2", attackDuration);
            attackAction = 0;
            StartCoroutine(AttackMoveForwardCoroutine(0.3f, 0.7f));
        }
    }

    IEnumerator AttackMoveForwardCoroutine(float moveDuration, float delayDuration)
    {
        float elapsedTime = 0f;

        // 0.2초 동안 대기
        yield return new WaitForSeconds(delayDuration);

        // 0.3초 동안 이동
        while (elapsedTime < moveDuration)
        {
            // 이동할 거리 계산
            float distance = Speed * Time.deltaTime;

            // 앞으로 이동
            transform.Translate((Vector3.forward * distance) / 10);

            // 경과 시간 업데이트
            elapsedTime += Time.deltaTime;

            // 다음 프레임까지 대기
            yield return null;
        }
    }
}