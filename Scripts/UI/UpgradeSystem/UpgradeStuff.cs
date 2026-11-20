using UnityEngine;
using TMPro;

public class UpgradeStuff : MonoBehaviour
{
    public float[] swordDamageUpgrades;
    public int[] swordUpgradeCosts;
    public float[] staffUpgrades;
    public int[] staffUpgradeCosts;
    public float[] maxHealthUpgrades;
    public int[] healthUpgradeCosts;

    public GameObject sword;
    public GameObject staff;

    private NuggetCollector nc;
    private SwordCombat sc;
    private StaffCombat stfc;
    private PlayerHealth ph;

    public TextMeshProUGUI text1Sword;
    public TextMeshProUGUI text2Staff;
    public TextMeshProUGUI text3Health;

    public bool menuOpen = false;


    public TextMeshProUGUI nuggetCount;

    public int swordStage = 0;
    public int staffStage = 0;
    public int healthStage = 0;

    public bool amMax = false;

    [Header("Audio")]
    public AudioClip upgradeSound;
    public float soundVol;

    void Start()
    {
        sc = sword.GetComponent<SwordCombat>();
        stfc = staff.GetComponent<StaffCombat>();
        nc = GetComponent<NuggetCollector>();
        ph = GetComponent<PlayerHealth>();
        Setup();
        UpdateText();
    }
    

    void Setup()
    {
        sc.UpgradeBaseStats(swordDamageUpgrades[swordStage]);
        stfc.fireRate = staffUpgrades[staffStage];
        ph.UpgradeMaxHealth(maxHealthUpgrades[healthStage]);


    }



    public bool UpgradeSword()
    {
        if (swordStage == 9) return false;

        if (nc.nuggetCount < swordUpgradeCosts[swordStage]) return false;


        nc.nuggetCount -= swordUpgradeCosts[swordStage];
        nc.UpdateText();

        swordStage++;
        sc.UpgradeBaseStats(swordDamageUpgrades[swordStage]);
        AudioManager.Instance.Play2D(upgradeSound, soundVol);
        UpdateText();

        return true;

    }

    public bool UpgradeHealth()
    {
        if (healthStage == 9) return false;

        if (nc.nuggetCount < healthUpgradeCosts[healthStage]) return false;


        nc.nuggetCount -= healthUpgradeCosts[healthStage];
        nc.UpdateText();

        healthStage++;


        ph.UpgradeMaxHealth(maxHealthUpgrades[healthStage]);
        AudioManager.Instance.Play2D(upgradeSound, soundVol);

        UpdateText();

        return true;
    }



    public bool CanAffordSword()
    {
        if (swordStage >= 9) return false;
        return nc.nuggetCount >= swordUpgradeCosts[swordStage];
    }

    public bool CanAffordStaff()
    {
        if (staffStage >= 9) return false;
        return nc.nuggetCount >= staffUpgradeCosts[staffStage];
    }

    public bool CanAffordHealth()
    {
        if (healthStage >= 9) return false;
        return nc.nuggetCount >= healthUpgradeCosts[healthStage];
    }

    public bool UpgradeStaff()
    {
        if (staffStage == 9) return false;

        if (nc.nuggetCount < staffUpgradeCosts[staffStage]) return false;



        nc.nuggetCount -= staffUpgradeCosts[staffStage];
        nc.UpdateText();

        staffStage++;
        stfc.fireRate = staffUpgrades[staffStage];
        AudioManager.Instance.Play2D(upgradeSound, soundVol);

        UpdateText();

        return true;

    }

    public void UpdateText()
    {
        if (swordStage != 9) text1Sword.text = swordUpgradeCosts[swordStage].ToString();
        else text1Sword.text = "MAX";
        if (staffStage != 9) text2Staff.text = staffUpgradeCosts[staffStage].ToString();
        else text2Staff.text = "MAX";
        if (healthStage != 9) text3Health.text = healthUpgradeCosts[healthStage].ToString();
        else text3Health.text = "MAX";


        amMax = AmMaxed();
        nuggetCount.text = nc.nuggetCount.ToString();
    }


    public void ClearBuffs()
    {
        sc.UpgradeBaseStats(swordDamageUpgrades[0]);

        stfc.fireRate = staffUpgrades[0];
        ph.UpgradeMaxHealth(maxHealthUpgrades[0]);

        text1Sword.text = swordUpgradeCosts[0].ToString();
        text2Staff.text = staffUpgradeCosts[0].ToString();
        text3Health.text = healthUpgradeCosts[0].ToString();

        swordStage = 0;
        staffStage = 0;
        healthStage = 0;
        amMax = false;

        UpdateText();

    }

    public bool AmMaxed()
    {
        return swordStage == 9 && staffStage == 9 && healthStage == 9;
    }

    public bool CanAffordAnything()
    {
        return CanAffordHealth() || CanAffordStaff() || CanAffordSword(); 
    }
}
