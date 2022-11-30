using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemyPlaceHolder : MonoBehaviour
{

    Vector2 target1;
    Vector2 target2;

    // Start is called before the first frame update
    void Start()
    {
        target1 = transform.position;
        target2 = new Vector2(transform.position.x - 5, transform.position.y);
    }

    // Update is called once per frame
    void Update()
    {

        transform.position = new Vector2(transform.position.x - 0.5f, transform.position.y);
    }
}
