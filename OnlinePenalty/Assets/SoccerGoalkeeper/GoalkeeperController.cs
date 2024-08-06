using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlinePenalty
{
    public class GoalkeeperController : MonoBehaviour
    {
        public static GoalkeeperController Instance;

        Animator animator;
        [SerializeField] AnimationClip idle;
        [SerializeField] AnimationClip bodyBlockAnim;
        [SerializeField] AnimationClip bodyBlockRightAnim;
        [SerializeField] AnimationClip divingSave;
        [SerializeField] AnimationClip divingRightSave;
        [SerializeField] AnimationClip catchAnim;
        [SerializeField] List<AnimationClip> animations = new List<AnimationClip>();
        [SerializeField] Transform yellowAreaParentTransform; // Sar� alan� temsil eden transform


        //PhotonView photonView;


        private void Awake()
        {
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
            animator = GetComponent<Animator>();
        }
        public void PlayBodyBlock()
        {
            animator.Play(bodyBlockAnim.name);
        }
        public void PlayBodyRightBlock()
        {
            animator.Play(bodyBlockRightAnim.name);
        }
        public void PlayDivingSave()
        {
            animator.Play(divingSave.name);
        }
        public void PlayDivingRightSave()
        {
            animator.Play(divingRightSave.name);
        }
        public void PlayCatch()
        {
            animator.Play(catchAnim.name);
        }


        public void RotateYellowArea(float rotationFactor)
        {
            float rotationAngle = Mathf.Lerp(-60f, 60f, -rotationFactor * 0.5f + 0.5f);
            yellowAreaParentTransform.localEulerAngles = new Vector3(0, 0, rotationAngle);
            //photonView.RPC("PunRPC_UpdateYellowAreaRotation", RpcTarget.All, yellowAreaParentTransform.localEulerAngles.z);
        }

        //[PunRPC]
        //public void PunRPC_UpdateYellowAreaRotation(float rotationZ)
        //{
        //    yellowAreaParentTransform.localEulerAngles = new Vector3(0, 0, rotationZ);
        //}
        public void StartSavingSingleMode()
        {
            animator.Play(animations[Random.Range(0, animations.Count)].name);
        }

        public void StartSaving()
        {
            float yellowAreaRotationZ = yellowAreaParentTransform.localEulerAngles.z;
            if (yellowAreaRotationZ > 180) yellowAreaRotationZ -= 360;
            //Debug.Log(yellowAreaRotationZ);

            // Sar� alan�n d�n�� a��s�na g�re animasyonlar� belirle
            if (IsInRange(yellowAreaRotationZ, -20, 20))
            {
                PlayCatch();
            }
            else if (IsInRange(yellowAreaRotationZ, -40, -20) || IsInRange(yellowAreaRotationZ, 20, 40))
            {
                if (IsInRange(yellowAreaRotationZ, -40, -20))
                {
                    PlayDivingRightSave();
                }
                else
                {
                    PlayDivingSave();
                }
            }
            else if (IsInRange(yellowAreaRotationZ, -61, -40) || IsInRange(yellowAreaRotationZ, 40, 61))
            {
                if (IsInRange(yellowAreaRotationZ, -61, -40))
                {
                    PlayBodyRightBlock();
                }
                else
                {
                    PlayBodyBlock();
                }
            }
            else
            {
                PlayCatch(); // Varsay�lan animasyon
            }

            // Belirtilen aral�kta olup olmad���n� kontrol eden yard�mc� metot
            bool IsInRange(float value, float min, float max)
            {
                return value >= min && value <= max;
            }
        }
    }
}
