using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class knight : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;

    public GameObject player; // 주인공 캐릭터의 Transform
    public float detectionRange = 10f; // 적이 플레이어를 감지하는 범위
    public float Speed = 5f; // 적의 이동 속도
    public int hp = 100; // 적의 체력
    public bool isLockOn = false; // 플레이거 적을 바라보는 중인지

    private bool findPlayer = false;
    private bool isMove = false;
    private bool isMoveMotionSwitch = false;
    private float moveDuration = 0.1f;
    private bool isGuard = false;
    private bool isGuardMoveLeft = false;
    private bool isAttackMotion = false;
    private bool isHit = false;

    // Start is called before the first frame update
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

        if (player == null)
        {
            Debug.LogError("Player transform is not assigned.");
        }
        
        // Y축 회전을 고정하여 캐릭터가 넘어지지 않게 함
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    // Update is called once per frame
    void Update()
    {
        isAttackMotion = animator.GetBool("isAttackMotion");
        isGuard = animator.GetBool("isGuard");

        if (player != null) {
            findPlayer = true;
        }

        if (findPlayer) {
            MoveToPlayer();
        }

        // 가드시 양옆 이동
        if (isGuardMoveLeft && isGuard) {
            transform.Translate(Vector3.left * (Speed / 40) * Time.deltaTime);
        }

        else if (!isGuardMoveLeft && isGuard) {
            transform.Translate(Vector3.right * (Speed / 40) * Time.deltaTime);
        }
    }

    void MoveToPlayer()
    {
        if (isAttackMotion || isHit) {
            return;
        }

        Transform PlayerTransform = player.transform;
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        // 포착거리이면서 사정거리밖일 경우 뛰어오기
        if (distanceToPlayer < detectionRange && distanceToPlayer > 3f)
        {
            Vector3 direction = (PlayerTransform.position - transform.position).normalized;

            transform.LookAt(PlayerTransform);
            rb.MovePosition(transform.position + direction * Speed * Time.deltaTime);
            MoveAni(true);
        }

        // 사정거리 안에 들어왔을 경우
        else if (distanceToPlayer <= 3f) {
            transform.LookAt(PlayerTransform);
            MoveAni(false);
            ActionChoice();
        }
    }

    void MoveAni(bool setMove) {
        if (setMove) {
            if (isMoveMotionSwitch) {
                return;
            }
            
            animator.SetBool("isMove", true);
            animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration / 4 * Time.deltaTime));
            animator.CrossFade("Move", moveDuration);

            isMoveMotionSwitch = true;

            isMove = true;
        }

        else {
            if (!isMoveMotionSwitch) {
                return;
            }

            animator.SetBool("isMove", false);
            animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration / 4 * Time.deltaTime));
            animator.CrossFade("Idle", moveDuration);

            isMoveMotionSwitch = false;

            isMove = false;
        }
    }
    
    void ActionChoice() {
        if (isAttackMotion || isGuard) {
            return;
        }

        float random = RandomFunction(0, 7);
        switch (random) {
            case 1: {
                Attack1();
                break;
            }

            case 2: {
                Attack2();
                break;
            }

            default: {
                Guard();
                break;
            }
        }
    }

    float RandomFunction(int min, int max)
    {
        return Random.Range(min, max);
    }

    void Guard() {
        animator.SetBool("isGuard", true);
        animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.deltaTime));
        animator.CrossFade("ShieldStop", moveDuration);

        bool random = RandomFunction(0, 2) == 1 ? true : false;
        if (random) {
            isGuardMoveLeft = true;
        }

        else {
            isGuardMoveLeft = false;
        }

        StartCoroutine(ActionWaitCoroutine(2f));
    }

    void Attack1() {
        animator.SetInteger("attack", 1);
        animator.SetBool("isAttackMotion", true);
        animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.deltaTime));
        animator.CrossFade("Attack1", moveDuration);
        StartCoroutine(AttackMoveForwardCoroutine(0.3f, 0.7f));
    }

    void Attack2() {
        animator.SetInteger("attack", 2);
        animator.SetBool("isAttackMotion", true);
        animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.deltaTime));
        animator.CrossFade("Attack2", moveDuration);
        StartCoroutine(AttackMoveForwardCoroutine(0.3f, 0.2f));
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

    IEnumerator ActionWaitCoroutine(float waitDuration)
    {
        yield return new WaitForSeconds(waitDuration);
        animator.SetBool("isGuard", false);
    }

    public void Hit(int damage) {
        if (isHit) {
            return;
        }

        isHit = true;
        StartCoroutine(HitWait(2f));
        
        if (isGuard) {
            Debug.Log("guard success");
            return;
        }

        Debug.Log("attack success");
        
        animator.SetBool("isHit", true);
        animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.deltaTime));
        animator.CrossFade("Hit", 0.01f);

        hp -= damage;
    }

    IEnumerator HitWait(float waitDuration)
    {
        yield return new WaitForSeconds(waitDuration);
        isHit = false;
    }
}