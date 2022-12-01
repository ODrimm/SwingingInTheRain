using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public int minBaseAttack;
    public int maxBaseAttack;
    public float nbCycles;
    public int numPhase;

    List<string> BaseAttacks = new List<string>();
    List<string> SpecialAttacks = new List<string>();
    



    // Start is called before the first frame update
    void Start()
    {
        BaseAttacks.Add("Base1");
        BaseAttacks.Add("Base2");
        SpecialAttacks.Add("SPECIAL1");
        SpecialAttacks.Add("SPECIAL2");
        SpecialAttacks.Add("SPECIAL3");

        print("---PHASE 1 ---");
        numPhase = 1;
        LaunchPhase();

        print("---PHASE 2 ---");
        numPhase = 2;
        LaunchPhase();

        print("---PHASE 3 ---");
        numPhase = 3;
        LaunchPhase();








    }

    // Update is called once per frame
    void Update()
    {

    }


    public void LaunchBaseAttack()
    {
        print(GetRandomItem(BaseAttacks));
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



    public string GetRandomItem(List<string> listToRandomize)
    {
        int randomNum = Random.Range(0, listToRandomize.Count);
        string RandomItem = listToRandomize[randomNum];
        return RandomItem;
    }


    public void LaunchPhase()
    {
        for (int numCycle = 1; numCycle <= nbCycles; numCycle++)
        {

            print("Cycle " + numCycle);
            int NbBaseAttack = Random.Range(minBaseAttack, maxBaseAttack + 1);
            print(NbBaseAttack + " attaques avant la spéciale");
            for (int BaseAttack = 1; BaseAttack <= NbBaseAttack; BaseAttack++)
            {

                LaunchBaseAttack();
            }
            if (numCycle != nbCycles)
            {
                LaunchSpecialAttack(numPhase);
            }
            else
            {
                LaunchCurrentSPecialAttack();
            }
        }
    }
}





