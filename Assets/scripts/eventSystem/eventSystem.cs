using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class eventSystem : MonoBehaviour
{
    public int keyType = 0; // 키 타입
    public int totalKeys = 0; // 모은 열쇠 개수
    public int keys = 0; // 모은 열쇠 개수
    public int key1 = 0; // 1번 키 개수
    public int key2 = 0; // 2번 키 개수
    
    
    // Start is called before the first frame update
    void Start()
    {
        // 랜덤 true or false
        bool isTrue = RandomFunction(0, 2) == 0 ? true : false;
        
        string targetKeyTag = isTrue ? "key1" : "key2";
        string destroyTag = isTrue ? "key2" : "key1";
        keyType = isTrue ? 1 : 2;

        // 사용안하는 키 제거
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(destroyTag))
        {
            Destroy(obj);
        }

        // 총 개수 반영
        totalKeys = GameObject.FindGameObjectsWithTag(targetKeyTag).Length;
    }

    // Update is called once per frame
    void Update()
    {
        // "key1" 태그를 가진 모든 게임 오브젝트
        GameObject[] key1Objects = GameObject.FindGameObjectsWithTag("key1");
        key1 = key1Objects.Length;
        
        // "key2" 태그를 가진 모든 게임 오브젝트
        GameObject[] key2Objects = GameObject.FindGameObjectsWithTag("key2");
        key2 = key2Objects.Length;

        // 타입에 따라 키 개수 반영
        if (keyType == 1)
        {
            keys = totalKeys - key1;
        }
        else
        {
            keys = totalKeys - key2;
        }
    }

    float RandomFunction(int min, int max)
    {
        return Random.Range(min, max);
    }
}
