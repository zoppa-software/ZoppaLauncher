using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        /// <summary>通常ログ出力。</summary>
        /// <param name="caller">呼び出し元クラス。</param>
        /// <param name="message">ログメッセージ。</param>
        /// <param name="memberName">呼び出し元メソッド名。</param>
        void WriteLog(object caller, string message, [CallerMemberName] string memberName = "");

        /// <summary>通常ログ出力。</summary>
        /// <param name="caller">呼び出し元クラス。</param>
        /// <param name="message">ログメッセージ。</param>
        /// <param name="memberName">呼び出し元メソッド名。</param>
        void WriteLog(Type caller, string message, [CallerMemberName] string memberName = "");

        /// <summary>エラーログ出力。</summary>
        /// <param name="caller">呼び出し元クラス。</param>
        /// <param name="ex">例外。</param>
        /// <param name="memberName">呼び出し元メソッド名。</param>
        void WriteErrorLog(object caller, Exception ex, [CallerMemberName] string memberName = "");

        /// <summary>エラーログ出力。</summary>
        /// <param name="caller">呼び出し元クラス。</param>
        /// <param name="ex">例外。</param>
        /// <param name="memberName">呼び出し元メソッド名。</param>
        void WriteErrorLog(Type caller, Exception ex, [CallerMemberName] string memberName = "");

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
