using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public TMP_Text labCounterText, bestLapTime, currentLabTime;

    public Text counter, go,raceResult;

    public GameObject resultScreen;
    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExitRace()
    {
        RaceManager.instance.RaceExit();
    }
}
