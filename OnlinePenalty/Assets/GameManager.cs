using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;


namespace OnlinePenalty
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public TargetMovement targetMovement;
        public ShootColorSelection shootColorSelection;
        //public SingleAndMultiplayerOptions singleAndMultiplayerOptions;

        private void Awake()
        {
            targetMovement.Init(this);
            shootColorSelection.Init(this);


            if (Instance == null)
            {
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

            [SerializeField] float barStart;
            [SerializeField] float barFinish;
            private Tween ArrowTween;
            Vector3 arrowPos;
           
            [SerializeField] RectTransform Arrow;
            [SerializeField] RectTransform failPoint;
            [SerializeField] RectTransform greenPoint_1;
            [SerializeField] RectTransform greenPoint_2;
            private Dictionary<string, (RectTransform rect, float force)> zones;

            public void Init(GameManager gameManager)
            {
                this.gameManager = gameManager;

                zones = new Dictionary<string, (RectTransform, float)>
               {
                 { "Fail", (failPoint, 50f) },
                 { "Green_1", (greenPoint_1, 20f) },
                 { "Green_2", (greenPoint_2, 20f) }
               };
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

                // Önce yeşil barları kontrol et
                if (IsWithinBounds(arrowPos, greenPoint_1))
                {
                    return "Green_1";
                }
                else if (IsWithinBounds(arrowPos, greenPoint_2))
                {
                    return "Green_2";
                }

                // Sonra mavi barları kontrol et
                foreach (var zone in zones)
                {
                    if (IsWithinBounds(arrowPos, zone.Value.rect))
                    {
                        return zone.Key;
                    }
                }

                return "Unknown";
            }

            public float BallMovementForceByColor()
            {
                string arrowColor = GetArrowColor();

                if (zones.TryGetValue(arrowColor, out var zone))
                {
                    return zone.force;
                }

                return 50f; // Default force
            }

            private bool IsWithinBounds(Vector3 arrowPos, RectTransform rect)
            {
                Vector3[] corners = new Vector3[4];
                rect.GetWorldCorners(corners); // bu fonksiyon ile rect objesinin 4 kenarininin tam olarak konumunu alabiliyorsun

                // Okun merkezinin alanın içinde olup olmadığını kontrol et
                return arrowPos.x >= corners[0].x && arrowPos.x <= corners[2].x &&
                       arrowPos.y >= corners[0].y && arrowPos.y <= corners[2].y;
            }
        }

    }
}
