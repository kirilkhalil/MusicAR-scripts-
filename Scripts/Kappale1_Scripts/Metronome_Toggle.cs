using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*=============================================================================
 Copyright 2019 Kiril Shenouda Khalil
 All added functionality has been solely designed and implemented by the 
 copyright owner.
==============================================================================*/

public class Metronome_Toggle : MonoBehaviour { //Skripti joka ajetaan, kun painetaan "Metronomi"-näppäintä UI:sta.

    public GameObject Metronome;
    public AudioSource Metronome_AudioSource;

    public void toggleMetronome()
    {
        if (Metronome_AudioSource.volume == 1.0f)
        {
            Metronome_AudioSource.volume = 0.0f;
        }
        else
        {
            Metronome_AudioSource.volume = 1.0f;
        }
    }
}
