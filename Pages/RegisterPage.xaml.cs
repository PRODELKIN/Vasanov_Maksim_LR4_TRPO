using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls; 

namespace Hotel_Vasanov_ISP43.Pages
{
    public partial class RegisterPage : Page
    {
        private MainWindow mainWindow;
        private Captcha captcha = new Captcha();

        public RegisterPage()
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;
            captcha.Refresh(CaptchaBorder);
        }

        private bool CheckName(string name)
        {
            if (name.Length < 2 || name.Length > 100)
            {
                return false;
            }
            if (!char.IsLetter(name[0]))
            {
                return false;
            }
            for (int i = 0; i < name.Length; i++)
            {
                if (!char.IsLetter(name[i]) && name[i] != ' ' && name[i] != '-' && name[i] != '\'')
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckPhone(string phone)
        {
            int start = 0;
            if (phone.StartsWith("+"))
            {
                start = 1;
            }
            int count = phone.Length - start;
            if (count < 10 || count > 15)
            {
                return false;
            }
            for (int i = start; i < phone.Length; i++)
            {
                if (phone[i] < '0' || phone[i] > '9')
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckEmail(string email)
        {
            int at = email.IndexOf('@');
            int dot = email.LastIndexOf('.');
            if (email.Length > 100 || at < 1 || at != email.LastIndexOf('@') || dot <= at + 1 || dot == email.Length - 1)
            {
                return false;
            }
            for (int i = 0; i < email.Length; i++)
            {
                if (char.IsWhiteSpace(email[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            captcha.Refresh(CaptchaBorder);
            CaptchaInput.Clear();
            StatusText.Text = "";
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string password = PasswordInput.Password;

            if (!CheckName(name))
            {
                StatusText.Text = "Укажите Ф. И. О. буквами (2–100 символов).";
                return;
            }
            if (!CheckPhone(phone))
            {
                StatusText.Text = "Укажите телефон: 10–15 цифр, возможен знак + в начале.";
                return;
            }
            if (!CheckEmail(email))
            {
                StatusText.Text = "Укажите корректную электронную почту.";
                return;
            }
            if (password.Length < 6 || password.Length > 100)
            {
                StatusText.Text = "Пароль должен содержать от 6 до 100 символов.";
                return;
            }
            if (password != PasswordRepeat.Password)
            {
                StatusText.Text = "Пароли не совпадают.";
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
                string error = DataAccess.Register(name, phone, email, password);
                if (error != "")
                {
                    StatusText.Text = error;
                    return;
                }
                MessageBox.Show("Регистрация завершена. Теперь войдите в аккаунт.", "Гостиница");
                mainWindow.ShowLogin();
            }
            catch (SqlException)
            {
                StatusText.Text = "Ошибка базы данных. Повторите попытку.";
            }
            catch (Exception)
            {
                StatusText.Text = "Не удалось завершить регистрацию.";
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.ShowLogin();
        }
    }
}
