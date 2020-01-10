using UnityEngine;
using DI.Injection;

public interface IAudioManager
{
	void PlayAudio();
}

public class GameManager : MonoBehaviour, IInjector
{
	[Inject] private IAudioManager audioManager;
	[Inject] private SpriteManager spriteManager;

	public void Start()
	{
		//Testing the dependencies injected
		audioManager?.PlayAudio();
		spriteManager?.DrawSprite();
	}
}