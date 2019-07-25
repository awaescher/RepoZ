using System;
using CoreGraphics;

namespace RepoZ.App.Mac
{
    public static class GeometryHelper
    {
        public static CGRect WithLeft(this CGRect rect, nfloat left)
        {
            return new CGRect(left, rect.Top, rect.Width, rect.Height);
        }

        public static CGRect WithTop(this CGRect rect, nfloat top)
        {
            return new CGRect(rect.Left, top, rect.Width, rect.Height);
        }

        public static CGRect WithWidth(this CGRect rect, nfloat width)
        {
            return new CGRect(rect.Left, rect.Top, width, rect.Height);
        }

        public static CGRect WithHeight(this CGRect rect, nfloat height)
        {
            return new CGRect(rect.Left, rect.Top, rect.Width, height);
        }
    }
}
 