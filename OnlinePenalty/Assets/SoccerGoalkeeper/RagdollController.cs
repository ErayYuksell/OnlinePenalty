using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlinePenalty
{
    //Eðer Ragdoll'u animasyon sýrasýnda doðrudan kullanýrsanýz, animasyon ve fizik simülasyonu arasýnda çakýþmalar meydana gelebilir. Bu durum, animasyon
    //sýrasýnda karakterin hareketlerinin fiziksel çarpýþmalarla kesintiye uðramasýna veya beklenmedik davranýþlara yol açabilir.
    //Dolayýsýyla, animasyon sýrasýnda Ragdoll'un collider ve rigidbody bileþenlerinin kinematic olarak ayarlanmasý önemlidir.
    public class RagdollController : MonoBehaviour
    {
        private Rigidbody[] rigidbodies;
        private Animator animator;

        void Start()
        {
            rigidbodies = GetComponentsInChildren<Rigidbody>();
            animator = GetComponent<Animator>();

            // Baþlangýçta tüm rigidbody'leri kinematic yap
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
            // Animator'ý devre dýþý býrak
            animator.enabled = false;

            // Ragdoll'u etkinleþtir
            SetKinematic(false);
        }

        public void DisableRagdoll()
        {
            // Ragdoll'u devre dýþý býrak
            SetKinematic(true);

            // Animator'ý etkinleþtir
            animator.enabled = true;
        }
    }

}
