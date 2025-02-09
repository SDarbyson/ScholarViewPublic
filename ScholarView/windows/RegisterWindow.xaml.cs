using System;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using FluentValidation.Results;
using Microsoft.Win32;
using ScholarView.Models;
using ScholarView.Validators;

namespace ScholarView.windows
{
    public partial class RegisterWindow : Window
    {
        private string UserPhotoPath { get; set; }

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var model = new RegisterModel
            {
                Email = EmailTextBox.Text,
                Name = NameTextBox.Text,
                Password = PasswordBox.Password,
                ConfirmPassword = ConfirmPasswordBox.Password
            };

            var validator = new RegisterValidator();
            ValidationResult validationResult = validator.Validate(model);

            if (!validationResult.IsValid)
            {
                MessageBox.Show(string.Join("\n", validationResult.Errors.Select(error => error.ErrorMessage)));
                return;
            }

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var existingUser = context.Users.FirstOrDefault(u => u.Email == model.Email);
                    if (existingUser != null)
                    {
                        MessageBox.Show("A user with this email already exists.");
                        return;
                    }

                    // Yeni kullanıcı ekle
                    var newUser = new User
                    {
                        Email = model.Email,
                        Name = model.Name,
                        Password = HashPassword(model.Password),
                        Role = RoleEnum.Student,
                        BirthDate = BirthDatePicker.SelectedDate.HasValue
                            ? BirthDatePicker.SelectedDate.Value
                            : throw new Exception("Birth date is required."),
                        UserPhoto = string.IsNullOrEmpty(UserPhotoPath) ? null : File.ReadAllBytes(UserPhotoPath)
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();
                }

                MessageBox.Show("Registration successful!");

                LoginPage loginWindow = new LoginPage();
                loginWindow.Show();
                this.Close();
            }
            catch (DbUpdateException ex)
            {
                string errorMessage = "An error occurred while updating the database.";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nDetails: {ex.InnerException.Message}";
                }

                MessageBox.Show(errorMessage);
                Console.WriteLine($"Database Update Exception: {ex}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
                Console.WriteLine($"Exception: {ex}");
            }
        }

        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Select Profile Photo"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                UserPhotoPath = openFileDialog.FileName;
                MessageBox.Show($"Photo selected: {UserPhotoPath}");
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

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPage loginWindow = new LoginPage();
            loginWindow.Show();
            this.Close();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow home = new MainWindow();
            home.Show();
            this.Close();
        }
    }
}
