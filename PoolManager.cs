using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // 프리펩들을 보관할 변수 필요
    public GameObject[] prefabs;

    // 풀 담당을 하는 리스트들 필요 
    List<GameObject>[] pools;

    void Awake()
    {
        pools = new List<GameObject>[prefabs.Length];

        for (int index = 0; index < pools.Length; index++)
        {
            pools[index] = new List<GameObject>();
        }

     
    }
    
    public GameObject Get(int index)
    {
        GameObject select = null;

        // 선택한 풀의 놀고(비활성) 있는 게임오브젝트 접근, 발견하면 select 변수에 할당

        //foreach 배열 리스트의 데이터를 순차적으로 접근
        foreach(GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                select = item;
                select.SetActive(true);
                break;
            } 
        }

        //못 찾았으면 새롭게 생성하고 select변수에 할당
        if (!select)
        {
            select = Instantiate(prefabs[index], transform);
            pools[index].Add(select);
        }

        return select;
    }

}
