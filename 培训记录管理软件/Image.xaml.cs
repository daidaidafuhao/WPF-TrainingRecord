using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Drawing;
using System.Windows.Media.Imaging;
namespace 培训记录管理软件
{
    public partial class ImagePage : MetroWindow
    {
        DatabaseManager dbManager = new DatabaseManager();

        private string IdNumber;

        private string selectedImagePath;


        public ImagePage(string IdNumber)
        {
            InitializeComponent();
            this.IdNumber=IdNumber;
            try
            {
                byte[] employeePhoto =   dbManager.GetPhotoByIDCard(IdNumber);

                if (employeePhoto != null)
                { // 使用字节数组创建 MemoryStream
                    using (MemoryStream ms = new MemoryStream(employeePhoto))
                    {
                        // 创建 BitmapImage 对象
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;  // 设置图片源为 MemoryStream
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;  // 加载时缓存图片
                        bitmapImage.EndInit();

                        // 将 BitmapImage 设置为 WPF Image 控件的 Source
                        SelectedImage.Source = bitmapImage;
                    }
                }
               
            }
            catch (Exception ex) {

                ShowPopu(ex.Message);
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取屏幕的工作区域
            var screen = SystemParameters.WorkArea;

            // 计算窗口的位置
            double left = (screen.Width - this.Width) / 2 + screen.Left;
            double top = (screen.Height - this.Height) / 2 + screen.Top;

            // 设置窗口的位置
            this.Left = left;
            this.Top = top;
        }


        // 选择图片按钮事件
        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开文件对话框让用户选择图片
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImagePath = openFileDialog.FileName;

                // 显示选择的图片
                SelectedImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(selectedImagePath));
            }
        }

        // 上传图片按钮事件
        private void UploadImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedImagePath))
            {
                MessageBox.Show("请选择一张图片。");
                return;
            }


            // 插入图片到数据库
            dbManager.InsertEmployeePhotoByIDCard(this.IdNumber, selectedImagePath);

            MessageBox.Show("图片上传成功！");
        }
        private void ShowPopu(string errorMessage)
        {
            PopupWindow popup = new PopupWindow(errorMessage);

            popup.ShowDialog(); // 显示模态对话框
        }

    }
}
