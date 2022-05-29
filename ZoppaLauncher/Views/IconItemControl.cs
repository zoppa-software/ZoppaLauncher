using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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

        /// <summary>削除イベントデリゲート。</summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="delCell">削除セル。</param>
        public delegate void RemoveIconHandler(object sender, CellInformation delCell);

        /// <summary>ドロップイベント。</summary>
        public event DropLinkFileHandler? DropLinkFile;

        /// <summary>入れ替えイベント。</summary>
        public event MoveIconHandler? MoveIcon;

        /// <summary>削除イベント。</summary>
        public event RemoveIconHandler? RemoveIcon;

        // ゴースト表示レイヤ
        private AdornerLayer? _layer;

        // ゴースト
        private DragFileGhost? _dragGhost = null;

        // 操作中セル情報（入れ替え、削除用）
        private CellInformation? _selectedIcon = null;

        // クリック位置
        private Point? _clickPoint;

        /// <summary>コンストラクタ。</summary>
        public IconItemControl() : base()
        {
            this.AllowDrop = true;
            this._dragGhost = null;
            this._layer = null;
            this._clickPoint = null;
        }

        /// <summary>マウス押下イベントです。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            this._clickPoint = e.GetPosition(this);
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

            // クリック位置より大きな動きがあれば入れ替え、削除コーストを作成
            if (this._clickPoint != null && this._selectedIcon?.LinkPath != null && 
                this._dragGhost == null && !this.StayMousePosition(e.GetPosition(this))) {
                this._dragGhost = new DragFileGhost(this, IconInformation.Load(MOVE_TAG, this._selectedIcon.LinkPath ));
                this.Layer.Add(this._dragGhost);
                this._clickPoint = null;
                this.CaptureMouse();
            }
            this._dragGhost?.InvalidateVisual();
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
                        this.MoveIcon?.Invoke(this, this._selectedIcon, info, this._dragGhost?.LinkPath ?? "");
                    }
                    else if (hits == null) {
                        this.RemoveIcon?.Invoke(this, this._selectedIcon);
                    }

                    // ゴーストをレイヤーより削除
                    this.Layer.Remove(this._dragGhost);
                    this._dragGhost = null;
                    this._selectedIcon = null;
                    this.InvalidateVisual();
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.OnMouseUp)}:{ex.ToString()}");
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
                    e.Effects = DragDropEffects.None;

                    // ドラッグされたファイルを調査し、対象ファイルならゴーストを表示する
                    var files = target as string[];
                    if (files != null && files.Length > 0 && 
                        (Path.GetExtension(files[0]).ToLower() == ".lnk" || Path.GetExtension(files[0]).ToLower() == ".exe")) {
                        e.Effects = DragDropEffects.Move;

                        this._dragGhost = new DragFileGhost(this, IconInformation.Load(DRAG_TAG, files[0]));
                        this.Layer.Add(this._dragGhost);
                    }
                    e.Handled = true;
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.OnDragEnter)}:{ex.ToString()}");
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
                Debug.WriteLine($"{nameof(this.OnDragOver)}:{ex.ToString()}");
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
                        this.DropLinkFile?.Invoke(this, info, this._dragGhost?.LinkPath ?? "");
                    }

                    // ゴーストをレイヤーより削除
                    this.Layer.Remove(this._dragGhost);
                    this._dragGhost = null;
                    this.InvalidateVisual();
                }
                catch (Exception ex) {
                    Debug.WriteLine($"{nameof(this.OnDrop)}:{ex.ToString()}");
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
                    this.Layer.Remove(this._dragGhost);
                    this._dragGhost = null;
                    this.InvalidateVisual();
                }
                catch (Exception ex) {
                    Debug.WriteLine($"{nameof(this.OnDragLeave)}:{ex.ToString()}");
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
