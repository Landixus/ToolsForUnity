using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class BikeComputerScanManager : MonoBehaviour
{
    //Pairing
    public Button[] devicesButtons;
    public TMP_Text[] buttonsText;
    public byte[] deviceTypes;
    public byte[] transTypes;
    public int[] deviceNumbers;
    private int maxDeviceNumber;
    private int currentDevice;
    public CadenceDisplay cd;
    public PowerMeterDisplay pd;
    public FitnessEquipmentDisplay fd;
    public HeartRateDisplay hr;


    // Start is called before the first frame update
    void Start()
    {
        maxDeviceNumber = devicesButtons.Length;
        deviceTypes = new byte[maxDeviceNumber];
        transTypes = new byte[maxDeviceNumber];
        deviceNumbers = new int[maxDeviceNumber];
        currentDevice = 0;
        resetButtons();
    }


    public void resetButtons()
    {
        currentDevice = 0;
        for (int i = 0; i < maxDeviceNumber; i++)
        {
            buttonsText[i].text = "";
            devicesButtons[i].gameObject.SetActive(false);
        }
    }

    public void addButton(string number, byte trans, byte type, string name)
    {
        if (currentDevice == maxDeviceNumber)
        {
            currentDevice = 0;
        }
        if (currentDevice < maxDeviceNumber)
        {
            buttonsText[currentDevice].text = name;
            devicesButtons[currentDevice].gameObject.SetActive(true);
            deviceTypes[currentDevice] = type;
            transTypes[currentDevice] = trans;
            deviceNumbers[currentDevice] = int.Parse(number);
            currentDevice++;
        }
    }

    public void ConnectToDevice(int deviceNumber)
    {
        byte b = this.deviceTypes[deviceNumber];
        if (b > 17)
        {
            if (b != 120)
            {
                if (b - 121 <= 1)
                {
                    AntDevice antDevice = new AntDevice();
                    antDevice.deviceType = this.deviceTypes[deviceNumber];
                    antDevice.deviceNumber = this.deviceNumbers[deviceNumber];
                    antDevice.transType = this.transTypes[deviceNumber];
                    antDevice.period = 8102;
                    antDevice.radiofreq = 57;
                    antDevice.name = "BikeCadence(" + antDevice.deviceNumber + ")";
                    this.cd.ConnectToDevice(antDevice);
                    return;
                }
            }
            else
            {
                AntDevice antDevice2 = new AntDevice();
                antDevice2.deviceType = this.deviceTypes[deviceNumber];
                antDevice2.deviceNumber = this.deviceNumbers[deviceNumber];
                antDevice2.transType = this.transTypes[deviceNumber];
                antDevice2.period = 8070;
                antDevice2.radiofreq = 57;
                antDevice2.name = "BikeCadence(" + antDevice2.deviceNumber + ")";
                this.hr.ConnectToDevice(antDevice2);
            }
            return;
        }
        if (b == 11)
        {
            AntDevice antDevice3 = new AntDevice();
            antDevice3.deviceType = this.deviceTypes[deviceNumber];
            antDevice3.deviceNumber = this.deviceNumbers[deviceNumber];
            antDevice3.transType = this.transTypes[deviceNumber];
            antDevice3.period = 8182;
            antDevice3.radiofreq = 57;
            antDevice3.name = "Powermeter(" + antDevice3.deviceNumber + ")";
            this.pd.ConnectToDevice(antDevice3);
            return;
        }
        if (b != 17)
        {
            return;
        }
        AntDevice antDevice4 = new AntDevice();
        antDevice4.deviceType = this.deviceTypes[deviceNumber];
        antDevice4.deviceNumber = this.deviceNumbers[deviceNumber];
        antDevice4.transType = this.transTypes[deviceNumber];
        antDevice4.period = 8192;
        antDevice4.radiofreq = 57;
        antDevice4.name = "FitnessEquipment(" + antDevice4.deviceNumber + ")";
        this.fd.ConnectToDevice(antDevice4);
    }



}

 
