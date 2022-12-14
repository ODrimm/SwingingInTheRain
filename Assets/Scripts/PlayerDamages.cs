using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDamages : MonoBehaviour
{
    public int playerHealth = 8;
    public int smallHitDamage = 1;

    bool canHit = true;
    public float hitCooldown = 0.4f;
    float lastTimeHit;

    public SpriteRenderer sprite;

    CharaMov charaMov;

    // Start is called before the first frame update
    void Start()
    {
        charaMov = gameObject.GetComponent<CharaMov>();
    }

    // Update is called once per frame
    void Update()
    {
       if(playerHealth <= 0)
       {
            SceneManager.LoadScene("Fin Perdu");
       }
       if (Time.time - lastTimeHit > hitCooldown)
       {
            canHit = true;
       }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.tag == "smallHit" && canHit && !charaMov.IsGuard)
        {
            playerHealth -= smallHitDamage;
            canHit = false;
            lastTimeHit = Time.time;
            StartCoroutine(KnockBack());
        }
    }

    IEnumerator KnockBack()
    {
        sprite.enabled = false;
        yield return new WaitForSeconds(0.1f);
        sprite.enabled = true;
        yield return new WaitForSeconds(0.1f);
        sprite.enabled = false;
        yield return new WaitForSeconds(0.1f);
        sprite.enabled = true;
        yield return new WaitForSeconds(0.1f);
        sprite.enabled = false;
        yield return new WaitForSeconds(0.1f);
        sprite.enabled = true;
        yield return new WaitForSeconds(0.1f);
        sprite.enabled = false;
        yield return new WaitForSeconds(0.1f);
        sprite.enabled = true;
        yield return null;
    }
}
