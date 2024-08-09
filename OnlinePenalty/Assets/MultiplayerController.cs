using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace OnlinePenalty
{
    public class MultiplayerController : MonoBehaviour
    {
        public static MultiplayerController Instance;

        [Header("Countdown")]
        [SerializeField] TextMeshProUGUI countdownText;
        int countdown = 10; // Baþlangýç deðerini 10 olarak ayarlayýn
        Coroutine countdownCoroutine;
        [Header("Score")]
        [SerializeField] TextMeshProUGUI scoreText;
        int score = 0;
        [Header("Multiplayer")]
        [SerializeField] TextMeshProUGUI player1Text;
        [SerializeField] TextMeshProUGUI player2Text;
        [SerializeField] TextMeshProUGUI multiplayerCountdownText;
        int _player1Score = 0;
        int _player2Score = 0;
        bool _isMultiplayer = false;
        bool _isPlayerShooting = false;
        bool _isPlayerControllingGoalkeeper = false;
        bool _isPlayerShootingButtonDone = false;
        bool _isPlayerControllingGoalkeeperButtonDone = false;

     


        GameManager gameManager;
        PhotonView photonView;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                photonView = GetComponent<PhotonView>();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            gameManager = GameManager.Instance;

            IsConnected();
            StartCountdownTimer();
            LoadScore();
            SetInitialTurn();
        }

        #region Countdown
        public void StartCountdownTimer()
        {
            if (GetMultiplayerMode() && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("PunRPC_StartCountdownTimer", RpcTarget.All);
            }
            else if (!GetMultiplayerMode())
            {
                StartLocalCountdownTimer();
            }
        }

        [PunRPC]
        public void PunRPC_StartCountdownTimer()
        {
            StartLocalCountdownTimer();
        }

        private void StartLocalCountdownTimer()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine); // Önceki coroutine'i durdur
            }
            countdown = 10; // Her baþlatýldýðýnda countdown deðerini sýfýrla
            countdownCoroutine = StartCoroutine(countdownTimer());
        }

        public void StopCountdownTimer()
        {
            if (GetMultiplayerMode() && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("PunRPC_StopCountdownTimer", RpcTarget.All);
            }
            else if (!GetMultiplayerMode())
            {
                StopLocalCountdownTimer();
            }
        }

        [PunRPC]
        public void PunRPC_StopCountdownTimer()
        {
            StopLocalCountdownTimer();
        }

        private void StopLocalCountdownTimer()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
        }

        IEnumerator countdownTimer()
        {
            while (countdown > 0)
            {
                if (GetMultiplayerMode())
                {
                    multiplayerCountdownText.text = countdown.ToString();
                }
                else
                {
                    countdownText.text = countdown.ToString();
                }
                yield return new WaitForSeconds(1);
                countdown--;
            }
            if (GetMultiplayerMode())
            {
                multiplayerCountdownText.text = "0"; // Sayaç bittiðinde 0 olarak güncelle
            }
            else
            {
                countdownText.text = "0"; // Sayaç bittiðinde 0 olarak güncelle
            }
            if (countdown == 0)
            {
                gameManager.targetMovement.StopTargetMovement();
                gameManager.shootColorSelection.StopArrowMovement();
                UIManager.Instance.CloseShootButton();
                UIManager.Instance.OpenFailCanvas();
            }
        }

        #endregion

        #region Score
        public void UpdateScore()
        {
            if (GetMultiplayerMode())
            {
                if (PhotonNetwork.LocalPlayer.ActorNumber == 1 && _isPlayerShooting)
                {
                    _player1Score++;
                    Debug.Log("Player1Score: " + _player1Score);
                }
                else if (PhotonNetwork.LocalPlayer.ActorNumber == 2 && _isPlayerShooting)
                {
                    _player2Score++;
                    Debug.Log("Player2Score: " + _player2Score);
                }

                SaveScore();
            }
            else
            {
                score++;
                scoreText.text = score.ToString();
                SaveScore(); // Skoru kaydet
            }

            SwitchTurn();
        }

        [PunRPC]
        void PunRPC_UpdateScore()
        {
            player1Text.text = _player1Score.ToString();
            player2Text.text = _player2Score.ToString();
        }

        void SaveScore()
        {
            if (GetMultiplayerMode())
            {
                if (PhotonNetwork.LocalPlayer.ActorNumber == 1 && _isPlayerShooting)
                {
                    PlayerPrefs.SetInt("Player1Score", _player1Score);

                }
                else if (PhotonNetwork.LocalPlayer.ActorNumber == 2 && _isPlayerShooting)
                {
                    PlayerPrefs.SetInt("Player2Score", _player2Score);
                }
                PlayerPrefs.Save();
            }
            else
            {
                PlayerPrefs.SetInt("Score", score); // Skoru kaydet
                PlayerPrefs.Save();
            }
        }

        public void LoadScore()
        {
            if (PlayerPrefs.HasKey("Score") && !GetMultiplayerMode())
            {
                score = PlayerPrefs.GetInt("Score"); // Skoru yükle
                scoreText.text = score.ToString();
            }
            else if (GetMultiplayerMode())
            {
                _player1Score = PlayerPrefs.GetInt("Player1Score");
                _player2Score = PlayerPrefs.GetInt("Player2Score");

                photonView.RPC("PunRPC_UpdateScore", RpcTarget.All);
            }
        }
        #endregion

        #region MultiplayerConnection
        public void SetMultiplayerMode(bool value)
        {
            _isMultiplayer = value;
        }
        public bool GetMultiplayerMode()
        {
            return _isMultiplayer;
        }

        public void IsConnected()
        {
            if (PhotonNetwork.IsConnected)
            {
                SetMultiplayerMode(true);
                UIManager.Instance.MultiplayerResultCanvas();
            }
        }
        #endregion

        #region PlayerTurn
        public void SetInitialTurn()
        {
            if (PhotonNetwork.IsMasterClient && !PlayerPrefs.HasKey("_isPlayerShooting"))
            {
                _isPlayerShooting = true;
                SetTurn();
                return;
            }
            else if (!PhotonNetwork.IsMasterClient && !PlayerPrefs.HasKey("_isPlayerControllingGoalkeeper"))
            {
                _isPlayerControllingGoalkeeper = true;
                SetTurn();
                return;
            }

            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                _isPlayerShooting = PlayerPrefs.GetInt("_isPlayerShooting") == 1 ? true : false;
                _isPlayerControllingGoalkeeper = PlayerPrefs.GetInt("_isPlayerControllingGoalkeeper") == 1 ? true : false;
                //Debug.Log("IsPlayer1Turn: " + PlayerPrefs.GetInt("IsPlayer1Turn"));
                //Debug.Log("IsPlayer2Turn: " + PlayerPrefs.GetInt("IsPlayer2Turn"));
            }
            else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                _isPlayerShooting = PlayerPrefs.GetInt("_isPlayerShooting_2") == 1 ? true : false;
                _isPlayerControllingGoalkeeper = PlayerPrefs.GetInt("_isPlayerControllingGoalkeeper_2") == 1 ? true : false;
                //Debug.Log("IsPlayer1Turn_2: " + PlayerPrefs.GetInt("IsPlayer1Turn_2"));
                //Debug.Log("IsPlayer2Turn_2: " + PlayerPrefs.GetInt("IsPlayer2Turn_2"));
            }

            SetTurn();
        }

        public void SetTurn()
        {
            if (_isPlayerShooting)
            {
                UIManager.Instance.Player1Panels();
            }
            else if (_isPlayerControllingGoalkeeper)
            {
                UIManager.Instance.Player2Panels();
            }
        }

        public void SwitchTurn()
        {
            _isPlayerShooting = !_isPlayerShooting;
            _isPlayerControllingGoalkeeper = !_isPlayerControllingGoalkeeper;

            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                PlayerPrefs.SetInt("_isPlayerShooting", _isPlayerShooting ? 1 : 0);
                PlayerPrefs.SetInt("_isPlayerControllingGoalkeeper", _isPlayerControllingGoalkeeper ? 1 : 0);
                //Debug.Log("Player1Turn: " + _isPlayerShooting);
                //Debug.Log("Player2Turn: " + _isPlayerControllingGoalkeeper);
            }
            else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                PlayerPrefs.SetInt("_isPlayerShooting_2", _isPlayerShooting ? 1 : 0);
                PlayerPrefs.SetInt("_isPlayerControllingGoalkeeper_2", _isPlayerControllingGoalkeeper ? 1 : 0);
                //Debug.Log("Player1Turn_2: " + _isPlayerShooting);
                //Debug.Log("Player2Turn_2: " + _isPlayerControllingGoalkeeper);
            }
        }

        public bool IsPlayerShooting()
        {
            return _isPlayerShooting;
        }
        public bool IsPlayerControllingGoalkeeper()
        {
            return _isPlayerControllingGoalkeeper;
        }
        #endregion

        #region Player1 and Player2 Tap to button

        public void IsPlayerShootingButtonDone()
        {
            _isPlayerShootingButtonDone = true;
            photonView.RPC("PunRPC_IsPlayerShootingButtonDone", RpcTarget.All);
        }

        [PunRPC]
        void PunRPC_IsPlayerShootingButtonDone()
        {
            _isPlayerShootingButtonDone = true;
            StartShootAndSaving();
        }
        public void IsPlayerControllingGoalkeeperButtonDone()
        {
            _isPlayerControllingGoalkeeperButtonDone = true;
            photonView.RPC("PunRPC_IsPlayerControllingGoalkeeperButtonDone", RpcTarget.All);
        }

        [PunRPC]
        void PunRPC_IsPlayerControllingGoalkeeperButtonDone()
        {
            _isPlayerControllingGoalkeeperButtonDone = true;
            StartShootAndSaving();
        }

        public void StartShootAndSaving()
        {
            photonView.RPC("PunRPC_StartShootAndSaving", RpcTarget.All);
        }

        [PunRPC]
        void PunRPC_StartShootAndSaving()
        {
            if (_isPlayerShootingButtonDone && _isPlayerControllingGoalkeeperButtonDone)
            {
                StopCountdownTimer();
                SoccerPlayerController.Instance.MultiplayerStartShooting();
                Debug.Log("Playing shoot and saving");
            }
        }
        #endregion

        private void OnApplicationQuit()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
