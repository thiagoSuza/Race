using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public float maxSpeed;
    public Rigidbody theRB;
   
    private float speedInput;
    private float turnInput;

    public float forwardAccel = 8f, reverseAccel = 4f;
    public float turnStrenht = 180f;

    private bool grounded;

    public Transform groundRayPoint,groundRayPoint2;

    public LayerMask whatIsGround;

    public float groundRayLenht = 0.75f;

    private float dragOnGround;

    public float gravityMod = 10f;

    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn = 25f;

    public ParticleSystem[] dustTrail;
    public float maxEmission = 25 , emissionFadeSpeed = 20f;
    private float emissionRate;

    public AudioSource engineSound;

    public int nextCheckpoint;
    public int currentLab;

    public float laptTime, bestLapTime;

    public bool isAI;

    public int currentTarget;
    private Vector3 targetPoint;
    public float aiAccelerateSpeed = 1f, aiTurnSpeed = .8f,aiReachPointRange = 5f,aiPointVariance = 3f, aiMaxTurn = 30f;
    private float aiSpeedInput, aiSpeedMod;

    public float resetCooldown = 2f;
    private float resetCounter;

    



    // Start is called before the first frame update
    void Start()
    {
        theRB.transform.parent = null;
        dragOnGround = theRB.drag;

        if (isAI)
        {
            targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
            RandoniseAITarge();
            aiSpeedMod = Random.Range(.8f, 1.1f);
        }
        emissionRate = 25f;
        UIManager.Instance.labCounterText.text = currentLab.ToString() + " / " + RaceManager.instance.totalLabs.ToString();
        resetCounter = resetCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if (!RaceManager.instance.isStarting)
        {


            laptTime += Time.deltaTime;
            if (!isAI)
            {



                var ts = System.TimeSpan.FromSeconds(laptTime);
                UIManager.Instance.currentLabTime.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);

                speedInput = 0f;
                if (Input.GetAxis("Vertical") > 0)
                {
                    speedInput = Input.GetAxis("Vertical") * forwardAccel;
                }
                else if (Input.GetAxis("Vertical") < 0)
                {
                    speedInput = Input.GetAxis("Vertical") * reverseAccel;
                }

                turnInput = Input.GetAxis("Horizontal");

                /* if(grounded && Input.GetAxis("Vertical") != 0)
                 {
                     transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f,turnInput * turnStrenht * Time.deltaTime *Mathf.Sign(speedInput) * (theRB.velocity.magnitude/maxSpeed),0f));
                 }*/

                if(resetCounter > 0)
                {
                    resetCounter -= Time.deltaTime;
                }


                if (Input.GetKeyDown(KeyCode.R) && resetCounter <= 0)
                {
                    ResetToTrack();
                }

               
            }
            else
            {
                targetPoint.y = transform.position.y;
                if (Vector3.Distance(transform.position, targetPoint) < aiReachPointRange)
                {
                    SetNextAITarge();
                }

                Vector3 targetDire = targetPoint - transform.position;
                float angle = Vector3.Angle(targetDire, transform.forward);

                Vector3 localPos = transform.InverseTransformPoint(targetPoint);
                if (localPos.x < 0f)
                {
                    angle = -angle;
                }

                turnInput = Mathf.Clamp(angle / aiMaxTurn, -1f, 1f);
                if (Mathf.Abs(angle) < aiMaxTurn)
                {
                    aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, 1f, aiAccelerateSpeed);
                }
                else
                {
                    aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, aiTurnSpeed, aiAccelerateSpeed);
                }


                speedInput = aiSpeedInput * forwardAccel * aiSpeedMod;
            }


            //Turning the wheels
            leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
            rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn), rightFrontWheel.localRotation.eulerAngles.z);

            // transform.position = theRB.position;

            // controll PS
            emissionRate = Mathf.MoveTowards(emissionRate, 0f, emissionFadeSpeed * Time.deltaTime);
            if (grounded && (Mathf.Abs(turnInput) > .5f || (theRB.velocity.magnitude < maxSpeed * .5f && theRB.velocity.magnitude != 0f)))
            {
                emissionRate = maxEmission;
            }

            if (theRB.velocity.magnitude == 0f)
            {
                emissionRate = 0f;
            }

            for (int i = 0; i < dustTrail.Length; i++)
            {
                var emissionModule = dustTrail[i].emission;
                emissionModule.rateOverTime = emissionRate;

            }

            if (engineSound != null)
            {
                engineSound.pitch = 1f + ((theRB.velocity.magnitude / maxSpeed) * 2f);
            }

        }
    }

    private void FixedUpdate()
    {
        grounded = false;
        RaycastHit hit;
        Vector3 normalTarget = Vector3.zero;

        if(Physics.Raycast(groundRayPoint.position,-transform.up,out hit,groundRayLenht,whatIsGround))
        {
            grounded = true;
            normalTarget = hit.normal;
        }

        if (Physics.Raycast(groundRayPoint2.position, -transform.up, out hit, groundRayLenht, whatIsGround))
        {
            grounded = true;
            normalTarget = (normalTarget + hit.normal)/2f;
        }

        // when on ground rotate to normal
        if (grounded)
        {
            transform.rotation = Quaternion.FromToRotation(transform.up,normalTarget) * transform.rotation;
        }


        // accelerate
        if (grounded)
        {
            theRB.drag = dragOnGround;
            theRB.AddForce(transform.forward * speedInput * 1000);
        }
        else
        {
            theRB.drag = .1f;
            theRB.AddForce(-Vector3.up * gravityMod * 100f);
        }
        
        if(theRB.velocity.magnitude > maxSpeed)
        {
            theRB.velocity = theRB.velocity.normalized* maxSpeed;
        }
        transform.position = theRB.position;

        if (grounded && speedInput != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrenht * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed), 0f));
        }
    }

    public void CheckPointHit(int cpNumber)
    {
        if(cpNumber == nextCheckpoint)
        {
            nextCheckpoint++;
            if(nextCheckpoint == RaceManager.instance.allCheckpoints.Length)
            {
                nextCheckpoint = 0;
                LapCompleted();
            }
        }
        if (isAI)
        {
            if(cpNumber == currentTarget)
            {
                SetNextAITarge();
            }
        }
    }

    public void SetNextAITarge()
    {
        currentTarget++;
        if (currentTarget >= RaceManager.instance.allCheckpoints.Length)
        {
            currentTarget = 0;
        }

        targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
        RandoniseAITarge();
    }

    public void LapCompleted()
    {
        currentLab++;
        if(laptTime < bestLapTime || bestLapTime == 0)
        {
            bestLapTime = laptTime;
            
        }
        if(currentLab <= RaceManager.instance.totalLabs)
        {
            laptTime = 0f;
            if (!isAI)
            {
                var ts = System.TimeSpan.FromSeconds(bestLapTime);
                UIManager.Instance.bestLapTime.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);

                UIManager.Instance.labCounterText.text = currentLab.ToString() + " / " + RaceManager.instance.totalLabs.ToString();
            }
        }
        else
        {
            if (!isAI)
            {
                aiSpeedMod = 1f;
                isAI = true;
                targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
                RandoniseAITarge();

                var ts = System.TimeSpan.FromSeconds(bestLapTime);
                UIManager.Instance.bestLapTime.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);

                RaceManager.instance.FinishRace();
            }
        }
       
    }

    public void RandoniseAITarge()
    {
        targetPoint += new Vector3( Random.Range(-aiPointVariance,aiPointVariance),0f , Random.Range(-aiPointVariance, aiPointVariance));
    }


    public void ResetToTrack()
    {
        int pointToGotTo = nextCheckpoint - 1;
        if(pointToGotTo < 0)
        {
            pointToGotTo = RaceManager.instance.allCheckpoints.Length -1;
        }


        transform.position = RaceManager.instance.allCheckpoints[pointToGotTo].transform.position;
        theRB.transform.position = RaceManager.instance.allCheckpoints[pointToGotTo].transform.position;
        theRB.velocity = Vector3.zero;
        speedInput = 0f;
        turnInput = 0f;

        resetCounter  = resetCooldown;
    }

}
