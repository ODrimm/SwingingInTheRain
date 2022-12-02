using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainScaleFix : MonoBehaviour
{
    Vector3 scale;
    float y;

    // Start is called before the first frame update
    void Start()
    {
        scale = gameObject.transform.localScale;
        y = transform.position.y;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(scale.x / transform.lossyScale.x, scale.y / transform.lossyScale.y, scale.z / transform.lossyScale.z);
    }
}
