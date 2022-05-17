using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ZoppaShortcutLibrary;

namespace ZoppaLauncher.Views
{
    public sealed class AdornerLayer : UserControl
    {
        private const string DRAG_TAG = "drag";

        private DragFileGhost? _dragGhost = null;

        public AdornerLayer()
        {
            this.IsHitTestVisible = false;  
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (this._dragGhost != null) {
                drawingContext.PushOpacity(0.8);

                var dp = System.Windows.Forms.Cursor.Position;
                var pos = this.PointFromScreen(new System.Windows.Point(dp.X, dp.Y));

                drawingContext.DrawImage(this._dragGhost.Image,
                    new Rect(
                        pos.X - this._dragGhost.Image.Width / 2,
                        pos.Y - this._dragGhost.Image.Height / 2,
                        this._dragGhost.Image.Width,
                        this._dragGhost.Image.Height
                    )
                );
            }
        }

        public void DragEnterHandler(DragEventArgs e)
        {
            if (this._dragGhost == null) {
                // ドラッグ情報を取得する
                var target = e.Data.GetData(DataFormats.FileDrop);
                e.Effects = DragDropEffects.None;

                // ドラッグされたファイルを調査し、対象ファイルならゴーストを表示する
                var files = target as string[];
                if (files != null && files.Length > 0) {
                    if (Path.GetExtension(files[0]) == ".lnk" || (Path.GetExtension(files[0]) == ".exe")) {
                        e.Effects = DragDropEffects.Move;

                        this._dragGhost = new DragFileGhost(this, IconInformation.Load(DRAG_TAG, files[0]));
                        Debug.WriteLine($"adding");
                        this.InvalidateVisual();
                    }
                }
                else {
                    throw new InvalidOperationException();
                }
                e.Handled = true;
            }
        }

        public void DragOverHandler(DragEventArgs e)
        {
            if (this._dragGhost != null) {
                this.InvalidateVisual();
            }    
        }

        public void DropHandler(DragEventArgs e)
        {
            base.OnDrop(e);
            if (this._dragGhost != null) {
                this._dragGhost = null;
                this.InvalidateVisual();
            }
        }

        private sealed class DragFileGhost
        {
            private readonly IconInformation _iconInfo;

            public DragFileGhost(UIElement ui, IconInformation icon)
            {
                this._iconInfo = icon;
            }

            public ImageSource Image {
                get { return this._iconInfo.ShortcutImage; }
            }
        }
    }
}
