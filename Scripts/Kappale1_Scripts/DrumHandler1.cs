using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*=============================================================================
 Copyright 2019 Kiril Shenouda Khalil
 All added functionality has been solely designed and implemented by the 
 copyright owner.
==============================================================================*/

public class DrumHandler1 : MonoBehaviour {

    public GameObject AudioHandler;

    // Use this for initialization
    void Start () {

        var BSL = AudioHandler.GetComponents<AudioSource>();

        AudioSource Guitar = new AudioSource();
        Guitar = BSL[2];
        Guitar.Play();
        Guitar.volume = 0.0f;
    }
	
	// Update is called once per frame
	void Update () {

    }
}
