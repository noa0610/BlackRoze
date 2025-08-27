using UnityEngine;
using UnityEngine.InputSystem;

namespace BlackRose
{
    [RequireComponent(typeof(Collider2D))]
    public class ClickableFloorLayerController : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("The GameObject whose layer will change on click.")]
        [SerializeField] private GameObject targetObject;
        private PlatformEffector2D _platform;
        private BoxCollider2D _boxCollider;

        [Tooltip("LayerMask selecting exactly one layer for the clickable state.")]
        [SerializeField] private LayerMask clickableLayerMask;

        private int defaultLayerIndex;
        private InputAction clickAction;

        [Header("Input Settings")]
        [SerializeField] private InputActionAsset actionAsset;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string clickActionName = "OnClickDown";

        private void Awake()
        {
            // Fallback to parent if no explicit target is set
            if (targetObject == null)
                targetObject = transform.parent != null ? transform.parent.gameObject : gameObject;

            defaultLayerIndex = targetObject.layer;
            _platform = targetObject.GetComponent<PlatformEffector2D>();
            _boxCollider = targetObject.GetComponent<BoxCollider2D>();

            // Retrieve the click action from the specified action map
            var actionMap = actionAsset.FindActionMap(actionMapName);
            clickAction = actionMap.FindAction(clickActionName);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //Debug.Log("Find");
            // Enable and bind click events when cursor enters the trigger
            clickAction.Enable();
            clickAction.performed += OnClickPressed;
            clickAction.canceled += OnClickReleased;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Unbind and disable click events when cursor exits the trigger
            clickAction.performed -= OnClickPressed;
            clickAction.canceled -= OnClickReleased;
            clickAction.Disable();
            ResetLayer();
        }

        private void OnClickPressed(InputAction.CallbackContext context)
        {
            // Calculate the layer index from the LayerMask value (bit position)
            int newLayerIndex = Mathf.RoundToInt(Mathf.Log(clickableLayerMask.value, 2));
            targetObject.layer = newLayerIndex;
            _boxCollider.usedByEffector = false;
            _platform.enabled = false;
        }

        private void OnClickReleased(InputAction.CallbackContext context)
        {
            // Revert to the original layer
            ResetLayer();
        }

        private void ResetLayer()
        {
            targetObject.layer = defaultLayerIndex;
            _boxCollider.usedByEffector = true;
            _platform.enabled = true;
        }
    }
}
