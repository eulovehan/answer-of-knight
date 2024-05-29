using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerHpbar : MonoBehaviour
{
    public GameObject parent;
    public Image healthBarImage;
    private float totalHp = 200f;

    void Start() {
        if (parent) {
            totalHp = parent.GetComponent<player>().hp;
        }

        // 초기 정보를 모수로
        // scaleX = totalHp;
    }

    void Update() {
        // fill amount 체력바를 비율로 조절
        healthBarImage.fillAmount = parent.GetComponent<player>().hp / totalHp;

        // 0이하로 안떨어지게 조절
        if (healthBarImage.fillAmount <= 0) {
            healthBarImage.fillAmount = 0;
        }
    }
}
