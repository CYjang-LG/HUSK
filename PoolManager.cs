using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // ��������� ������ ���� �ʿ�
    public GameObject[] prefabs;

    // Ǯ ����� �ϴ� ����Ʈ�� �ʿ� 
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

        // ������ Ǯ�� ���(��Ȱ��) �ִ� ���ӿ�����Ʈ ����, �߰��ϸ� select ������ �Ҵ�

        //foreach �迭 ����Ʈ�� �����͸� ���������� ����
        foreach(GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                select = item;
                select.SetActive(true);
                break;
            } 
        }

        //�� ã������ ���Ӱ� �����ϰ� select������ �Ҵ�
        if (!select)
        {
            select = Instantiate(prefabs[index], transform);
            pools[index].Add(select);
        }

        return select;
    }

}
