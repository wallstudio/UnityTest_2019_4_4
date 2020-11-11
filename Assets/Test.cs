using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class Test : MonoBehaviour
{
	static readonly Lazy<AndroidJavaObject> m_Context = new Lazy<AndroidJavaObject>(() =>
	{
		var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		return activity.Call<AndroidJavaObject>("getApplicationContext");
	});
	static readonly Lazy<AndroidJavaClass> helloWorld =  new Lazy<AndroidJavaClass>(() => new AndroidJavaClass("world.hello.HelloWorldKt"));

	StringBuilder m_Result = new StringBuilder();
	void OnGUI()
	{
		{
			if(GUI.Button(new Rect(0, 10, Screen.width, 100), "Button"))
			{
				var text = helloWorld.Value.CallStatic<string>("getText", m_Context.Value, "xxx", 123);
				Debug.Log(text);
				m_Result.AppendLine(text);
			}

			GUI.Label(new Rect(10, 110, Screen.width, 100), m_Result.ToString());
		}
	}
	
	// Start is called before the first frame update
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		
	}
}
