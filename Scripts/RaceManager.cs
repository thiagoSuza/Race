using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceManager : MonoBehaviour
{
    public static RaceManager instance;

    public CheclPoint[] allCheckpoints;

    public int totalLabs;

    public CarController playerCar;
    public List<CarController> allAIcars = new List<CarController> ();
    public int playerPosition;

    public float timeBetweenPosCheck = .2f;
    private float posCkCounter;

    public Text aiCarsP, playerP;


    public float aiDefaultSpeed = 30, playerDefaultSpeed = 30f, rubberBandSpeedMod = 3.5f,rubberAccel = .5f;

    public bool isStarting;
    public float timeBetweenStartCount = 1f;
    private float startCounter;
    public int countdownCurrent = 3;

    public int playerStartPosition, aiNumberToSpawn;
    public Transform[] startPoints;
    public List<CarController> carsToSpwn = new List<CarController>();

    public bool raceComplete;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < allCheckpoints.Length; i++)
        {
            allCheckpoints[i].cpNumber = i;
        }

        aiCarsP.text = (allAIcars.Count + 1).ToString();

        isStarting = true;
        startCounter = timeBetweenStartCount;

        UIManager.Instance.counter.text = countdownCurrent.ToString() + "!";

        playerStartPosition = Random.Range(0, aiNumberToSpawn+ 1);

        playerCar.transform.position = startPoints[playerStartPosition].position;
        playerCar.theRB.transform.position = startPoints[playerStartPosition].position;

        for(int i = 0;i< aiNumberToSpawn + 1; i++)
        {
            if( i != playerStartPosition)
            {
                int selectedCar = Random.Range(0,carsToSpwn.Count);
              allAIcars.Add(  Instantiate(carsToSpwn[selectedCar], startPoints[i].position, startPoints[i].rotation));
                if(carsToSpwn.Count > aiNumberToSpawn - i)
                {
                    carsToSpwn.RemoveAt(selectedCar);
                }
                
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (isStarting)
        {
            startCounter -= Time.deltaTime;

            if(startCounter <= 0)
            {
                countdownCurrent--;
                startCounter = timeBetweenStartCount;
                UIManager.Instance.counter.text = countdownCurrent.ToString() + "!";
                if (countdownCurrent == 0)
                {
                    isStarting = false;
                    UIManager.Instance.counter.gameObject.SetActive(false);
                    UIManager.Instance.go.gameObject.SetActive(true);
                }
            }
        }
        else
        {



            posCkCounter -= Time.deltaTime;
            if (posCkCounter <= 0)
            {



                playerPosition = 1;
                playerP.text = playerPosition.ToString();
                foreach (CarController car in allAIcars)
                {
                    if (car.currentLab > playerCar.currentLab)
                    {
                        playerPosition++;
                        playerP.text = playerPosition.ToString();
                    }
                    else if (car.currentLab == playerCar.currentLab)
                    {
                        if (car.nextCheckpoint > playerCar.nextCheckpoint)
                        {
                            playerPosition++;
                            playerP.text = playerPosition.ToString();
                        }
                        else if (car.nextCheckpoint == playerCar.nextCheckpoint)
                        {
                            if (Vector3.Distance(car.transform.position, allCheckpoints[car.nextCheckpoint].transform.position) < Vector3.Distance(playerCar.transform.position, allCheckpoints[car.nextCheckpoint].transform.position))
                            {
                                playerPosition++;
                                playerP.text = playerPosition.ToString();
                            }
                        }
                    }
                }

                posCkCounter = timeBetweenPosCheck;
            }

            // manage Rubber
            if (playerPosition == 1)
            {
                foreach (CarController car in allAIcars)
                {
                    car.maxSpeed = Mathf.MoveTowards(car.maxSpeed, aiDefaultSpeed + rubberBandSpeedMod, rubberAccel * Time.deltaTime);
                }

                playerCar.maxSpeed = Mathf.MoveTowards(playerCar.maxSpeed, playerDefaultSpeed - rubberBandSpeedMod, rubberAccel * Time.deltaTime);
            }
            else
            {
                foreach (CarController car in allAIcars)
                {
                    car.maxSpeed = Mathf.MoveTowards(car.maxSpeed, aiDefaultSpeed - (rubberBandSpeedMod * ((float)playerPosition / (float)allAIcars.Count + 1)), rubberAccel * Time.deltaTime);
                }

                playerCar.maxSpeed = Mathf.MoveTowards(playerCar.maxSpeed, playerDefaultSpeed + (rubberBandSpeedMod * ((float)playerPosition / (float)allAIcars.Count + 1)), rubberAccel * Time.deltaTime);
            }

        }

    }


    public void FinishRace()
    {
        raceComplete = true;
        UIManager.Instance.raceResult.text = "YOU FINISHED " + playerPosition.ToString() + " Position";

        UIManager.Instance.resultScreen.SetActive(true);
    }

    public void RaceExit()
    {

    }
}
