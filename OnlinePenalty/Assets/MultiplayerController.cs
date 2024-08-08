using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        bool _isPlayer1Turn = false;
        bool _isPlayer2Turn = false;
        bool _isPlayer1ButtonDone = false;
        bool _isPlayer2ButtonDone = false;
        bool whoTapToButton = false;

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
                if (_isPlayer1Turn && GetWhoTapToButton())
                {
                    _player1Score++;
                    Debug.Log("Player1Score: " + _player1Score);
                }
                else if (_isPlayer2Turn && GetWhoTapToButton())
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

        public void WhoTapToButton(bool value)
        {
            whoTapToButton = value;
        }
        public bool GetWhoTapToButton()
        {
            return whoTapToButton;
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
                if (_isPlayer1Turn)
                {
                    PlayerPrefs.SetInt("Player1Score", _player1Score);

                }
                else if (_isPlayer2Turn)
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
            if (PhotonNetwork.IsMasterClient && !PlayerPrefs.HasKey("IsPlayer1Turn"))
            {
                _isPlayer1Turn = true;
                SetTurn();
                return;
            }
            else if (!PhotonNetwork.IsMasterClient && !PlayerPrefs.HasKey("IsPlayer2Turn"))
            {
                _isPlayer2Turn = true;
                SetTurn();
                return;
            }

            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                _isPlayer1Turn = PlayerPrefs.GetInt("IsPlayer1Turn") == 1 ? true : false;
                _isPlayer2Turn = PlayerPrefs.GetInt("IsPlayer2Turn") == 1 ? true : false;
                Debug.Log("IsPlayer1Turn: " + PlayerPrefs.GetInt("IsPlayer1Turn"));
                Debug.Log("IsPlayer2Turn: " + PlayerPrefs.GetInt("IsPlayer2Turn"));
            }
            else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                _isPlayer1Turn = PlayerPrefs.GetInt("IsPlayer1Turn_2") == 1 ? true : false;
                _isPlayer2Turn = PlayerPrefs.GetInt("IsPlayer2Turn_2") == 1 ? true : false;
                Debug.Log("IsPlayer1Turn_2: " + PlayerPrefs.GetInt("IsPlayer1Turn_2"));
                Debug.Log("IsPlayer2Turn_2: " + PlayerPrefs.GetInt("IsPlayer2Turn_2"));
            }

            SetTurn();
        }

        public void SetTurn()
        {
            if (_isPlayer1Turn)
            {
                UIManager.Instance.Player1Panels();
            }
            else if (_isPlayer2Turn)
            {
                UIManager.Instance.Player2Panels();
            }
        }

        public void SwitchTurn()
        {
            _isPlayer1Turn = !_isPlayer1Turn;
            _isPlayer2Turn = !_isPlayer2Turn;

            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                PlayerPrefs.SetInt("IsPlayer1Turn", _isPlayer1Turn ? 1 : 0);
                PlayerPrefs.SetInt("IsPlayer2Turn", _isPlayer2Turn ? 1 : 0);
                Debug.Log("Player1Turn: " + _isPlayer1Turn);
                Debug.Log("Player2Turn: " + _isPlayer2Turn);
            }
            else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                PlayerPrefs.SetInt("IsPlayer1Turn_2", _isPlayer1Turn ? 1 : 0);
                PlayerPrefs.SetInt("IsPlayer2Turn_2", _isPlayer2Turn ? 1 : 0);
                Debug.Log("Player1Turn_2: " + _isPlayer1Turn);
                Debug.Log("Player2Turn_2: " + _isPlayer2Turn);
            }
        }

        public bool IsPlayer1Turn()
        {
            return _isPlayer1Turn;
        }
        public bool IsPlayer2Turn()
        {
            return _isPlayer2Turn;
        }
        #endregion

        #region Player1 and Player2 Tap to button
        public void IsPlayer1ButtonDone()
        {
            _isPlayer1ButtonDone = true;
            photonView.RPC("PunRPC_IsPlayer1ButtonDone", RpcTarget.All);
        }

        [PunRPC]
        void PunRPC_IsPlayer1ButtonDone()
        {
            _isPlayer1ButtonDone = true;
            StartShootAndSaving();
        }

        public void IsPlayer2ButtonDone()
        {
            _isPlayer2ButtonDone = true;
            photonView.RPC("PunRPC_IsPlayer2ButtonDone", RpcTarget.All);
        }

        [PunRPC]
        void PunRPC_IsPlayer2ButtonDone()
        {
            _isPlayer2ButtonDone = true;
            StartShootAndSaving();
        }

        public void StartShootAndSaving()
        {
            photonView.RPC("PunRPC_StartShootAndSaving", RpcTarget.All);
        }

        [PunRPC]
        void PunRPC_StartShootAndSaving()
        {
            if (_isPlayer1ButtonDone && _isPlayer2ButtonDone)
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
