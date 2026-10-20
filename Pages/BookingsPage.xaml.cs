using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls; 

namespace Hotel_Vasanov_ISP43.Pages
{
    public partial class BookingsPage : Page
    {
        private MainWindow mainWindow;
        private bool allUsers;

        public BookingsPage()
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;
            AdminPanel.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;

            if (mainWindow.CurrentUser == null)
            {
                StatusText.Text = "Войдите в аккаунт.";
                return;
            }
            allUsers = mainWindow.CurrentUser.Role == "Администратор";

            if (allUsers)
            {
                TitleText.Text = "Управление бронированиями";
                AdminPanel.Visibility = Visibility.Visible;
            }
            else
            {
                TitleText.Text = "Мои бронирования";
                CancelButton.Visibility = Visibility.Visible;
            }
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                int userId = mainWindow.CurrentUser.Id;
                if (allUsers)
                {
                    userId = 0;
                }
                BookingsGrid.ItemsSource = DataAccess.Bookings(userId).DefaultView;
                StatusText.Text = "";
            }
            catch (SqlException)
            {
                StatusText.Text = "Не удалось загрузить бронирования.";
            }
        }

        private DataRowView SelectedBooking()
        {
            DataRowView selected = (DataRowView)BookingsGrid.SelectedItem;
            if (selected == null)
            {
                StatusText.Text = "Выберите бронирование.";
            }
            return selected;
        }

        private void BookingsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!allUsers)
            {
                return;
            }
            DataRowView selected = (DataRowView)BookingsGrid.SelectedItem;
            if (selected == null)
            {
                return;
            }

            StartDate.SelectedDate = Convert.ToDateTime(selected["Check_in_date"]);
            EndDate.SelectedDate = Convert.ToDateTime(selected["Check_out_date"]);
            string status = selected["Booking_status"].ToString();
            for (int i = 0; i < StatusBox.Items.Count; i++)
            {
                ComboBoxItem item = (ComboBoxItem)StatusBox.Items[i];
                if (item.Content.ToString() == status)
                {
                    StatusBox.SelectedIndex = i;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (allUsers || mainWindow.CurrentUser == null)
            {
                return;
            }
            DataRowView selected = SelectedBooking();
            if (selected == null)
            {
                return;
            }

            try
            {
                int bookingId = Convert.ToInt32(selected["ID_Booking"]);
                string error = DataAccess.CancelBooking(bookingId, mainWindow.CurrentUser.Id);
                if (error != "")
                {
                    StatusText.Text = error;
                    return;
                }
                LoadData();
            }
            catch (SqlException)
            {
                StatusText.Text = "Не удалось отменить бронирование.";
            }
            catch (Exception exception)
            {
                StatusText.Text = exception.Message;
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!allUsers || mainWindow.CurrentUser == null || mainWindow.CurrentUser.Role != "Администратор")
            {
                return;
            }
            DataRowView selected = SelectedBooking();
            if (selected == null)
            {
                return;
            }
            if (StartDate.SelectedDate == null || EndDate.SelectedDate == null)
            {
                StatusText.Text = "Укажите даты заезда и выезда.";
                return;
            }

            try
            {
                int bookingId = Convert.ToInt32(selected["ID_Booking"]);
                DateTime start = StartDate.SelectedDate.Value;
                DateTime end = EndDate.SelectedDate.Value;
                ComboBoxItem item = (ComboBoxItem)StatusBox.SelectedItem;
                string status = item.Content.ToString();
                string error = DataAccess.UpdateBooking(bookingId, start, end, status);
                if (error != "")
                {
                    StatusText.Text = error;
                    return;
                }
                LoadData();
            }
            catch (SqlException)
            {
                StatusText.Text = "Не удалось сохранить изменения.";
            }
            catch (Exception exception)
            {
                StatusText.Text = exception.Message;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!allUsers || mainWindow.CurrentUser == null || mainWindow.CurrentUser.Role != "Администратор")
            {
                return;
            }
            DataRowView selected = SelectedBooking();
            if (selected == null)
            {
                return;
            }
            MessageBoxResult answer = MessageBox.Show("Удалить бронирование и его услуги?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                int bookingId = Convert.ToInt32(selected["ID_Booking"]);
                string error = DataAccess.DeleteBooking(bookingId);
                if (error != "")
                {
                    StatusText.Text = error;
                    return;
                }
                LoadData();
            }
            catch (SqlException)
            {
                StatusText.Text = "Не удалось удалить бронирование.";
            }
            catch (Exception exception)
            {
                StatusText.Text = exception.Message;
            }
        }
    }
}
