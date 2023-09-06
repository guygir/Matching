using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    [SerializeField] GameObject SoundOn, SoundOff;

    private bool soundIsOn = true;
    
    
    // Start is called before the first frame update
    void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name==name);
        if (s == null)
            return;
        s.source.Play();
    }

    public void FlipSound()
    {
        if (soundIsOn)
        {
            soundIsOn = false;
            SoundOn.SetActive(false);
            SoundOff.SetActive(true);
            foreach (Sound s in sounds)
            {
                s.source.volume = 0f;
            }

        }
        else
        {
            soundIsOn = true;
            SoundOn.SetActive(true);
            SoundOff.SetActive(false);
            foreach (Sound s in sounds)
            {
                s.source.volume = s.volume;
            }
        }
    }
    
}
