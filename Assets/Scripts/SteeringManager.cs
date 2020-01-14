using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringManager : MonoBehaviour
{
    LeaderControl leader;

    //Follower Spawn Attributes
    public GameObject[] unitList;
    public GameObject unitPrefab;
    [Header("Spawn Settings")]
    public int spawnNumber = 10;
    public float spawnRange = 20f;


    // Start is called before the first frame update
    void Start()
    {
        leader = GameObject.FindObjectOfType(typeof(LeaderControl)) as LeaderControl;

        unitList = new GameObject[spawnNumber];
        for (int i = 0; i < spawnNumber; i++)
        {
            Vector3 unitPos = new Vector3(Random.Range(-spawnRange, spawnRange), 0.6f, Random.Range(-spawnRange, spawnRange));
            unitList[i] = Instantiate(unitPrefab, this.transform.position + unitPos, Quaternion.identity) as GameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
