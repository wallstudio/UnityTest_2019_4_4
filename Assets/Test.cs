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
	[SerializeField] Texture2D texture;
	void OnGUI()
	{
		{
			if(GUI.Button(new Rect(0, 10, Screen.width, 100), "Button"))
			{
				var text = helloWorld.Value.CallStatic<string>("getText", m_Context.Value, "xxx", 123);
				Debug.Log(text);
				m_Result.AppendLine(text);

				var path = Path.Combine(Application.persistentDataPath, "texture.png");
				File.WriteAllBytes(path, texture.EncodeToPNG());
				m_Result.AppendLine(path);
				if(Application.platform != RuntimePlatform.Android)
				{
					Application.OpenURL(path);
				}
				else
				{
					var result = helloWorld.Value.CallStatic<string>("openFile", m_Context.Value, path, "image/*");
					Debug.Log(result);
					m_Result.AppendLine(result);
				}
			}

			GUI.Label(new Rect(10, 110, Screen.width, 100), m_Result.ToString());
		}
	}
}