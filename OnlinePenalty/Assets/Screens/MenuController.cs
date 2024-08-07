using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
namespace OnlinePenalty
{
    public class MenuController : MonoBehaviour
    {
        public void TapToSinglePlayerButton()
        {
            SceneManager.LoadScene("Game");
        }
        public void TapToMultiplePlayerButton()
        {
            SceneManager.LoadScene("LoadingScreen");
        }

    }
}
