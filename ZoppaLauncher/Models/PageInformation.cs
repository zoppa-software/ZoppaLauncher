using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoppaLauncher.Models
{
    /// <summary>ページ情報。</summary>
    public sealed class PageInformation : INotifyPropertyChanged
    {
        /// <summary>プロパティ変更イベントです。</summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        public PageInformation(int idx, bool seld)
        {
            this.Index = idx;
            this.IsSelect = seld;
            this.Width = (seld ? 35 : 15);

            this.OnPropertyChanged("Index");
            this.OnPropertyChanged("IsSelect");
            this.OnPropertyChanged("Width");
        }

        /// <summary>プロパティ変更イベントの発行を行います。</summary>
        /// <param name="name">プロパティ名。</param>
        private void OnPropertyChanged(string? name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public int Index { get; }

        public bool IsSelect { get; }

        public int Width { get; }

    }
}
