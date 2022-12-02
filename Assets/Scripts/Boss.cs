using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boss : MonoBehaviour
{
    public int minBaseAttack;
    public int maxBaseAttack;
    public float nbCycles;
    public int numPhase;

    public int bossHealth = 30;
    
    public List<GameObject> BaseAttacks = new List<GameObject>();
    List<string> SpecialAttacks = new List<string>();

    GameObject currentAttack;
    



    // Start is called before the first frame update
    void Start()
    {
        SpecialAttacks.Add("SPECIAL1");
        SpecialAttacks.Add("SPECIAL2");
        SpecialAttacks.Add("SPECIAL3");


        print("---PHASE 1 ---");
        numPhase = 1;
        StartCoroutine(LaunchPhase());
    }

    // Update is called once per frame
    void Update()
    {
        if(bossHealth <= 0)
        {
            SceneManager.LoadScene("Fin Gagné");

        }

    }


    public void LaunchBaseAttack()
    {

        currentAttack = GameObject.Instantiate(GetRandomItem(BaseAttacks));
    }

    public void LaunchCurrentSPecialAttack()
    {
        print(SpecialAttacks[numPhase-1]);
    }


    public void LaunchSpecialAttack(int numPhase)
    {
        
        if (numPhase == 1)
        {
            print(SpecialAttacks[0]);
        } 
        else
        {
            int x = Random.Range(0, 2);
            if (x == 1)
            {
                print(SpecialAttacks[numPhase - 1]);
            }
            else
            {
                string SpecialAttack = SpecialAttacks[numPhase - 1];
                while (SpecialAttack == SpecialAttacks[numPhase - 1])
                {
                    SpecialAttack = SpecialAttacks[Random.Range(0,numPhase)];
                }
                print(SpecialAttack);
            }


        }


    }



    public GameObject GetRandomItem(List<GameObject> listToRandomize)
    {
        int randomNum = Random.Range(0, listToRandomize.Count);
        GameObject RandomItem = listToRandomize[randomNum];
        return RandomItem;
    }


     IEnumerator LaunchPhase()
    {
        for (int numCycle = 1; numCycle <= nbCycles; numCycle++)
        {
            yield return new WaitForSeconds(3);
            print("Cycle " + numCycle);
            int NbBaseAttack = Random.Range(minBaseAttack, maxBaseAttack + 1);
            print(NbBaseAttack + " attaques avant la spéciale");
            for (int BaseAttack = 1; BaseAttack <= NbBaseAttack; BaseAttack++)
            {
                LaunchBaseAttack();
                yield return new WaitForSeconds(3);
            }
            if (numCycle != nbCycles)
            {
                LaunchSpecialAttack(numPhase);
                yield return new WaitForSeconds(3);
            }
            else
            {
                LaunchCurrentSPecialAttack();
                yield return new WaitForSeconds(3);
            }
        }
        StartCoroutine(LaunchPhase());
        yield return null;
    }


    public void Damage(int damages)
    {
        //prend des dgts
        bossHealth -= damages;
        print(bossHealth);
    }
}





