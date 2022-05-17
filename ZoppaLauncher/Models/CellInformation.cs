using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ZoppaLauncher.Models
{
    /// <summary>セル情報。</summary>
    public sealed class CellInformation : INotifyPropertyChanged
    {
        /// <summary>プロパティ変更イベントです。</summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        // サイズ情報
        private int _page, _x, _y, _width, _height;

        /// <summary>コンストラクタ。</summary>
        /// <param name="page">ページNo。</param>
        /// <param name="x">横位置。</param>
        /// <param name="y">縦位置。</param>
        public CellInformation(int page, int x, int y)
        {
            this._page = page;
            this._x = x;
            this._y = y;
            this.Name = "";
            this.Image = null;
            this.LinkPath = null;
        }

        /// <summary>コンストラクタ。</summary>
        /// <param name="page">ページNo。</param>
        /// <param name="x">横位置。</param>
        /// <param name="y">縦位置。</param>
        /// <param name="name">セル名称。</param>
        /// <param name="image">セルアイコン。</param>
        /// <param name="linkPath">リンク先パス。</param>
        public CellInformation(int page, int x, int y, string name, ImageSource image, string linkPath)
        {
            this._page = page;
            this._x = x;
            this._y = y;
            this.Name = name;
            this.Image = image;
            this.LinkPath = linkPath;
        }

        /// <summary>プロパティ変更イベントの発行を行います。</summary>
        /// <param name="name">プロパティ名。</param>
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>幅を取得します。</summary>
        public int Width {
            get { return this._width; }
            set {
                this._width = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>高さを取得します。</summary>
        public int Height {
            get { return this._height; }
            set {
                this._height = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>ページを取得します。</summary>
        public int Page { get { return this._page; } }

        /// <summary>列位置を取得します。</summary>
        public int Column { get { return this._x; } }

        /// <summary>行位置を取得します。</summary>
        public int Row { get { return this._y; } }

        /// <summary>名前を取得します。</summary>
        public string Name { get; }

        /// <summary>アイコン画像を取得します。</summary>
        public ImageSource? Image { get; }

        /// <summary>リンク先パスを取得します。</summary>
        public string? LinkPath { get; }
    }
}
