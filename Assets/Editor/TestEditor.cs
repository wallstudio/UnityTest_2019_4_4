using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Test))] 
class TestEditor : Editor
{
	static Lazy<GUIStyle> STYLE = new Lazy<GUIStyle>(() => (GUIStyle)"CN Message");

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		// https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/ConsoleWindow.cs#L718
		var script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
		var scriptLink = $@"<a href=""{AssetDatabase.GetAssetPath(script)}"" line=""{10}"">{script.name}#{10}</a>";
		EditorGUILayout.SelectableLabel(scriptLink, STYLE.Value);
		
		Debug.Log(scriptLink);
	}
}