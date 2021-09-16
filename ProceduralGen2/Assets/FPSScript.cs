using System.Collections;
using UnityEngine;

public class FPSScript : MonoBehaviour
{
	float deltaTime = 0.0f;
	float onePercentLoopCounter;
	float pointOnePercentLoopCounter;

	ArrayList fpsStore = new ArrayList();
	float averageFPS = 0.0f;
	float onePercentLow = 0.0f;
	float pointOnePercentLow = 0.0f;


	void Update()
	{
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
	}

	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		fpsStore.Add(fps);

		if (onePercentLoopCounter >= 100)
        {
			onePercentLow = 0;
			fpsStore.Sort();

			for (int i = 0; i < fpsStore.Count * 0.01f; i++)
			{
				onePercentLow += (float) fpsStore[i];
			}
			onePercentLow /= fpsStore.Count * 0.01f;
			onePercentLoopCounter = 0;

			for (int i = 0; i < fpsStore.Count; i++)
			{
				averageFPS += (float)fpsStore[i];
			}
			averageFPS /= fpsStore.Count;
		}

		if(pointOnePercentLoopCounter >= 1000)
        {
			pointOnePercentLow = 0;
			fpsStore.Sort();

			for (int i = 0; i < fpsStore.Count * 0.001f; i++)
			{
				pointOnePercentLow += (float) fpsStore[i];
			}
			pointOnePercentLow /= fpsStore.Count * 0.001f;
			pointOnePercentLoopCounter = 0;
		}


		string text = string.Format("{0:0.0} ms ({1:0.} fps)\n{4:0.0} (Average fps)\n{2:0.0} (1% low fps)\n{3:0.0} (0.1% low fps)", msec, fps, onePercentLow, pointOnePercentLow, averageFPS);
		GUI.Label(rect, text, style);
		onePercentLoopCounter++;
		pointOnePercentLoopCounter++;
	}
}