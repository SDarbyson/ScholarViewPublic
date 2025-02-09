using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using ScholarView.Models;
using ScholarView.Validators; // LoggedInUser sınıfını kullanmak için

namespace ScholarView.windows
{
    public partial class LoginPage : Window
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            var user = AuthenticateUser(email, password);

            if (user != null)
            {
                LoggedInUser.UserId = user.UserId;
                LoggedInUser.UserName = user.Name;

                // Pass RoleEnum to DashboardWindow to load appropriate view
                bool isTeacher = user.Role == RoleEnum.Teacher;
                DashboardWindow dashboardWindow = new DashboardWindow(LoggedInUser.UserId, LoggedInUser.UserName, isTeacher);
                dashboardWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid credentials. Please try again.");
            }
        }

        private User AuthenticateUser(string email, string password)
        {
            using (var context = new ApplicationDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Email == email);

                if (user != null)
                {
                    string hashedPassword = HashPassword(password);

                    if (user.Password == hashedPassword)
                    {
                        return user;
                    }
                }

                return null;
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));   
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }


        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow home = new MainWindow();
            home.Show();
            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }
    }
}
