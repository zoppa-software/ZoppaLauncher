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
                    this._cellCollection.BackColor = new SolidColorBrush(this._iconSetting.BackColor);
                    this.CurrentPage = 0;
                }

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

        private void IconFrame_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try {
                if (this.cellControl.IsSelectedIcon &&
                    this.cellControl.StayMousePosition(e.GetPosition(this.cellControl))) {
                    var icon = (e.Source as FrameworkElement)?.DataContext as CellInformation;
                    if (icon?.LinkPath != null) {
                        var linkInfo = new ProcessStartInfo();
                        linkInfo.FileName = icon.LinkPath;
                        linkInfo.UseShellExecute = true;
                        Process.Start(linkInfo);

                        this.WindowState = WindowState.Minimized;
                        this.cellControl.ClearCellInformation();
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"{nameof(this.IconFrame_MouseUp)}:{ex.ToString()}");
            }
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
                var exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                
                foreach (var path in new string[] { $"{exePath}\\setting.xml", $"{exePath}\\setting_back.xml" }) {
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
            var exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            var setFile = new FileInfo($"{exePath}\\setting.xml");
            var backFile = new FileInfo($"{exePath}\\setting_back.xml");

            await Task.Run(() => {
                if (this._iconSetting != null) {
                    setFile.CopyTo(backFile.FullName, true);

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
    }
}
