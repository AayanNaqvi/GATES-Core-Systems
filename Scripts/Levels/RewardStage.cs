using UnityEngine;

public class RewardStage : MonoBehaviour
{
    public GameObject[] gates;
    public GameObject[] beginnerStages;
    public bool beginnerStage = true;

    public GameObject chest;
    public GameObject upgradeTotem;

    private GameObject player;



    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        SetUpGates();
        
    }

    

    void SetUpGates()
    {
        int level = GameManager.currentStage;

        if (level > beginnerStages.Length)
        {
            beginnerStage = false;
        }

        if (beginnerStage)
        {
            for (int i = 0; i < gates.Length; i++)
            {
                Gate gat = gates[i].GetComponent<Gate>();

                int arrayIndex = level - 1;
                if (arrayIndex >= 0 && arrayIndex < beginnerStages.Length)
                {
                    gat.stageToGoTo = beginnerStages[arrayIndex];
                }
                else
                {
                    Debug.LogError($"Tried to load Beginner Stage {arrayIndex}, but it's not in the array!");
                }
            }
        }
        else
        {
            int diff = GameManager.Instance.currentDiff;

            if (diff == 0) diff = 1;

            foreach (GameObject gate in gates)
            {
                Gate gat = gate.GetComponent<Gate>();

                if (diff == 1 && GameManager.Instance.easyStages.Length > 0)
                {
                    int index = Random.Range(0, GameManager.Instance.easyStages.Length);
                    gat.stageToGoTo = GameManager.Instance.easyStages[index];
                }
                else if (diff == 2 && GameManager.Instance.mediumStages.Length > 0)
                {
                    int index = Random.Range(0, GameManager.Instance.mediumStages.Length);
                    gat.stageToGoTo = GameManager.Instance.mediumStages[index];
                }
                else if (diff == 3 && GameManager.Instance.hardStages.Length > 0)
                {
                    int index = Random.Range(0, GameManager.Instance.hardStages.Length);
                    gat.stageToGoTo = GameManager.Instance.hardStages[index];
                }
                else if(diff == 4 && GameManager.Instance.extremeStages.Length > 0)
                {
                    int index = Random.Range(0, GameManager.Instance.extremeStages.Length);
                    gat.stageToGoTo = GameManager.Instance.extremeStages[index];
                }
            }

            int rand = UnityEngine.Random.Range(0, 2);
            UpgradeStuff upgrader = player.GetComponent<UpgradeStuff>();
            if (rand == 0 || upgrader.amMax || !upgrader.CanAffordAnything() || player.GetComponent<PlayerHealth>().currHealth <= 20) Instantiate(chest, Vector3.zero, Quaternion.identity);
            else
            {  
                Instantiate(upgradeTotem, Vector3.zero, Quaternion.identity);
            }
        }
    }
}
