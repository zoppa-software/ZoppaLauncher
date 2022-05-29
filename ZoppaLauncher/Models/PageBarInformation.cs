using System.ComponentModel;

namespace ZoppaLauncher.Models
{
    /// <summary>ページ情報。</summary>
    public sealed class PageBarInformation : INotifyPropertyChanged
    {
        /// <summary>プロパティ変更イベントです。</summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>コンストラクタ。</summary>
        /// <param name="idx">ページインデックス。</param>
        /// <param name="seld">選択状態。</param>
        public PageBarInformation(int idx, bool seld)
        {
            this.Index = idx;
            this.IsSelect = seld;

            this.OnPropertyChanged(nameof(this.Index));
            this.OnPropertyChanged(nameof(this.IsSelect));
        }

        /// <summary>プロパティ変更イベントの発行を行います。</summary>
        /// <param name="name">プロパティ名。</param>
        private void OnPropertyChanged(string? name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>ページインデックスを取得する。</summary>
        public int Index { get; }

        /// <summary>選択状態を取得する。</summary>
        public bool IsSelect { get; }
    }
}
