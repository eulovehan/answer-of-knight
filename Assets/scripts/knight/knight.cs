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
    public float attackRange = 1.3f; // 적의 공격 범위
    public float Speed = 5f; // 적의 이동 속도
    public float hp = 100f; // 적의 체력
    public bool isLockOn = false; // 플레이거 적을 바라보는 중인지
    public bool isDeath = false;
    public float attackDamage = 15f;
    public bool isAttackDamage = false;
    public AudioClip[] audioClips; // 여러 개의 오디오 클립 배열

    private bool findPlayer = false;
    private bool isMove = false;
    private bool isMoveMotionSwitch = false;
    private float moveDuration = 0.1f;
    private bool isGuard = false;
    private bool isGuardMoveLeft = false;
    private bool isHit = false;
    private bool isAttackMotion = false;
    private bool isAttackFailed = false;
    private AudioSource audioSource1;
    private AudioSource audioSource2;

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

        audioSource1 = GetComponent<AudioSource>();
        if (audioSource1 == null)
        {
            Debug.LogError("AudioSource component missing from this game object. Please add one.");
        }

        audioSource2 = gameObject.AddComponent<AudioSource>();
        if (audioSource2 == null)
        {
            Debug.LogError("AudioSource component missing from this game object. Please add one.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        isAttackMotion = animator.GetBool("isAttackMotion");
        isGuard = animator.GetBool("isGuard");
        isDeath = animator.GetBool("isDeath");

        if (isDeath || isAttackFailed) {
            return;
        }

        if (player != null) {
            findPlayer = true;
        }

        if (hp <= 0) {
            Death();
        }

        Attack();
    }

    void FixedUpdate() {
        if (isDeath || isAttackFailed) {
            return;
        }
        
        if (findPlayer) {
            MoveToPlayer();
        }

        // 가드시 양옆 이동
        if (isGuardMoveLeft && isGuard) {
            transform.Translate(Vector3.left * (Speed / 10) * Time.fixedDeltaTime);
        }

        else if (!isGuardMoveLeft && isGuard) {
            transform.Translate(Vector3.right * (Speed / 10) * Time.fixedDeltaTime);
        }
    }

    void MoveToPlayer()
    {
        if (isAttackMotion || isHit) {
            return;
        }

        Transform PlayerTransform = player.transform;
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        Vector3 targetPosition = PlayerTransform.position;
        targetPosition.y = transform.position.y; // 현재 위치의 y값으로 설정하여 y값을 무시

        // 포착거리이면서 사정거리밖일 경우 뛰어오기
        if (distanceToPlayer < detectionRange && distanceToPlayer > 3f)
        {
            Vector3 direction = (PlayerTransform.position - transform.position).normalized;

            transform.LookAt(targetPosition);
            rb.MovePosition(transform.position + direction * Speed * Time.fixedDeltaTime);
            MoveAni(true);
        }

        // 사정거리 안에 들어왔을 경우
        else if (distanceToPlayer <= 3f) {
            transform.LookAt(targetPosition);
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
            animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration / 4 * Time.fixedDeltaTime));
            animator.CrossFade("Move", moveDuration);

            isMoveMotionSwitch = true;

            isMove = true;
        }

        else {
            if (!isMoveMotionSwitch) {
                return;
            }

            animator.SetBool("isMove", false);
            animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration / 4 * Time.fixedDeltaTime));
            animator.CrossFade("Idle", moveDuration);

            isMoveMotionSwitch = false;

            isMove = false;
        }
    }
    
    void ActionChoice() {
        if (isAttackMotion || isGuard) {
            return;
        }

        float random = RandomFunction(0, 4);
        switch (random) {
            case 0: {
                Attack1();
                break;
            }

            case 1: {
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
        animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.fixedDeltaTime));
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
        animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.fixedDeltaTime));
        animator.CrossFade("Attack1", moveDuration);
        StartCoroutine(AttackMoveForwardCoroutine(0.3f, 0.7f));
        StartCoroutine(AttackedCoroutine(0.3f, 0.6f));
    }

    void Attack2() {
        animator.SetInteger("attack", 2);
        animator.SetBool("isAttackMotion", true);
        animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.fixedDeltaTime));
        animator.CrossFade("Attack2", moveDuration);
        StartCoroutine(AttackMoveForwardCoroutine(0.2f, 0.4f));
        StartCoroutine(AttackedCoroutine(0.2f, 0.3f));
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
            float distance = Speed * Time.fixedDeltaTime;

            // 앞으로 이동
            transform.Translate((Vector3.forward * distance) / 2);

            // 경과 시간 업데이트
            elapsedTime += Time.fixedDeltaTime;

            // 다음 프레임까지 대기
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator ActionWaitCoroutine(float waitDuration)
    {
        yield return new WaitForSeconds(waitDuration);
        animator.SetBool("isGuard", false);
    }

    public void Hit(float damage) {
        if (isHit) {
            return;
        }

        if (hp <= 0) {
            return;
        }
        
        StartCoroutine(HitWait(0.4f));
        
        // 막음
        if (isGuard) {
            // 가드음
            audioSource2.pitch = 0.7f;
            audioSource2.clip = audioClips[0];
            audioSource2.Play();
            
            return;
        }

        // 피격음
        bool random = RandomFunction(0, 2) == 1 ? true : false;
        if (random) {
            audioSource1.pitch = 0.7f;
            audioSource1.clip = audioClips[1];
            audioSource1.Play();
        }

        else {
            audioSource1.pitch = 0.8f;
            audioSource1.clip = audioClips[2];
            audioSource1.Play();
        }
        
        animator.SetBool("isHit", true);
        animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.fixedDeltaTime));
        animator.CrossFade("Hit", 0.01f);

        hp -= damage;
    }

    public void Death() {
        animator.SetBool("isDeath", true);
        animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.fixedDeltaTime));
        animator.CrossFade("Death", 1f);

        // GameObject[] findText = GameObject.FindGameObjectsWithTag("textUI");
        // foreach (GameObject textUI in findText)
        // {
        //     textUI.GetComponent<text>().TextView("적이 쓰러졌습니다.");
        // }
    }

    public void Attack() {
        if (!isAttackDamage) {
            return;
        }
        
        RaycastHit hit;
        Vector3 forward = transform.TransformDirection(Vector3.forward);

        if (!Physics.Raycast(transform.position, forward, out hit, detectionRange)) {
            return;
        }

        if (hit.collider.CompareTag("Player"))
        {
            AttackResult result = hit.collider.GetComponent<player>().Hit(attackDamage);

            // // 상대방이 가드를 성공하면 딜레이 발생
            // if (result == AttackResult.Guard) {
            //     animator.SetBool("isAttackFailed", true);
            //     animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.fixedDeltaTime));
            //     animator.CrossFade("AttackFailed", 0.01f);
            //     StartCoroutine(AttackFailedWait(0.8f));
            // }
        }
    }

    IEnumerator HitWait(float waitDuration)
    {
        isHit = true;

        yield return new WaitForSeconds(waitDuration);
        isHit = false;
    }

    IEnumerator AttackFailedWait(float waitDuration)
    {
        isAttackFailed = true;

        yield return new WaitForSeconds(waitDuration);
        isAttackFailed = false;
    }

    IEnumerator AttackedCoroutine(float attackDuration, float delayDuration)
    {   
        isAttackDamage = false;

        // N초 동안 대기
        yield return new WaitForSeconds(delayDuration);

        // 맞는중엔 공격안함
        if (!isHit) {
            isAttackDamage = true;
        }

        // N초 동안 공격
        yield return new WaitForSeconds(attackDuration);
        isAttackDamage = false;
    }
}