using UnityEngine;

public class Stage : MonoBehaviour
{
    public enum StageDifficulty
    {
        Tutorial,
        Easy,
        Medium,
        Hard,
        Extreme
    }

    public int numOfSpawners;
    public StageDifficulty difficulty;
    public Transform spawnPoint;

    [TextArea] public string[] gateMessages;
    public bool rewardStage;


    




}
