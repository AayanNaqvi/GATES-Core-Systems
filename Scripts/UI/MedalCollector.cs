using UnityEngine;

public class MedalCollector : MonoBehaviour
{
    public GameObject bronze;
    public GameObject silver;
    public GameObject gold;

    private bool bronzeOn;
    private bool silverOn;
    private bool goldOn;

    private void Update()
    {
        int maxStage = PlayerPrefs.GetInt("MaxRunStage", 1);

        if (!bronzeOn && maxStage >= 10)
        {
            bronzeOn = true;
            bronze.SetActive(true);
        }
        if(!silverOn && maxStage >= 35)
        {
            silverOn = true;
            silver.SetActive(true);
        }
        if(!goldOn && maxStage >= 40)
        {
            goldOn = true;
            gold.SetActive(true);
        }
    }
}
