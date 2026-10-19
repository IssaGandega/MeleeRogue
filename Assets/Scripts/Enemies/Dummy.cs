using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(SpriteRenderer))]
public class Dummy : MonoBehaviour, IDamageable
{
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage()
    {
        Debug.Log(gameObject.name + " damaged");
        StartCoroutine(IsHit());
    }

    private IEnumerator IsHit()
    {
        _spriteRenderer.color = Color.green;
        yield return new WaitForSeconds(0.5f);
        
        _spriteRenderer.color = Color.blue;
    }
}
