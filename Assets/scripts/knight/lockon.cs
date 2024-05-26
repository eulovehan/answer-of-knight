using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lockon : MonoBehaviour
{
    public GameObject parent;
    public GameObject lockonUI;

    void Update() {
        if (parent.GetComponent<knight>().isLockOn) {
            lockonUI.transform.LookAt(Camera.main.transform);
            
            // 자신을 보이게
            lockonUI.SetActive(true);
        }

        else {
            // 자신을 안보이게
            lockonUI.SetActive(false);
        }
    }
}
