using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZoppaLauncher.Models;
using ZoppaShortcutLibrary;

namespace ZoppaLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string LINK_STOCK_PATH = "links";

        private IconCollection? _iconSetting;

        private CellsTblInformation _cellCollection;

        private NowTime _nowTime;

        private bool _hitAnimaflg;

        public MainWindow()
        {
            this._cellCollection = new CellsTblInformation();
            this._nowTime = new NowTime();

            InitializeComponent();

            this.DataContext = this._cellCollection;
            this.nowTimeLabel.DataContext = this._nowTime;
        }

        public MainWindow(CellsTblInformation? collection, NowTime? nowTime)
        {
            this._cellCollection = collection ?? new CellsTblInformation();
            this._nowTime = nowTime ?? new NowTime();

            InitializeComponent();

            this.DataContext = this._cellCollection;
            this.nowTimeLabel.DataContext = this._nowTime;
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            try {
                this._iconSetting = await this.LoadSettingFile();
                if (this._iconSetting != null) {
                    this.Resources["foreColor"] = new SolidColorBrush(this._iconSetting.ForeColor);

                    this._cellCollection.BackColor = new SolidColorBrush(this._iconSetting.BackColor);

                    var style0 = this.FindResource("hoverAction") as Style;
                    var b = ((style0?.Triggers[0].EnterActions[0] as BeginStoryboard)?.Storyboard.Children[0] as ColorAnimationUsingKeyFrames)?.KeyFrames[1];
                    if (b != null) {
                        b.Value = Color.FromArgb(80, this._iconSetting.AccentColor.R, this._iconSetting.AccentColor.G, this._iconSetting.AccentColor.B);
                    }

                    var style1 = this.FindResource("hoverBarAction") as Style;
                    var a = ((style1?.Triggers[0].EnterActions[0] as BeginStoryboard)?.Storyboard.Children[0] as ColorAnimationUsingKeyFrames)?.KeyFrames[1];
                    if (a != null) {
                        a.Value = this._iconSetting.AccentColor;
                    }
                    
                    this.CurrentPage = 0;
                }
                else {
                    this._iconSetting = new IconCollection();
                }

                this.hiddenBtn.Style = this.FindResource("hoverAction") as Style;
                this.contAdminRun.Style = this.FindResource("hoverAction") as Style;
                this.contDelBtn.Style = this.FindResource("hoverAction") as Style;

                await Dispatcher.BeginInvoke(
                    () => { 
                        this.SetPage(); 
                    },
                    System.Windows.Threading.DispatcherPriority.Loaded
                );
            }
            catch (Exception ex) { 
                Debug.WriteLine($"{nameof(this.OnInitialized)}:{ex.ToString()}");
            }
        }

        protected override void OnDeactivated(EventArgs e)
        {
            try {
                this.cellMenuPop.IsOpen = false;
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.OnDeactivated)}:{ex.ToString()}");
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            try {
                if (this.cellMenuPop.IsOpen) {
                    this.cellMenuPop.IsOpen = false;
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.OnMouseDown)}:{ex.ToString()}");
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
                        this.cellMenuPop.PlacementTarget = (Rectangle)sender;
                        this.cellMenuPop.Placement = PlacementMode.Bottom;
                        this.cellMenuPop.IsOpen = true;
                        this.cellMenuPop.DataContext = icon;
                    }
                    e.Handled = true;
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.IconFrame_MouseDown)}:{ex.ToString()}");
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

                            this.WindowState = WindowState.Minimized;
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
                this.WindowState = WindowState.Minimized;
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.hiddenBtn_Click)}:{ex.ToString()}");
            }
        }

        private async void cellControl_DropLinkFile(object sender, CellInformation toCell, string linkPath)
        {
            try {
                var newIcon = await Task.Run(() => { return IconInformation.Create(linkPath, LINK_STOCK_PATH); });

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
                var newIcon = await Task.Run(() => { return IconInformation.Create(linkPath, LINK_STOCK_PATH); });

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

        private void Page_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var page = (e.Source as FrameworkElement)?.DataContext as PageInformation;
            try {
                if (page != null) {
                    this.CurrentPage = page.Index;
                    this.SetPage();
                }

                this.UpdateSettingFile();
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

                    this.WindowState = WindowState.Minimized;
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

        private async Task<IconCollection?> LoadSettingFile()
        {
            return await Task.Run(() => {
                foreach (var path in new string[] { 
                            $"{this.SettingPath}\\setting.xml", 
                            $"{this.SettingPath}\\setting_back.xml" }) {
                    var setFile = new FileInfo(path);
                    if (setFile.Exists) {
                        var doc = new System.Xml.XmlDocument();
                        doc.Load(setFile.FullName);
                        return IconCollection.Load(doc);
                    }
                }
                return null;
            });
        }

        private async void UpdateSettingFile()
        {
            var setFile = new FileInfo($"{this.SettingPath}\\setting.xml");
            var backFile = new FileInfo($"{this.SettingPath}\\setting_back.xml");

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

        private int CurrentPage {
            get; set; 
        }

        private string SettingPath {
            get {
                var appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dirInto = new DirectoryInfo($"{appPath}\\ZoppaLauncher");
                if (!dirInto.Exists) {
                    dirInto.Create();
                }
                return dirInto.FullName;
            }
        }
    }
}
