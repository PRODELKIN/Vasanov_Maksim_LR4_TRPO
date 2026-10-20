using System.Windows;
using Hotel_Vasanov_ISP43.Pages;

namespace Hotel_Vasanov_ISP43
{
    public partial class MainWindow : Window
    {
        public UserSession CurrentUser;

        public MainWindow()
        {
            InitializeComponent();
            UpdateNavigation();
            ShowRooms();
        }

        private void UpdateNavigation()
        {
            BookingsButton.Visibility = Visibility.Collapsed;
            AdminButton.Visibility = Visibility.Collapsed;

            if (CurrentUser == null)
            {
                UserLabel.Text = "Гость";
                LoginButton.Visibility = Visibility.Visible;
                RegisterButton.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                UserLabel.Text = CurrentUser.FullName;
                LoginButton.Visibility = Visibility.Collapsed;
                RegisterButton.Visibility = Visibility.Collapsed;
                LogoutButton.Visibility = Visibility.Visible;

                if (CurrentUser.Role == "Администратор")
                {
                    AdminButton.Visibility = Visibility.Visible;
                }
                else
                {
                    BookingsButton.Visibility = Visibility.Visible;
                }
            }
        }

        public void SignIn(UserSession user)
        {
            CurrentUser = user;
            UpdateNavigation();
            ShowRooms();
        }

        public void ShowRooms()
        {
            ContentFrame.Navigate(new RoomsPage());
        }

        public void ShowLogin()
        {
            ContentFrame.Navigate(new LoginPage());
        }

        public void ShowRegister()
        {
            ContentFrame.Navigate(new RegisterPage());
        }

        public void ShowBookings()
        {
            ContentFrame.Navigate(new BookingsPage());
        }

        private void RoomsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowRooms();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ShowLogin();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ShowRegister();
        }

        private void BookingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowBookings();
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            ShowBookings();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser = null;
            UpdateNavigation();
            ShowRooms();
        }
    }
}
