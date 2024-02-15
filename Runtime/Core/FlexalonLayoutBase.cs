using UnityEngine;

namespace Flexalon
{
    /// <summary>
    /// Base class for all layout componets. See [custom layout](/docs/customLayout) for details
    /// on how to extend this class. Assigns the Layout method to FlexalonNode and keeps the
    /// node's children up to date.
    /// </summary>
    [ExecuteAlways, DisallowMultipleComponent]
    public abstract class LayoutBase : FlexalonComponent, Layout
    {
        /// <inheritdoc />
        protected override void DoOnEnable()
        {
            if (GetComponent<FlexalonObject>() == null)
            {
                var obj = Flexalon.AddComponent<FlexalonObject>(gameObject);

#if UNITY_UI
                if (GetComponent<Canvas>() == null)
#endif
                {
                    obj.WidthType = SizeType.Layout;
                    obj.HeightType = SizeType.Layout;
                    obj.DepthType = SizeType.Layout;
                }
            }

            _node.DetachAllChildren();
            foreach (Transform child in transform)
            {
                _node.AddChild(Flexalon.GetOrCreateNode(child.gameObject));
            }

            Flexalon.GetOrCreate().PreUpdate += DetectChanges;
            _node.SetMethod(this);
        }

        /// <inheritdoc />
        protected override void DoOnDisable()
        {
            _node.SetMethod(null);
            var flexalon = Flexalon.Get();
            if (flexalon)
            {
                flexalon.PreUpdate -= DetectChanges;
            }
        }

        /// <inheritdoc />
        protected override void ResetProperties()
        {
            _node.DetachAllChildren();
        }

        // This function is complicated because it's working around two issues.
        // First, OnTransformChildrenChanged doesn't always run on 2019.4 due to a bug.
        // See https://issuetracker.unity3d.com/issues/ontransformchildrenchanged-doesnt-get-called-in-the-edit-mode-when-dragging-a-prefab-from-the-project-window-to-the-hierarchy
        // Second, we need to deal with undo/redo. The strategy here is to do nothing on undo/redo except fix
        // the node.Children list, since it isn't serialzed. To detect undo/redo, we check if the Parent or SiblingIndex
        // values change in the serialized FlexalonResult matches the transform children.
        private void DetectChanges()
        {
            // Check if any old children changed parents. They need to be marked dirty
            // since their size may change after leaving the layout.
            for (int i = 0; i < _node.Children.Count; i++)
            {
                var childNode = _node.Children[i];
                if (!childNode.GameObject)
                {
                    Flexalon.RecordFrameChanges = true;
                    childNode.Detach();
                    MarkDirty();
                }
                else if (childNode.GameObject.transform.parent != transform || childNode.IsDragging || childNode.SkipLayout || childNode.Constraint != null)
                {
                    i--;
                    childNode.Detach();
                    if (childNode.Result.Parent == transform)
                    {
#if UNITY_EDITOR
                        UnityEditor.Undo.RecordObject(childNode.Result, "Parent change");
                        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(childNode.Result);
                        Flexalon.RecordFrameChanges = true;
#endif
                        childNode.Result.Parent = null;
                        childNode.Result.SiblingIndex = 0;
                        childNode.MarkDirty();
                        MarkDirty();
                    }
                }
            }

            // Check if we have any new or out of order children.
            int index = 0;
            foreach (Transform child in transform)
            {
                var childNode = Flexalon.GetOrCreateNode(child.gameObject);
                if (childNode.IsDragging || childNode.SkipLayout || childNode.Constraint != null)
                {
                    continue;
                }

                _node.InsertChild(childNode, index);
                if (childNode.Result.Parent != transform || childNode.Result.SiblingIndex != index)
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(childNode.Result, "Parent change");
                    UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(childNode.Result);
                    Flexalon.RecordFrameChanges = true;
#endif
                    childNode.Result.Parent = transform;
                    childNode.Result.SiblingIndex = index;

                    childNode.MarkDirty();
                    MarkDirty();
                }

                index++;
            }
        }

        /// <inheritdoc />
        public virtual Bounds Measure(FlexalonNode node, Vector3 size, Vector3 min, Vector3 max)
        {
            throw new System.NotImplementedException();
        }

        /// <summary> Helper to assign the fill and shrink size for all children. </summary>
        protected void SetChildrenFillShrinkSize(FlexalonNode node, Vector3 childSize, Vector3 layoutSize)
        {
            foreach (var child in node.Children)
            {
                child.SetShrinkFillSize(childSize, layoutSize);
            }
        }

        /// <inheritdoc />
        public virtual void Arrange(FlexalonNode node, Vector3 layoutSize) {}
    }
}