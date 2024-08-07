using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace OnlinePenalty
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public TargetMovement targetMovement;
        public ShootColorSelection shootColorSelection;
        public SingleAndMultiplayerOptions singleAndMultiplayerOptions;


        private void Awake()
        {
            targetMovement.Init(this);
            shootColorSelection.Init(this);
            singleAndMultiplayerOptions.Init(this);

            if (Instance == null)
            {
                //DontDestroyOnLoad(gameObject);
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            targetMovement.ChooseRandomPoint();
            shootColorSelection.MovementArrow();

            singleAndMultiplayerOptions.StartCountdownTimer();
            singleAndMultiplayerOptions.LoadScore();
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.DeleteKey("Score"); // Oyunu kapatırken skoru sıfırla
        }

        [Serializable]
        public class TargetMovement
        {
            GameManager gameManager;
            public List<GameObject> shootPoints = new List<GameObject>();
            public List<GameObject> failShootPoints = new List<GameObject>();
            public Transform targetObj;
            private Tween targetTween;
            public void Init(GameManager gameManager)
            {
                this.gameManager = gameManager;
            }

            public void ChooseRandomPoint(GameObject oldPoint1 = null, GameObject oldPoint2 = null)
            {
                if (shootPoints.Count < 2)
                {
                    Debug.LogError("Not enough shoot points.");
                    return;
                }

                GameObject point1, point2;
                do
                {
                    point1 = shootPoints[UnityEngine.Random.Range(0, shootPoints.Count)];
                    point2 = shootPoints[UnityEngine.Random.Range(0, shootPoints.Count)];
                } while ((oldPoint1 != null && (point1 == oldPoint1 || point2 == oldPoint1)) ||
                         (oldPoint2 != null && (point1 == oldPoint2 || point2 == oldPoint2)) ||
                         (point1 == point2));

                MovementBetweenPoints(point1, point2);
            }

            public void MovementBetweenPoints(GameObject point1, GameObject point2)
            {
                targetTween?.Kill(); // Mevcut hareketi durdur

                Sequence sequence = DOTween.Sequence(); // dotween sirasi veya dizisi birden fazla dotween i birlikte kullanmak istiyorsan sirayla calisirlar 
                targetTween = sequence.Append(targetObj.DOMove(point1.transform.position, 1f).SetEase(Ease.Linear))
                        .Append(targetObj.DOMove(point2.transform.position, 1f).SetEase(Ease.Linear))
                        .OnComplete(() =>
                        {
                            ChooseRandomPoint(point1, point2); // Hareket tamamland���nda yeni iki nokta se�ilir ve hareket tekrar ba�lar
                        });
            }

            public Vector3 StopTargetMovement()
            {
                targetTween?.Kill(); // targetImage hareketini durdur
                targetObj.gameObject.SetActive(false);
                return targetObj.position; // O anki pozisyon bilgisini al
            }
        }

        [Serializable]
        public class ShootColorSelection
        {
            GameManager gameManager;

            [SerializeField] RectTransform Arrow;
            [SerializeField] float barStart;
            [SerializeField] float barFinish;
            private Tween ArrowTween;
            Vector3 arrowPos;
            [SerializeField] RectTransform failPoint;
            [SerializeField] RectTransform greenPoint_1;
            [SerializeField] RectTransform greenPoint_2;

            float force;
            public void Init(GameManager gameManager)
            {
                this.gameManager = gameManager;
            }

            public void MovementArrow()
            {
                Vector3 startPos = new Vector3(barStart, Arrow.localPosition.y, Arrow.localPosition.z);
                Vector3 finishPos = new Vector3(barFinish, Arrow.localPosition.y, Arrow.localPosition.z);
                Arrow.localPosition = startPos; // baska normal pos olarak almaya calistim alakasiz yerlerde git gel yapti o yuzden local aliyoruz
                ArrowTween = Arrow.DOLocalMove(finishPos, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
            }
            public Vector3 StopArrowMovement()
            {
                ArrowTween?.Kill();
                arrowPos = Arrow.localPosition; // O anki pozisyon bilgisini al
                return arrowPos;
            }

            public string GetArrowColor()
            {
                Vector3 arrowPos = Arrow.position;

                if (IsWithinBounds(arrowPos, failPoint))
                {
                    return "Fail";
                }
                else if (IsWithinBounds(arrowPos, greenPoint_1))
                {
                    return "Green_1";
                }
                else if (IsWithinBounds(arrowPos, greenPoint_2))
                {
                    return "Green_2";
                }

                return "Unknown";
            }

            public float BallMovementForceByColor()
            {
                switch (GetArrowColor())
                {
                    default: return 2f;
                    case "Fail":
                        force = 2;
                        break;
                    case "Green_1":
                        force = 3;
                        break;
                    case "Green_2":
                        force = 3;
                        break;
                }
                return force;
            }
            public float BallMovementHightByColor()
            {
                switch (GetArrowColor())
                {
                    default: return 1f;
                    case "Fail":
                        force = 1;
                        break;
                    case "Green_1":
                        force = 2;
                        break;
                    case "Green_2":
                        force = 2;
                        break;
                }
                return force;
            }

            private bool IsWithinBounds(Vector3 arrowPos, RectTransform rect)
            {
                Vector3[] corners = new Vector3[4];
                rect.GetWorldCorners(corners); // bu fonksiyon ile rect objesinin 4 kenarininin tam olarak konumunu alabiliyorsun 
                return arrowPos.x >= corners[0].x && arrowPos.x <= corners[2].x;
            }
        }

        [Serializable]

        public class SingleAndMultiplayerOptions
        {

            GameManager gameManager;
            [SerializeField] TextMeshProUGUI countdownText;
            int countdown = 10; // Başlangıç değerini 10 olarak ayarlayın
            Coroutine countdownCoroutine;

            [SerializeField] TextMeshProUGUI scoreText;
            int score = 0;

            public void Init(GameManager gameManager)
            {
                this.gameManager = gameManager;
            }

            #region Singleplayer

            public void StartCountdownTimer()
            {
                if (countdownCoroutine != null)
                {
                    gameManager.StopCoroutine(countdownCoroutine); // Önceki coroutine'i durdur
                }
                countdown = 10; // Her başlatıldığında countdown değerini sıfırla
                countdownCoroutine = gameManager.StartCoroutine(countdownTimer());
            }

            public void StopCountdownTimer()
            {
                if (countdownCoroutine != null)
                {
                    gameManager.StopCoroutine(countdownCoroutine);
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
                countdownText.text = "0"; // Sayaç bittiğinde 0 olarak güncelle
                //if (countdown == 0)
                //{
                //    gameManager.targetMovement.StopTargetMovement();
                //    gameManager.shootColorSelection.StopArrowMovement();
                //    UIManager.Instance.CloseShootButton();
                //    UIManager.Instance.OpenFailCanvas();
                //}
            }

            public void UpdateScore()
            {
                score++;
                scoreText.text = score.ToString();
                SaveScore(); // Skoru kaydet
            }

            void SaveScore()
            {
                PlayerPrefs.SetInt("Score", score); // Skoru kaydet
                PlayerPrefs.Save();
            }

            public void LoadScore()
            {
                if (PlayerPrefs.HasKey("Score"))
                {
                    score = PlayerPrefs.GetInt("Score"); // Skoru yükle
                    scoreText.text = score.ToString();
                }
            }

            #endregion
        }

    }
}
