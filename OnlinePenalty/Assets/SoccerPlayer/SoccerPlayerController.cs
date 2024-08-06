using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerPlayerController : MonoBehaviour
{
    Animator animator;
    [SerializeField] AnimationClip idle;
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play(idle.name);
    }

}
