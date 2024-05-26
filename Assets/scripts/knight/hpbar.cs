using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class hpbar : MonoBehaviour
{
    public GameObject parent;
    private int totalHp = 0;
    private float scaleX; // 초기 전체 x값

    void Start() {
        if (parent) {
            totalHp = parent.GetComponent<knight>().hp;
        }

        // 초기 정보를 모수로
        scaleX = transform.localScale.x;
    }

    void Update() {
        transform.LookAt(Camera.main.transform);

        // 0이하로는 안떨어지게
        if (transform.localScale.x <= 0) {
            transform.localScale = new Vector3(0, transform.localScale.y, transform.localScale.z);
            return;
        }

        if (parent) {
            float hp = (float)parent.GetComponent<knight>().hp / totalHp;
            transform.localScale = new Vector3(scaleX * hp, transform.localScale.y, transform.localScale.z);
        }
    }
}