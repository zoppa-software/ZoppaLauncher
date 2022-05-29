using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using ZoppaShortcutLibrary;

namespace ZoppaLauncher.Models
{
    /// <summary>画面情報。</summary>
    public sealed class LauncherForm : INotifyPropertyChanged
    {
        /// <summary>プロパティ変更イベントです。</summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        // セル情報コレクション
        private ObservableCollection<CellInformation> _cells;

        // ページコレクション
        private ObservableCollection<PageBarInformation> _pages;

        // 背景色
        private Brush _color;

        /// <summary>コンストラクタ。</summary>
        public LauncherForm()
        {
            this._cells = new ObservableCollection<CellInformation>();
            this._pages = new ObservableCollection<PageBarInformation>();
            this._color = new SolidColorBrush(Color.FromRgb(26, 119, 189));
        }

        /// <summary>アイコン情報リストを設定します。</summary>
        /// <param name="page">ページ。</param>
        /// <param name="pageMax">最大ページ。</param>
        /// <param name="wcount">列セル数。</param>
        /// <param name="hcount">行セル数。</param>
        /// <param name="iconPairs">アイコン情報リスト。</param>
        public void SetPage(int page, int pageMax, 
            int wcount, int hcount, List<LauncherCollection.IconPair> iconPairs)
        {
            // セルマップを作成
            this._cells.Clear();
            for (int y = 0; y < hcount; ++y) {
                for (int x = 0; x < wcount; ++x) {
                    this._cells.Add(new CellInformation(page, x, y));
                }
            }

            // アイコン設定展開
            foreach (var pair in iconPairs) {
                int index = pair.position.Row * hcount + pair.position.Column;
                this._cells[index] = new CellInformation(
                    page,
                    pair.position.Column,
                    pair.position.Row,
                    pair.informatition.Name, 
                    pair.informatition.ShortcutImage,
                    pair.informatition.ShortcutPath
                );
            }

            // ページを設定
            this._pages.Clear();
            for (int i = 0; i <= pageMax; ++i) {
                this._pages.Add(new PageBarInformation(i, i == page));
            }

            // 変更を通知
            this.OnPropertyChanged(nameof(this.Cells));
            this.OnPropertyChanged(nameof(this.Pages));
        }

        /// <summary>プロパティ変更イベントの発行を行います。</summary>
        /// <param name="name">プロパティ名。</param>
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>セル情報コレクションを取得します。</summary>
        public ObservableCollection<CellInformation> Cells { 
            get { return this._cells; }
            private set {
                this._cells = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>ページコレクションを取得します。</summary>
        public ObservableCollection<PageBarInformation> Pages {
            get { return this._pages; }
            private set {
                this._pages = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>背景色を取得します。</summary>
        public Brush BackColor {
            get { return this._color; }
            set {
                this._color = value;
                this.OnPropertyChanged();
            }
        }
    }
}
