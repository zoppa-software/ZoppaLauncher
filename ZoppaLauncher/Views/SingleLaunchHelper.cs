using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ZoppaLauncher.Views
{
    public static class SingleLaunchHelper
    {
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
        public static extern int SendMessage(IntPtr hwnd, uint msg, int wParam, int lParam);

        public interface ITargetWindow
        {
            void Reshow();
        }

        private static Application? _app;

        private static string _pipeName = "";

        private static Mutex? _mutex;

        private static bool _hasHnd = false;


        public static void UseSinglton(this Application app, string appName)
        {
            app.Startup += App_Startup;
            app.Exit += App_Exit;

            _app = app;
            _mutex = new Mutex(false, appName);
            _pipeName = $"{appName} pipe";
        }

        private static void App_Startup(object sender, StartupEventArgs e)
        {
            _hasHnd = _mutex?.WaitOne(0, false) ?? false;
            if (_hasHnd) {
                Task.Run(() => { ListenShowMessage(); }) ;
            }
            else {
                // プロセスの名称見つけて、表示メッセージを送信
                SendShowMessage();
                _app?.Shutdown();
            }
        }

        private static void ListenShowMessage()
        {
            while (true) {
                using (var pipe = new NamedPipeServerStream(_pipeName, PipeDirection.InOut)) {
                    pipe.WaitForConnection();

                    if (pipe.IsConnected) {
                        var buf = new byte[1024];
                        pipe.Read(buf, 0, buf.Length);

                        (_app as ITargetWindow)?.Reshow();
                    }
                }
            }
        }
        private static void SendShowMessage()
        {
            try {
                using (var pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out)) {
                    pipe.Connect();
                    pipe.Write(System.Text.Encoding.UTF8.GetBytes("show"));
                    pipe.WaitForPipeDrain();
                }
            }
            catch {

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
