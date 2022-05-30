using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using ZoppaLauncher.Logs;
using ZoppaLauncher.Models;
using ZoppaLauncher.Views;

namespace ZoppaLauncher
{
    /// <summary>アプリケーションクラス。</summary>
    public partial class App : Application
    {
        // DIサービス
        private ServiceCollection _diService;

        // DIプロバイダ
        private ServiceProvider _diProvider;

        /// <summary>コンストラクタ。</summary>
        public App()
        {
            // DIコンテナ設定
            this._diService = new ServiceCollection();
            this._diService.AddSingleton<MainWindow>(
                (service) => { 
                    return new MainWindow(
                        service.GetService<LauncherForm>(), 
                        service.GetService<NowTimeInformation>(),
                        service.GetService<ILogWriter>()); 
                }
            );
            this._diService.AddSingleton<LauncherForm>();
            this._diService.AddSingleton<NowTimeInformation>();
            this._diService.AddSingleton<ILogWriter, LogWriter>(
                (service) => {
                    return new LogWriter($"{LogPath}\\operation.log");
                }
            );

            this._diProvider = this._diService.BuildServiceProvider();

            this.UseSinglton(this._diProvider, "zoppa launcherr mutex");
        }

        /// <summary>アプリケーション開始イベント。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWin = this._diProvider.GetService<MainWindow>();
            if (mainWin != null) {
                mainWin.AjustWindowPosition();
            }
            this.MainWindow = mainWin;
            this.MainWindow?.Show();
        }

        /// <summary>アプリケーション終了イベント。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        /// <summary>アプリケーションの設定フォルダのパスを取得します。</summary>
        public static string SettingPath {
            get {
                var appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dirInto = new System.IO.DirectoryInfo($"{appPath}\\ZoppaLauncher");
                if (!dirInto.Exists) {
                    dirInto.Create();
                }
                return dirInto.FullName;
            }
        }

        /// <summary>アプリケーションのログフォルダのパスを取得します。</summary>
        public static string LogPath {
            get {
                var dirInto = new System.IO.DirectoryInfo($"{SettingPath}\\logs");
                if (!dirInto.Exists) {
                    dirInto.Create();
                }
                return dirInto.FullName;
            }
        }

        /// <summary>アプリケーションのショートカット保存フォルダのパスを取得します。</summary>
        public static string StockPath {
            get {
                var dirInto = new System.IO.DirectoryInfo($"{SettingPath}\\links");
                if (!dirInto.Exists) {
                    dirInto.Create();
                }
                return dirInto.FullName;
            }
        }

        public ServiceProvider? DiProvider {
            get {
                return this._diProvider;
            }
        }
    }
}
