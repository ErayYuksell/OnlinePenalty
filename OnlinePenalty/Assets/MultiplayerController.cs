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
        int _player1Score = 0;
        int _player2Score = 0;
        bool _isMultiplayer = false;
        bool _isPlayer1Turn = false;
        bool _isPlayer2Turn = false;
        bool _isPlayer1ButtonDone = false;
        bool _isPlayer2ButtonDone = false;

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
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine); // Önceki coroutine'i durdur
            }
            countdown = 10; // Her baþlatýldýðýnda countdown deðerini sýfýrla
            countdownCoroutine = StartCoroutine(countdownTimer());
        }

        public void StopCountdownTimer()
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
                countdownText.text = countdown.ToString();
                yield return new WaitForSeconds(1);
                countdown--;
            }
            countdownText.text = "0"; // Sayaç bittiðinde 0 olarak güncelle
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
                if (_isPlayer1Turn)
                {
                    _player1Score++;
                }
                else if (_isPlayer2Turn)
                {
                    _player2Score++;
                }
                SaveScore();
            }
            else
            {
                score++;
                scoreText.text = score.ToString();
                SaveScore(); // Skoru kaydet
            }
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
        public bool IsPlayer1Turn()
        {
            return _isPlayer1Turn;
        }
        public bool IsPlayer2Turn()
        {
            return _isPlayer2Turn;
        }

        public void SetInitialTurn()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _isPlayer1Turn = true;
                SetTurn();
            }
            else
            {
                _isPlayer2Turn = true;
                SetTurn();
            }
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

            SetTurn();
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
                SoccerPlayerController.Instance.MultiplayerStartShooting();
                GoalkeeperController.Instance.StartSaving();
                Debug.Log("Playing shoot and saving");
            }
        }
        #endregion

        private void OnApplicationQuit()
        {
            PlayerPrefs.DeleteKey("Score"); // Oyunu kapatýrken skoru sýfýrla
        }
    }
}
