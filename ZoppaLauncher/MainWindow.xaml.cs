using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZoppaLauncher.Logs;
using ZoppaLauncher.Models;
using ZoppaLauncher.Views;
using ZoppaShortcutLibrary;

namespace ZoppaLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LauncherCollection? _iconSetting;

        private LauncherForm _cellCollection;

        private NowTimeInformation _nowTime;

        private ILogWriter _logger;

        private bool _hitAnimaflg;

        public MainWindow()
        {
            this._cellCollection = new LauncherForm();
            this._nowTime = new NowTimeInformation();
            this._logger = new LogWriter();

            InitializeComponent();

            this.DataContext = this._cellCollection;
            this.nowTimeLabel.DataContext = this._nowTime;
        }

        public MainWindow(LauncherForm? collection, NowTimeInformation? nowTime, ILogWriter? logger)
        {
            this._cellCollection = collection ?? new LauncherForm();
            this._nowTime = nowTime ?? new NowTimeInformation();
            this._logger = logger ?? new LogWriter();

            InitializeComponent();

            this.DataContext = this._cellCollection;
            this.nowTimeLabel.DataContext = this._nowTime;
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            try {
                this._logger?.WriteLog(this, "start");

                this.Opacity = 0;

                this._iconSetting = await this.LoadSettingFile();
                if (this._iconSetting != null) {
                    this._logger?.WriteLog(this, "load setting");
                    this.Resources["foreColor"] = new SolidColorBrush(this._iconSetting.ForeColor);
                    this._cellCollection.BackColor = new SolidColorBrush(this._iconSetting.BackColor);
                    this.SetHoverAminationColor("hoverAction", 80, this._iconSetting.AccentColor);
                    this.SetHoverAminationColor("hoverBarAction", 255, this._iconSetting.AccentColor);
                    this.CurrentPage = await this.LoadCurrentPage();
                }
                else {
                    this._logger?.WriteLog(this, "create default setting");
                    this._iconSetting = new LauncherCollection();
                    this.CurrentPage = 0;
                }

                // マウスホバースタイルの割り当て
                foreach (Rectangle rec in new Rectangle[] { 
                    this.hiddenBtn , this.contAdminRun, this.contDelBtn, this.contOpenBtn 
                }) {
                    rec.Style = this.FindResource("hoverAction") as Style;
                }

                await Dispatcher.BeginInvoke(
                    () => {
                        this.Opacity = 1.0;
                        this.SetPage(); 
                    },
                    System.Windows.Threading.DispatcherPriority.Loaded
                );
            }
            catch (Exception ex) {
                this._logger?.WriteErrorLog(this, ex);
            }
        }

        private void SetHoverAminationColor(string storyName, byte transLv, Color hcolor)
        {
            var style = this.FindResource(storyName) as Style;
            var frame = ((style?.Triggers[0].EnterActions[0] as BeginStoryboard)?
                            .Storyboard.Children[0] as ColorAnimationUsingKeyFrames)?.KeyFrames[1];
            if (frame != null) {
                frame.Value = Color.FromArgb(transLv, hcolor.R, hcolor.G, hcolor.B);
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            try {
                this._nowTime.IsEnable = (this.WindowState != WindowState.Minimized);
            }
            catch (Exception ex) {
                this._logger?.WriteErrorLog(this, ex);
            }
        }

        private void window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try {
                this._nowTime.IsEnable = this.IsVisible;
            }
            catch (Exception ex) {
                this._logger?.WriteErrorLog(this, ex);
            }
        }

        protected override void OnDeactivated(EventArgs e)
        {
            try {
                this.cellMenuPop.IsOpen = false;
                this.infoPop.IsOpen = false;
            }
            catch (Exception ex) {
                this._logger?.WriteErrorLog(this, ex);
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            try {
                if (this.cellMenuPop.IsOpen) {
                    this.cellMenuPop.IsOpen = false;
                }
                if (this.infoPop.IsOpen) {
                    this.infoPop.IsOpen = false;
                }
            }
            catch (Exception ex) {
                this._logger?.WriteErrorLog(this, ex);
            }
        }

        private void IconFrame_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try {
                this.cellMenuPop.IsOpen = false;
                var icon = (e.Source as FrameworkElement)?.DataContext as CellInformation;
                if (icon != null) {
                    if (e.LeftButton == MouseButtonState.Pressed) {
                        this.cellControl.SelectCellInformation(e.GetPosition(this.cellControl), icon);
                    }
                    else if (e.RightButton == MouseButtonState.Pressed && icon.Name?.Trim() != "") {
                        this.cellMenuPop.PlacementTarget = (UIElement)sender;
                        this.cellMenuPop.Placement = PlacementMode.Bottom;
                        this.cellMenuPop.IsOpen = true;
                        this.cellMenuPop.DataContext = icon;
                    }
                    e.Handled = true;
                }
            }
            catch (Exception ex) {
                this._logger?.WriteErrorLog(this, ex);
            }
        }

        private async void IconFrame_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try {
                if (this.cellControl.IsSelectedIcon &&
                    this.cellControl.StayMousePosition(e.GetPosition(this.cellControl))) {
                    var icon = (e.Source as FrameworkElement)?.DataContext as CellInformation;
                    if (icon?.LinkPath != null) {
                        var sb = this.FindResource("hitAnimation") as Storyboard;
                        if (sb != null) {
                            sb.Children.ToList().ForEach(child => {
                                Storyboard.SetTarget(child, (e.Source as FrameworkElement)?.Parent);
                            });

                            this._hitAnimaflg = false;
                            sb.Begin();
                            await Task.Run(() => {
                                while(!this._hitAnimaflg) {
                                    System.Threading.Thread.Sleep(100);
                                }
                            });

                            var linkInfo = new ProcessStartInfo();
                            linkInfo.FileName = icon.LinkPath;
                            linkInfo.UseShellExecute = true;
                            Process.Start(linkInfo);

                            this.Hide();
                            this.cellControl.ClearCellInformation();
                        }
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.IconFrame_MouseUp)}:{ex.ToString()}");
            }
        }

        private void hitAnimation_Completed(object sender, EventArgs e)
        {
            this._hitAnimaflg = true;
        }

        private void hiddenBtn_Click(object sender, RoutedEventArgs e)
        {
            try {
                this.Hide();
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.hiddenBtn_Click)}:{ex.ToString()}");
            }
        }

        private async void cellControl_DropLinkFile(object sender, CellInformation toCell, string linkPath)
        {
            try {
                var newIcon = await Task.Run(() => { return IconInformation.Create(linkPath, App.StockPath); });

                if (this._iconSetting?.UsedPosition(this.CurrentPage, toCell.Row, toCell.Column) ?? false) {
                    this._iconSetting?.Remove(this.CurrentPage, toCell.Row, toCell.Column);
                }
                this._iconSetting?.UpdateIcon(this.CurrentPage, toCell.Row, toCell.Column, newIcon);
                this.SetPage();

                this.UpdateSettingFile();
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.cellControl_DropLinkFile)}:{ex.ToString()}");
            }
        }

        private async void cellControl_MoveIcon(object sender, CellInformation fromCell, CellInformation toCell, string linkPath)
        {
            try {
                var newIcon = await Task.Run(() => { return IconInformation.Create(linkPath, App.StockPath); });

                this._iconSetting?.Remove(fromCell.Page, fromCell.Row, fromCell.Column);
                if (this._iconSetting?.UsedPosition(this.CurrentPage, toCell.Row, toCell.Column) ?? false) {
                    this._iconSetting?.Remove(this.CurrentPage, toCell.Row, toCell.Column);
                }
                this._iconSetting?.UpdateIcon(this.CurrentPage, toCell.Row, toCell.Column, newIcon);
                this.SetPage();

                this.UpdateSettingFile();
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.cellControl_RemoveIcon)}:{ex.ToString()}");
            }
        }

        private void cellControl_RemoveIcon(object sender, CellInformation delCell)
        {
            try {
                this._iconSetting?.Remove(delCell.Page, delCell.Row, delCell.Column);
                this.SetPage();

                this.UpdateSettingFile();
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.cellControl_RemoveIcon)}:{ex.ToString()}");
            }
        }


        private void cellControl_MovingIcon(object sender, EventArgs e)
        {
            try {
                if (this.infoPop.IsOpen) {
                    this.infoPop.IsOpen = false;
                }
            }
            catch (Exception ex) {
                this._logger?.WriteErrorLog(this, ex);
            }
        }

        private void cellControl_StayIcon(object sender, CellInformation infoCell)
        {
            try {
                if (infoCell.HasLink) {
                    this.infoPop.PlacementTarget = this.SearchParentElement(sender);
                    this.infoPop.Placement = PlacementMode.Top;
                    this.infoPop.IsOpen = true;
                    this.infoPop.DataContext = infoCell;
                }
            }
            catch (Exception ex) {
                this._logger?.WriteErrorLog(this, ex);
            }
        }

        private UIElement? SearchParentElement(object sender)
        {
            if (sender is Image) {
                return ((sender as Image)?.Parent as FrameworkElement)?.Parent as UIElement;
            }
            else if (sender is TextBlock) {
                return ((sender as TextBlock)?.Parent as FrameworkElement)?.Parent as UIElement;
            }
            else {
                return sender as UIElement;
            }
        }

        private void Page_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var page = (e.Source as FrameworkElement)?.DataContext as PageBarInformation;
            try {
                if (page != null) {
                    this.CurrentPage = page.Index;
                    this.SetPage();
                }
                this.UpdateCurrentPage();
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.Page_MouseLeftButtonUp)}:{ex.ToString()}");
            }
        }

        private void AdministrateRun_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var icon = (e.Source as FrameworkElement)?.DataContext as CellInformation;
            try {
                if (icon?.LinkPath != null) {
                    var linkInfo = new ProcessStartInfo();
                    linkInfo.FileName = icon.LinkPath;
                    linkInfo.UseShellExecute = true;
                    linkInfo.Verb = "RunAs";
                    Process.Start(linkInfo);

                    this.Hide();
                    this.cellControl.ClearCellInformation();
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.AdministrateRun_MouseDown)}:{ex.ToString()}");
            }
        }

        private void DeleteIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var icon = (e.Source as FrameworkElement)?.DataContext as CellInformation;
            try {
                if (icon != null) {
                    this._iconSetting?.Remove(icon.Page, icon.Row, icon.Column);
                    this.SetPage();

                    this.UpdateSettingFile();
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.DeleteIcon_MouseDown)}:{ex.ToString()}");
            }
        }

        private void OpenLocation_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var icon = (e.Source as FrameworkElement)?.DataContext as CellInformation;
            try {
                if (icon?.LinkPath != null) {
                    var path = new FileInfo(icon.LinkPath);
                    if (path.Directory != null) {
                        Process.Start("explorer.exe", path.Directory.FullName);
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.OpenLocation_MouseDown)}:{ex.ToString()}");
            }
        }

        private async Task<LauncherCollection?> LoadSettingFile()
        {
            return await Task.Run(() => {
                foreach (var path in new string[] { 
                            $"{App.SettingPath}\\setting.xml", 
                            $"{App.SettingPath}\\setting_back.xml" }) {
                    var setFile = new FileInfo(path);
                    if (setFile.Exists) {
                        var doc = new System.Xml.XmlDocument();
                        doc.Load(setFile.FullName);
                        return LauncherCollection.Load(doc);
                    }
                }
                return null;
            });
        }

        private async void UpdateSettingFile()
        {
            var setFile = new FileInfo($"{App.SettingPath}\\setting.xml");
            var backFile = new FileInfo($"{App.SettingPath}\\setting_back.xml");

            await Task.Run(() => {
                if (this._iconSetting != null) {
                    if (setFile.Exists) {
                        setFile.CopyTo(backFile.FullName, true);
                    }

                    var doc = this._iconSetting?.Save();
                    doc?.Save(setFile.FullName);
                }
            });
        }

        private void SetPage()
        {
            if (this._iconSetting != null) {
                this._cellCollection.SetPage(
                    this.CurrentPage,
                    this._iconSetting.PageCount, 
                    this._iconSetting.WCount, 
                    this._iconSetting.HCount, 
                    this._iconSetting.Collect(this.CurrentPage)
                );
            }
        }

        private Task<int> LoadCurrentPage()
        {
            return Task.Run(() => {
                var pageFile = new FileInfo($"{App.SettingPath}\\current.txt");
                if (pageFile.Exists) {
                    try {
                        string ln = "0";
                        using (var sr = new StreamReader(pageFile.FullName)) {
                            ln = sr.ReadToEnd();
                        }
                        return int.Parse(ln);
                    }
                    catch {
                        return 0;
                    }
                }
                else {
                    return 0;
                }
            });
        }

        private async void UpdateCurrentPage()
        {
            var pageFile = new FileInfo($"{App.SettingPath}\\current.txt");

            await Task.Run(() => {
                using (var sw = new StreamWriter(pageFile.FullName, false)) {
                    sw.Write($"{this.CurrentPage}");
                }
            });
        }

        private int CurrentPage {
            get; set; 
        }
    }
}
