﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.UIControls
{
    /// <summary>
    /// Data Item Expanding event arguments
    /// </summary>
    public class VirtualizingItemExpandingArgs : EventArgs
    {
        /// <summary>
        /// Data Item
        /// </summary>
        public object Item
        {
            get;
            private set;
        }

        /// <summary>
        /// Specify item's children using this property
        /// </summary>
        public IEnumerable Children
        {
            get;
            set;
        }

        public VirtualizingItemExpandingArgs(object item)
        {
            Item = item;
        }
    }

    /// <summary>
    /// TreeView data binding event arguments
    /// </summary>
    public class VirtualizingTreeViewItemDataBindingArgs : ItemDataBindingArgs
    {
        /// <summary>
        /// Set to true if data bound item has children
        /// </summary>
        public bool HasChildren
        {
            get;
            set;
        }
    }

    public class VirtualizingParentChangedEventArgs : EventArgs
    {
        public TreeViewItemContainerData OldParent
        {
            get;
            private set;
        }

        public TreeViewItemContainerData NewParent
        {
            get;
            private set;
        }

        public VirtualizingParentChangedEventArgs(TreeViewItemContainerData oldParent, TreeViewItemContainerData newParent)
        {
            OldParent = oldParent;
            NewParent = newParent;
        }
    }

    public class TreeViewItemContainerData : ItemContainerData
    {

        public static event EventHandler<VirtualizingParentChangedEventArgs> ParentChanged;


        private TreeViewItemContainerData m_parent;
        public TreeViewItemContainerData Parent
        {
            get { return m_parent; }
            set
            {
                if(m_parent == value)
                {
                    return;
                }

                TreeViewItemContainerData oldParent = m_parent;
                m_parent = value;
                if (ParentChanged != null)
                {
                    ParentChanged(this, new VirtualizingParentChangedEventArgs(oldParent, m_parent));
                }
            }
        }

        /// <summary>
        /// Get TreeViewItem absolute indent
        /// </summary>
        public int Indent
        {
            get;
            set;
        }

        /// <summary>
        /// Can Expand TreeView item ?
        /// </summary>
        public bool CanExpand
        {
            get;
            set;
        }

        /// <summary>
        /// Is TreeViewItem expanded ?
        /// </summary>
        public bool IsExpanded
        {
            get;
            set;
        }

        /// <summary>
        /// Is treeviewitem is descendant of another element
        /// </summary>
        /// <param name="ancestor">treeview item to test</param>
        /// <returns>true if treeview item is descendant of ancestor</returns>
        public bool IsDescendantOf(TreeViewItemContainerData ancestor)
        {
            if (ancestor == null)
            {
                return true;
            }

            if (ancestor == this)
            {
                return false;
            }

            TreeViewItemContainerData testItem = this;
            while (testItem != null)
            {
                if (ancestor == testItem)
                {
                    return true;
                }

                testItem = testItem.Parent;
            }

            return false;
        }

        /// <summary>
        /// Returns true if TreeViewItem has children. DO NOT USE THIS PROPERTY DURING DRAG&DROP OPERATION
        /// </summary>
        public bool HasChildren(VirtualizingTreeView treeView)
        {
            if(treeView == null)
            {
                return false;
            }
            int index = treeView.IndexOf(Item);

            TreeViewItemContainerData nextItem = (TreeViewItemContainerData)treeView.GetItemContainerData(index + 1);
            return nextItem != null && nextItem.Parent == this;
        }



        /// <summary>
        /// Get First Child of TreeViewItem
        /// </summary>
        /// <returns>First Child</returns>
        public TreeViewItemContainerData FirstChild(VirtualizingTreeView treeView)
        {
            if (!HasChildren(treeView))
            {
                return null;
            }

            int siblingIndex = treeView.IndexOf(Item);
            siblingIndex++;

            TreeViewItemContainerData child = (TreeViewItemContainerData)treeView.GetItemContainerData(siblingIndex);

            Debug.Assert(child != null && child.Parent == this);

            return child;
        }


        /// <summary>
        /// Get Next Child Of TreeViewItem
        /// </summary>
        /// <param name="currentChild"></param>
        /// <returns>next treeview item after current child</returns>
        public TreeViewItemContainerData NextChild(VirtualizingTreeView treeView, TreeViewItemContainerData currentChild)
        {
            if (currentChild == null)
            {
                throw new ArgumentNullException("currentChild");
            }

            int siblingIndex = treeView.IndexOf(currentChild.Item);
            siblingIndex++;

            TreeViewItemContainerData nextChild = (TreeViewItemContainerData)treeView.GetItemContainerData(siblingIndex);
            while (nextChild != null && nextChild.IsDescendantOf(this))
            {
                if (nextChild.Parent == this)
                {
                    return nextChild;
                }

                siblingIndex++;
                nextChild = (TreeViewItemContainerData)treeView.GetItemContainerData(siblingIndex);
            }

            return null;
        }

        /// <summary>
        /// Get Last Child of TreeViewItem
        /// </summary>
        /// <returns>Last Child</returns>
        public TreeViewItemContainerData LastChild(VirtualizingTreeView treeView)
        {
            if (!HasChildren(treeView))
            {
                return null;
            }

            int siblingIndex = treeView.IndexOf(Item);

            TreeViewItemContainerData lastChild = null;
            while (true)
            {
                siblingIndex++;
                TreeViewItemContainerData child = (TreeViewItemContainerData)treeView.GetItemContainerData(siblingIndex);
                if (child == null || !child.IsDescendantOf(this))
                {
                    return lastChild;
                }
                if (child.Parent == this)
                {
                    lastChild = child;
                }
            }
        }

        /// <summary>
        /// Get Last Descendant Of TreeViewItem
        /// </summary>
        /// <returns>Last Descendant</returns>
        public TreeViewItemContainerData LastDescendant(VirtualizingTreeView treeView)
        {
            if (!HasChildren(treeView))
            {
                return null;
            }

            int siblingIndex = treeView.IndexOf(Item);
            TreeViewItemContainerData lastDescendant = null;
            while (true)
            {
                siblingIndex++;
                TreeViewItemContainerData child = (TreeViewItemContainerData)treeView.GetItemContainerData(siblingIndex);

                if (child == null || !child.IsDescendantOf(this))
                {
                    return lastDescendant;
                }

                lastDescendant = child;
            }
        }

        public override string ToString()
        {
            return base.ToString();// "TVIDAT " + Indent + " " + IsSelected + " " + CanExpand + " " + IsExpanded;// + " " + 
                //Item.ToString() + " " + Parent != null ? Parent.Item.ToString() : "parent null";
        }
    }

    public class VirtualizingTreeView : VirtualizingItemsControl<VirtualizingTreeViewItemDataBindingArgs>
    {
        /// <summary>
        /// Raised on item expanding
        /// </summary>
        public event EventHandler<VirtualizingItemExpandingArgs> ItemExpanding;

        /// <summary>
        /// Indent between parent and children
        /// </summary>
        public int Indent = 20;

        public bool CanReparent = true;
        protected override bool CanScroll
        {
            get { return base.CanScroll || CanReparent; }
        }

        /// <summary>
        /// Auto Expand items
        /// </summary>
        public bool AutoExpand = false;

        protected override void OnEnableOverride()
        {
            base.OnEnableOverride();
            TreeViewItemContainerData.ParentChanged += OnTreeViewItemParentChanged;
        }

        protected override void OnDisableOverride()
        {
            base.OnDisableOverride();
            TreeViewItemContainerData.ParentChanged -= OnTreeViewItemParentChanged;
        }

        protected override ItemContainerData InstantiateItemContainerData(object item)
        {
            return new TreeViewItemContainerData
            {
                Item = item,
            };
        }

        /// <summary>
        /// Add data item as last child of parent
        /// </summary>
        /// <param name="parent">parent data item</param>
        /// <param name="item">data item to add</param>
        public void AddChild(object parent, object item)
        {
            if (parent == null)
            {
                Add(item);
            }
            else
            {
                VirtualizingTreeViewItem parentContainer = (VirtualizingTreeViewItem)GetItemContainer(parent);
                if (parentContainer == null)
                {
                    return;
                }

                int index = -1;
                if (parentContainer.IsExpanded)
                {
                    if (parentContainer.HasChildren)
                    {
                        TreeViewItemContainerData lastDescendant = parentContainer.LastDescendant();
                        index = IndexOf(lastDescendant.Item) + 1;
                    }
                    else
                    {
                        index = IndexOf(parentContainer.Item) + 1;
                    }
                }
                else
                {
                    parentContainer.CanExpand = true;
                }

                if (index > -1)
                {
                    TreeViewItemContainerData addedItemData = (TreeViewItemContainerData)Insert(index, item);
                    VirtualizingTreeViewItem addedTreeViewItem = (VirtualizingTreeViewItem)GetItemContainer(item);
                    if(addedTreeViewItem != null)
                    {
                        addedTreeViewItem.Parent = parentContainer.TreeViewItemData;
                    }
                    else
                    {
                        addedItemData.Parent = parentContainer.TreeViewItemData;
                    }
                }
            }
        }

        public override void Remove(object item)
        {
            throw new NotSupportedException("This method is not supported for TreeView use RemoveChild instead");
        }

        public void RemoveChild(object parent, object item, bool isLastChild)
        {
            if (parent == null)
            {
                base.Remove(item);
            }
            else
            {
                if (GetItemContainer(item) != null)
                {
                    base.Remove(item);
                }
                else
                {
                    //Parent item is not expanded (if isLastChild just remove parent expander
                    if (isLastChild)
                    {
                        VirtualizingTreeViewItem parentContainer = (VirtualizingTreeViewItem)GetItemContainer(parent);
                        if (parentContainer)
                        {
                            parentContainer.CanExpand = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Change data item parent
        /// </summary>
        /// <param name="parent">new parent</param>
        /// <param name="item">data item</param>
        public void ChangeParent(object parent, object item)
        {
            if (IsDropInProgress)
            {
                return;
            }

            ItemContainerData dragItem = GetItemContainerData(item);
            if (dragItem == null)
            {
                return;
            }

            ItemContainerData dropTarget = GetItemContainerData(parent);
            ItemContainerData[] dragItems = new[] { dragItem };
            if (CanDrop(dragItems, dropTarget))
            {
                Drop(dragItems, dropTarget, ItemDropAction.SetLastChild);
            }
        }



        /// <summary>
        /// Check wheter item is expanded
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool IsExpanded(object item)
        {
            TreeViewItemContainerData itemContainerData = (TreeViewItemContainerData)GetItemContainerData(item);
            if (itemContainerData == null)
            {
                return false;
            }
            return itemContainerData.IsExpanded;
        }

        /// <summary>
        /// To prevent Expand method call during drag drop operation 
        /// </summary>
        private bool m_expandSilently;
        public void Expand(object item)
        {
            if (m_expandSilently)
            {
                return;
            }

            if (ItemExpanding != null)
            {
                TreeViewItemContainerData treeViewItemData = (TreeViewItemContainerData)GetItemContainerData(item);

                VirtualizingItemExpandingArgs args = new VirtualizingItemExpandingArgs(treeViewItemData.Item);
                ItemExpanding(this, args);

                IEnumerable children = args.Children.OfType<object>().ToArray();
                int itemIndex = IndexOf(treeViewItemData.Item);

                VirtualizingTreeViewItem treeViewItem = (VirtualizingTreeViewItem)GetItemContainer(treeViewItemData.Item);
                if(treeViewItem != null)
                {
                    treeViewItem.CanExpand = children != null;
                }
                else
                {
                    treeViewItemData.CanExpand = children != null;
                }

                if (treeViewItemData.CanExpand)
                {
                    foreach (object childItem in children)
                    {
                        itemIndex++;

                        TreeViewItemContainerData childData = (TreeViewItemContainerData)Insert(itemIndex, childItem);
                        VirtualizingTreeViewItem childContainer = (VirtualizingTreeViewItem)GetItemContainer(childItem);
                        if (childContainer != null)
                        {
                            childContainer.Parent = treeViewItemData;
                        }
                        else
                        {
                            childData.Parent = treeViewItemData;
                        }
                    }

                    UpdateSelectedItemIndex();
                }
            }
        }

        public void Collapse(object item)
        {
            TreeViewItemContainerData treeViewItemData = (TreeViewItemContainerData)GetItemContainerData(item);

            int itemIndex = IndexOf(treeViewItemData.Item);
            List<object> itemsToDestroy = new List<object>();
            Collapse(treeViewItemData, itemIndex + 1, itemsToDestroy);

            if(itemsToDestroy.Count > 0)
            {
                bool unselect = false;
                base.DestroyItems(itemsToDestroy.ToArray(), unselect);
            }            
        }

        private void Collapse(object[] items)
        {
            List<object> itemsToDestroy = new List<object>();
            
            for (int i = 0; i < items.Length; ++i)
            {
                int itemIndex = IndexOf(items[i]);
                if(itemIndex < 0)
                {
                    continue;
                }
                TreeViewItemContainerData itemData = (TreeViewItemContainerData)GetItemContainerData(itemIndex);
                Collapse(itemData, itemIndex + 1, itemsToDestroy);
            }

            if (itemsToDestroy.Count > 0)
            {
                bool unselect = false;
                base.DestroyItems(itemsToDestroy.ToArray(), unselect);
            }
        }

        private void Collapse(TreeViewItemContainerData item, int itemIndex, List<object> itemsToDestroy)
        {
            while (true)
            {
                TreeViewItemContainerData child = (TreeViewItemContainerData)GetItemContainerData(itemIndex);
                if (child == null || !child.IsDescendantOf(item))
                {
                    break;
                }

                itemsToDestroy.Add(child.Item);
                itemIndex++;
            }
        }

        public override void DataBindItem(object item, ItemContainerData containerData, VirtualizingItemContainer itemContainer)
        {
            itemContainer.Clear();

            VirtualizingTreeViewItemDataBindingArgs args = new VirtualizingTreeViewItemDataBindingArgs();
            args.Item = item;
            args.ItemPresenter = itemContainer.ItemPresenter == null ? gameObject : itemContainer.ItemPresenter;
            args.EditorPresenter = itemContainer.EditorPresenter == null ? gameObject : itemContainer.EditorPresenter;
          
            RaiseItemDataBinding(args);

            VirtualizingTreeViewItem treeViewItem = (VirtualizingTreeViewItem)itemContainer;
            treeViewItem.CanExpand = args.HasChildren;
            treeViewItem.CanEdit = args.CanEdit;
            treeViewItem.CanDrag = args.CanDrag;
            treeViewItem.UpdateIndent();
        }

        private void OnTreeViewItemParentChanged(object sender, VirtualizingParentChangedEventArgs e)
        {
            TreeViewItemContainerData tvItem = (TreeViewItemContainerData)sender;
         
            TreeViewItemContainerData oldParent = e.OldParent;
            if (DropMarker.Action != ItemDropAction.SetLastChild && DropMarker.Action != ItemDropAction.None)
            {
                if (oldParent != null && !oldParent.HasChildren(this))
                {
                    VirtualizingTreeViewItem tvOldParent = (VirtualizingTreeViewItem)GetItemContainer(oldParent.Item);
                    if (tvOldParent != null)
                    {
                        tvOldParent.CanExpand = false;
                    }
                    else
                    {
                        oldParent.CanExpand = false;
                    }
                }
                return;
            }

            TreeViewItemContainerData tvDropTargetData = e.NewParent;
            VirtualizingTreeViewItem tvDropTarget = null;
            if (tvDropTargetData != null)
            {
                tvDropTarget = (VirtualizingTreeViewItem)GetItemContainer(tvDropTargetData.Item);
            }
             
            if (tvDropTarget != null)
            {
                if (tvDropTarget.CanExpand)
                {
                    tvDropTarget.IsExpanded = true;
                }
                else
                {
                    tvDropTarget.CanExpand = true;
                    try
                    {
                        m_expandSilently = true;
                        tvDropTarget.IsExpanded = true;
                    }
                    finally
                    {
                        m_expandSilently = false;
                    }
                }
            }
            else
            {
                if(tvDropTargetData != null)
                {
                    tvDropTargetData.CanExpand = true;
                    tvDropTargetData.IsExpanded = true;
                }
            }

            TreeViewItemContainerData dragItemChild = tvItem.FirstChild(this);
            TreeViewItemContainerData lastChild = null;
            if (tvDropTargetData != null)
            {
                lastChild = tvDropTargetData.LastChild(this);
                if (lastChild == null)
                {
                    lastChild = tvDropTargetData;
                }
            }
            else
            {
                
                lastChild = (TreeViewItemContainerData)LastItemContainerData();
            }

            if (lastChild != tvItem)
            {
                TreeViewItemContainerData lastDescendant = lastChild.LastDescendant(this);
                if (lastDescendant != null)
                {
                    lastChild = lastDescendant;
                }

                if (!lastChild.IsDescendantOf(tvItem))
                {
                     base.SetNextSiblingInternal(lastChild, tvItem);
                }
            }

            if (dragItemChild != null)
            {
                MoveSubtree(tvItem, dragItemChild);
            }

            if (oldParent != null && !oldParent.HasChildren(this))
            {
                VirtualizingTreeViewItem tvOldParent = (VirtualizingTreeViewItem)GetItemContainer(oldParent.Item);
                if (tvOldParent != null)
                {
                    tvOldParent.CanExpand = false;
                }
                else
                {
                    oldParent.CanExpand = false;
                }
            }
        }

        private void MoveSubtree(TreeViewItemContainerData parent, TreeViewItemContainerData child)
        {
            int parentSiblingIndex = IndexOf(parent.Item);
            int siblingIndex = IndexOf(child.Item);
            bool incrementSiblingIndex = false;
            if (parentSiblingIndex < siblingIndex)
            {
                incrementSiblingIndex = true;
            }

            TreeViewItemContainerData prev = parent;
            VirtualizingTreeViewItem tvItem = (VirtualizingTreeViewItem)GetItemContainer(prev.Item);
            if (tvItem != null)
            {
                tvItem.UpdateIndent();
            }
            while (child != null && child.IsDescendantOf(parent))
            {
                if (prev == child)
                {
                    break;
                }
                base.SetNextSiblingInternal(prev, child);

                tvItem = (VirtualizingTreeViewItem)GetItemContainer(child.Item);
                if(tvItem != null)
                {
                    tvItem.UpdateIndent();
                }

                prev = child;
                if (incrementSiblingIndex)
                {
                    siblingIndex++;
                }
                child = (TreeViewItemContainerData)GetItemContainerData(siblingIndex);
            }
        }

        protected override bool CanDrop(ItemContainerData[] dragItems, ItemContainerData dropTarget)
        {
            if(base.CanDrop(dragItems, dropTarget))
            {
                TreeViewItemContainerData tvDropTarget = (TreeViewItemContainerData)dropTarget;
                for (int i = 0; i < dragItems.Length; ++i)
                {
                    TreeViewItemContainerData dragItemData = (TreeViewItemContainerData)dragItems[i];
                    if (tvDropTarget == dragItemData || tvDropTarget != null && tvDropTarget.IsDescendantOf(dragItemData)) //disallow self parenting
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected override void Drop(ItemContainerData[] dragItems, ItemContainerData dropTarget, ItemDropAction action)
        {
            TreeViewItemContainerData tvDropTarget = (TreeViewItemContainerData)dropTarget;
            if (action == ItemDropAction.SetLastChild)
            {
                for (int i = 0; i < dragItems.Length; ++i)
                {
                    TreeViewItemContainerData dragItemData = (TreeViewItemContainerData)dragItems[i];
                    if (tvDropTarget == null || tvDropTarget != dragItemData && !tvDropTarget.IsDescendantOf(dragItemData)) //disallow self parenting
                    {
                        SetParent(tvDropTarget, dragItemData);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else if (action == ItemDropAction.SetPrevSibling)
            {
                for (int i = 0; i < dragItems.Length; ++i)
                {
                    SetPrevSibling(tvDropTarget, dragItems[i]);
                }
            }
            else if (action == ItemDropAction.SetNextSibling)
            {
                for (int i = dragItems.Length - 1; i >= 0; --i)
                {
                    SetNextSiblingInternal(tvDropTarget, dragItems[i]);
                }
            }

            UpdateSelectedItemIndex();
        }

        protected override void SetNextSiblingInternal(ItemContainerData sibling, ItemContainerData nextSibling)
        {
            TreeViewItemContainerData tvSibling = (TreeViewItemContainerData)sibling;
            TreeViewItemContainerData lastDescendant = tvSibling.LastDescendant(this);
            if (lastDescendant == null)
            {
                lastDescendant = tvSibling;
            }
            TreeViewItemContainerData tvItemData = (TreeViewItemContainerData)nextSibling;
            TreeViewItemContainerData dragItemChild = tvItemData.FirstChild(this);

            base.SetNextSiblingInternal(lastDescendant, nextSibling);
            if (dragItemChild != null)
            {
                MoveSubtree(tvItemData, dragItemChild);
            }

            SetParent(tvSibling.Parent, tvItemData);

            VirtualizingTreeViewItem tvItem = (VirtualizingTreeViewItem)GetItemContainer(tvItemData.Item);
            if (tvItem != null)
            {
                tvItem.UpdateIndent();
            }
        }

        protected override void SetPrevSibling(ItemContainerData sibling, ItemContainerData prevSibling)
        {
            TreeViewItemContainerData tvSiblingData = (TreeViewItemContainerData)sibling;
            TreeViewItemContainerData tvItemData = (TreeViewItemContainerData)prevSibling;
            TreeViewItemContainerData tvDragItemChild = tvItemData.FirstChild(this);

            base.SetPrevSibling(sibling, prevSibling);

            if (tvDragItemChild != null)
            {
                MoveSubtree(tvItemData, tvDragItemChild);
            }

            SetParent(tvSiblingData.Parent, tvItemData);

            VirtualizingTreeViewItem tvItem = (VirtualizingTreeViewItem)GetItemContainer(tvItemData.Item);
            if (tvItem != null)
            {
                tvItem.UpdateIndent();
            }
        }

        private void SetParent(TreeViewItemContainerData parent, TreeViewItemContainerData child)
        {
            VirtualizingTreeViewItem tvDragItem = (VirtualizingTreeViewItem)GetItemContainer(child.Item);
            if (tvDragItem != null)
            {
                tvDragItem.Parent = parent;
            }
            else
            {
                child.Parent = parent;
            }
        }

        public void UpdateIndent(object obj)
        {
            VirtualizingTreeViewItem item = (VirtualizingTreeViewItem)GetItemContainer(obj);
            if (item == null)
            {
                return;
            }

            item.UpdateIndent();
        }

        protected override void DestroyItems(object[] items, bool unselect)
        {
            TreeViewItemContainerData[] itemContainers = items.Select(item => GetItemContainerData(item)).OfType<TreeViewItemContainerData>().ToArray();
            TreeViewItemContainerData[] parents = itemContainers.Where(container => container.Parent != null).Select(container => container.Parent).ToArray();

            Collapse(items);

            base.DestroyItems(items, unselect);

            foreach(TreeViewItemContainerData parent in parents)
            {
                if(!parent.HasChildren(this))
                {
                    VirtualizingTreeViewItem treeViewItem = (VirtualizingTreeViewItem)GetItemContainer(parent.Item);
                    if(treeViewItem != null)
                    {
                        treeViewItem.CanExpand = false;
                    }
                }
            }
        }


    }
}
