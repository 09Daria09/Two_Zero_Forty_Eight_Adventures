using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace Two_Zero_Forty_Eight_Adventures
{
    /// <summary>
    /// Interaction logic for Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        public Registration()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            User newUser = new User
            {
                Username = usernameTextBox.Text,
                Password = passwordBox.Text,
                Highscore = 0
            };

            List<User> users = LoadUsers();

            var existingUser = users.FirstOrDefault(u => u.Username == newUser.Username);
            if (existingUser != null)
            {
                existingUser.Password = newUser.Password;
            }
            else
            {
                users.Add(newUser);
            }

            try
            {
                SaveUsers(users);
                MessageBox.Show("Регистрация прошла успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                MainWindow childWindow = new MainWindow(newUser);
                childWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<User> LoadUsers()
        {
            if (!File.Exists("users.json"))
                return new List<User>();

            string json = File.ReadAllText("users.json");
            return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
        }

        private void SaveUsers(List<User> users)
        {
            string json = JsonConvert.SerializeObject(users, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("users.json", json);
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            List<User> users = LoadUsers();

            User matchingUser = users.FirstOrDefault(u => u.Username == usernameTextBox.Text && u.Password == passwordBox.Text);

            if (matchingUser != null)
            {
                MainWindow childWindow = new MainWindow(matchingUser);
                childWindow.Show();
                this.Close();
            }
            else if (users.Any(u => u.Username == usernameTextBox.Text))
            {
                MessageBox.Show("Неверный пароль!");
            }
            else
            {
                MessageBox.Show("Пользователь не зарегистрирован!");
            }
        }

        public class User
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public int Highscore { get; set; }
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text == "Имя пользователя" || textBox.Text == "Пароль")
                {
                    textBox.Text = string.Empty;
                }
            }
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    if (textBox == usernameTextBox)
                    {
                        textBox.Text = "Имя пользователя";
                    }
                    else if (textBox == passwordBox)
                    {
                        textBox.Text = "Пароль";
                    }
                }
            }
        }


    }
}
