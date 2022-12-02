using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brella : MonoBehaviour
{
    
    [Space(10)]
    [SerializeField] GameObject wiggleBrella;
    [SerializeField] GameObject WaterLoadBrella;
    [Space(10)]
    public GameObject wiggleSpawn;
    public GameObject WaterLoadSpawn;
    [Space(10)]
    public float waterLoadDuration;
    [Space(10)]
    public float wiggleDuration;
    public float wiggleDistance;
    public float wiggleSpeed;

    Transform WiggleSpawnPos;
    Transform WaterLoadSpawnPos;


    private void Awake()
    {
        
        WiggleSpawnPos = wiggleSpawn.transform;
        WaterLoadSpawnPos = WaterLoadSpawn.transform;
        print("WiggleSpawnPos = " + wiggleBrella.transform);
        print("WaterLoadSpawnPos = " + WaterLoadBrella.transform);
        StartCoroutine(LaunchAttack());
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    GameObject InstanciateBrella(GameObject gameObject, Transform spawnPos)
    {
        return GameObject.Instantiate(gameObject, spawnPos);
    }
 

     IEnumerator LaunchAttack()
    {
        GameObject wiggle = InstanciateBrella(wiggleBrella, WiggleSpawnPos);
        wiggle.transform.parent = gameObject.transform;
        yield return new WaitForSeconds(wiggleDuration);
        GameObject.Destroy(wiggle);
        GameObject waterLoad = InstanciateBrella(WaterLoadBrella, WaterLoadSpawnPos);
        waterLoad.transform.parent = gameObject.transform;

        yield return new WaitForSeconds(waterLoadDuration);
        GameObject.Destroy(waterLoad);

    }
}
