using UnityEngine;

namespace Flexalon
{
    /// <summary> Simple input provider that uses the mouse for input. </summary>
    public class FlexalonMouseInputProvider : InputProvider
    {
        public bool Active => Input.GetMouseButton(0);
        public Vector3 UIPointer => Input.mousePosition;
        public Ray Ray => Camera.main.ScreenPointToRay(Input.mousePosition);
        public InputMode InputMode => InputMode.Raycast;
        public GameObject ExternalFocusedObject => null;
    }
}