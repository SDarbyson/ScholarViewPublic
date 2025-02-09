using System;
using System.Collections.Generic;
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
using ScholarView.Models;
using ScholarView.Validators;

namespace ScholarView.windows
{
    public partial class TeacherScheduleWindow : Window
    {
        private int TeacherId { get; set; } // Logged-in teacher's ID
        private string UserName { get; set; } // Logged-in teacher's name

        public TeacherScheduleWindow(int teacherId, string teacherName)
        {
            InitializeComponent();
            this.TeacherId = teacherId;
            this.UserName = teacherName;
            LoadSchedule(teacherId);

            if (teacherId > 0) // Valid TeacherId check
            {
                LoadSchedule(teacherId);
            }
            else
            {
                MessageBox.Show("Invalid Teacher ID");
                this.Close();
            }
        }

        private void LoadSchedule(int teacherId)
        {
            using (var context = new ApplicationDbContext())
            {
                var scheduleData = from Course in context.Courses
                                   where Course.TeacherId == TeacherId
                                   select new
                                   {
                                       Course.CourseName,
                                       Course.CourseDOW,
                                       Course.StartTime,
                                       Course.EndTime
                                   };

                foreach (var course in scheduleData)
                {
                    int column = GetColumnForDay(course.CourseDOW); // Day column
                    int row = GetRowForTime(course.StartTime); // Time row

                    if (column > 0 && row > 0)
                    {
                        // Add a Rectangle for visual representation
                        Rectangle rectangle = new Rectangle
                        {
                            Fill = new SolidColorBrush(Colors.LightGreen),
                            Margin = new Thickness(2)
                        };

                        Grid.SetColumn(rectangle, column);
                        Grid.SetRow(rectangle, row);
                        ScheduleGrid.Children.Add(rectangle);

                        // Add a TextBlock for course name
                        TextBlock courseText = new TextBlock
                        {
                            Text = course.CourseName,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Foreground = Brushes.Black,
                            FontWeight = FontWeights.Bold
                        };

                        Grid.SetColumn(courseText, column);
                        Grid.SetRow(courseText, row);
                        ScheduleGrid.Children.Add(courseText);
                    }
                }
            }
        }

        private int GetColumnForDay(string dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case "Monday": return 1;
                case "Tuesday": return 2;
                case "Wednesday": return 3;
                case "Thursday": return 4;
                case "Friday": return 5;
                default: return 0;
            }
        }

        private int GetRowForTime(TimeSpan startTime)
        {
            if (startTime.Hours >= 8 && startTime.Hours < 10) return 1;
            if (startTime.Hours >= 10 && startTime.Hours < 12) return 2;
            if (startTime.Hours >= 12 && startTime.Hours < 14) return 3;
            if (startTime.Hours >= 14 && startTime.Hours < 16) return 4;
            if (startTime.Hours >= 16 && startTime.Hours < 18) return 5;
            if (startTime.Hours >= 18 && startTime.Hours < 20) return 6;
            return 0;
        }

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboardWindow = new DashboardWindow(LoggedInUser.UserId.Value, LoggedInUser.UserName, true);
            dashboardWindow.Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
