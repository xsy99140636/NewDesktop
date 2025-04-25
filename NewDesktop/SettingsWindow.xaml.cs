using System.Runtime.InteropServices;
using NewDesktop.ViewModels;
using NewDesktop.Views.SettingsPage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace NewDesktop;

public partial class SettingsWindow : Window
{
    private readonly HomeSettingsPage _homeSettingsPage = new();
    private readonly BoxSettingsPage _boxSettingsPage = new();
    private readonly SaveSettingsPage _saveSettingsPage = new();
    private readonly ss _ss = new();

    public SettingsWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        //DataContext = mainViewModel;

        //var boxSettingsPage = new BoxSettingsPage();
        _boxSettingsPage.DataContext = mainViewModel;
        _saveSettingsPage.DataContext = mainViewModel;
        _homeSettingsPage.DataContext = mainViewModel;
        _ss.DataContext = mainViewModel;

        //ContentFrame.Navigate(_boxSettingsPage); // 导航到实例
        //ContentFrame.Navigate(new Uri("Views/SettingsPage/BoxSettingsPage.xaml", UriKind.Relative));
        Loaded += SettingsWindow_Loaded; 
    }

    private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 方式1: 通过 ListView 触发导航
        MyListView.SelectedIndex = 0;

    }


    private void MyListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MyListView.SelectedItem is StackPanel selectedPanel)
        {
            switch (selectedPanel.Tag as string)
            {
                case "Home":
                    ContentFrame.Navigate(_homeSettingsPage);
                    break;
                case "Box":
                    ContentFrame.Navigate(_boxSettingsPage);
                    break;
                case "Save":
                    ContentFrame.Navigate(_saveSettingsPage);
                    break;
            }
        }
    }

    private void ww(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(_ss);
        MyListView.SelectedItem = null;
    }
    
}

