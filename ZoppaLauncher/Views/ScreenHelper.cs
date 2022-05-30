using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;

namespace ZoppaLauncher.Views
{
    /// <summary>画面表示位置調整機能です。</summary>
    public static class ScreenHelper
    {
        public static void AjustWindowPosition(this MainWindow win)
        {
            var srn = Screen.AllScreens.Where(
                (s) => {
                    return s.Bounds.Contains(System.Windows.Forms.Cursor.Position);
                }
            ).First();

            var dpi = VisualTreeHelper.GetDpi(win);

            if (srn.Bounds.Left < srn.WorkingArea.Left) {
                
            }
            else if (srn.Bounds.Top < srn.WorkingArea.Top) {
                win.Top = srn.WorkingArea.Top / dpi.DpiScaleY + 5;
                AjustWidthPosition(win, srn.Bounds.Left, srn.Bounds.Right, dpi.DpiScaleX);
            }
            else if (srn.Bounds.Height > srn.WorkingArea.Height) {
                win.Top = srn.WorkingArea.Bottom / dpi.DpiScaleY - win.Height - 5;
                AjustWidthPosition(win, srn.Bounds.Left, srn.Bounds.Right, dpi.DpiScaleX);
            }
            else if (srn.Bounds.Width > srn.WorkingArea.Width) {
                
            }
        }

        private static void AjustWidthPosition(MainWindow win, double left, double right, double scale)
        {
            left = left / scale;
            right = right / scale;

            var dp = System.Windows.Forms.Cursor.Position;
            var x = dp.X / scale - win.Width / 2;
            if (left > x) {
                win.Left = left;
            }
            else if (right - win.Width < x) {
                win.Left = right - win.Width;
            }
            else {
                win.Left = x;
            }
        }
    }
}
