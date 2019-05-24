using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*=============================================================================
 Copyright 2019 Kiril Shenouda Khalil
 All added functionality has been solely designed and implemented by the 
 copyright owner.
==============================================================================*/

public class Sample_Player : MonoBehaviour {

    public GameObject AudioHandler; //Haetaan AudioHandler objekti muistiin
    private AudioSource Guitar = new AudioSource(); //Määritellään AudioSourcet, niin ei tarvitse hakea TrackingFound ja -Lost erikseen
    private AudioSource Bass = new AudioSource();
    private AudioSource Piano = new AudioSource();
    private AudioSource Vocal = new AudioSource();
    private AudioSource Drums = new AudioSource();
    private AudioSource BackingVocal = new AudioSource();
    //private AudioSource RhythmGuitar = new AudioSource();
    private static AudioSource[] ASL;
    private static Button Metronome_Button; //Jotta voidaan ottaa metronomi-napista kiinni ja muokata sen ominaisuuksia tarpeen tullen.
    private GameObject Metronome;
    private AudioSource Metronome_AudioSource;
    private bool MetronomeOn = false; //Bool, jolla voidaan seurata oliko metronomi päällä.

    private void Start()
    {
        Metronome_Button = GameObject.Find("Metronomi_nappi").GetComponent<Button>(); //Haetaan skeneistä löytyvä metronomi-nappi muuttujaan.
        Metronome = GameObject.Find("Metronome"); //Haetaan metronomi, jotta voidaan resettaa se tarvittaessa
        Metronome_AudioSource = Metronome.GetComponent<AudioSource>(); //Haetaan metronomin AudioSource, jotta voidaan tarkistaa mikä oli metrononomin tila, kun markkerin tunnistus tapahtuu.
    }

    public void playSample() //Haetaan kyseisen kappaleen eri soittimien raidat ja soitetaan niistä 5s "sample" napin painalluksesta
    {
        Metronome_Button.interactable = false; //Estetään metronomi-napin painaminen, kun on tunnistettu markkeri ja muutetaan se harmaaksi.
        if (Metronome_AudioSource.volume == 1.0f) //Otetaan kiinni tieto siitä oliko metronomi päällä vai ei ennen kuin komponentti resetoidaan, jotta voidaan palauttaa se alkuperäiseen tilaan.
        {
            MetronomeOn = true;
        }
        else
        {
            MetronomeOn = false;
        }
        Metronome_AudioSource.volume = 0.0f; //Hiljennetään komponentti, kun markkeri tunnistetaan

        //Debug.Log("Ollaanko playSamplessä");
        ASL = AudioHandler.GetComponents<AudioSource>();
        Guitar = ASL[0];
        Bass = ASL[1];
        Piano = ASL[2];
        Vocal = ASL[3];
        Drums = ASL[4];
        BackingVocal = ASL[5];
        //RhythmGuitar = ASL[6];

        for (int i = 0; i < ASL.Length; i++)
        {
            ASL[i].Play();
            ASL[i].volume = 1.0f;
        }
        StartCoroutine(sampleDelay());
       
    }

    IEnumerator sampleDelay() //WaitForSeconds()-arvoa muuttamalla määritellään kuinka kauan sample soi napin painalluksesta.
    {
        yield return new WaitForSeconds(20);
        for (int i = 0; i < ASL.Length; i++)
        {
            ASL[i].volume = 0.0f;
            ASL[i].Stop();
        }
        Metronome_Button.interactable = true;
        if (MetronomeOn == true) //Jos metronomi oli käytössä ennen resettiä asetetaan se kuuluviin taas.
        {
            Metronome_AudioSource.volume = 1.0f; //Jos metronomi oli päällä ennen kuin markkeri tunnistettiin, pistetään metronomi takaisin päälle
        }
    }
}
