using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*=============================================================================
 Copyright 2019 Kiril Shenouda Khalil
 All added functionality has been solely designed and implemented by the 
 copyright owner.
==============================================================================*/

public class Lataa_Kappale1 : MonoBehaviour { //Tämä skripti kiinni "Kappale1_Button"-napissa, jota painamalla kutsutaan tätä luokkaa

    public void LataaKappale1()
    {
        SceneManager.LoadScene("MusicAR");
    }
}
