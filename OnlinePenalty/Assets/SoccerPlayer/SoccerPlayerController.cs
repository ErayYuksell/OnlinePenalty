using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using OnlinePenalty;

public class SoccerPlayerController : MonoBehaviour
{
    Animator animator;
    GameManager gameManager;
    BallController ballController;

    [SerializeField] Transform ball;
    [SerializeField] Transform goal;

    [SerializeField] AnimationClip idle;
    [SerializeField] AnimationClip penaltyKickAnim;

    Vector3 targetPosition;
    private bool animationFinished = false;

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

        targetPosition = gameManager.targetMovement.StopTargetMovement();

        //Single icin zamani durdur 
        gameManager.singleAndMultiplayerOptions.StopCountdownTimer();

        //single icin atislari yap
        StartShooting();
    }

    public void StartShooting()
    {
        animator.Play(penaltyKickAnim.name);
        Debug.Log("Penalty Animasyonu calisti");
        animationFinished = false;
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
        ShootBall(targetPosition, gameManager.shootColorSelection.BallMovementForceByColor());
        GoalkeeperController.Instance.StartSavingSingleMode();
    }
    // Animasyon Event tarafinda animasyonun sonunda calistiriyorum 
    public void OnAnimationComplete()
    {
        animationFinished = true;
        animator.Play(idle.name);
    }
}
