using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZRGPictureBox
{
    public class cCommonCursors
    {
        [DllImport("user32.dll", EntryPoint = "DestroyIcon")]
        private static extern bool DestroyIcon([System.Runtime.InteropServices.InAttribute()] System.IntPtr hIcon);
        private Icon myInternalIcon = null;
        private System.Windows.Forms.Cursor myCustomCursor = null;
        public enum enCursorType : int
        {
            Zoom = 0,
            Edit = 2
        }
        public cCommonCursors(enCursorType CursorType)
        {
            try
            {
                if (CursorType == enCursorType.Zoom)
                {
                    //Bitmap bmp = LoadBmpRes("Zoom-32.png");
                    Bitmap bmp = global::ZRGPictureBox.Properties.Resources.Zoom_32;
                    myInternalIcon = Icon.FromHandle(bmp.GetHicon());
                    myCustomCursor = new Cursor(myInternalIcon.Handle);
                }
                else if (CursorType == enCursorType.Edit)
                {
                    //Bitmap bmp = LoadBmpRes("Edit.png");
                    Bitmap bmp = global::ZRGPictureBox.Properties.Resources.Edit;
                    myInternalIcon = Icon.FromHandle(bmp.GetHicon());
                    myCustomCursor = new Cursor(myInternalIcon.Handle);
                }
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
            }
        }

        //cursorName = assemblyName + "." + cursorName
        static string[] static_LoadBmpRes_resourcesNames;
        private static System.Drawing.Bitmap LoadBmpRes(string cursorName)
        {
            try
            {
                System.Reflection.Assembly thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                string assemblyName = thisAssembly.GetName().Name;
                static_LoadBmpRes_resourcesNames = thisAssembly.GetManifestResourceNames();
                foreach (string name in static_LoadBmpRes_resourcesNames)
                {
                    if (name.EndsWith(cursorName))
                    {
                        System.IO.Stream file = thisAssembly.GetManifestResourceStream(name);
                        return new System.Drawing.Bitmap(file);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
                return null;
            }
        }
        public Cursor CustomCursor
        {
            get { return myCustomCursor; }
        }
        private static cCommonCursors myEditCursorHelper;
        public static Cursor EditCursor
        {
            get
            {
                try
                {
                    if (myEditCursorHelper == null)
                    {
                        myEditCursorHelper = new cCommonCursors(cCommonCursors.enCursorType.Edit);
                    }
                    return myEditCursorHelper.CustomCursor;
                }
                catch (Exception ex)
                {
                    //Interaction.MsgBox(ex.Message);
                    return Cursors.No;
                }
            }
        }
        private static cCommonCursors myZoomCursorHelper;
        public static Cursor ZoomCursor
        {
            get
            {
                try
                {
                    if (myZoomCursorHelper == null)
                    {
                        myZoomCursorHelper = new cCommonCursors(cCommonCursors.enCursorType.Zoom);
                    }
                    return myZoomCursorHelper.CustomCursor;
                }
                catch (Exception ex)
                {
                    //Interaction.MsgBox(ex.Message);
                    return Cursors.No;
                }
            }
        }
    }
}
