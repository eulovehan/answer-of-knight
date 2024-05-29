using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class keyText : MonoBehaviour
{
    public TextMeshProUGUI keyCountText;  // UI 텍스트 요소

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        keyTextIn();
    }

    void keyTextIn() {
        // "key1" 태그를 가진 모든 게임 오브젝트를 찾습니다.
        GameObject[] systemFind = GameObject.FindGameObjectsWithTag("system");

        if (systemFind.Length == 0) {
            return;
        }
        
        GameObject system = systemFind[0];
        int keys = system.GetComponent<eventSystem>().keys;
        int totalKeys = system.GetComponent<eventSystem>().totalKeys;

        // UI 텍스트 요소에 반영합니다.
        if (keyCountText != null)
        {
            keyCountText.text = "Keys: " + keys + " / " + totalKeys;
        }
        
        return;
    }
}
