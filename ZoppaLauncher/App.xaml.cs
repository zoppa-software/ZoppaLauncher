using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using ZoppaLauncher.Models;
using ZoppaLauncher.Views;

namespace ZoppaLauncher
{
    /// <summary>アプリケーションクラス。</summary>
    public partial class App : Application
    {
        // DIサービス
        private ServiceCollection _diService;

        /// <summary>コンストラクタ。</summary>
        public App()
        {
            // DIコンテナ設定
            this._diService = new ServiceCollection();
            this._diService.AddSingleton<MainWindow>(
                (service) => { 
                    return new MainWindow(service.GetService<CellsTblInformation>(), service.GetService<NowTime>()); 
                }
            );
            this._diService.AddSingleton<CellsTblInformation>();
            this._diService.AddSingleton<NowTime>();
        }

        /// <summary>アプリケーション開始イベント。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWin = this._diService.BuildServiceProvider().GetService<MainWindow>();
            this.MainWindow = mainWin;
            if (mainWin != null) {
                mainWin.AjustWindowPosition();
            }
            this.MainWindow?.Show();
        }

        /// <summary>アプリケーション終了イベント。</summary>
        /// <param name="e">イベントオブジェクト。</param>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
