using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using ZoppaLauncher.Logs;

namespace ZoppaLauncher.Views
{
    /// <summary>画面表示位置調整機能です。</summary>
    public static class ScreenHelper
    {
        /// <summary>ウィンドウの位置をタスクバーの位置に合わせて調整します。</summary>
        /// <param name="win">メインウィンドウ位置。</param>
        public static void AjustWindowPosition(this MainWindow win, ILogWriter? logger = null)
        {
            logger?.WriteLog(typeof(ScreenHelper), "ajust window position start");
            var srn = Screen.AllScreens.Where(
                (s) => {
                    // マウスカーソルのあるウィンドウを取得します
                    return s.Bounds.Contains(System.Windows.Forms.Cursor.Position);
                }
            ).First();

            // 拡大率が設定のあるウィンドウの計算のため、DPIを取得します
            var dpi = VisualTreeHelper.GetDpi(win);

            // タスクバーの位置に合わせてウィンドウ位置を設定
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
