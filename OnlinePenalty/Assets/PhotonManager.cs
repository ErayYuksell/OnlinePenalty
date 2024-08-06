using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace OnlinePenalty
{
    public class PhotonManager : MonoBehaviour
    {
        public static PhotonManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartConnection()
        {
            
        }
    }
}
