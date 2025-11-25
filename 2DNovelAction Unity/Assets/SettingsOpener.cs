using UnityEngine;
using UnityEngine.InputSystem;

namespace BlackRose
{
    public class SettingsOpener : MonoBehaviour
    {
        [SerializeField] private GameObject _togglableObj;
        private void OnPause(InputValue _)
        {
            _togglableObj.SetActive(!_togglableObj.activeInHierarchy);
        }
    }
}