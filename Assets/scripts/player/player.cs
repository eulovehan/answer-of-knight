using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class player : MonoBehaviour
{
    public float Speed = 5f; // 이동 속도
    public float attackPower = 1f; // 공격력 배율
    public bool isGuard = false;
    public float totalHp = 200f; // 전체 체력
    public float hp = 200f; // 체력
    public AudioClip[] audioClips; // 여러 개의 오디오 클립 배열
    public bool isStop = true; // 정지 상태

    private Rigidbody rb;
    private Animator animator;
    private bool isMove = false;
    private bool isMoveMotionSwitch = false;
    private bool isGuardMoveMotionSwtich = false;
    public bool isAttack = false;
    public bool isAttackDamage = false;
    private bool isGuardStopMotionSwitch = false;
    private int attackAction = 0;
    private float moveDuration = 0.1f;
    private bool isBackMove = false;
    private bool isHit = false;
    private AudioSource audioSource1;
    private AudioSource audioSource2;
    private ParticleSystem guardEffect; // 가드 이펙트
    private ParticleSystem hitEffect; // 히트 이펙트
    private GameObject textUI; // 텍스트 UI

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

        // 이펙트 모체
        GameObject[] findEffect = GameObject.FindGameObjectsWithTag("guardParticle");
        foreach (GameObject effect in findEffect)
        {
            guardEffect = effect.GetComponent<ParticleSystem>();
        }

        GameObject[] findHitEffect = GameObject.FindGameObjectsWithTag("hitParticle");
        foreach (GameObject effect in findHitEffect)
        {
            hitEffect = effect.GetComponent<ParticleSystem>();
        }

        // 텍스트 UI
        GameObject[] findUI = GameObject.FindGameObjectsWithTag("textUI");
        foreach (GameObject ui in findUI)
        {
            textUI = ui;
        }
        
        StartCoroutine(Openning());
    }

    void Update() {
        // 마우스 오른쪽 버튼이 클릭되면 가드
        if (Input.GetMouseButton(1)) {
            isGuard = true;
        }

        // 마우스 오른쪽 버튼이 떼지면 가드 해제
        else {
            isGuard = false;
        }

        animator.SetBool("isGuard", isGuard);
        
        // 스탑 가드 상태이고 락온이 된 상태일시 적을 바라보기
        if (isGuard && !isMove) {
            bool isLockOn = Camera.main.GetComponent<mainCamera>().isLockOn;
            if (!isLockOn) {
                return;
            }
            
            GameObject currentTarget = Camera.main.GetComponent<mainCamera>().currentTarget;
            
            Vector3 targetPosition = currentTarget.transform.position;

            // rotate y 30정도 오차
            Vector3 direction = targetPosition - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            Quaternion additionalRotation = Quaternion.Euler(0, 50, 0); // y값에 30도 회전을 추가
            transform.rotation = rotation * additionalRotation;
        }
    }

    void FixedUpdate() {
        Move();
        GuardStop();
    }

    void LateUpdate()
    {
        // 마우스 왼쪽 버튼이 클릭되면 공격
        if (Input.GetMouseButtonDown(0)) {
            Attack();
        }
    }

    void Move()
    {
        // 공격 / 히트 중 액션불가
        if (isAttack || isHit) {
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
            
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 15f);
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 15f);
        }
        
        // 오른쪽 키를 누르면
        if (moveX > 0)
        {
            // 현재 메인 카메라의 오른쪽 방향 벡터를 가져옴
            Vector3 cameraRightDirection = Camera.main.transform.right;

            // 카메라의 오른쪽 방향 벡터를 기준으로 플레이어를 회전
            Quaternion targetRotation = Quaternion.LookRotation(cameraRightDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }

        if (!isStop) {
            rb.MovePosition(transform.position + transform.forward * MoveSpeed * Time.fixedDeltaTime);
        }
        
        MoveAni(true);
    }

    void MoveAni(bool setMove) {
        if (setMove && !isStop) {
            float soundPitch = 1f;
            
            if (!isGuard) {
                if (isMoveMotionSwitch) {
                    return;
                }

                isMove = true;
                animator.SetBool("isMove", true);
                animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.fixedDeltaTime));
                animator.CrossFade("Move", moveDuration / 2);

                soundPitch = 0.9f;

                isGuardMoveMotionSwtich = false;
                isMoveMotionSwitch = true;
            }

            else {
                if (isGuardMoveMotionSwtich) {
                    return;
                }

                isMove = true;
                animator.SetBool("isMove", true);
                animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.fixedDeltaTime));
                animator.CrossFade("GuardMove", moveDuration / 2);

                // 오디오 재생 속도 조절
                soundPitch = soundPitch / 2.2f;
                
                isGuardMoveMotionSwtich = true;
                isMoveMotionSwitch = false;
            }

            // 오디오 재생
            audioSource1.loop = true;
            audioSource1.pitch = soundPitch;
            audioSource1.clip = audioClips[0];
            audioSource1.Play();
        }

        else {
            if (!isGuard) {
                if (!isMoveMotionSwitch) {
                    return;
                }

                isMove = false;
                animator.SetBool("isMove", false);
                animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.fixedDeltaTime));
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
                    animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.fixedDeltaTime));
                    animator.CrossFade("Idle", moveDuration);
                }

                else {
                    animator.SetBool("isMove", false);
                    animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.fixedDeltaTime));
                    animator.CrossFade("GuardStop", moveDuration / 2);

                    isGuardStopMotionSwitch = true;
                }
                
                isGuardMoveMotionSwtich = false;
                isMoveMotionSwitch = false;
            }

            // 이동 중이 아니면 오디오 중지
            if (audioSource1.isPlaying) {
                audioSource1.loop = false;
                audioSource1.Stop();
            }
        }
    }
    
    void GuardStop() {
        if (isMove || isAttack || isHit) {
            return;
        }
        
        if (isGuard) {
            if (isGuardStopMotionSwitch) {
                return;
            }

            animator.SetBool("isMove", false);
            animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.fixedDeltaTime));
            animator.CrossFade("GuardStop", moveDuration / 2);

            isGuardStopMotionSwitch = true;
        }

        else {
            if (!isGuardStopMotionSwitch) {
                return;
            }
            
            animator.SetBool("isMove", false);
            animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.fixedDeltaTime));
            animator.CrossFade("Idle", moveDuration);

            isGuardStopMotionSwitch = false;
        }

        isBackMove = false;
    }

    void Attack() {
        if (isAttack || isHit || isStop) {
            return;
        }
        
        // 기존 소리 제거
        if (audioSource1.isPlaying) {
            audioSource1.loop = false;
            audioSource1.Stop();
        }

        if (audioSource2.isPlaying) {
            audioSource2.loop = false;
            audioSource2.Stop();
        }
        
        float attackDuration = 0.05f;
        
        isMove = false;
        animator.SetBool("isMove", false);
        animator.SetInteger("attackAction", attackAction);
        animator.SetBool("isAttack", true);
        
        animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, attackDuration * Time.fixedDeltaTime));

        if (attackAction == 0) {
            animator.CrossFade("Attack1", attackDuration);
            attackAction = 1;
            StartCoroutine(AttackMoveForwardCoroutine(0.4f, 0.1f));
            StartCoroutine(AttackedCoroutine(0.7f, 0.05f, 1));
            StartCoroutine(AttackedSoundCoroutine(0.1f, 0.6f));
        }

        else if (attackAction == 1) {
            animator.CrossFade("Attack2", attackDuration);
            attackAction = 2;
            StartCoroutine(AttackMoveForwardCoroutine(0.6f, 0.3f, 3f));
            StartCoroutine(AttackedCoroutine(0.4f, 0.1f, 1, false));
            StartCoroutine(AttackedSoundCoroutine(0.1f, 0.58f));
            StartCoroutine(AttackedCoroutine(0.4f, 0.65f, 1));
            StartCoroutine(AttackedSoundCoroutine(0.65f, 0.55f));
        }
        
        else {
            animator.CrossFade("Attack3", attackDuration);
            attackAction = 0;
            StartCoroutine(AttackMoveForwardCoroutine(0.6f, 0.55f));
            StartCoroutine(AttackRotateForwardCoroutine(0.6f, 0.4f, 1.5f, 600f));
            StartCoroutine(AttackedCoroutine(0.5f, 0.55f, 1.5f));
            StartCoroutine(AttackedSoundCoroutine(0.55f, 0.52f));
        }

        // 기존 모션 스위치 모두 취소
        isMoveMotionSwitch = false;
        isGuardMoveMotionSwtich = false;
    }

    IEnumerator AttackMoveForwardCoroutine(float moveDuration, float delayDuration, float slow = 2f)
    {
        float elapsedTime = 0f;

        // N초 동안 대기
        yield return new WaitForSeconds(delayDuration);

        // N초 동안 이동
        while (elapsedTime < moveDuration)
        {
            // 이동할 거리 계산
            float distance = Speed * Time.fixedDeltaTime;

            // 앞으로 이동
            transform.Translate((Vector3.forward * distance) / slow);

            // 경과 시간 업데이트
            elapsedTime += Time.fixedDeltaTime;

            // 다음 프레임까지 대기
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator AttackRotateForwardCoroutine(float moveDuration, float delayDuration, float slow = 2f, float rotate = 0f)
    {
        float elapsedTime = 0f;

        // N초 동안 대기
        yield return new WaitForSeconds(delayDuration);

        // N초 동안 이동
        while (elapsedTime < moveDuration)
        {
            // 이동할 거리 계산
            float distance = Speed * Time.fixedDeltaTime;

            // 회전 주기
            if (rotate != 0f) {
                transform.Rotate(Vector3.up * rotate * Time.fixedDeltaTime);
            }

            // 경과 시간 업데이트
            elapsedTime += Time.fixedDeltaTime;

            // 다음 프레임까지 대기
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator AttackedCoroutine(float attackDuration, float delayDuration, float power, bool isAttackReturn = true)
    {   
        isAttackDamage = false;
        isAttack = true;
        attackPower = power;

        // N초 동안 대기
        yield return new WaitForSeconds(delayDuration);
        
        // 맞는중엔 공격안함
        if (!isHit) {
            isAttackDamage = true;
        }

        // N초 동안 공격
        yield return new WaitForSeconds(attackDuration);
        isAttackDamage = false;
        
        // 리턴받을지에 대한여부 (연속 공격 등 제어)
        if (isAttackReturn) {
            isAttack = false;
        }

        attackPower = 1f;
    }

    IEnumerator AttackedSoundCoroutine(float delayDuration, float audioPitch = 0.5f)
    {  
        // N초 동안 대기
        yield return new WaitForSeconds(delayDuration);
        
        // 맞는중엔 공격안함
        if (!isHit) {
            // 검음 출력
            if (!audioSource1.isPlaying) {
                audioSource1.loop = false;
                audioSource1.pitch = audioPitch;
                audioSource1.clip = audioClips[1];
                audioSource1.Play();
            }

            else {
                audioSource2.loop = false;
                audioSource2.pitch = audioPitch;
                audioSource2.clip = audioClips[1];
                audioSource2.Play();
            }
        }
    }

    public AttackResult Hit(float damege) {
        if (isHit) {
            return AttackResult.Wait;
        }
        
        if (isGuard) {
            StartCoroutine(HitWait(0.4f));

            // 가드음
            audioSource1.loop = false;
            audioSource2.pitch = 0.8f;
            audioSource2.clip = audioClips[2];
            audioSource2.Play();

            // 가드 이펙트
            ParticleSystem instantiatedEffect = Instantiate(guardEffect, transform.position + new Vector3(0, 1.5f, 0), Quaternion.identity);
            instantiatedEffect.Play();
            Destroy(instantiatedEffect.gameObject, 1f); // 적절한 시간 후에 파티클 시스템 삭제

            return AttackResult.Guard;
        }

        // 효과음
        audioSource1.loop = false;
        audioSource1.pitch = 1f;
        audioSource1.clip = audioClips[3];
        audioSource1.Play();

        animator.SetBool("isHit", true);
        animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), 0, moveDuration * Time.fixedDeltaTime));
        animator.CrossFade("Hit", 0);

        // 히트 이펙트
        ParticleSystem instantiatedHitEffect = Instantiate(hitEffect, transform.position + new Vector3(0, 1.5f, 0), Quaternion.identity);
        instantiatedHitEffect.Play();
        Destroy(instantiatedHitEffect.gameObject, 1f); // 적절한 시간 후에 파티클 시스템 삭제

        hp -= damege;

        StartCoroutine(HitWait(0.4f));
        return AttackResult.Success;
    }

    IEnumerator HitWait(float waitDuration)
    {
        isHit = true;

        yield return new WaitForSeconds(waitDuration);
        isHit = false;
    }

    IEnumerator Openning()
    {
        yield return new WaitForSeconds(1f);
        isStop = false;
    }
}
