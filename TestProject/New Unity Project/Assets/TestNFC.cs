using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

class TNF
{
    public TNF(short c) { constantVal = c; }
    public short constantVal;
}

public class TestNFC : MonoBehaviour
{

    public string tagID;
    public Text tag_output_text;
    public bool tagFound = false;

    private AndroidJavaObject mActivity;
    private AndroidJavaObject mIntent;
    private AndroidJavaObject mNfcA;
    private AndroidJavaObject mMifareUltraLight;
    private AndroidJavaObject mNdef;
    private AndroidJavaObject techNfcA;
    private AndroidJavaObject techMifareUltraLight;
    private AndroidJavaObject techNdef;
    private string sAction;

    private AndroidJavaObject ourNdefMessage;
    private AndroidJavaObject firstNdefRecord;
    private AndroidJavaObject[] followingNdefRecords;
    private AndroidJavaClass NdefRecordClass;


    [SerializeField]
    String writeData = "";

    [SerializeField]
    Text readableData;

    [SerializeField] Toggle toggleReadWrite;
    bool isReadToggled = false;


    Byte[] dataToWrite;
    bool readNewData = false;

    Byte[] dataRead;

    [SerializeField] Button findNewButton;
    bool lookForNFC = false;



    TNF TNF_ABSOLUTE_URI = new TNF(0x00000003);
    TNF TNF_EMPTY = new TNF(0x00000000);
    TNF TNF_EXTERNAL_TYPE = new TNF(0x00000004);
    TNF TNF_MIME_MEDIA = new TNF(0x00000002);
    TNF TNF_UNCHANGED = new TNF(0x00000006);
    TNF TNF_UNKNOWN = new TNF(0x00000005);
    TNF TNF_WELL_KNOWN = new TNF(0x00000001);

    Byte[] languageBytes;

    void Start()
    {
        tag_output_text.text = "No tag...";

        findNewButton.interactable = true;


        dataToWrite = System.Convert.FromBase64String(writeData);
        

        Debug.Log("String: " + writeData + " ByteData: " + dataToWrite);

        NdefRecordClass = new AndroidJavaClass("android.nfc.NdefRecord");

        firstNdefRecord = new AndroidJavaObject("android.nfc.NdefRecord", NdefRecordClass.GetStatic<short>("TNF_EMPTY"), new Byte[] { }, new Byte[] { }, new Byte[] { });



        followingNdefRecords = new AndroidJavaObject[1];
        
        followingNdefRecords[0] = new AndroidJavaObject("android.nfc.NdefRecord", NdefRecordClass.GetStatic<short>("TNF_WELL_KNOWN"), NdefRecordClass.GetStatic<Byte[]>("RTD_TEXT"), new Byte[] { }, dataToWrite);
        //followingNdefRecords[0] = firstNdefRecord;

        try
        {
            //AndroidJavaObject javaArray = javaArrayFromCS(followingNdefRecords);

            

            ourNdefMessage = new AndroidJavaObject("android.nfc.NdefMessage", firstNdefRecord, followingNdefRecords);

            Debug.Log("NDEF DATA: " + ourNdefMessage.Call<AndroidJavaObject[]>("getRecords")[1].Call<Byte[]>("getPayload"));
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
    }

    private AndroidJavaObject javaArrayFromCS(AndroidJavaObject[] values)
    {
        
        AndroidJavaClass arrayClass = new AndroidJavaClass("java.lang.reflect.Array");
        Debug.Log("Creating Array...");
        AndroidJavaObject arrayObject = arrayClass.CallStatic<AndroidJavaObject>("newInstance", new AndroidJavaClass("android.nfc.NdefRecord"), values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            Debug.Log("Setting values at :" + i);
            arrayClass.CallStatic("set", arrayObject, i, values[i]);
        }
        Debug.Log("Created...");
        
        return arrayObject;
    }


void Update()
    {
        if (!lookForNFC)
            return;

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
                    mNdef = new AndroidJavaObject("android.nfc.tech.Ndef");

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

                                    }

                                    if (textOut == "android.nfc.tech.Ndef")
                                    {
                                        techNdef = mNdef.CallStatic<AndroidJavaObject>("get", mNdefMessage);
                                        Debug.Log(textOut);

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
                            // tag_output_text.text = "No ID found !";
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
                    Debug.Log("ERROR at Finding Tag: ");

                    string text = ex.Message;
                    tag_output_text.text = text;
                }
            }
        }

        WriteTest();

    }

    bool CheckReadNewTag()
    {


        isReadToggled = toggleReadWrite.isOn;
        return isReadToggled;
    }


    void WriteTest()
    {
        try
        {

            //Debug.Log(" Looking for connection to tech ");
            if (techNdef != null)
            {
                try
                {
                    techNdef.Call("connect");
                    //if (techMifareUltraLight.Call<Boolean>("isConnected"))
                    //{
                    Debug.Log("Connected");

                    Debug.Log(" Can write is : "+ techNdef.Call<Boolean>("isWritable"));

                    if (!isReadToggled)
                    {
                        Debug.Log("Write");

                        techNdef.Call("writeNdefMessage", ourNdefMessage);

                        //  techMifareUltraLight.Call("writePage", 1, dataToWrite);
                    }
                    else
                    {
                         AndroidJavaObject newNdefMessage = techNdef.Call<AndroidJavaObject>("getNdefMessage");
                        AndroidJavaObject[] newRecords = newNdefMessage.Call<AndroidJavaObject[]>("getRecords");
                        //dataRead = newNdefMessage.Call<Byte[]>("toByteArray");

                        dataRead = newRecords[1].Call<Byte[]>("getPayload");

                        Debug.Log("Read");
                        readableData.text = System.Convert.ToBase64String(dataRead);
                        readNewData = true;

                    }
                    techNdef.Call("close");
                }
                catch (Exception econnect)
                {
                    Debug.Log("ERROR CONNECTING : ");
                    Debug.LogException(econnect);
                    mIntent.Call("removeExtra", "android.nfc.extra.TAG");
                    techMifareUltraLight = null;
                    techNfcA = null;
                    techNdef = null;
                    lookForNFC = false;
                    findNewButton.interactable = true;
                }
                finally
                {
                    try
                    {
                        if (techNdef != null)
                        {
                            techNdef.Call("close");
                        }
                    }
                    catch (Exception eclose)
                    {
                        Debug.Log("ERROR CLOSING: ");
                        Debug.LogException(eclose);
                        mIntent.Call("removeExtra", "android.nfc.extra.TAG");
                        techMifareUltraLight = null;
                        techNfcA = null;
                        techNdef = null;
                        lookForNFC = false;
                        findNewButton.interactable = true;
                    }
                    mIntent.Call("removeExtra", "android.nfc.extra.TAG");
                }

                techMifareUltraLight = null;
                techNfcA = null;
                techNdef = null;
                // toggleRead.isOn = false;
                findNewButton.interactable = true;

                lookForNFC = false;
                //}
            }
            else
            {
                mIntent.Call("removeExtra", "android.nfc.extra.TAG");
                techMifareUltraLight = null;
                techNfcA = null;
                techNdef = null;
                lookForNFC = false;
                findNewButton.interactable = true;
                //toggleReadTag.isOn = false;
            }

        }
        catch (Exception ex)
        {
            Debug.Log("ERROR at read/write : ");
            Debug.LogException(ex);
            if (techMifareUltraLight != null)
            {
                techMifareUltraLight.Call("close");
            }
            techMifareUltraLight = null;
            techNfcA = null;
            mIntent.Call("removeExtra", "android.nfc.extra.TAG");


            lookForNFC = false;
            findNewButton.interactable = true;
        }
    }


    public void StartLookingForTag()
    {
        lookForNFC = true;
        findNewButton.interactable = false;

    }
}