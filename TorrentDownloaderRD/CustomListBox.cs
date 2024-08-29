using System;
using System.Windows.Forms;

public class CustomListBox : ListBox
{
    private int scrollAmount;

    public CustomListBox()
    {
        this.DoubleBuffered = true;
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_MOUSEWHEEL = 0x020A;

        if (m.Msg == WM_MOUSEWHEEL)
        {
            int delta = (int)m.WParam >> 16;
            scrollAmount = (delta / SystemInformation.MouseWheelScrollDelta) * 3; // Move 3 items at a time

            int newIndex = this.TopIndex - scrollAmount;

            if (newIndex < 0)
            {
                newIndex = 0;
            }
            else if (newIndex >= this.Items.Count)
            {
                newIndex = this.Items.Count - 1;
            }

            this.TopIndex = newIndex;
            return;
        }

        base.WndProc(ref m);
    }
}
