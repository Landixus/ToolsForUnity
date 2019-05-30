using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using ANT_Managed_Library;
using UnityEngine.UI;



public class UIScanManager : MonoBehaviour
{
    public Button[] devicesButtons;
    public Text[] buttonsText;
    private int maxDeviceNumber;
    private int currentDevice;
    AntDevice antDevice;


    public CadenceDisplay cadenceDisplay;

    public void Start()
    {

        maxDeviceNumber = devicesButtons.Length;
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

    public void addButton(string number)
    {
        if (currentDevice == maxDeviceNumber)
        {
            currentDevice = 0;
        }
        if (currentDevice < maxDeviceNumber)
        {
            buttonsText[currentDevice].text = number;
            devicesButtons[currentDevice].gameObject.SetActive(true);
            currentDevice++;
        }

    }
    
    public void ConnectToDevice()
    {


        cadenceDisplay.ConnectToDevice
         

    }
}
