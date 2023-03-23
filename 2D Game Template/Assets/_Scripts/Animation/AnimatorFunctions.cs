using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorFunctions : MonoBehaviour

//note: you can call functions with parameters in animation events by setting the argument when clicking on the animation event
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] animatorSounds;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HideObject() => gameObject.SetActive(false);

    public void DestroyObject() => Destroy(gameObject);

    public void ShowObject() => gameObject.SetActive(false);

    public void PlaySound(string soundName)
    {
        AudioClip audioClip;
        foreach (var sound in animatorSounds)
        {
            if (sound.name.ToLower() == soundName.ToLower())
            {
                audioClip = sound;
                audioSource.clip = audioClip;
                audioSource.Play();
                break;
            }
        }
        UnityEngine.Debug.Log($"If no sound was played the arguemnt '{soundName}' does not match the name of any AudioClip in the object's animatorSounds[] in Animator Functions");
    }

    public void StopPlaying() => audioSource.Stop();

}
