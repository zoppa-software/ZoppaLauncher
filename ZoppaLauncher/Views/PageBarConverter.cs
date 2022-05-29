using System;
using System.Globalization;
using System.Windows.Data;

namespace ZoppaLauncher.Views
{
    /// <summary>選択状態をバーの長さへ変換する。</summary>
    public class PageBarConverter : IValueConverter
    {
        /// <summary>モデルの値を画面へ変換する。</summary>
        /// <param name="value">モデルの値。</param>
        /// <param name="targetType">値の型。</param>
        /// <param name="parameter">変換パラメータ。</param>
        /// <param name="culture">カルチャー。</param>
        /// <returns>変換後の値。</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try {
                return System.Convert.ToBoolean(value) ? 35 : 15;
            }
            catch {
                return 15;
            }
        }

        /// <summary>画面の値をモデルへ変換する。</summary>
        /// <param name="value">画面の値。</param>
        /// <param name="targetType">値の型。</param>
        /// <param name="parameter">変換パラメータ。</param>
        /// <param name="culture">カルチャー。</param>
        /// <returns>変換後の値。</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("bar to value convert err!");
        }
    }
}
