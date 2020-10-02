using System;
using System.Collections.Generic;
using System.Numerics;

namespace Open3dmm.Core.Scenes
{
    public class SceneNode
    {
        private SceneNode next;
        private SceneNode parent;
        private SceneNode firstChild;

        public SceneNode Parent => parent;
        public SceneNode Next => next;
        public SceneNode Previous {
            get {
                if (parent == null || parent.firstChild == this)
                    return null;
                ref var child = ref parent.firstChild;
                while (child != null && child.next != this)
                    child = ref child.next;
                return child;
            }
        }
        public SceneNode FirstChild => firstChild;

        public Matrix4x4 LocalTransform { get; set; } = Matrix4x4.Identity;

        public IEnumerable<SceneNode> Children {
            get {
                var child = firstChild;
                while (child != null)
                {
                    yield return child;
                    child = child.next;
                }
            }
        }

        public void InsertAfter(SceneNode node)
        {
            if (this != node && this != node.next)
            {
                Remove();
                this.next = node.next;
                node.next = this;
                this.parent = node.parent;
            }
        }

        public void InsertBefore(SceneNode node)
        {
            if (this != node && this != node.next)
            {
                Remove();
                this.next = node;
                if (node.parent?.firstChild == node)
                    node.parent.firstChild = this;
                this.parent = node.parent;
            }
        }

        public void AddChild(SceneNode newChild)
        {
            newChild.Remove();
            ref var field = ref firstChild;
            while (field != null)
                field = ref field.next;
            field = newChild;
            newChild.parent = this;
        }

        public bool RemoveChild(SceneNode child)
        {
            ref var field = ref firstChild;
            while (field != null)
            {
                if (field == child)
                {
                    field = child.next;
                    child.parent = null;
                    child.next = null;
                    return true;
                }
                field = ref field.next;
            }
            return false;
        }

        public bool Remove()
        {
            return Parent?.RemoveChild(this) ?? false;
        }

        public static IEnumerable<SceneNode> Traverse(SceneNode root)
        {
            var current = root ?? throw new ArgumentNullException();
            while (true)
            {
                yield return current;
                if (current.firstChild != null)
                    current = current.firstChild;
                else
                {
                // No more children
                RootCheck:
                    if (current == root)
                        break; // Don't enumerate siblings of root!
                    if (current.next == null)
                    {
                        current = current.parent;
                        goto RootCheck;
                    }
                    current = current.next;
                }
            }
        }

        public void AcceptVisitor(SceneNodeVisitor visitor)
        {
            var current = this;
            while (true)
            {
                if (visitor.VisitNode(current) && current.firstChild != null)
                    current = current.firstChild;
                else
                {
                // No more children OR skip children
                RootCheck:
                    visitor.LeaveNode(current);
                    if (current == this)
                        break; // Don't enumerate siblings of root!
                    if (current.next == null)
                    {
                        current = current.parent;
                        goto RootCheck;
                    }
                    current = current.next;
                }
            }
        }
    }
}
