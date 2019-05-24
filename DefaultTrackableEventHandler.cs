/*==============================================================================
Copyright (c) 2017 PTC Inc. All Rights Reserved.

Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/

/*=============================================================================
 Copyright 2019 Kiril Shenouda Khalil
 All added functionality has been solely designed and implemented by the 
 copyright owner.
==============================================================================*/

using UnityEngine;
using Vuforia;
using UnityEngine.UI;

/// <summary>
///     A custom handler that implements the ITrackableEventHandler interface.
/// </summary>
public class DefaultTrackableEventHandler : MonoBehaviour, ITrackableEventHandler
{
    /// <summary>
    /// T‰ss‰ prototyypiss‰ asetettu 7 ‰‰niraidan max. limit. Huomioi Audiosourcen j‰rjestys, sill‰ audiohandlerill‰ on sama m‰‰r‰ sourceja ja kyseisen kappaleen instrumentit on j‰rjestetty samalla tavalla.
    /// T‰ten jos halutaan kutsua esim. Drums audiosourcea on se Audiohandlerin viides elementti eli indeksiss‰ [4]
    /// </summary>

    private bool MetronomeOn = false; //Bool, jolla voidaan seurata oliko metronomi p‰‰ll‰, kun tunnistettiin ensimm‰inen markkeri.
    private GameObject Metronome;
    private AudioSource Metronome_AudioSource;
    public GameObject AudioHandler; //Haetaan AudioHandler objekti muistiin
    private AudioSource LeadGuitar = new AudioSource(); //M‰‰ritell‰‰n AudioSourcet, niin ei tarvitse hakea TrackingFound ja -Lost erikseen
    private AudioSource Bass = new AudioSource();
    private AudioSource Piano = new AudioSource();
    private AudioSource Vocal = new AudioSource(); //K‰ytet‰‰n t‰ll‰ hetkell‰ kappale1 skeness‰ myˆs "noise" audiotrackin markkerina. Mik‰li jatkokehitet‰‰n tai saadaan lis‰‰ aikaa voidaan tehd‰ monitahoisempia markkereita ja laajentaa sis‰ltˆ‰. 
    private AudioSource Drums = new AudioSource();
    private AudioSource BackingVocal = new AudioSource(); //K‰ytet‰‰n kappale1 skeness‰, myˆs syna2 audiotrackin markkerina.
    //private AudioSource RhythmGuitar = new AudioSource();
    private static AudioSource[] ASL; //Tyydytty protossa tekem‰‰n staattinen muuttuja, jonka avulla ei tarvitse hakea AudioSource objekteja jokaisessa metodissa erikseen, sek‰ helpotettu raitojen soittomekaniikan hallintaa huomattavasti.
    private static bool FirstTimeFlag; //Tarkistetaan onko kyseisen skenen ajokerran aikana viel‰ tunnistettu yht‰‰n markkeria vai ei.
    private static Button Metronome_Button; //Jotta voidaan ottaa metronomi-napista kiinni ja muokata sen ominaisuuksia tarpeen tullen.
    private static int TrackingState; //K‰ytet‰‰n countterina omalla toiminnallisuudelle, jolla tarkistetaan onko kaikkien markkereiden trackaus loppunut.

    #region PROTECTED_MEMBER_VARIABLES

    protected TrackableBehaviour mTrackableBehaviour;

    #endregion // PROTECTED_MEMBER_VARIABLES

    #region UNITY_MONOBEHAVIOUR_METHODS

    protected virtual void Start()
    {
        FirstTimeFlag = true; //Asetetaan k‰ynnistyksen yhteydess‰, jotta tiedet‰‰n ett‰ ei ole viel‰ tunnistettu mit‰‰n markkereita.
        ASL = AudioHandler.GetComponents<AudioSource>(); //K‰yd‰‰n hakemassa AudioHandlerin elementit taulukkoon ja m‰‰ritell‰‰n niille paikat, jota aina k‰ytet‰‰n. Mik‰li haluaa jatkokehitt‰‰n, niin lis‰‰ vaan AudioSourceja sen mukaan kun tarvii soittimia yms.
        LeadGuitar = ASL[0];
        Bass = ASL[1];
        Piano = ASL[2];
        Vocal = ASL[3];
        Drums = ASL[4];
        BackingVocal = ASL[5];
        //RhythmGuitar = ASL[6];
        Metronome = GameObject.Find("Metronome"); //Haetaan metronomi, jotta voidaan resettaa se tarvittaessa
        Metronome_AudioSource = Metronome.GetComponent<AudioSource>(); //Haetaan metronomin AudioSource, jotta voidaan tarkistaa mik‰ oli metrononomin tila, kun markkerin tunnistus tapahtuu.
        Metronome_Button = GameObject.Find("Metronomi_nappi").GetComponent<Button>(); //Haetaan skeneist‰ lˆytyv‰ metronomi-nappi muuttujaan.
        //Debug.Log("TrackingStaten arvo on " + TrackingState);

        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour)
            mTrackableBehaviour.RegisterTrackableEventHandler(this);
    }

    protected virtual void OnDestroy()
    {
        if (mTrackableBehaviour)
            mTrackableBehaviour.UnregisterTrackableEventHandler(this);
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS

    #region PUBLIC_METHODS

    /// <summary>
    ///     Implementation of the ITrackableEventHandler function called when the
    ///     tracking state changes.
    /// </summary>
    public void OnTrackableStateChanged(
        TrackableBehaviour.Status previousStatus,
        TrackableBehaviour.Status newStatus)
    {
        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED ||
            newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " found");
            OnTrackingFound();
        }
        else if (previousStatus == TrackableBehaviour.Status.TRACKED &&
                 newStatus == TrackableBehaviour.Status.NOT_FOUND)
        {
            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " lost");
            OnTrackingLost();
        }
        else
        {
            // For combo of previousStatus=UNKNOWN + newStatus=UNKNOWN|NOT_FOUND
            // Vuforia is starting, but tracking has not been lost or found yet
            // Call OnTrackingLost() to hide the augmentations
            OnTrackingLost();
        }
    }

    #endregion // PUBLIC_METHODS

    #region PROTECTED_METHODS

    protected virtual void OnTrackingFound() //Tehty omat tunnistukseen liittyv‰t koodit t‰nne. Markkerin lˆydetty‰ ruvetaan soittamaan siihen sidottua audioraitaa. Tarkoituksena lopuksi olla esim. kitaran kuva markkerina, ja t‰m‰ triggerˆi kitaran raidan
    {
        Metronome_Button.interactable = false; //Estet‰‰n metronomi-napin painaminen, kun on tunnistettu markkeri ja muutetaan se harmaaksi.
        if (FirstTimeFlag == true) //Tarkistetaan onko ensimm‰inen kerta t‰ss‰ ajossa, jolloin tunnistetaan markkeri ja jos on niin k‰yd‰‰n silmukasssa kaikki AudioSourcet l‰pi ja laitetaan ne k‰yntiin vasta t‰ss‰ vaiheessa.
        {
            TrackingState = 0; //Alustus pit‰‰ tehd‰ t‰‰ll‰, miss‰ tarkistetaan, ett‰ onko kyseess‰ ensimm‰inen tunnistettu markkeri t‰ss‰ ajossa. T‰m‰ johtuu siit‰, ett‰ Vuforia aloitusrutiinissaan k‰y l‰pi kaikki skeness‰ olevat markkerit ja toteuttaa ainakin onTrackingLost(); metodin. T‰m‰ tarkoittaa k‰yt‰nnnˆss‰, ett‰ mik‰li alustuksen tekisi Start();-lohkossa, niin pit‰isi jokaisella skenell‰ olla oma kontrolleri joka alustaa TrackingState-muuttujan markkereiden lukum‰‰r‰ksi + 1 (eli esim. 7 kappale 2 kohdalla), jotta se alustuisi 0:ksi.
            for (int i = 0; i < ASL.Length; i++)
            {
                ASL[i].Play();
                ASL[i].volume = 0.0f;
            }
            /*Metronome.SetActive(true); //K‰ynnistet‰‰n uudestaan metronomi.
            Metronome_AudioSource.Play(); //K‰ynnistet‰‰n AudioSource erikseen.
            if (MetronomeOn == true) //Jos metrnomi oli k‰ytˆss‰ ennen resetti‰ asetetaan se kuuluviin taas.
            {
                Metronome_AudioSource.volume = 1.0f;
            }*/

            FirstTimeFlag = false; //Ollaan kerran jo tunnistettu markkeri skeness‰.
        }
        if (Metronome_AudioSource.volume == 1.0f && TrackingState == 0) //Otetaan kiinni tieto siit‰ oliko metronomi p‰‰ll‰ vai ei ennen kuin komponentti resetoidaan, jotta voidaan palauttaa se alkuper‰iseen tilaan.
        {
            MetronomeOn = true;
        }
        else if (Metronome_AudioSource.volume == 0.0f && TrackingState == 0)
        {
            MetronomeOn = false;
        }
        Metronome_AudioSource.volume = 0.0f; //Hiljennet‰‰n komponentti, kun markkeri tunnistetaan
        //Metronome_AudioSource.Stop(); //Ilmeisesti vaikka komponentti disabloidaan niin sen lapsena olevan audiosource pit‰‰ stopata viel‰ erikseen tai tahditus ei toimi kunnolla.
        if (mTrackableBehaviour.TrackableName == "bass_marker")
        {
            Bass.volume = 1.0f;
            TrackingState++;
        }
        if (mTrackableBehaviour.TrackableName == "microphone_marker")
        {
            Vocal.volume = 1.0f;
            TrackingState++;
        }
        if (mTrackableBehaviour.TrackableName == "choir_marker")
        {
            BackingVocal.volume = 1.0f;
            TrackingState++;
        }
        if (mTrackableBehaviour.TrackableName == "guitar_marker")
        {
            LeadGuitar.volume = 1.0f;
            TrackingState++;
        }
        if (mTrackableBehaviour.TrackableName == "piano_marker")
        {
            Piano.volume = 1.0f;
            TrackingState++;
            //Debug.Log(TrackingState);
        }
        if (mTrackableBehaviour.TrackableName == "drum_marker")
        {
            Drums.volume = 1.0f;
            TrackingState++;
        }

        var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var colliderComponents = GetComponentsInChildren<Collider>(true);
        var canvasComponents = GetComponentsInChildren<Canvas>(true);

        // Enable rendering:
        foreach (var component in rendererComponents)
            component.enabled = true;

        // Enable colliders:
        foreach (var component in colliderComponents)
            component.enabled = true;

        // Enable canvas':
        foreach (var component in canvasComponents)
            component.enabled = true;
    }

    protected virtual void OnTrackingLost() //Sama kuin yll‰ olevat "OnTrackingFound", mutta toiseen suuntaan eli kun menetet‰‰n markkeriin n‰kˆyhteys lopetetaan siihen liittyv‰n soittimen raidan soitto.
    {
        if (mTrackableBehaviour.TrackableName == "bass_marker")
        {
            Bass.volume = 0.0f;
            TrackingState--;
        }
        if (mTrackableBehaviour.TrackableName == "microphone_marker")
        {
            Vocal.volume = 0.0f;
            TrackingState--;
            //Debug.Log(TrackingState);
        }
        if (mTrackableBehaviour.TrackableName == "choir_marker")
        {
            BackingVocal.volume = 0.0f;
            TrackingState--;
            //Debug.Log(TrackingState);
        }
        if (mTrackableBehaviour.TrackableName == "guitar_marker")
        {
            LeadGuitar.volume = 0.0f;
            TrackingState--;
        }
        if (mTrackableBehaviour.TrackableName == "piano_marker")
        {
            Piano.volume = 0.0f;
            TrackingState--;
        }
        if (mTrackableBehaviour.TrackableName == "drum_marker")
        {
            Drums.volume = 0.0f;
            TrackingState--;
        }

        if (MetronomeOn == true && TrackingState == 0) //Jos metronomi oli k‰ytˆss‰ ennen resetti‰ asetetaan se kuuluviin taas.
        {
            //Metronome.SetActive(true); //K‰ynnistet‰‰n uudestaan metronomi.
            //Metronome_AudioSource.Play(); //K‰ynnistet‰‰n AudioSource erikseen.
            Metronome_AudioSource.volume = 1.0f; //Jos metronomi oli p‰‰ll‰ ennen kuin markkeri tunnistettiin, pistet‰‰n metronomi takaisin p‰‰lle
        }
        if (TrackingState == 0)
        {
            Metronome_Button.interactable = true;
        }

        var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var colliderComponents = GetComponentsInChildren<Collider>(true);
        var canvasComponents = GetComponentsInChildren<Canvas>(true);

        // Disable rendering:
        foreach (var component in rendererComponents)
            component.enabled = false;

        // Disable colliders:
        foreach (var component in colliderComponents)
            component.enabled = false;

        // Disable canvas':
        foreach (var component in canvasComponents)
            component.enabled = false;
    }

    #endregion // PROTECTED_METHODS
}
