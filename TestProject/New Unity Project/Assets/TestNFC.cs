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
    private string sAction;


    void Start()
    {
        tag_output_text.text = "No tag...";
    }

    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (!tagFound)
            {
                try
                {
                    // Create new NFC Android object
                    mActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");

                    mNfcA = new AndroidJavaObject("android.nfc.tech.NfcA");

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
                                foreach(string textOut in techList)
                                {
                                    Debug.Log(textOut);
                                    AndroidJavaObject tech = mNfcA.CallStatic<AndroidJavaObject>("get", mNdefMessage);
                                    tech.Call("connect");
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
                            catch(Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                        else
                        {
                            tag_output_text.text = "No ID found !";
                        }
                        tagFound = true;
                        return;
                    }
                    else if (sAction == "android.nfc.action.TAG_DISCOVERED")
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
    }
}