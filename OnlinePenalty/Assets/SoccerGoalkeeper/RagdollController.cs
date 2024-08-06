using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlinePenalty
{
    //E�er Ragdoll'u animasyon s�ras�nda do�rudan kullan�rsan�z, animasyon ve fizik sim�lasyonu aras�nda �ak��malar meydana gelebilir. Bu durum, animasyon
    //s�ras�nda karakterin hareketlerinin fiziksel �arp��malarla kesintiye u�ramas�na veya beklenmedik davran��lara yol a�abilir.
    //Dolay�s�yla, animasyon s�ras�nda Ragdoll'un collider ve rigidbody bile�enlerinin kinematic olarak ayarlanmas� �nemlidir.
    public class RagdollController : MonoBehaviour
    {
        private Rigidbody[] rigidbodies;
        private Animator animator;

        void Start()
        {
            rigidbodies = GetComponentsInChildren<Rigidbody>();
            animator = GetComponent<Animator>();

            // Ba�lang��ta t�m rigidbody'leri kinematic yap
            SetKinematic(true);
        }

        void SetKinematic(bool newValue)
        {
            foreach (var rb in rigidbodies)
            {
                rb.isKinematic = newValue;
            }
        }

        public void EnableRagdoll()
        {
            // Animator'� devre d��� b�rak
            animator.enabled = false;

            // Ragdoll'u etkinle�tir
            SetKinematic(false);
        }

        public void DisableRagdoll()
        {
            // Ragdoll'u devre d��� b�rak
            SetKinematic(true);

            // Animator'� etkinle�tir
            animator.enabled = true;
        }
    }

}
