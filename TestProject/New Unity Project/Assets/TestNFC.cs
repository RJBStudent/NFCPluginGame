using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class TestNFC : MonoBehaviour
{

    public string tagID;
    public Text tag_output_text;
    public bool tagFound = false;

    private AndroidJavaObject mActivity;
    private AndroidJavaObject mIntent;
    private AndroidJavaObject mNfcA;
    private AndroidJavaObject mMifareUltraLight;
    private AndroidJavaObject techNfcA;
    private AndroidJavaObject techMifareUltraLight;
    private string sAction;

    [SerializeField]
    String writeData = "";

    [SerializeField]
    Text readableData;

    [SerializeField] Toggle toggleReadWrite;
    bool isReadToggled = false;

    [SerializeField] Toggle toggleReadTag;
    bool shouldReadTag = false;

    Byte[] dataToWrite;
    bool readNewData = false;

    Byte[] dataRead;

    void Start()
    {
        tag_output_text.text = "No tag...";

            

        dataToWrite = System.Convert.FromBase64String(writeData);
        Debug.Log("String: " + writeData + " ByteData: " + System.Convert.ToBase64String(dataToWrite));
    }

    void Update()
    {
        CheckReadNewTag();

        if (Application.platform == RuntimePlatform.Android)
        {
            if (!tagFound)
            {
                try
                {
                    // Create new NFC Android object
                    mActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");

                    mNfcA = new AndroidJavaObject("android.nfc.tech.NfcA");
                    mMifareUltraLight = new AndroidJavaObject("android.nfc.tech.MifareUltralight");

                    mIntent = mActivity.Call<AndroidJavaObject>("getIntent");
                    sAction = mIntent.Call<String>("getAction");
                    if (sAction == "android.nfc.action.NDEF_DISCOVERED")
                    {
                        Debug.Log("Tag of type NDEF");
                    }
                    else if (sAction == "android.nfc.action.TECH_DISCOVERED")
                    {
                        Debug.Log("TAG DISCOVERED");
                        // Get ID of tag
                        AndroidJavaObject mNdefMessage = mIntent.Call<AndroidJavaObject>("getParcelableExtra", "android.nfc.extra.TAG");
                       if (mNdefMessage != null)
                       {
                            if (shouldReadTag)
                            {



                                byte[] payLoad = mNdefMessage.Call<byte[]>("getId");
                                string text = System.Convert.ToBase64String(payLoad);
                                tag_output_text.text = text;
                                tagID = text;

                                string[] techList = mNdefMessage.Call<string[]>("getTechList");

                                //AndroidJavaObject tech = mIntent.Call<AndroidJavaObject>("getParcelableExtra", "android.nfc.tech.NfcA");
                                //Debug.Log(tech);

                                //Testing set data
                                try
                                {
                                    foreach (string textOut in techList)
                                    {

                                        if (textOut == "android.nfc.tech.NfcA")
                                        {
                                            techNfcA = mNfcA.CallStatic<AndroidJavaObject>("get", mNdefMessage);
                                            Debug.Log(textOut);
                                        }

                                        if (textOut == "android.nfc.tech.MifareUltralight")
                                        {
                                            techMifareUltraLight = mMifareUltraLight.CallStatic<AndroidJavaObject>("get", mNdefMessage);
                                            Debug.Log(textOut);

                                            techMifareUltraLight.Call("connect");
                                        }

                                        //tech.Call("connect");

                                        /*
                                        if(tech.Call<Boolean>("isConnected"))
                                        {
                                            Debug.Log("CONNECTED!!!");
                                        }
                                        else
                                        {
                                            Debug.Log("oof nope");
                                        }*/
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Debug.LogException(ex);
                                }
                            }
                            else
                            {
                                mIntent.Call("removeExtra", "android.nfc.extra.TAG");
                               // toggleReadTag.isOn = false;
                            }

                           // tagFound = true;
                        }
                        else
                        {
                            tag_output_text.text = "No ID found !";
                            mIntent.Call("removeExtra", "android.nfc.extra.TAG");
                            
                        }
                        tagFound = true;
                        return;
                    }
                      if (sAction == "android.nfc.action.TAG_DISCOVERED")
                    {
                        Debug.Log("This type of tag is not supported !");
                    }
                    else
                    {
                        tag_output_text.text = "No tag...";
                        return;
                    }
                }
                catch (Exception ex)
                {
                    string text = ex.Message;
                    tag_output_text.text = text;
                }
            }
        }

        WriteTest();

    }

    bool CheckReadNewTag()
    {
        shouldReadTag = toggleReadTag.isOn;

        isReadToggled = toggleReadWrite.isOn;

        if(shouldReadTag)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    void WriteTest()
    {
        try
        {
            if (techMifareUltraLight != null)
            {

                if (techMifareUltraLight.Call<Boolean>("isConnected"))
                {
                    Debug.Log("Connected");
                        
                        if (!isReadToggled)
                        {
                        Debug.Log("Write");
                            //techMifareUltraLight.Call("writePage", 0, dataToWrite);
                        }
                        else
                        {
                        //  dataRead = techMifareUltraLight.Call<Byte[]>("readPage", 0);
                        Debug.Log("Read");
                        //readableData.text = System.Convert.ToBase64String(dataRead);
                        readNewData = true;

                        }
                        

                    techMifareUltraLight.Call("close");
                    mIntent.Call("removeExtra", "android.nfc.extra.TAG");
                    techMifareUltraLight = null;
                    techNfcA = null;
                   // tagFound = false;
                    toggleReadTag.isOn = false;
                    // toggleRead.isOn = false;
                }
            }
            else
            {
                mIntent.Call("removeExtra", "android.nfc.extra.TAG");
                techMifareUltraLight = null;
                techNfcA = null;
                tagFound = false;
                //toggleReadTag.isOn = false;
            }

        }
        catch (Exception ex)
        {
            Debug.Log("ERROR: ");
            Debug.LogException(ex);
            mIntent.Call("removeExtra", "android.nfc.extra.TAG");
        }
    }

}