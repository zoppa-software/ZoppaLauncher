using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ZoppaLauncher.Views
{
    public static class MouseStayHelper
    {
        public interface INotification
        {
            void MouseMoving();

            void MouseStay(FrameworkElement element);
        };

        public static State MouswStaySurveillance(this FrameworkElement element)
        {
            if (element is INotification) {
                return new State((INotification)element, element);
            }
            else {
                throw new ArgumentException("not implement INotification interface error");
            }
        }


        public sealed class State
        {
            // 移動を通知するインターフェイス
            private INotification _notification;

            // マウス移動を判定するエレメント
            private FrameworkElement _target;

            // マウス停止タイマー
            private DispatcherTimer _stayTimer;

            // マウス停止フラグ
            private bool _stayed;

            // マウス停止時刻
            private DateTime _stayTime;

            // マウス位置
            private Point _stayPoint;

            public State(INotification notification, FrameworkElement target)
            {
                this._notification = notification;
                this._target = target;
                this._stayTimer = new DispatcherTimer();
                this._stayTimer.Tick += new EventHandler(StayTimer_Tick);
                this._stayTimer.Interval = TimeSpan.FromMilliseconds(100);
                this._stayTimer.Start();
                this._stayed = false;
                this._stayTime = DateTime.MinValue;
                this._stayPoint = new Point(-10000, -10000);
            }

            /// <summary>マウス停止タイマーイベントハンドラ。</summary>
            /// <param name="sender">イベント発行元。</param>
            /// <param name="e">イベントオブジェクト。</param>
            private void StayTimer_Tick(object? sender, EventArgs e)
            {
                var dp = System.Windows.Forms.Cursor.Position;
                var pt = this._target.PointFromScreen(new Point(dp.X, dp.Y));

                if (pt.X > 0 && pt.Y > 0 && pt.X < this._target.ActualWidth && pt.Y < this._target.ActualHeight) {
                    if (this._stayPoint != pt) {
                        // マウス位置の移動があれば時刻チェック
                        this._stayPoint = pt;
                        this._stayTime = DateTime.Now;
                        this._stayed = false;
                        this._notification.MouseMoving();
                    }
                    else if (DateTime.Now.Subtract(this._stayTime).TotalSeconds > 1.0) {
                        // マウスの移動が一定期間以上なければ通知
                        if (!this._stayed) {
                            this._stayed = true;
                            var hits = VisualTreeHelper.HitTest(this._target, pt);
                            var element = hits.VisualHit as FrameworkElement;
                            if (element != null && element.Visibility == Visibility.Visible) {
                                this._notification.MouseStay(element);
                            }
                        }
                    }
                }
                else {
                    // マウス位置がウィンドウ以外ならば非表示にする
                    this._notification.MouseMoving();
                    this._stayed = false;
                }
            }
        }
    }
}
