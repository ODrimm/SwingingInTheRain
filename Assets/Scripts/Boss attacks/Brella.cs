using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brella : MonoBehaviour
{
    [Space(10)]
    [SerializeField] GameObject block;
    [Space(10)]
    Transform spawnPos;


    private void Awake()
    {
        StartCoroutine(Wiggle());
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    /*private GameObject BlockFall(Transform pos)
    {
        spawnPos = pos;
        spawnPos.position = new Vector2(spawnPos.position.x, spawnPos.position.y + spawnOffset);
        return GameObject.Instantiate(block, spawnPos);
    }*/

     IEnumerator Wiggle()
    {

        yield return null;
    }
}
