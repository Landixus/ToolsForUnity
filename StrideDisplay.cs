using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ANT_Managed_Library;
using System;

public class StrideDisplay : MonoBehaviour {

    public bool autoStartScan = true; //start scan on play
    public bool connected = false; //will be set to true once connected
  

    //windows and mac settings
    public bool autoConnectToFirstSensorFound = true; //for windows and mac, either connect to the first sensor found or let you pick a sensor manually in the scanResult list with your own UI and call ConnectToDevice(AntDevice device)
    public List<AntDevice> scanResult;

    //android settings
    public bool useAndroidUI = true; //will open the unified ant+ UI on the android app if set to true, otherwise will connect to the first found device
    public bool skipPreferredSearch = true;  //- True = Don't automatically connect to user's preferred device, but always go to search for other devices.

    //the sensor values we receive fron the onReceiveData event
    public float speed; // The speed in km/h
    public float distance; //the distance in meters
    public int strides;
    public int cadence;


    
    private AntChannel backgroundScanChannel;
    private AntChannel deviceChannel;

    public int deviceID = 0; //set this to connect to a specific device ID
    void Start() {

        if (autoStartScan)
            StartScan();

    }

    //Start a background Scan to find the device
    public void StartScan() {

        Debug.Log("Looking for ANT + Speed sensor");
#if UNITY_ANDROID && !UNITY_EDITOR
        
        //java:  connect_stride(String gameobjectName, boolean useAndroidUI, boolean skipPreferredSearch, int deviceID)
        AndroidJNI.AttachCurrentThread();
        using (AndroidJavaClass javaClass = new AndroidJavaClass("com.ant.plugin.Ant_Connector")) {
            using (AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("mContext")) {
                activity.Call("connect_stride", this.gameObject.name, useAndroidUI, skipPreferredSearch,deviceID);
            }
        }
#else
        AntManager.Instance.Init();
        scanResult = new List<AntDevice>();
        backgroundScanChannel = AntManager.Instance.OpenBackgroundScanChannel(0);
        backgroundScanChannel.onReceiveData += ReceivedBackgroundScanData;
#endif

    }

    
    //Android function
    void ANTPLUG_ConnectEvent(string resultCode) {
        switch (resultCode) {
            case "SUCCESS":
                connected = true;
                break;
            case "CHANNEL_NOT_AVAILABLE":
                //Channel Not Available

                break;
            case "ADAPTER_NOT_DETECTED":
                //ANT Adapter Not Available. Built-in ANT hardware or external adapter required.
                Debug.Log("ANT Adapter Not Available. Built-in ANT hardware or external adapter required.");
                break;
            case "BAD_PARAMS":

                //Bad request parameters.

                break;
            case "OTHER_FAILUR":

                //RequestAccess failed. See logcat for details.

                break;
            case "DEPENDENCY_NOT_INSTALLED":
            //You need to install the ANT+ Plugins service or you may need to update your existing version if you already have it. 

            case "USER_CANCELLED":
                //USER_CANCELLED
                break;
            case "UNRECOGNIZED":
                //UNRECOGNIZED. PluginLib Upgrade Required?",

                break;
            default:
                //UNRECOGNIZED
                break;
        }
    }

    void ANTPLUG_StateChange(string newDeviceState) {
        switch (newDeviceState) {
            case "DEAD":
                connected = false;
                break;
            case "CLOSED":

                break;
            case "SEARCHING":
                //searching
                break;
            case "TRACKING":
                //tracking
                break;
            case "PROCESSING_REQUEST":

                break;
            default:
                //UNRECOGNIZED
                break;
        }
    }

   
    void ANTPLUG_Receive_Stride_instantaneousCadence(string s) {
        cadence = int.Parse(s);
    }
    void ANTPLUG_Receive_Stride_cumulativeDistance(string s) {
        distance = float.Parse(s);
    }
    void ANTPLUG_Receive_Stride_instantaneousSpeed(string s) {
        speed = float.Parse(s)*3.6f;
    }
    void ANTPLUG_Receive_Stride_cumulativeStrides(string s) {
        strides = int.Parse(s);
    }

    //Windows and mac 
    //If the device is found
    void ReceivedBackgroundScanData(Byte[] data) {

        byte deviceType = (data[12]); // extended info Device Type byte

        switch (deviceType) {

         

            default: {

                    break;
                }
        }

    }

    void ConnectToDevice(AntDevice device) {
        AntManager.Instance.CloseBackgroundScanChannel();
        byte channelID = AntManager.Instance.GetFreeChannelID();
        deviceChannel = AntManager.Instance.OpenChannel(ANT_ReferenceLibrary.ChannelType.BASE_Slave_Receive_0x00, channelID, (ushort)device.deviceNumber,device.deviceType, device.transType, (byte)device.radiofreq, (ushort)device.period, false);
        connected = true;
        deviceChannel.onReceiveData += Data;
        deviceChannel.onChannelResponse += ChannelResponse;

        deviceChannel.hideRXFAIL = true;
    }

    //Deal with the received Data
    public void Data(Byte[] data) {

      


    }



    void ChannelResponse(ANT_Response response) {

       
    }

}
