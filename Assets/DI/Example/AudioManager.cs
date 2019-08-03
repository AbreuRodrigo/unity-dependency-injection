using DI.Injection;
using UnityEngine;

public class AudioManager : MonoBehaviour, IDependency
{
	public void PlayAudio()
	{
		Debug.Log("Playing audio...");
	}
}