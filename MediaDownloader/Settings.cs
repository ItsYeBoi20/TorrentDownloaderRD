// This file is only for the people who downloaded the previous version before I renamed the files

using RealDebridAPI;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaDownloader
{
    public partial class Settings : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
        private const int IDC_HAND = 32649;

        public static string currentVersion = "1.0.2";
    }
}
