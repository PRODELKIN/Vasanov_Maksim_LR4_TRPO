using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
 
namespace Hotel_Vasanov_ISP43.Pages
{
    public partial class LoginPage : Page
    {
        private MainWindow mainWindow;
        private Captcha captcha = new Captcha();

        public LoginPage()
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;
            captcha.Refresh(CaptchaBorder);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            captcha.Refresh(CaptchaBorder);
            CaptchaInput.Clear();
            StatusText.Text = "";
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text.Trim();
            string password = PasswordInput.Password;

            if (email == "" || password == "")
            {
                StatusText.Text = "Введите почту и пароль.";
                return;
            }
            if (!captcha.Matches(CaptchaInput.Text))
            {
                RefreshButton_Click(sender, e);
                StatusText.Text = "Неверный код с картинки.";
                return;
            }

            try
            {
                UserSession user = DataAccess.Login(email, password);
                if (user == null)
                {
                    PasswordInput.Clear();
                    RefreshButton_Click(sender, e);
                    StatusText.Text = "Неверная почта или пароль.";
                }
                else
                {
                    mainWindow.SignIn(user);
                }
            }
            catch (SqlException)
            {
                StatusText.Text = "Ошибка подключения к базе данных.";
            }
            catch (Exception)
            {
                StatusText.Text = "Не удалось выполнить вход.";
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.ShowRegister();
        }
    }
}
