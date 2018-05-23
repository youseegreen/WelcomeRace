using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public AudioClip basilisk;
    public AudioClip olga;
    public AudioClip wish;
    public AudioClip bgm;
    public AudioClip[] chain;
    private AudioSource chainSource;
    private AudioSource bgmSource;
    private AudioSource basiliskSource;
    private AudioSource boiseSource;

	// Use this for initialization
	void Start () {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        bgmSource = audioSources[0];
        chainSource = audioSources[1];
        boiseSource = audioSources[2];
        basiliskSource = audioSources[3];

        //bgmの再生
        chainSource.volume = 0.7f;
        bgmSource.clip = bgm;
        bgmSource.volume = 0.7f;
        bgmSource.loop = true;
        bgmSource.Play();
        basiliskSource.clip = basilisk;
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

    public void CallBasilisk(bool frag) {
        if (frag) {
            //バジリスクタイム
            bgmSource.mute = true;
            basiliskSource.time = 0.0f;
            basiliskSource.mute = false;
            basiliskSource.Play();
        }
        else {
            //元に戻す
            bgmSource.mute = false;
            //    basiliskSource.time = 0.0f;
            basiliskSource.mute = true;
        }
    }
}
