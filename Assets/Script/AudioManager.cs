using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public AudioClip olga;
    public AudioClip wish;
    public AudioClip bgm;
    public AudioClip[] chain;
    private AudioSource chainSource;
    private AudioSource bgmSource;
    private AudioSource boiseSource;

	// Use this for initialization
	void Start () {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        bgmSource = audioSources[0];
        chainSource = audioSources[1];
        boiseSource = audioSources[2];

        //bgmの再生
        chainSource.volume = 0.1f;
        bgmSource.clip = bgm;
        bgmSource.volume = 0.1f;
        bgmSource.loop = true;
        bgmSource.Play();
	}
	
    public void CallBoise(string type) {
        if (type == "Olga") boiseSource.clip = olga;
        else if (type == "Wish") boiseSource.clip = wish;
        else boiseSource.clip = null;
        boiseSource.Play();
    }

    public void CallChain(int num) {
        if (num > 7) num = 7;
        chainSource.clip = chain[num];
        chainSource.Play();
    }

}
