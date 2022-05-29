using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoppaLauncher.Logs
{
    /// <summary>ログ出力機能のインターフェイスです。</summary>
    public interface ILogWriter
    {
        /// <summary>ログを出力します。</summary>
        /// <param name="message">出力する文字列。</param>
        void Write(string message);

        /// <summary>書き込み中のログがあれば、書き込みが完了するまで待機します。</summary>
        void WaitFinish();

        /// <summary>現在時刻取得機能を設定、取得します。</summary>
        IDateHelper? UseDateHelper { get; set; }

        /// <summary>現在時刻を取得します。</summary>
        DateTime DateNow { get; }

        /// <summary>現在時刻取得機能インターフェイスです。</summary>
        interface IDateHelper
        {
            /// <summary>現在時刻を取得します。</summary>
            DateTime Now { get; }
        }
    }
}
