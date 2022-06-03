using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ZoppaLauncher.Logs;
using ZoppaLauncher.Models;
using ZoppaShortcutLibrary;

namespace ZoppaLauncher.Views
{
    /// <summary>ドラッグ・ドロップの表示を行うためのコントロール。</summary>
    public sealed class IconItemControl : ItemsControl
    {
        /// <summary>ドラッグ・ドロップ中ゴースト識別タグ。</summary>
        public const string DRAG_TAG = "drag";

        /// <summary>入れ替え中ゴースト識別タグ。</summary>
        public const string MOVE_TAG = "move";

        /// <summary>ドロップイベントデリゲート。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="toCell">ドロップ先セル。</param>
        /// <param name="linkPath">リンク先パス。</param>
        public delegate void DropLinkFileHandler(object sender, CellInformation toCell, string linkPath);

        /// <summary>入れ替えイベントデリゲート。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="fromCell">元セル。</param>
        /// <param name="toCell">先セル。</param>
        /// <param name="linkPath">リンク先パス。</param>
        public delegate void MoveIconHandler(object sender, CellInformation fromCell, CellInformation toCell, string linkPath);

        /// <summary>選択イベントデリゲート。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="delCell">削除セル。</param>
        public delegate void SelectIconHandler(object sender, CellInformation delCell);

        /// <summary>ドロップイベント。</summary>
        public event DropLinkFileHandler? DropLinkFile;

        /// <summary>入れ替えイベント。</summary>
        public event MoveIconHandler? MoveIcon;

        /// <summary>削除イベント。</summary>
        public event SelectIconHandler? RemoveIcon;

        /// <summary>マウス移動イベント。</summary>
        public event EventHandler? MouseMovingIcon;

        /// <summary>マウス待機イベント。</summary>
        public event SelectIconHandler? MouseStayIcon;

        // ゴースト表示レイヤ
        private AdornerLayer? _layer;

        // ゴースト
        private DragFileGhost? _dragGhost = null;

        // 操作中セル情報（入れ替え、削除用）
        private CellInformation? _selectedIcon = null;

        // クリック位置
        private Point? _clickPoint;

        // マウス停止タイマー
        private DispatcherTimer _stayTimer;

        // マウス停止フラグ
        private bool _stayed;

        // マウス停止時刻
        private DateTime _stayTime;

        // マウス位置
        private Point _stayPoint;

        // ログ出力機能
        private ILogWriter? _logger;

        /// <summary>コンストラクタ。</summary>
        public IconItemControl() : base()
        {
            this.AllowDrop = true;
            this._dragGhost = null;
            this._layer = null;
            this._clickPoint = null;
            this._logger = (App.Current as App)?.DiProvider?.GetService<ILogWriter>();
            this._stayTimer = new DispatcherTimer();
            this._stayTimer.Tick += new EventHandler(StayTimer_Tick);
            this._stayTimer.Interval = TimeSpan.FromMilliseconds(100);
            this._stayTimer.Start();
            this._stayed = false;
            this._stayTime = DateTime.MinValue;
            this._stayPoint = new Point(-10000, -10000);
        }

        /// <summary>マウス押下イベントです。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            try {
                this._clickPoint = e.GetPosition(this);
                _logger?.WriteLog(this, $"mouse down x:{this._clickPoint?.X} y:{this._clickPoint?.Y}");
            }
            catch (Exception ex) {
                _logger?.WriteErrorLog(this, ex);
            }
        }

        /// <summary>選択されたマウス位置とセル情報を消去します。</summary>
        public void ClearCellInformation()
        {
            this._clickPoint = null;
            this._selectedIcon = null;
        }

        /// <summary>選択されたマウス位置とセル情報を記憶します。</summary>
        /// <param name="point">マウス位置。</param>
        /// <param name="icon">セル情報。</param>
        public void SelectCellInformation(Point point, CellInformation icon)
        {
            this._clickPoint = point;
            this._selectedIcon = icon;
        }

        /// <summary>マウス移動イベントです。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            try {
                // クリック位置より大きな動きがあれば入れ替え、削除コーストを作成
                if (this._clickPoint != null && this._selectedIcon?.LinkPath != null &&
                    this._dragGhost == null && !this.StayMousePosition(e.GetPosition(this))) {
                    _logger?.WriteLog(this, "drag start! (icon move)");
                    this._dragGhost = new DragFileGhost(this, IconInformation.Load(MOVE_TAG, this._selectedIcon.LinkPath));
                    this.Layer.Add(this._dragGhost);
                    this._clickPoint = null;
                    this.CaptureMouse();
                }
                this._dragGhost?.InvalidateVisual();
            }
            catch (Exception ex) {
                _logger?.WriteErrorLog(this, ex);
            }
        }

        /// <summary>クリック位置より大きな動きがなければ真を返します。</summary>
        /// <param name="point">現在の位置。</param>
        /// <returns>大きな動きがなければ真。</returns>
        public bool StayMousePosition(Point point)
        {
            return (this._clickPoint != null && 
                Math.Sqrt(
                    Math.Pow((this._clickPoint?.X ?? 0) - point.X, 2) + 
                    Math.Pow((this._clickPoint?.Y ?? 0) - point.Y, 2)) <= 8);
        }

        /// <summary>マウス停止タイマーイベントハンドラ。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベントオブジェクト。</param>
        private void StayTimer_Tick(object? sender, EventArgs e)
        {
            var dp = System.Windows.Forms.Cursor.Position;
            var pt = this.PointFromScreen(new Point(dp.X, dp.Y));

            if (pt.X > 0 && pt.Y > 0 && pt.X < this.ActualWidth && pt.Y < this.ActualHeight) {
                if (this._stayPoint != pt) {
                    // マウス位置の移動があれば時刻チェック
                    this._stayPoint = pt;
                    this._stayTime = DateTime.Now;
                    this._stayed = false;
                    this.MouseMovingIcon?.Invoke(this, EventArgs.Empty);
                }
                else if (DateTime.Now.Subtract(this._stayTime).TotalSeconds > 1.0) {
                    // マウスの移動が一定期間以上なければ通知
                    if (!this._stayed) {
                        this._stayed = true;
                        var hits = VisualTreeHelper.HitTest(this, pt);
                        CellInformation? info = null;
                        if ((info = (hits.VisualHit as FrameworkElement)?.DataContext as CellInformation) != null) {
                            this.MouseStayIcon?.Invoke(hits.VisualHit, info);
                        }
                    }
                }
            }
            else {
                // マウス位置がウィンドウ以外ならば非表示にする
                this.MouseMovingIcon?.Invoke(this, EventArgs.Empty);
                this._stayed = false;
            }
        }

        /// <summary>マウスアップイベントです。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            try {
                this._clickPoint = null;

                // 入れ替え、削除操作中のゴースト、かつ、セル情報を取得していること
                if (this._dragGhost?.Name == MOVE_TAG && this._selectedIcon != null) {
                    // マウスキャプチャを解放
                    this.ReleaseMouseCapture();

                    // マウスアップされた位置を調査し、セルがあれは入れ替え、なければ削除
                    var hits = VisualTreeHelper.HitTest(this, e.GetPosition(this));
                    CellInformation? info = null;
                    if ((info = (hits?.VisualHit as FrameworkElement)?.DataContext as CellInformation) != null) {
                        _logger?.WriteLog(this, $"drop, move icon x:{this._selectedIcon.Column} y:{this._selectedIcon.Row} → x:{info.Column} y:{info.Row}");
                        this.MoveIcon?.Invoke(this, this._selectedIcon, info, this._dragGhost?.LinkPath ?? "");
                    }
                    else if (hits == null) {
                        _logger?.WriteLog(this, "drop, remove icon");
                        this.RemoveIcon?.Invoke(this, this._selectedIcon);
                    }

                    // ゴーストをレイヤーより削除
                    _logger?.WriteLog(this, "remove icon ghost");
                    this.Layer.Remove(this._dragGhost);
                    this._dragGhost = null;
                    this._selectedIcon = null;
                    this.InvalidateVisual();
                }
            }
            catch (Exception ex) {
                _logger?.WriteErrorLog(this, ex);
            }
        }

        /// <summary>ドラッグ・ドロップ操作中のドラッグ開始イベント処理です。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnDragEnter(DragEventArgs e)
        {
            try {
                if (this._dragGhost == null) {
                    // ドラッグ情報を取得する
                    var target = e.Data.GetData(DataFormats.FileDrop);
                    
                    // ドラッグされたファイルを調査し、対象ファイルならゴーストを表示する
                    var files = target as string[];
                    if (files != null && files.Length > 0 && 
                        (Path.GetExtension(files[0]).ToLower() == ".lnk" || Path.GetExtension(files[0]).ToLower() == ".exe")) {
                        _logger?.WriteLog(this, $"drag start! (apend link {files[0]})");
                        e.Effects = DragDropEffects.Move;

                        this._dragGhost = new DragFileGhost(this, IconInformation.Load(DRAG_TAG, files[0]));
                        this.Layer.Add(this._dragGhost);
                    }
                    else {
                        e.Effects = DragDropEffects.None;
                    }

                    e.Handled = true;
                }
            }
            catch (Exception ex) {
                _logger?.WriteErrorLog(this, ex);
            }
        }

        /// <summary>ドラッグ・ドロップ操作中のドラッグ中イベント処理です。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnDragOver(DragEventArgs e)
        {
            try {
                this._dragGhost?.InvalidateVisual();
            }
            catch (Exception ex) {
                _logger?.WriteErrorLog(this, ex);
            }
        }

        /// <summary>ドラッグ・ドロップ操作中のドロップイベント処理です。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnDrop(DragEventArgs e)
        {
            // ドラッグ・ドロップ操作中のゴーストであること
            if (this._dragGhost?.Name == DRAG_TAG) {
                try {
                    // マウス位置のセル情報を取得できたらイベントで通知
                    var hits = VisualTreeHelper.HitTest(this, e.GetPosition(this));
                    CellInformation? info = null;
                    if ((info = (hits.VisualHit as FrameworkElement)?.DataContext as CellInformation) != null) {
                        _logger?.WriteLog(this, "fire drop link file event");
                        this.DropLinkFile?.Invoke(this, info, this._dragGhost?.LinkPath ?? "");
                    }

                    // ゴーストをレイヤーより削除
                    _logger?.WriteLog(this, "remove icon ghost");
                    this.Layer.Remove(this._dragGhost);
                    this._dragGhost = null;
                    this.InvalidateVisual();
                }
                catch (Exception ex) {
                    _logger?.WriteErrorLog(this, ex);
                }
            }    
        }

        /// <summary>ドラッグ・ドロップ、リーブイベント処理です。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnDragLeave(DragEventArgs e)
        {
            if (this._dragGhost?.Name == DRAG_TAG) {
                try {
                    // ゴーストをレイヤーより削除
                    _logger?.WriteLog(this, "remove ghost");
                    this.Layer.Remove(this._dragGhost);
                    this._dragGhost = null;
                    this.InvalidateVisual();
                }
                catch (Exception ex) {
                    _logger?.WriteErrorLog(this, ex);
                }
            }
        }

        /// <summary>ゴースト描画レイヤーを取得します。</summary>
        private AdornerLayer Layer {
            get {
                if (this._layer == null) {
                    this._layer = AdornerLayer.GetAdornerLayer(this);
                    this._layer.IsHitTestVisible = false;
                    this._layer.Opacity = 0.7;
                }
                return this._layer; 
            }
        }

        /// <summary>入れ替え、削除操作中のセル情報があれば真を返します。</summary>
        public bool IsSelectedIcon {
            get { return this._selectedIcon != null; }
        }

        /// <summary>ゴースト情報。</summary>
        private sealed class DragFileGhost : Adorner
        {
            // 対象エレメント
            private readonly UIElement _target;

            // アイコン情報
            private readonly IconInformation _iconInfo;

            /// <summary>コンストラクタ。</summary>
            /// <param name="ui">対象エレメント。</param>
            /// <param name="icon">アイコン情報。</param>
            public DragFileGhost(UIElement ui, IconInformation icon) :
                base(ui)
            {
                this._target = ui;
                this._iconInfo = icon;
                this.Name = icon.Name;
                this.AllowDrop = false;
            }

            /// <summary>ゴーストを描画します。</summary>
            /// <param name="drawingContext">描画コンテキスト。</param>
            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                // 描画サイズを計算
                var dpi = VisualTreeHelper.GetDpi(this);
                var wid = this.Image.Width / dpi.DpiScaleX;
                var hid = this.Image.Height / dpi.DpiScaleY;

                // 現在のマウス位置を中心に画像を表示
                var dp = System.Windows.Forms.Cursor.Position;
                var pt = this._target.PointFromScreen(new Point(dp.X, dp.Y));
                drawingContext.DrawImage(this.Image, 
                    new Rect(pt.X - wid / 2, pt.Y - hid / 2, wid, hid)
                );
            }

            /// <summary>アイコン画像を取得します。</summary>
            private ImageSource Image { 
                get { return this._iconInfo.ShortcutImage; } 
            }

            /// <summary>アイコンリンク先パスを取得します。</summary>
            public string LinkPath {
                get { return this._iconInfo.ShortcutPath; }
            }
        }
    }
}
