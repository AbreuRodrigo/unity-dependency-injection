using UnityEngine;
using DI.Injection;

public class GameManager : MonoBehaviour, IInjector
{
	[Inject] private IAudioManager audioManager = null;
	[Inject] private ISpriteManager spriteManager = null;

	public void Start()
	{
		//Testing the dependencies injected
		audioManager?.PlayAudio();
		spriteManager?.DrawSprite();
	}
}