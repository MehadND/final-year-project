using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.UI
{
    public class WristMenu : MonoBehaviour
    {
        public InputActionAsset inputActionAsset;
        private InputAction menu;
        
        public GameObject wristMenuPanel;
        public GameObject marker;
        
        private void Start()
        {
            marker.SetActive(false);
            menu = inputActionAsset.FindActionMap("XRI LeftHand").FindAction("Wrist Menu");
            menu.Enable();
            menu.performed += ToggleMenu;
        }

        private void OnDestroy()
        {
            menu.performed -= ToggleMenu;
        }

        public void ToggleMenu(InputAction.CallbackContext callbackContext)
        {
            marker.SetActive(false);
            
            bool isMenuActive = !wristMenuPanel.activeSelf;
            wristMenuPanel.SetActive(isMenuActive);
        }

        public void PositionMarkerController()
        {
            print("Position Marker Selected");
            
            wristMenuPanel.SetActive(false);
            
            bool isActive = !marker.activeSelf;
            marker.SetActive(isActive);
        }
    }
}