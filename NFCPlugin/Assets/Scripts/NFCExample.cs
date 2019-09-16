using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class NFCExample : MonoBehaviour
{
    public Text tag_output_text;
    AndroidJavaClass pluginTutorialActivityJavaClass;

    void Start()
    {
        AndroidJNI.AttachCurrentThread();
        pluginTutorialActivityJavaClass = new AndroidJavaClass("com.twinsprite.nfcplugin.NFCPluginTest");
    }

    void Update()
    {
        string value = pluginTutorialActivityJavaClass.CallStatic<string>("getValue");
        tag_output_text.text = "Value:\n" + value;
    }
}