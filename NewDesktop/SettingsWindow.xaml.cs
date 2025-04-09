using NewDesktop.ViewModels;
using NewDesktop.Views.SettingsPage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NewDesktop;

public partial class SettingsWindow : Window
{
    private readonly BoxSettingsPage _boxSettingsPage = new();
    private readonly SaveSettingsPage _saveSettingsPage = new();

    public SettingsWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        DataContext = mainViewModel;

        //var boxSettingsPage = new BoxSettingsPage();
        _boxSettingsPage.DataContext = mainViewModel;
        _saveSettingsPage.DataContext = mainViewModel;
        ContentFrame.Navigate(_boxSettingsPage); // 导航到实例
        //ContentFrame.Navigate(new Uri("Views/SettingsPage/BoxSettingsPage.xaml", UriKind.Relative));
    }
    private void MyListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MyListView.SelectedItem is StackPanel selectedPanel)
        {
            switch (selectedPanel.Tag as string)
            {
                case "Home":
                    NavigateHomeSettingsPage();
                    break;
                case "Box":
                    NavigateBoxSettingsPage();
                    break;
                case "Save":
                    NavigateSaveSettingsPage();
                    break;
            }
        }
    }

    private void NavigateHomeSettingsPage() => ContentFrame.Navigate(_saveSettingsPage);

    private void NavigateBoxSettingsPage() => ContentFrame.Navigate(_boxSettingsPage);

    private void NavigateSaveSettingsPage() => ContentFrame.Navigate(_saveSettingsPage);
}
    
