using System.Windows;
using TFlex.Model;
using TFlex.Model.Model2D;

namespace TFlex
{
    public class SerifsBuilder
    {
        public static void Run()
        {
            var document = Application.ActiveDocument;
            if (document == null)
                return;

            document.BeginChanges("SerifsBuilder");

            foreach (var obj in document.Selection.GetAllObjects())
            {
                if (obj.GroupType != ObjectType.Dimension)
                    continue;

                if (!(obj is LinearDimension dimension))
                    continue;

                if (dimension.LinkedStart.Node is IntersectionNode node1)
                {
                    CreateSerifs(document, node1);
                }

                if (dimension.LinkedEnd.Node is IntersectionNode node2)
                {
                    CreateSerifs(document, node2);
                }
            }

            document.EndChanges();
        }

        private static ObjectNode GetNode(IntersectionNode inode, Construction construction, int flag)
        {
            double r = 0.0;
            ObjectNode targetNode = null;

            foreach (var parent in construction.Parents)
            {
                if (!(parent.Object is ObjectNode node))
                    continue;

                double distance = inode.GetDistance(node.Coordinates);

                if (flag > 0)
                {
                    if (distance > r)
                    {
                        r = distance; // max
                        targetNode = node;
                    }
                }
                else
                {
                    if (distance < r || r == 0.0)
                    {
                        r = distance; // min
                        targetNode = r > 0.0 ? node : null;
                    }
                }
            }

            return targetNode;
        }

        private static Vector GetVector(IntersectionNode inode, ObjectNode onode, double radius)
        {
            Vector v1 = new Vector(inode.AbsX, inode.AbsY);
            Vector v2 = new Vector(onode.AbsX, onode.AbsY);
            Vector v3 = Vector.Subtract(v1, v2);
            double r1 = radius / v3.Length;
            Vector v4 = Vector.Multiply(r1, v3);
            return v1 + v4;
        }

        private static void CreateSerifs(Document document, IntersectionNode node)
        {
            var on_1 = GetNode(node, node.Construction1, 0);
            var on_2 = GetNode(node, node.Construction2, 0);

            if (on_1 == null || on_2 == null)
                return;

            foreach (Node item in document.GetNodes())
            {
                if (item.SubType != NodeType.BetweenNode)
                    continue;

                foreach (var p in item.Parents)
                {
                    if (p.Object.Equals(on_1) || p.Object.Equals(on_2))
                    {
                        return;
                    }
                }
            }

            var page = node.Page;
            var bn_1 = new BetweenNode(document, on_1, on_2, 0.5) { Page = page };

            document.ApplyChanges();

            var point = new Drawing.Point(bn_1.AbsX * page.Scale.Value, bn_1.AbsY * page.Scale.Value);
            double radius = node.GetDistance(point) * (1 / page.Scale.Value);

            Vector v1 = GetVector(node, on_1, radius);
            Vector v2 = GetVector(node, on_2, radius);

            var fn_1 = new FreeNode(document, v1.X, v1.Y) { Page = page };
            var fn_2 = new FreeNode(document, v2.X, v2.Y) { Page = page };

            document.ApplyChanges();

            _ = new ConstructionOutline(document, on_1, fn_1)
            {
                Page = page,
                Style = OutlineStyle.Thin
            };
            _ = new ConstructionOutline(document, on_2, fn_2)
            {
                Page = page,
                Style = OutlineStyle.Thin
            };

            document.ApplyChanges();
        }
    }
}
