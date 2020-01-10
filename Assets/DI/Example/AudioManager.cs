using DI.Injection;
using UnityEngine;

public class AudioManager : MonoBehaviour, IDependency, IAudioManager
{
	public void PlayAudio()
	{
		Debug.Log("Playing audio...");
	}
}