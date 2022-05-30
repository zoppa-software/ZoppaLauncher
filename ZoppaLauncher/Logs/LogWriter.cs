using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace ZoppaLauncher.Logs
{
    /// <summary>ログ出力機能です。</summary>
    class LogWriter : ILogWriter
    {
        private Logger? _log;

        /// <summary>コンストラクタ。</summary>
        public LogWriter()
        {
            this._log = null;
        }

        /// <summary>コンストラクタ。</summary>
        /// <param name="logFilePath">出力ファイルパス。</param>
        /// <param name="encode">出力エンコード。</param>
        /// <param name="maxLogSize">最大ログファイルサイズ。</param>
        /// <param name="logGeneration">ログ世代数。</param>
        /// <param name="dateChangee">日付変更切り替えフラグ。</param>
        public LogWriter(string logFilePath, Encoding? encode = null, int maxLogSize = 30 * 1024 * 1024, int logGeneration = 10, bool dateChange = false)
        {
            this._log = new Logger(this, logFilePath, encode ?? System.Text.Encoding.Default, maxLogSize, logGeneration, dateChange);
        }

        /// <summary>ログを出力します。</summary>
        /// <param name="message">出力する文字列。</param>
        public void Write(string message)
        {
            this._log?.Write($"[{this.DateNow.ToString("yyyy/MM/dd HH:mm:ss")}] {message}");
        }

        /// <summary>通常ログ出力。</summary>
        /// <param name="caller">呼び出し元クラス。</param>
        /// <param name="message">ログメッセージ。</param>
        /// <param name="memberName">呼び出し元メソッド名。</param>
        public void WriteLog(object caller, string message, [CallerMemberName] string memberName = "")
        {
            this.Write($"[{caller.GetType().Name}.{memberName}] {message}");
        }

        /// <summary>通常ログ出力。</summary>
        /// <param name="caller">呼び出し元クラス。</param>
        /// <param name="message">ログメッセージ。</param>
        /// <param name="memberName">呼び出し元メソッド名。</param>
        public void WriteLog(Type caller, string message, [CallerMemberName] string memberName = "")
        {
            this.Write($"[{caller.Name}.{memberName}] {message}");
        }

        /// <summary>エラーログ出力。</summary>
        /// <param name="caller">呼び出し元クラス。</param>
        /// <param name="ex">例外。</param>
        /// <param name="memberName">呼び出し元メソッド名。</param>
        public void WriteErrorLog(object caller, Exception ex, [CallerMemberName] string memberName = "")
        {
            this.Write($"[{caller.GetType().Name}.{memberName}] error!:{ex.ToString()}");
            this.Write(ex.StackTrace ?? "");
        }

        /// <summary>エラーログ出力。</summary>
        /// <param name="caller">呼び出し元クラス。</param>
        /// <param name="ex">例外。</param>
        /// <param name="memberName">呼び出し元メソッド名。</param>
        public void WriteErrorLog(Type caller, Exception ex, [CallerMemberName] string memberName = "")
        {
            this.Write($"[{caller.Name}.{memberName}] error!:{ex.ToString()}");
            this.Write(ex.StackTrace ?? "");
        }

        /// <summary>書き込み中のログがあれば、書き込みが完了するまで待機します。</summary>
        public void WaitFinish()
        {
            for (int i = 0; i < 100; ++i) {
                if (this._log?.IsWriting ?? false) {
                    System.Threading.Thread.Sleep(1000);
                }
                else {
                    break;
                }
            }
        }

        /// <summary>現在時刻取得機能を設定、取得します。</summary>
        public ILogWriter.IDateHelper? UseDateHelper { get; set; }

        /// <summary>現在時刻を取得します。</summary>
        public DateTime DateNow {
            get { return this.UseDateHelper?.Now ?? DateTime.Now; }
        }

        /// <summary>ログ出力を行います。</summary>
        private sealed class Logger
        {
            // 親機能参照
            private readonly ILogWriter _parent;

            // 対象ファイル
            private readonly FileInfo _logFile;

            // 出力エンコード
            private readonly Encoding _encode;

            // 最大ログサイズ
            private readonly int _maxLogSize;

            // 最大ログ世代数
            private readonly int _logGen;

            // 日付変更切り替えフラグ
            private readonly bool _dateChange;

            // 書き込みバッファ
            private readonly Queue<string> _queue = new Queue<string>();

            // 前回書き込み完了日時
            private DateTime _prevWriteDate;

            // 書き込み中フラグ
            private bool _writing;

            /// <summary>コンストラクタ。</summary>
            /// <param name="parent">ログ出力機能参照。</param>
            /// <param name="logFilePath">出力ファイルパス。</param>
            /// <param name="encode">出力エンコード。</param>
            /// <param name="maxLogSize">最大ログファイルサイズ。</param>
            /// <param name="logGeneration">ログ世代数。</param>
            /// <param name="dateChangee">日付変更切り替えフラグ。</param>
            public Logger(ILogWriter parent, string logFilePath, Encoding encode, int maxLogSize, int logGeneration, bool dateChangee)
            {
                this._parent = parent;
                this._logFile = new FileInfo(logFilePath);
                this._encode = encode;
                this._maxLogSize = maxLogSize;
                this._logGen = logGeneration;
                this._writing = false;
                this._dateChange = dateChangee;
                this._prevWriteDate = DateTime.MaxValue;
            }

            /// <summary>ログをファイルに出力します。</summary>
            /// <param name="message">出力する文字列。</param>
            public void Write(string message)
            {
                // 書き出す情報をため込む
                bool wrt = false;
                lock(this) {
                    this._queue.Enqueue(message);
                    wrt = this._writing;
                }

                // 別スレッドでファイルに出力
                if (!wrt) {
                    this._writing = true;
                    Task.Run(() => { this.Write(); });
                }
            }

            /// <summary>ログをファイルに出力する。</summary>
            private void Write()
            {
                this._logFile.Refresh();

                if (this._logFile.Exists &&
                    (this._logFile.Length > this._maxLogSize || this.ChangeOfDate)) {
                    try {
                        // 以前のファイルをリネーム
                        var ext = Path.GetExtension(this._logFile.Name);
                        var nm = this._logFile.Name.Substring(0, this._logFile.Name.Length - ext.Length);
                        var tn = this._parent.DateNow.ToString("yyyyMMddHHmmssfff");

                        if (this._logFile.Directory != null) {
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveFile(this._logFile.FullName, $"{this._logFile.Directory.FullName}\\{nm}_{tn}\\{nm}{ext}");
                            ZipFile.CreateFromDirectory(
                                $"{this._logFile.Directory.FullName}\\{nm}_{tn}",
                                $"{this._logFile.Directory.FullName}\\{nm}_{tn}.zip"
                            );
                            Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(
                                $"{this._logFile.Directory.FullName}\\{nm}_{tn}", 
                                Microsoft.VisualBasic.FileIO.DeleteDirectoryOption.DeleteAllContents
                            );

                            // 過去ファイルを整理
                            var oldfiles = Directory.GetFiles(this._logFile.Directory.FullName, $"{nm}*.zip").ToList();
                            oldfiles.Sort();
                            while (oldfiles.Count > this._logGen) {
                                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(oldfiles.First());
                                oldfiles.RemoveAt(0);
                            }
                        }
                    }
                    catch {
                        lock (this) {
                            this._writing = false;
                        }
                        return;
                    }
                }

                try {
                    using (var sw = new StreamWriter(this._logFile.FullName, true, this._encode)) {
                        bool writed;
                        do {
                            // キュー内の文字列を取得
                            writed = false;
                            string? ln = null;
                            lock (this) {
                                if (this._queue.Count > 0) {
                                    ln = this._queue.Dequeue();
                                }
                                else {
                                    this._writing = false;
                                }
                            }

                            // ファイルに書き出す
                            if (ln != null) {
                                sw.WriteLine(ln);
                                writed = true;
                            }

                        } while (writed);
                    }
                }
                catch {
                    lock (this) {
                        this._writing = false;
                    }
                }
                finally {
                    this._prevWriteDate = this._parent.DateNow;
                }
            }

            /// <summary>書き込み中状態ならば真を返します。</summary>
            public bool IsWriting {
                get {
                    lock(this) {
                        return this._writing;
                    }
                }
            }

            /// <summary>日付の変更でログを切り替えるならば真を返します。</summary>
            public bool ChangeOfDate {
                get { 
                    return this._dateChange && this._prevWriteDate.Date < this._parent.DateNow.Date; 
                }
            }
        }
    }
}
