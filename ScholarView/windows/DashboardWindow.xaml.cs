using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SqlClient;
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
using ScholarView.Validators;
using ScholarView.windows;

namespace ScholarView.windows
{
    public partial class DashboardWindow : Window
    {
        private readonly string userName;
        private readonly bool isTeacher;

        public int UserId { get; private set; }

        public DashboardWindow(int userId, string userName, bool isTeacher)
        {
            InitializeComponent();
            this.userName = userName;
            this.isTeacher = isTeacher;
            
            UserNameText.Text = userName;

            LoadProfileImage(userId);

            // Loads Dashboard view based on Role
            AdjustDashboardForUser();

            LoggedInUser.UserId = userId;
            WelcomeMessage.Text = $"Welcome, {userName}!";
        }

        private void AdjustDashboardForUser()
        {
            if (isTeacher)
            {
                StudentOptionsPanel.Visibility = Visibility.Collapsed;
                TeacherOptionsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                TeacherOptionsPanel.Visibility = Visibility.Collapsed;
                StudentOptionsPanel.Visibility = Visibility.Visible;
            }
        }

        private void LoadProfileImage(int userId)
        {
            using (var context = new ApplicationDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.UserId == userId);

                if (user != null && user.UserPhoto != null)
                {
                    using (var ms = new System.IO.MemoryStream(user.UserPhoto))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.StreamSource = ms;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.EndInit();
                        ProfileImage.Source = image;
                    }
                }
                else
                {
                    try
                    {
                        ProfileImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/default_profile.png"));
                    }
                    catch (System.IO.IOException ex)
                    {
                        Console.WriteLine("No profile Image yet.");
                    }
                }
            }
        }


        private async void ViewSchedule_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(500); // Slight delay for a smoother transition

            ScheduleSelectionWindow scheduleSelectionWindow = new ScheduleSelectionWindow(LoggedInUser.UserId, isTeacher);
            scheduleSelectionWindow.Show();
            this.Close();
        }

        private void ViewGrades_Click(object sender, RoutedEventArgs e)
        {
            StudentGrades studentGradesWindow = new StudentGrades(LoggedInUser.UserId);
            studentGradesWindow.Show();
            this.Close();
        }

        private void ManageCourses_Click(object sender, RoutedEventArgs e)
        {
            TeacherCourses teacherCoursesWindow = new TeacherCourses(LoggedInUser.UserId);
            teacherCoursesWindow.Show();
            this.Close();
        }
        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                byte[] imageBytes = System.IO.File.ReadAllBytes(openFileDialog.FileName);

                using (var context = new ApplicationDbContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == LoggedInUser.UserId);

                    if (user != null)
                    {
                        user.UserPhoto = imageBytes;
                        context.SaveChanges();
                        MessageBox.Show("Profile photo updated!");
                        LoadProfileImage(LoggedInUser.UserId);
                    }
                }
            }
        }
        private void BrowseCourses_Click(object sender, RoutedEventArgs e)
        {
            StudentCourseSearchWindow courseSearchWindow = new StudentCourseSearchWindow(LoggedInUser.UserId, userName);
            courseSearchWindow.Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginPage loginPage = new LoginPage();
            loginPage.Show();
            this.Close();
        }

    }
}
