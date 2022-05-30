using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ZoppaLauncher.Logs;

namespace ZoppaLauncher.Views
{
    /// <summary>単一起動機能です。</summary>
    public static class SingleLaunchHelper
    {
        // アプリケーション参照
        private static Application? _app;

        // パイプ名称
        private static string _pipeName = "";

        // ミューティクス
        private static Mutex? _mutex;

        // ログ出力機能
        private static ILogWriter? _logger;

        // 起動済みなら真
        private static bool _hasHnd = false;

        /// <summary>アプリケーションを単一起動にします。</summary>
        /// <param name="app">アプリケーション。</param>
        /// <param name="diProvider">DIプロバイダ。</param>
        /// <param name="appName">アプリケーション名。</param>
        public static void UseSinglton(this Application app, ServiceProvider diProvider, string appName)
        {
            // パラメータ設定
            _app = app;
            _pipeName = $"{appName} pipe";
            _mutex = new Mutex(false, appName);
            _logger = diProvider.GetService<ILogWriter>();

            // イベント割り当て
            app.Startup += App_Startup;
            app.Exit += App_Exit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void App_Startup(object sender, StartupEventArgs e)
        {
            try {
                _logger?.WriteLog(typeof(SingleLaunchHelper), "start");

                _hasHnd = _mutex?.WaitOne(0, false) ?? false;
                if (_hasHnd) {
                    // 表示メッセージ受信待機
                    _logger?.WriteLog(typeof(SingleLaunchHelper), "listen show message");
                    ListenShowMessageAsync();
                }
                else {
                    // 表示メッセージを送信
                    _logger?.WriteLog(typeof(SingleLaunchHelper), "send show runned process");
                    SendShowMessage();
                    _app?.Shutdown();
                }
            }
            catch (Exception ex) {
                _logger?.WriteErrorLog(typeof(SingleLaunchHelper), ex);
            }
        }

        private static async void ListenShowMessageAsync()
        {
            try {
                while (true) {
                    using (var pipe = new NamedPipeServerStream(_pipeName, PipeDirection.InOut)) {
                        await pipe.WaitForConnectionAsync();

                        if (pipe.IsConnected) {
                            var buf = new byte[1024];
                            pipe.Read(buf, 0, buf.Length);

                            _app?.MainWindow?.Show();
                        }
                    }
                }
            }
            catch (Exception ex) {
                _logger?.WriteErrorLog(typeof(SingleLaunchHelper), ex);
            }
        }

        private static void SendShowMessage()
        {
            using (var pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out)) {
                pipe.Connect();
                pipe.Write(System.Text.Encoding.UTF8.GetBytes("show"));
                pipe.WaitForPipeDrain();
            }
        }

        private static void App_Exit(object sender, ExitEventArgs e)
        {
            if (_hasHnd) {
                _mutex?.ReleaseMutex();
            }
            _mutex?.Close();
        }
    }
}
