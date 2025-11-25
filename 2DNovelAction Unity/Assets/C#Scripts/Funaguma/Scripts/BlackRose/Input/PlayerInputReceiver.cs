using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Core.Inputs
{
    [RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
    public class PlayerInputReceiver : MonoBehaviour
    {
        [SerializeField] private GameObject _pauseMenu;
        private bool _isPaused = false;
        public void OnPause()
        {
            // Handle pause input
            Debug.Log("Pause input received.");
            UnitManager.instance.Pause(!_isPaused);
            if (_pauseMenu != null)
            {
                _pauseMenu.SetActive(!_isPaused);
            }
            _isPaused = !_isPaused;
        }
    }
}