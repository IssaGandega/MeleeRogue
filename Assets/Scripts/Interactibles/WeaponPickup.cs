using System;
using System.Collections;
using Controller;
using Interfaces;
using Unity.VisualScripting;
using UnityEngine;
using Weapons;

namespace Interactibles
{
    public class WeaponPickup : MonoBehaviour, IInteractible
    {
        [SerializeField] private PlayerController.EquippedWeapon weaponType;
        [SerializeField] private Sprite weaponSprite;
        
        [SerializeField] private SpriteRenderer display;
        [SerializeField] private float upDownPower;
        [SerializeField] private float bobSpeed;
        
        private SpriteRenderer _weaponRenderer;
        private Vector3 _defaultTransform;
        private bool isInteractible;
        private PlayerController _playerController;
        private float _bobTimer;

        
        
        private void Awake()
        {
            display.sprite = weaponSprite;
        }

        private void Start()
        {
            _defaultTransform = new Vector3(display.transform.localPosition.x, display.transform.localPosition.y, display.transform.localPosition.z);
        }

        private void Update()
        {
            display.transform.Rotate(0,.2f,0);
            if (isInteractible)
            {
                _bobTimer += Time.deltaTime;
                float y = _defaultTransform.y + Mathf.Sin(_bobTimer * bobSpeed) * upDownPower;
                
                display.transform.localPosition = new Vector3(_defaultTransform.x, y, _defaultTransform.z);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.GetComponent<PlayerController>())
            {
                _bobTimer = 0f;
                isInteractible = true;
                _playerController = collision.gameObject.GetComponent<PlayerController>();
                _playerController.NearbyInteractible =  this;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.GetComponent<PlayerController>())
            {
                display.transform.localPosition =  _defaultTransform;
                isInteractible = false;
                _playerController.NearbyInteractible =  null;
                _playerController = null;
            }
        }

        public void Interact()
        {
            _playerController.EquipWeapon(weaponType);
        }
    }
}
