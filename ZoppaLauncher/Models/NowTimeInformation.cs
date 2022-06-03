using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace ZoppaLauncher.Models
{
    /// <summary>現在時刻情報。</summary>
    public sealed class NowTimeInformation : INotifyPropertyChanged
    {
        /// <summary>プロパティ変更イベントです。</summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        // タイマー
        private DispatcherTimer _timer;

        /// <summary>コンストラクタ。</summary>
        public NowTimeInformation()
        {
            this._timer = new DispatcherTimer();
            this._timer.Tick += new EventHandler(Timer_Tick);
            this._timer.Interval = TimeSpan.FromMilliseconds(100);
            this._timer.Start();
        }

        /// <summary>タイマーイベントハンドラ。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベントオブジェクト。</param>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            this.Time = DateTime.Now;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Time)));
        }

        /// <summary>現在時刻を取得します。</summary>
        public DateTime Time { get; private set; }

        /// <summary>タイマーの有効、無効を設定します。</summary>
        public bool IsEnable {
            get { return this._timer.IsEnabled; }
            set {
                if (value) {
                    if (!this._timer.IsEnabled) { 
                        this._timer.Start();
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Time)));
                    }
                }
                else {
                    if (this._timer.IsEnabled) { this._timer.Stop(); }
                }
            }
        }
    }
}
