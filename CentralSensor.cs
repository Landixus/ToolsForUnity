using System;
using UnityEngine;


public class CentralSensor : MonoBehaviour
{


    public PowerMeterDisplay powermeterDisplay;
    public HeartRateDisplay heartRateDisplay;
    public FitnessEquipmentDisplay fitnessEquipmentDisplay;
    public CadenceDisplay cadenceDisplay;
    public SpeedCadenceDisplay speedCadenceDisplay;
    public SpeedDisplay pureSpeedDisplay;
    public bl_PlayerMovement bl_PlayerMovement;
   
    public bool SpeedVirtual;
    public bool SpeedOfDevice;

    public float speed;
    public float cadence;
    public float power;
    public float heartrate;

    //Calculation for only Power needed!
    public float Fgravity;
    public float FrollingResitance;
    public float FaeroDrag = 0.3f;
    public float airDensity = 1.226f;
    public float frontalArea = 0.6f;
    public float drivetrainEffectiveness = 95f;
    public float coefficientOfRollingResistance = 0.004f;
    public float dragCoef = 0.63f;
    public float weight = 80f;
    public float slopeGrade = 0.5f;
    public float brakeForce = 0.1f;
    public float powerVirtualCalc;
    public float fakeCadence = 80;

    public bool FEC_Online = false;
    public bool PM_Online = false;
    public bool CAD_Online = false;
    public bool SPEED_Online = false;
  //  public bool SPEED_CAD_Online = false;
    public bool HR_Online = false; 


    private void Start()
    {
        powermeterDisplay = GameObject.Find("PowerMeterDisplay").GetComponent<PowerMeterDisplay>();
        heartRateDisplay = GameObject.Find("HeartRateDisplay").GetComponent<HeartRateDisplay>();
        fitnessEquipmentDisplay = GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>();
        cadenceDisplay = GameObject.Find("CadenceDisplay").GetComponent<CadenceDisplay>();
        speedCadenceDisplay = GameObject.Find("SpeedCadenceDisplay").GetComponent<SpeedCadenceDisplay>();
        pureSpeedDisplay = GameObject.Find("SpeedDisplay").GetComponent<SpeedDisplay>();
        bl_PlayerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<bl_PlayerMovement>();
        SpeedOfDevice = false;
        SpeedVirtual = false;

        if (weight <= 45f)
        {
            weight = 80f;
        }
    }

    public void Update()
    {
        //always check the slopeGrade to reduce or raise virtual speed
        if (bl_PlayerMovement)
        {
        slopeGrade = bl_PlayerMovement.slopeGrade;
        }
        heartrate = heartRateDisplay.heartRate;

        //Check the devices and get the values
        if (fitnessEquipmentDisplay && fitnessEquipmentDisplay.connected == true)
        {
            GetFecValues();
            SpeedOfDevice = true;
            SpeedVirtual = false;
            Debug.Log("we Have FEC Speed");
        }
        else if (powermeterDisplay && powermeterDisplay.connected == true)
        {
            PMValues();
            SpeedOfDevice = false;
            SpeedVirtual = true;
            Debug.Log("Speed is virtual");
            //CalcSpeed from Power
            Fgravity = speed * 9.8067f * 1.01f * Mathf.Sin(Mathf.Atan(slopeGrade / 100f)) * weight;
            FrollingResitance = speed * 9.8067f * 1f * Mathf.Cos(Mathf.Atan(slopeGrade / 100f)) * weight * coefficientOfRollingResistance;
            FaeroDrag = speed * 0.3f * dragCoef * frontalArea * airDensity * (speed * speed);
            brakeForce = (Fgravity + FrollingResitance + FaeroDrag) / 100;
            //    Debug.Log("BrakeForce" + brakeForce);
            powerVirtualCalc = power - brakeForce;
            speed = powerVirtualCalc / 4.9f;
        }
        else if (cadenceDisplay && cadenceDisplay.connected == true)
        {
            GetCadValues();
            Debug.Log("cadence only)");
        }
        else if (pureSpeedDisplay && pureSpeedDisplay.connected == true)
        {
            SPEED_Online = true;
            GetSpeedValues();
            Debug.Log("GoToPureSpeed");
        }
        //combined speed cadenceDisplay are Rare, enable first if we have user feedback that it is needed
        /*else if (speedCadenceDisplay && speedCadenceDisplay.connected == true)
        {
            GetSpeedCadValues();
        }*/
        //Check if we have a chestStrap active
        else if (heartRateDisplay && heartRateDisplay.connected == true)
        {
            if (!SPEED_Online)
            {
                fakeSpeedwithHr();
                Debug.Log("GoToFakeSpeed");
            }
            
        }
        else
        {
            FEC_Online = false;
            PM_Online = false;
            CAD_Online = false;
            SPEED_Online = false;
            HR_Online = false;
        }

    }
    //Get Values of the FEC Device
    public void GetFecValues()
    {
        speed = fitnessEquipmentDisplay.speed;
        cadence = fitnessEquipmentDisplay.cadence;
        power = fitnessEquipmentDisplay.instantaneousPower;

    }
    //Get Values of the PowerMeter
    public void PMValues()
    {
        power = powermeterDisplay.instantaneousPower;
        cadence = powermeterDisplay.instantaneousCadence;
    }
    //Get values of cadence sensor and calculate speed only "if (!SPEED_Online)"
    public void GetCadValues()
    {
        cadence = cadenceDisplay.cadence;
        if (!SPEED_Online)
        {
        speed = cadence * heartRateDisplay.heartRate / 35.5599976f / 10;
        Debug.Log("WithRealCadence");
        }
        else
        {
            GetSpeedValues();
        }
  
    }
    public void GetSpeedValues()
    {
        speed = pureSpeedDisplay.speed;
        cadence = cadenceDisplay.cadence;
        Debug.Log("PureSpeed");
    }

    /*
    public void GetSpeedCadValues()
    {
        speed = speedCadenceDisplay.speed;
        cadence = speedCadenceDisplay.cadence;
    }
    */
    public void fakeSpeedwithHr()
    {       
            cadence = fakeCadence;
            speed = fakeCadence * heartRateDisplay.heartRate / 35.5599976f / 10;
            Debug.Log("With Fake Cadence");
    }
   

}
