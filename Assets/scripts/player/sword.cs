using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sword : MonoBehaviour
{
    public GameObject player;
    public int damage = 15;
    private List<GameObject> collidedObjects = new List<GameObject>();


    void OnTriggerEnter(Collider collision)
    {
        if (player.GetComponent<player>().isAttackDamage == false) {
            return;
        }

        GameObject collidedObject = collision.gameObject;

        // 충돌 쿨타임에 있는 경우 허용하지 않음
        if (collidedObjects.Contains(collidedObject)) {
            return;
        }

        if (collidedObject.CompareTag("Enemy")) {
            collidedObject.GetComponent<knight>().Hit(damage);
            collidedObjects.Add(collidedObject);
            StartCoroutine(AttackWaitCoroutine(1f, collidedObject));
        }
    }

    IEnumerator AttackWaitCoroutine(float waitDuration, GameObject collidedObject)
    {
        yield return new WaitForSeconds(waitDuration);
        collidedObjects.Remove(collidedObject);
    }
}
