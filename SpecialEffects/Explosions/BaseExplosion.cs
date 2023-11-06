using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseExplosion : MonoBehaviour
{
    [SerializeField] protected Sprite[] sprites;
    protected ExplosionState state = ExplosionState.Searching;
    protected Player mainPlayer;
    protected Transform curTransform;
    protected Animator animator;
    void Start()
    {
        mainPlayer = Player.Main;
        curTransform = gameObject.GetComponent<Transform>();
        animator = gameObject.GetComponent<Animator>();
        animator.onAnimationEnd += OnAnimationEnd;
        animator.Sprites = sprites;
        animator.BeiginAnimation();
    }

    void OnAnimationEnd(object _sender, EventArgs _e)
    {
        state = ExplosionState.Done;
        Destroy(gameObject);
    }

}
