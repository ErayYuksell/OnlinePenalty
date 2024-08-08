using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using OnlinePenalty;

public class SoccerPlayerController : MonoBehaviour
{
    public static SoccerPlayerController Instance;

    Animator animator;
    GameManager gameManager;
    BallController ballController;

    [SerializeField] Transform ball;
    [SerializeField] Transform goal;

    [SerializeField] AnimationClip idle;
    [SerializeField] AnimationClip penaltyKickAnim;

    Vector3 targetPosition;
    private bool animationFinished = false;
    PhotonView photonView;
    private void Awake()
    {
        if (Instance == null)
        {
            photonView = GetComponent<PhotonView>();
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play(idle.name);

        gameManager = GameManager.Instance;
        ballController = BallController.Instance;
    }

    // Shoot butonuna tiklandiginda calisir 
    public void OnShootButtonPressed()
    {

        string arrowColor = gameManager.shootColorSelection.GetArrowColor();
        Debug.Log("Arrow Color: " + arrowColor);
        gameManager.shootColorSelection.StopArrowMovement();

        //targetPosition = gameManager.targetMovement.StopTargetMovement();
        photonView.RPC("PunRPC_GetTargetPos", RpcTarget.All);

        if (!MultiplayerController.Instance.GetMultiplayerMode())
        {
            Debug.Log("Single Mode");
            //Single icin zamani durdur 
            MultiplayerController.Instance.StopCountdownTimer();

            //single icin atislari yap
            StartShooting();
        }
        else
        {
            Debug.Log("ButtonPlayer1: " + MultiplayerController.Instance.IsPlayer1Turn());
            Debug.Log("ButtonPlayer2: " + MultiplayerController.Instance.IsPlayer2Turn());

            if (MultiplayerController.Instance.IsPlayer1Turn())
            {
                MultiplayerController.Instance.IsPlayer1ButtonDone();
                MultiplayerController.Instance.WhoTapToButton(true);
                Debug.Log("Player1 tap to button");
                Debug.Log(MultiplayerController.Instance.GetWhoTapToButton());
            }
            else if (MultiplayerController.Instance.IsPlayer2Turn())
            {
                MultiplayerController.Instance.IsPlayer2ButtonDone();
                MultiplayerController.Instance.WhoTapToButton(true);
                Debug.Log("Player2 tap to button");
                Debug.Log(MultiplayerController.Instance.GetWhoTapToButton());
            }
        }
    }

    [PunRPC]
    void PunRPC_GetTargetPos()
    {
        targetPosition = gameManager.targetMovement.StopTargetMovement();
    }

    public void StartShooting()
    {
        animator.Play(penaltyKickAnim.name);
        Debug.Log("Penalty Animasyonu calisti");
        animationFinished = false;
    }

    public void MultiplayerStartShooting() // multiplayerControllerda cagirmak icin bu fonksiyonu olusturdum 
    {
        photonView.RPC("PunRPC_MultiplayerShooting", RpcTarget.All);
    }

    [PunRPC]
    public void PunRPC_MultiplayerShooting()
    {
        StartShooting();
    }
    public void ShootBall(Vector3 targetPos, float kickForce)
    {
        Vector3 direction = (targetPos - ball.position).normalized;
        Vector3 finalForce = direction * kickForce; // Final kuvveti belirleniyor
        ballController.KickBall(targetPos, gameManager.shootColorSelection.BallMovementHightByColor(), 1f, finalForce); // 2 high i temsil ediyor, 1 duration, daha iyi bir degerle daha iyi goruntu cikarabilirsin 
        Debug.Log("Top hareketi basladi");


        // Animasyon tamamland���nda yap�lacak i�lemler
        animationFinished = true;

        // Idle state'e ge�meden �nce pozisyonu ve rotasyonu sabitle
        animator.Play(idle.name);
    }
    // Animasyon Event tarafinda animasyonun ortasinda calistiriyorum 
    public void OnKick()
    {
        if (!MultiplayerController.Instance.GetMultiplayerMode())
        {
            ShootBall(targetPosition, gameManager.shootColorSelection.BallMovementForceByColor());
            GoalkeeperController.Instance.StartSavingSingleMode();
        }
        else if (MultiplayerController.Instance.GetMultiplayerMode())
        {
            photonView.RPC("PunRPC_ShootBall", RpcTarget.All, targetPosition, gameManager.shootColorSelection.BallMovementForceByColor());
        }
    }

    [PunRPC]
    public void PunRPC_ShootBall(Vector3 targetPos, float kickForce)
    {
        ShootBall(targetPosition, kickForce);
    }
    
    // Animasyon Event tarafinda animasyonun sonunda calistiriyorum 
    public void OnAnimationComplete()
    {
        animationFinished = true;
        animator.Play(idle.name);
    }
}
