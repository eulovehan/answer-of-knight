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
    private bool isMoveMotionSwitch = false;
    private bool isGuardMoveMotionSwtich = false;
    public bool isAttack = false;
    public bool isAttackDamage = false;
    private bool isGuard = false;
    private bool isGuardStopMotionSwitch = false;
    private int attackAction = 0;
    private float moveDuration = 0.1f;
    private bool isBackMove = false;

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

        // 마우스 오른쪽 버튼이 클릭되면 가드
        if (Input.GetMouseButton(1)) {
            isGuard = true;
        }

        // 마우스 오른쪽 버튼이 떼지면 가드 해제
        else {
            isGuard = false;
        }

        animator.SetBool("isGuard", isGuard);
        GuardStop();
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
        // 공격 중 액션불가
        if (isAttack) {
            return;
        }

        float moveX = Input.GetAxis("Horizontal"); // A, D 키 또는 좌우 화살표 입력
        float moveZ = Input.GetAxis("Vertical");   // W, S 키 또는 상하 화살표 입력

        // 이동안할 시 애니메이션 변경
        if (moveX == 0 && moveZ == 0) {
            MoveAni(false);
            isBackMove = false;

            return;
        }

        float MoveSpeed = isGuard ? Speed / 3.5f : Speed;

        // 앞으로 이동
        if (moveZ > 0) {
            Quaternion playerRotation = Camera.main.transform.rotation;
            Quaternion targetRotation = Quaternion.Euler(0f, playerRotation.eulerAngles.y, 0f);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
        }
        
        // 뒤쪽 키를 누르면 (반복작업되면 안됨)
        if (moveZ < 0 && !isBackMove) {
            Quaternion playerRotation = Camera.main.transform.rotation;

            // 현재 플레이어의 Y축 회전값에 180도를 더한 후 회전
            transform.rotation = Quaternion.Euler(0f, playerRotation.eulerAngles.y + 180f, 0f);
            isBackMove = true;
        }

        else if (!(moveZ < 0)) {
            isBackMove = false;
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

        rb.MovePosition(transform.position + transform.forward * MoveSpeed * Time.deltaTime);
        
        MoveAni(true);
    }

    void MoveAni(bool setMove) {
        if (setMove) {
            if (!isGuard) {
                if (isMoveMotionSwitch) {
                    return;
                }

                isMove = true;
                animator.SetBool("isMove", true);
                animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.deltaTime));
                animator.CrossFade("Move", moveDuration / 2);

                isGuardMoveMotionSwtich = false;
                isMoveMotionSwitch = true;
            }

            else {
                if (isGuardMoveMotionSwtich) {
                    return;
                }

                isMove = true;
                animator.SetBool("isMove", true);
                animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.deltaTime));
                animator.CrossFade("GuardMove", moveDuration / 2);
                
                isGuardMoveMotionSwtich = true;
                isMoveMotionSwitch = false;
            }
        }

        else {
            if (!isGuard) {
                if (!isMoveMotionSwitch) {
                    return;
                }

                isMove = false;
                animator.SetBool("isMove", false);
                animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.deltaTime));
                animator.CrossFade("Idle", moveDuration);

                isGuardMoveMotionSwtich = false;
                isMoveMotionSwitch = false;
            }

            else {
                if (!isGuardMoveMotionSwtich) {
                    return;
                }

                isMove = false;
                if (!Input.GetMouseButton(1)) {
                    animator.SetBool("isMove", false);
                    animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.deltaTime));
                    animator.CrossFade("Idle", moveDuration);
                }

                else {
                    animator.SetBool("isMove", false);
                    animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.deltaTime));
                    animator.CrossFade("GuardStop", moveDuration / 2);

                    isGuardStopMotionSwitch = true;
                }
                
                isGuardMoveMotionSwtich = false;
                isMoveMotionSwitch = false;
            }
        }
    }
    
    void GuardStop() {
        if (isMove || isAttack) {
            return;
        }
        
        if (isGuard) {
            if (isGuardStopMotionSwitch) {
                return;
            }

            animator.SetBool("isMove", false);
            animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.deltaTime));
            animator.CrossFade("GuardStop", moveDuration / 2);

            isGuardStopMotionSwitch = true;
        }

        else {
            if (!isGuardStopMotionSwitch) {
                return;
            }
            
            animator.SetBool("isMove", false);
            animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.deltaTime));
            animator.CrossFade("Idle", moveDuration);

            isGuardStopMotionSwitch = false;
        }

        isBackMove = false;
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
            StartCoroutine(AttackedCoroutine(1f, 0.4f));
        }

        else {
            animator.CrossFade("Attack2", attackDuration);
            attackAction = 0;
            StartCoroutine(AttackMoveForwardCoroutine(0.3f, 0.7f));
            StartCoroutine(AttackedCoroutine(1f, 0.4f));
        }

        // 기존 모션 스위치 모두 취소
        isMoveMotionSwitch = false;
        isGuardMoveMotionSwtich = false;
    }

    IEnumerator AttackMoveForwardCoroutine(float moveDuration, float delayDuration)
    {
        float elapsedTime = 0f;

        // N초 동안 대기
        yield return new WaitForSeconds(delayDuration);

        // N초 동안 이동
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

    IEnumerator AttackedCoroutine(float attackDuration, float delayDuration)
    {   
        isAttackDamage = false;

        // N초 동안 대기
        yield return new WaitForSeconds(delayDuration);
        isAttackDamage = true;

        // N초 동안 공격
        yield return new WaitForSeconds(attackDuration);
        isAttackDamage = false;
    }
}