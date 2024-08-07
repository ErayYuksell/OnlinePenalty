﻿using OnlinePenalty;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BallController : MonoBehaviour
{
    public static BallController Instance;
    private Rigidbody rb;

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
        rb = GetComponent<Rigidbody>();
    }

    public void KickBall(Vector3 targetPosition, float height, float duration, Vector3 finalForce)
    {
        StartCoroutine(SmoothKickBall(targetPosition, height, duration, finalForce));
    }
    public void StopBallMovement()
    {
        StopAllCoroutines();
    }
    private IEnumerator SmoothKickBall(Vector3 targetPosition, float height, float duration, Vector3 finalForce)
    {
        rb.isKinematic = true; // Kinematic durumu a��l�yor
        Vector3 startPosition = transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            // Parabolik hareket i�in Lerp kullan�m�
            float t = elapsedTime / duration;
            float yOffset = height * Mathf.Sin(Mathf.PI * t);
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, t) + new Vector3(0, yOffset, -1);

            transform.position = currentPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        rb.isKinematic = false; // Kinematic durumu kapat�l�yor

        // Top hedef pozisyona ula�t�ktan sonra kuvvet uygulan�yor
        //rb.AddForce(finalForce, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SoccerGoal"))
        {
            Debug.Log("Top Aglarda");
            StopBallMovement();

            UIManager.Instance.OpenGoalCanvas();
            MultiplayerController.Instance.UpdateScore();
        }
    }

}
