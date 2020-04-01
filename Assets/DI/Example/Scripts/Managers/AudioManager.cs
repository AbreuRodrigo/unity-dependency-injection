using DI.Injection;
using DI.Injection.Scope;
using UnityEngine;

[DependencyScope(Scope.Singleton)]
public class AudioManager : IAudioManager, IDependency
{
	public void PlayAudio()
	{
		Debug.Log("Playing audio...");
	}
}