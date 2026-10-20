using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient; 

namespace Hotel_Vasanov_ISP43
{
    public class UserSession
    {
        public int Id;
        public string FullName;
        public string Role;
    }

    public static class DataAccess
    {
        public static SqlConnection Connect()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["HotelDatabase"].ConnectionString;
            return new SqlConnection(connectionString);
        }

        public static UserSession Login(string email, string password)
        {
            using (SqlConnection connection = Connect())
            using (SqlCommand command = new SqlCommand("SELECT Users.ID_User, Users.Full_name, Roles.Name FROM Users, Roles WHERE Users.ID_Role = Roles.ID_Role AND Users.Email_address = @Email AND Users.Password = @Password", connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        UserSession user = new UserSession();
                        user.Id = Convert.ToInt32(reader["ID_User"]);
                        user.FullName = reader["Full_name"].ToString();
                        user.Role = reader["Name"].ToString();
                        return user;
                    }
                }

                return null;
            }
        }

        public static string Register(string fullName, string phone, string email, string password)
        {
            using (SqlConnection connection = Connect())
            using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email_address = @Email OR Phone_number = @Phone", connection))
            {
                command.Parameters.AddWithValue("@Name", fullName);
                command.Parameters.AddWithValue("@Phone", phone);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password);
                connection.Open();
                if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                {
                    return "Такая почта или телефон уже зарегистрированы.";
                }
                command.CommandText = "INSERT INTO Users (Full_name, Phone_number, Email_address, Password, ID_Role) SELECT @Name, @Phone, @Email, @Password, ID_Role FROM Roles WHERE Name = N'Пользователь'";
                if (command.ExecuteNonQuery() != 1)
                {
                    return "Роль пользователя не найдена в базе данных.";
                }
            }
            return "";
        }

        public static DataTable Rooms()
        {
            using (SqlConnection connection = Connect())
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT Rooms.ID_Room, Rooms.Room_number, RoomCategories.Category_name, RoomCategories.Room_capacity, Rooms.Floor_number, Rooms.Base_rate, Rooms.Room_status FROM Rooms, RoomCategories WHERE Rooms.ID_Room_Category = RoomCategories.ID_Room_Category ORDER BY Rooms.Room_number", connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public static DataTable Categories()
        {
            using (SqlConnection connection = Connect())
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT ID_Room_Category, Category_name FROM RoomCategories ORDER BY Category_name", connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public static string AddRoom(UserSession user, string number, int categoryId, int floor, double rate, string status)
        {
            if (user == null || user.Role != "Администратор")
            {
                return "Добавлять номера может только администратор.";
            }
            number = number.Trim();
            if (number.Length == 0 || number.Length > 10)
            {
                return "Обозначение номера должно содержать от 1 до 10 символов.";
            }
            if (categoryId <= 0)
            {
                return "Выберите категорию номера.";
            }
            if (floor < 1 || floor > 30)
            {
                return "Этаж должен быть от 1 до 30.";
            }
            if (rate <= 0 || rate >= 1000000 || Math.Round(rate, 2) != rate)
            {
                return "Цена за ночь должна быть больше нуля, не выше 1000000 и содержать не более двух знаков после запятой.";
            }
            if (status != "Свободен" && status != "На обслуживании")
            {
                return "Выберите состояние номера.";
            }

            using (SqlConnection connection = Connect())
            using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Rooms WHERE Room_number = @Number", connection))
            {
                command.Parameters.AddWithValue("@Number", number);
                command.Parameters.AddWithValue("@Category", categoryId);
                command.Parameters.AddWithValue("@Floor", floor);
                command.Parameters.AddWithValue("@Rate", rate);
                command.Parameters.AddWithValue("@Status", status);
                connection.Open();
                if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                {
                    return "Номер с таким обозначением уже существует.";
                }
                command.CommandText = "INSERT INTO Rooms (Room_number, ID_Room_Category, Floor_number, Base_rate, Room_status) VALUES (@Number, @Category, @Floor, @Rate, @Status)";
                command.ExecuteNonQuery();
            }
            return "";
        }

        public static string AddService(UserSession user, string name, double price)
        {
            if (user == null || user.Role != "Администратор")
            {
                return "Добавлять услуги может только администратор.";
            }
            name = name.Trim();
            if (name.Length == 0 || name.Length > 60)
            {
                return "Название услуги должно содержать от 1 до 60 символов.";
            }
            if (price < 0 || price >= 1000000 || Math.Round(price, 2) != price)
            {
                return "Цена услуги должна быть от 0 до 1000000 и содержать не более двух знаков после запятой.";
            }

            using (SqlConnection connection = Connect())
            using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Services WHERE Service_name = @Name", connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Price", price);
                connection.Open();
                if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                {
                    return "Услуга с таким названием уже существует.";
                }
                command.CommandText = "INSERT INTO Services (Service_name, Current_price) VALUES (@Name, @Price)";
                command.ExecuteNonQuery();
            }
            return "";
        }

        public static DataTable Services()
        {
            using (SqlConnection connection = Connect())
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT Service_name, Current_price FROM Services ORDER BY Service_name", connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public static DataTable Bookings(int userId)
        {
            string sql = "SELECT Bookings.ID_Booking, Users.Full_name, Rooms.Room_number, Bookings.Check_in_date, Bookings.Check_out_date, Bookings.Nightly_rate, Bookings.Booking_status FROM Bookings, Users, Rooms WHERE Bookings.ID_User = Users.ID_User AND Bookings.ID_Room = Rooms.ID_Room ";
            if (userId != 0)
            {
                sql = sql + "AND Bookings.ID_User = @User ";
            }
            sql = sql + "ORDER BY Bookings.Check_in_date DESC, Bookings.ID_Booking DESC";

            using (SqlConnection connection = Connect())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (userId != 0)
                {
                    command.Parameters.AddWithValue("@User", userId);
                }

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        private static bool HasOverlap(SqlConnection connection, int roomId, DateTime start, DateTime end, int bookingId)
        {
            using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Bookings WHERE ID_Room = @Room AND ID_Booking != @Booking AND Booking_status = N'Подтверждено' AND Check_in_date < @End AND Check_out_date > @Start", connection))
            {
                command.Parameters.AddWithValue("@Room", roomId);
                command.Parameters.AddWithValue("@Booking", bookingId);
                command.Parameters.AddWithValue("@Start", start);
                command.Parameters.AddWithValue("@End", end);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public static string CreateBooking(int userId, int roomId, DateTime start, DateTime end)
        {
            if (start.Date < DateTime.Today || end.Date <= start.Date)
            {
                return "Укажите корректные даты: выезд должен быть позже заезда.";
            }

            using (SqlConnection connection = Connect())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("", connection))
                {
                    command.CommandText = "SELECT Base_rate FROM Rooms WHERE ID_Room = @Room AND Room_status = N'Свободен'";
                    command.Parameters.AddWithValue("@Room", roomId);
                    double rate;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return "Номер сейчас недоступен.";
                        }
                        rate = Convert.ToDouble(reader["Base_rate"]);
                    }

                    if (HasOverlap(connection, roomId, start, end, 0))
                    {
                        return "На эти даты номер уже забронирован.";
                    }

                    command.CommandText = "INSERT INTO Bookings (ID_User, ID_Room, Check_in_date, Check_out_date, Nightly_rate, Booking_status) VALUES (@User, @Room, @Start, @End, @Rate, N'Подтверждено')";
                    command.Parameters.AddWithValue("@User", userId);
                    command.Parameters.AddWithValue("@Start", start);
                    command.Parameters.AddWithValue("@End", end);
                    command.Parameters.AddWithValue("@Rate", rate);
                    command.ExecuteNonQuery();
                }
            }
            return "";
        }

        public static string CancelBooking(int bookingId, int userId)
        {
            using (SqlConnection connection = Connect())
            using (SqlCommand command = new SqlCommand("UPDATE Bookings SET Booking_status = N'Отменено' WHERE ID_Booking = @Booking AND ID_User = @User AND Booking_status = N'Подтверждено'", connection))
            {
                command.Parameters.AddWithValue("@Booking", bookingId);
                command.Parameters.AddWithValue("@User", userId);
                connection.Open();

                if (command.ExecuteNonQuery() != 1)
                {
                    return "Это бронирование нельзя отменить.";
                }
            }
            return "";
        }

        public static string UpdateBooking(int bookingId, DateTime start, DateTime end, string status)
        {
            if (end.Date <= start.Date)
            {
                return "Дата выезда должна быть позже даты заезда.";
            }
            if (status != "Подтверждено" && status != "Завершено" && status != "Отменено")
            {
                return "Неизвестный статус бронирования.";
            }

            using (SqlConnection connection = Connect())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("", connection))
                {
                    command.CommandText = "SELECT ID_Room FROM Bookings WHERE ID_Booking = @Booking";
                    command.Parameters.AddWithValue("@Booking", bookingId);
                    int roomId;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return "Бронирование не найдено.";
                        }
                        roomId = reader.GetInt32(0);
                    }

                    if (status == "Подтверждено" && HasOverlap(connection, roomId, start, end, bookingId))
                    {
                        return "На эти даты номер уже забронирован.";
                    }

                    command.CommandText = "UPDATE Bookings SET Check_in_date = @Start, Check_out_date = @End, Booking_status = @Status WHERE ID_Booking = @Booking";
                    command.Parameters.AddWithValue("@Start", start);
                    command.Parameters.AddWithValue("@End", end);
                    command.Parameters.AddWithValue("@Status", status);
                    command.ExecuteNonQuery();
                }
            }
            return "";
        }

        public static string DeleteBooking(int bookingId)
        {
            using (SqlConnection connection = Connect())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("", connection))
                {
                    command.CommandText = "DELETE FROM BookingServices WHERE ID_Booking = @Booking";
                    command.Parameters.AddWithValue("@Booking", bookingId);
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM Bookings WHERE ID_Booking = @Booking";
                    if (command.ExecuteNonQuery() != 1)
                    {
                        return "Бронирование не найдено.";
                    }
                }
            }
            return "";
        }
    }
}

