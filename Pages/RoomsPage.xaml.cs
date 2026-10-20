using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls; 

namespace Hotel_Vasanov_ISP43.Pages
{
    public partial class RoomsPage : Page
    {
        private MainWindow mainWindow;
        private DataTable roomsTable;

        public RoomsPage()
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;
            BookingPanel.Visibility = Visibility.Collapsed;
            if (mainWindow.CurrentUser != null && mainWindow.CurrentUser.Role == "Пользователь")
            {
                BookingPanel.Visibility = Visibility.Visible;
            }
            if (mainWindow.CurrentUser != null && mainWindow.CurrentUser.Role == "Администратор")
            {
                AdminRoomsPanel.Visibility = Visibility.Visible;
                AdminServicesPanel.Visibility = Visibility.Visible;
            }
            StartDate.SelectedDate = DateTime.Today;
            EndDate.SelectedDate = DateTime.Today.AddDays(1);
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                roomsTable = DataAccess.Rooms();
                FilterRooms();
                ServicesGrid.ItemsSource = DataAccess.Services().DefaultView;
                if (mainWindow.CurrentUser != null && mainWindow.CurrentUser.Role == "Администратор")
                {
                    CategoryBox.ItemsSource = DataAccess.Categories().DefaultView;
                    CategoryBox.SelectedIndex = 0;
                }
                StatusText.Text = "";
            }
            catch (SqlException)
            {
                StatusText.Text = "Не удалось загрузить данные. Проверьте подключение к базе.";
            }
        }

        private void FilterRooms()
        {
            if (roomsTable == null)
            {
                return;
            }

            string search = CategorySearchBox.Text.Trim().ToLower();
            DataTable filteredRooms = roomsTable.Clone();
            foreach (DataRow row in roomsTable.Rows)
            {
                string category = row["Category_name"].ToString().ToLower();
                if (category.StartsWith(search))
                {
                    filteredRooms.ImportRow(row);
                }
            }

            if (PriceSortBox.SelectedIndex == 1)
            {
                filteredRooms.DefaultView.Sort = "Base_rate ASC";
            }
            else if (PriceSortBox.SelectedIndex == 2)
            {
                filteredRooms.DefaultView.Sort = "Base_rate DESC";
            }
            RoomsGrid.ItemsSource = filteredRooms.DefaultView;
        }

        private void CategorySearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterRooms();
        }

        private void PriceSortBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterRooms();
        }

        private void AddRoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.CurrentUser == null || mainWindow.CurrentUser.Role != "Администратор")
            {
                return;
            }
            RoomStatusText.Text = "";

            int floor;
            double rate;
            if (!int.TryParse(FloorBox.Text.Trim(), out floor))
            {
                RoomStatusText.Text = "Введите этаж целым числом от 1 до 30.";
                return;
            }
            if (!double.TryParse(RoomRateBox.Text.Replace('.', ','), out rate))
            {
                RoomStatusText.Text = "Введите цену числом, например 2500 или 2500,50.";
                return;
            }
            if (CategoryBox.SelectedValue == null || RoomStatusBox.SelectedItem == null)
            {
                RoomStatusText.Text = "Выберите категорию и состояние номера.";
                return;
            }

            try
            {
                int categoryId = Convert.ToInt32(CategoryBox.SelectedValue);
                ComboBoxItem item = (ComboBoxItem)RoomStatusBox.SelectedItem;
                string status = item.Content.ToString();
                string error = DataAccess.AddRoom(mainWindow.CurrentUser, RoomNumberBox.Text, categoryId, floor, rate, status);
                if (error != "")
                {
                    RoomStatusText.Text = error;
                    return;
                }
                RoomNumberBox.Clear();
                FloorBox.Clear();
                RoomRateBox.Clear();
                RoomStatusBox.SelectedIndex = 0;
                LoadData();
                MessageBox.Show("Номер добавлен.", "Гостиница");
            }
            catch (SqlException)
            {
                RoomStatusText.Text = "Не удалось добавить номер в базу данных.";
            }
            catch (Exception exception)
            {
                RoomStatusText.Text = exception.Message;
            }
        }

        private void AddServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.CurrentUser == null || mainWindow.CurrentUser.Role != "Администратор")
            {
                return;
            }
            ServiceStatusText.Text = "";

            double price;
            if (!double.TryParse(ServicePriceBox.Text.Replace('.', ','), out price))
            {
                ServiceStatusText.Text = "Введите цену числом, например 500 или 500,50.";
                return;
            }

            try
            {
                string error = DataAccess.AddService(mainWindow.CurrentUser, ServiceNameBox.Text, price);
                if (error != "")
                {
                    ServiceStatusText.Text = error;
                    return;
                }
                ServiceNameBox.Clear();
                ServicePriceBox.Clear();
                LoadData();
                MessageBox.Show("Услуга добавлена.", "Гостиница");
            }
            catch (SqlException)
            {
                ServiceStatusText.Text = "Не удалось добавить услугу в базу данных.";
            }
            catch (Exception exception)
            {
                ServiceStatusText.Text = exception.Message;
            }
        }

        private void BookButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.CurrentUser == null || mainWindow.CurrentUser.Role != "Пользователь")
            {
                StatusText.Text = "Для бронирования войдите как пользователь.";
                return;
            }
            DataRowView selected = (DataRowView)RoomsGrid.SelectedItem;
            if (selected == null)
            {
                StatusText.Text = "Выберите номер.";
                return;
            }
            if (StartDate.SelectedDate == null || EndDate.SelectedDate == null)
            {
                StatusText.Text = "Укажите даты заезда и выезда.";
                return;
            }

            try
            {
                int roomId = Convert.ToInt32(selected["ID_Room"]);
                DateTime start = StartDate.SelectedDate.Value;
                DateTime end = EndDate.SelectedDate.Value;
                string error = DataAccess.CreateBooking(mainWindow.CurrentUser.Id, roomId, start, end);
                if (error != "")
                {
                    StatusText.Text = error;
                    return;
                }
                MessageBox.Show("Бронирование создано.", "Гостиница");
                mainWindow.ShowBookings();
            }
            catch (SqlException)
            {
                StatusText.Text = "Не удалось сохранить бронирование в базе.";
            }
            catch (Exception exception)
            {
                StatusText.Text = exception.Message;
            }
        }
    }
}