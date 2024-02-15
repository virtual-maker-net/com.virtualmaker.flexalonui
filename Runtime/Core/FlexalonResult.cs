using System;
using UnityEngine;

namespace Flexalon
{
    /// <summary>
    /// This component is added to each object in a layout. It stores the results of the layout process
    /// so they can be loaded from a scene/prefab without rerunning layout.
    /// </summary>
    [ExecuteAlways, DisallowMultipleComponent]
    public class FlexalonResult : MonoBehaviour
    {
        /// <summary> Parent layout </summary>
        public Transform Parent;

        /// <summary> Index in layout </summary>
        public int SiblingIndex;

        /// <summary> Arranged position in parent layout space. </summary>
        public Vector3 LayoutPosition = Vector3.zero;

        /// <summary> Arranged rotation in parent layout space. </summary>
        public Quaternion LayoutRotation = Quaternion.identity;

        /// <summary> Bounds deteremined by Adapter.Measure function. </summary>
        public Bounds AdapterBounds = new Bounds();

        /// <summary> Combined bounds of Layout.Measure function and Adapter.Measure functions. </summary>
        public Bounds LayoutBounds = new Bounds();

        /// <summary> Bounds after layout, scale and rotation used in the parent layout. </summary>
        public Bounds RotatedAndScaledBounds = new Bounds();

        /// <summary> What the component updater thinks the scale should be in layout space. </summary>
        public Vector3 ComponentScale = Vector3.one;

        /// <summary> Allocated size for this child when using fill size. </summary>
        public Vector3 FillSize = Vector2.zero;

        /// <summary> Reduced size if parent doesn't have space for child. </summary>
        public Vector3 ShrinkSize = Math.MaxVector;

        /// <summary> Expected local position set by the layout system. </summary>
        public Vector3 TargetPosition = Vector3.zero;

        /// <summary> Expected local rotation set by the layout system. </summary>
        public Quaternion TargetRotation = Quaternion.identity;

        /// <summary> Expected local scale set by the layout system. </summary>
        public Vector3 TargetScale = Vector3.one;

        /// <summary> Expected rect size set by the layout system. </summary>
        public Vector3 TargetRectSize = Vector2.zero;

        /// <summary> Last position set by transform updater. Used to detect unexpected changes. </summary>
        public Vector3 TransformPosition = Vector3.zero;

        /// <summary> Last rotation set by transform updater. Used to detect unexpected changes. </summary>
        public Quaternion TransformRotation = Quaternion.identity;

        /// <summary> Last scale set by transform updater. Used to detect unexpected changes. </summary>
        public Vector3 TransformScale = Vector3.one;

        /// <summary> Last rect size set by layout system. Used to detect unexpected changes. </summary>
        public Vector2 TransformRectSize = Vector2.zero;

        void Awake()
        {
            hideFlags = HideFlags.HideInInspector;
        }
    };
}