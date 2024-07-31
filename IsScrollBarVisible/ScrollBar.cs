using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IsScrollbarVisible
{
    /// <summary>apparently, there was something missing somewhere in Rectangle...</summary>
    public struct RECT
    {
        /// <summary>Left</summary>
        public int left;
        /// <summary>Top</summary>
        public int top;
        /// <summary>Right</summary>
        public int right;
        /// <summary>Bottom</summary>
        public int bottom;
    }
    public static class Scrollbars
    {
        /// <summary>Object ID for Horizontal Scrollbars</summary>
        public const uint OBJID_HSCROLL = 0xFFFFFFFA;
        /// <summary>Object ID for Vertical Scrollbars</summary>
        public const uint OBJID_VSCROLL = 0xFFFFFFFB;
        /// <summary>Provides structure for a ScrollBarInfo</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SCROLLBARINFO
        {
            /// <summary></summary>
            public int cbSize;
            /// <summary></summary>
            public RECT rcScrollBar;
            /// <summary></summary>
            public int dxyLineButton;
            /// <summary></summary>
            public int xyThumbTop;
            /// <summary></summary>
            public int xyThumbBottom;
            /// <summary></summary>
            public int reserved;
            /// <summary></summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public int[] rgstate;
        }

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetScrollBarInfo")]
        private static extern int GetScrollBarInfo(IntPtr hWnd, uint idObject, ref SCROLLBARINFO psbi);
        /// <summary>Vertical, Horizontal, either or both</summary>
        public enum V_H_Either_OrBoth
        {
            /// <summary>Vertical</summary>
            V,
            /// <summary>Horizontal</summary>
            H,
            /// <summary>Either Vertical or Horizontal</summary>
            Either,
            /// <summary>Vertical and Horizontal</summary>
            Both
        }
        public static bool IsScrollbarVisible(this Control c, V_H_Either_OrBoth which)
        {
            bool h = false;
            bool v = false;

            // this first section avoids using the pInvoke in favor of using the ScrollableControl implementation where possible

            if (c.InheritsType(typeof(ScrollableControl)))
            {
                //MessageBox.Show(c.Name += " implements ScrollableControl");
                ScrollableControl sc = (ScrollableControl)c;
                h = sc.HorizontalScroll.Visible;
                v = sc.VerticalScroll.Visible;
            }
            else
            {
                // for everything else, use a check on the pInvoked SCROLLBARINFO
                //MessageBox.Show(c.Name += " doesn't implement ScrollableControl");
                SCROLLBARINFO psbi = new SCROLLBARINFO();
                psbi.cbSize = Marshal.SizeOf(psbi);

                int HResult = GetScrollBarInfo(c.Handle, OBJID_HSCROLL, ref psbi);

                if (HResult == 0)
                {
                    MessageBox.Show("error getting SBI");
                    //int nLatError = GetLastError(); // in kernel32.dll
                }
                else
                {
                    h = psbi.rgstate[0] == 0;
                }

                int VResult = GetScrollBarInfo(c.Handle, OBJID_VSCROLL, ref psbi);

                if (VResult == 0)
                {
                    MessageBox.Show("error getting SBI");
                    //int nLatError = GetLastError(); // in kernel32.dll
                }
                else
                {
                    v = psbi.rgstate[0] == 0;
                }

            }

            switch (which)
            {
                case V_H_Either_OrBoth.V:
                    return v;
                case V_H_Either_OrBoth.H:
                    return h;
                case V_H_Either_OrBoth.Either:
                    return h || v;
                case V_H_Either_OrBoth.Both:
                    return h && v;
                default:
                    return false;
            }
        }
        /// <summary>
        /// Check whether the Type of an Object instance is a SubType of a different Type, or the same
        /// </summary>
        /// <param name="BaseType"></param>
        /// <param name="potentialDescendant"></param>
        /// <param name="includesSelf"></param>
        /// <returns></returns>
        private static bool InheritsType(this object potentialDescendant, Type BaseType, bool includesSelf = true)
        {
            return IsSubtype(potentialDescendant.GetType(), BaseType, includesSelf);
        }
        /// <summary>
        /// Check whether Type is a SubType of a different Type, or the same
        /// </summary>
        /// <param name="BaseType"></param>
        /// <param name="potentialDescendant"></param>
        /// <param name="includesSelf"></param>
        /// <returns></returns>
        private static bool IsSubtype(this Type potentialDescendant, Type BaseType, bool includesSelf = true)
        {
            return potentialDescendant.IsSubclassOf(BaseType) || (includesSelf && potentialDescendant == BaseType);
        }
    }
}