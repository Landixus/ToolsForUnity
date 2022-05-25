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
    //public FakePowerCadenceHr fakePowerCadenceHR;

    public float FECspeed;
    public float FECcadence;
    public float FECinstPower;

    public float PMPower;
    public float PMCadence;

    public float SPDSpeed;
    public float SPDCadence;

    public float CadCadence;

    public bool SpeedVirtual;
    public bool SpeedOfDevice;

    public float speed;
    public float VSpeed;
    public float RSpeed;

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
        if (powermeterDisplay == null)
        {
            powermeterDisplay = GameObject.Find("PowerMeterDisplay").GetComponent<PowerMeterDisplay>();
        }

        if (heartRateDisplay == null)
        {
            heartRateDisplay = GameObject.Find("HeartRateDisplay").GetComponent<HeartRateDisplay>();
        }

        if (fitnessEquipmentDisplay == null)
        {
            fitnessEquipmentDisplay = GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>();

        }

        if (cadenceDisplay == null)
        {
            cadenceDisplay = GameObject.Find("CadenceDisplay").GetComponent<CadenceDisplay>();
        }

        if (speedCadenceDisplay == null)
        {
            speedCadenceDisplay = GameObject.Find("SpeedCadenceDisplay").GetComponent<SpeedCadenceDisplay>();
        }

        if (heartRateDisplay == null)
        {
            heartRateDisplay = GameObject.Find("HeartRateDisplay").GetComponent<HeartRateDisplay>();
        }
        if (pureSpeedDisplay == null)
        {
            pureSpeedDisplay = GameObject.Find("SpeedDisplay").GetComponent<SpeedDisplay>();
        }

        if (bl_PlayerMovement == null)
        {
            bl_PlayerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<bl_PlayerMovement>();
        }



        if (fitnessEquipmentDisplay.connected == true)
        { 
            GetFecValues();
            if (!FEC_Online)
            {
                FEC_Online = true;

                SpeedOfDevice = true;
                SpeedVirtual = false;
                cadence = FECcadence;
                power = FECinstPower;
                speed = FECspeed;
                Debug.Log("we Have FEC Speed");
            }
            
        }
        else if (powermeterDisplay.connected == true)
        {
            PMValues();
        }

        if (!PM_Online)
        {
            PM_Online = true;
            power = PMPower;
            cadence = PMCadence;
            Debug.Log("Speed is virtual");
        }
        else
        {
            FEC_Online = false;
            PM_Online = false;
        }



    }

/*

        if(cadenceDisplay.connected == true)
        {
            GetCadValues();
            cadence = CadCadence;
        }
        if (speedCadenceDisplay.connected == true)
        {
            GetSpeedCadValues();
            speed = SPDSpeed;
            cadence = SPDCadence;
            SpeedOfDevice = true;  // needs a break for Elevation
            SpeedVirtual = false;
        }
        if (pureSpeedDisplay.connected == true)
        {
            speed = pureSpeedDisplay.GetComponent<SpeedDisplay>().speed; // needs a break for Elevation

        }

        if (SpeedVirtual == true)
        {
            CalcForSpeedWithPower();
         
        }

        heartrate = heartRateDisplay.heartRate;
        slopeGrade = bl_PlayerMovement.slopeGrade;
        if (heartRateDisplay.connected == true && fitnessEquipmentDisplay.connected == false && powermeterDisplay.connected == false && speedCadenceDisplay.connected == false && cadenceDisplay.connected == false)
        {
            fakeSpeedwithHr();
            cadence = fakeCadence;
        }

    }*/

    public void GetFecValues()
    {
        FECspeed = fitnessEquipmentDisplay.GetComponent<FitnessEquipmentDisplay>().speed;
        FECcadence = fitnessEquipmentDisplay.GetComponent<FitnessEquipmentDisplay>().cadence;
        FECinstPower = fitnessEquipmentDisplay.GetComponent<FitnessEquipmentDisplay>().instantaneousPower;
        
    }

    public void PMValues()
    {
        PMPower = powermeterDisplay.GetComponent<PowerMeterDisplay>().instantaneousPower;
        PMCadence = powermeterDisplay.GetComponent<PowerMeterDisplay>().instantaneousCadence;
        if (PMPower >= 0 && fitnessEquipmentDisplay.instantaneousPower <= 0 && fitnessEquipmentDisplay.speed <= 0)
        {
            SpeedVirtual = true;
            Debug.Log("Speed is virtual");
        }
        else
        {
            SpeedVirtual = false;
        }
    }

    public void GetCadValues()
    {
        CadCadence = cadenceDisplay.GetComponent<CadenceDisplay>().cadence;
    }

    public void GetSpeedCadValues()
    {
        SPDSpeed = speedCadenceDisplay.GetComponent<SpeedCadenceDisplay>().speed;
        SPDCadence = speedCadenceDisplay.GetComponent<SpeedCadenceDisplay>().cadence;
    }

    public void CalcForSpeedWithPower()
    {
        if ( SpeedVirtual == true)
        {
            
            
            Fgravity = speed * 9.8067f * 1.01f * Mathf.Sin(Mathf.Atan(slopeGrade / 100f)) * weight;
            FrollingResitance = speed * 9.8067f * 1f * Mathf.Cos(Mathf.Atan(slopeGrade / 100f)) * weight * coefficientOfRollingResistance;
            FaeroDrag = speed * 0.3f * dragCoef * frontalArea * airDensity * (speed * speed);
            brakeForce = (Fgravity + FrollingResitance + FaeroDrag) /100;
        //    Debug.Log("BrakeForce" + brakeForce);
            powerVirtualCalc = power - brakeForce;
            speed = powerVirtualCalc / 4.9f;
                       
        }
            
    }

    public void fakeSpeedwithHr()
    {
           //fake with HR
            speed = fakeCadence * heartRateDisplay.heartRate / 35.5599976f /10;
            Debug.Log("we us fakedSpeed" + speed);
    }
   

}





    /*public int GetPower()
    {
        if (powermeterDisplay.connected)
        {
            return powermeterDisplay.instantaneousPower;
        }
        if (fitnessEquipmentDisplay.connected)
        {
            return fitnessEquipmentDisplay.instantaneousPower;
        }
        if (cadenceDisplay.connected)
        {
            return (int)fakePowerCadenceHR.fakePower;
        }
        return 0;
    }

    
    public int GetCadence()
    {
        if (powermeterDisplay.connected)
        {
            return powermeterDisplay.instantaneousCadence;
        }
        if (fitnessEquipmentDisplay.connected)
        {
            return fitnessEquipmentDisplay.cadence;
        }
        if (cadenceDisplay.connected)
        {
            return cadenceDisplay.cadence;
        }
        return 0;
    }

   
    public float GetHeartRate()
    {
        if (heartRateDisplay.connected)
        {
            return heartRateDisplay.heartRate;
        }
        return 0f;
    }*/


