using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopFall : MonoBehaviour
{

    public Transform pos1;
    public Transform pos2;
    public Transform pos3;
    public Transform pos4;
    public float fallspeed = 5;
    [Space(10)]
    [SerializeField] GameObject block;
    [Space(10)]
    public int spawnOffset = 10;
    public int maxOffset = 4;
    Transform target;


    List<Transform> posList = new List<Transform>();
    List<GameObject> fallingBlocks = new List<GameObject>();

    int destroyCount = 0;
    int posEmpty;
    float maxPos;
    Transform spawnPos;

    private void Awake()
    {
        destroyCount = 0;
        posEmpty = Random.Range(0, 4);
        posList.Add(pos1); posList.Add(pos2); posList.Add(pos3); posList.Add(pos4);

        for (int i = 0; i < posList.Count; i++)
        {
            if (posEmpty != i)
            {
                print("oui");
                fallingBlocks.Add(BlockFall(posList[i]));
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i < fallingBlocks.Count;i++)
        {
            fallingBlocks[i].transform.position = Vector3.Lerp(fallingBlocks[i].transform.position, new Vector3(fallingBlocks[i].transform.position.x, fallingBlocks[i].transform.position.y - maxOffset, fallingBlocks[i].transform.position.z),Time.deltaTime);
            maxPos = spawnPos.transform.position.y - maxOffset;
            print(maxPos);
            if (fallingBlocks[i].transform.position.y <= maxPos)
            {
                GameObject.Destroy(fallingBlocks[i]);
                destroyCount++;
            }
        }

        if (destroyCount == 3)
        {
            GameObject.Destroy(gameObject);
        }


    }

    private GameObject BlockFall(Transform pos)
    {
        spawnPos = pos;
        spawnPos.position = new Vector2(spawnPos.position.x, spawnPos.position.y + spawnOffset);
        return GameObject.Instantiate(block, spawnPos);
    }
}
