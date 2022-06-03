using System;
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
            try {
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
                    win.Left = srn.WorkingArea.Left / dpi.DpiScaleY + 5;
                    AjustHeightPosition(win, srn.Bounds.Top, srn.Bounds.Bottom, dpi.DpiScaleX);
                }
                else if (srn.Bounds.Top < srn.WorkingArea.Top) {
                    win.Top = srn.WorkingArea.Top / dpi.DpiScaleY + 5;
                    AjustWidthPosition(win, srn.Bounds.Left, srn.Bounds.Right, dpi.DpiScaleX);
                }
                else if (srn.Bounds.Height > srn.WorkingArea.Height) {
                    win.Top = srn.WorkingArea.Bottom / dpi.DpiScaleY - win.Height - 5;
                    AjustWidthPosition(win, srn.Bounds.Top, srn.Bounds.Bottom, dpi.DpiScaleX);
                }
                else if (srn.Bounds.Width > srn.WorkingArea.Width) {
                    win.Left = srn.WorkingArea.Right / dpi.DpiScaleY - win.Width - 5;
                    AjustHeightPosition(win, srn.Bounds.Left, srn.Bounds.Right, dpi.DpiScaleX);
                }
            }
            catch (Exception ex) {
                logger?.WriteErrorLog(typeof(ScreenHelper), ex);
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

        private static void AjustHeightPosition(MainWindow win, double top, double bottom, double scale)
        {
            top = top / scale;
            bottom = bottom / scale;

            var dp = System.Windows.Forms.Cursor.Position;
            var y = dp.Y / scale - win.Height / 2;
            if (top > y) {
                win.Top = top;
            }
            else if (bottom - win.Height < y) {
                win.Top = bottom - win.Height;
            }
            else {
                win.Top = y;
            }
        }
    }
}
