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
    public partial class ScheduleWindow : Window
    {
        private int _studentId { get; set; } // Logged-in student's ID
        private int _year { get; set; }
        private SemesterEnum _semester {  get; set; }
        private bool _isTeacher { get; set; }

        public ScheduleWindow(int studentId, bool isTeacher, int year, SemesterEnum semester)
        {
            InitializeComponent();
            this._studentId = studentId;
            this._year = year;
            this._semester = semester;
            this._isTeacher = isTeacher;
            TxbSchedule.Text = $"Your Weekly Schedule for {_semester} {_year}";
            LoadSchedule();
        }

        private void LoadSchedule()
        {
            using (var context = new ApplicationDbContext())
            {
                dynamic scheduleData;
                if (!_isTeacher){
                    scheduleData = from Enrollment in context.Enrollments
                                       join Course in context.Courses on Enrollment.CourseId equals Course.CourseId
                                       where Enrollment.StudentId == _studentId
                                          && Course.Semester == _semester
                                         && Course.Year == _year
                                       select new
                                       {
                                           Course.CourseName,
                                           Course.CourseDOW,
                                           Course.StartTime,
                                           Course.EndTime
                                       };
                }
                else
                {
                    scheduleData = from Course in context.Courses
                                       where Course.TeacherId == _studentId
                                       && Course.Semester == _semester
                                       && Course.Year == _year
                                       select new
                                       {
                                           Course.CourseName,
                                           Course.CourseDOW,
                                           Course.StartTime,
                                           Course.EndTime
                                       };
                }

                // Add schedule data to the UI
                foreach (var course in scheduleData)
                {
                    int column = GetColumnForDay(course.CourseDOW);
                    int row = GetRowForTime(course.StartTime);

                    if (column > 0 && row > 0)
                    {
                        // Add the course placement to the Grid
                        Rectangle rectangle = new Rectangle
                        {
                            Fill = new SolidColorBrush(Colors.LightBlue),
                            Margin = new Thickness(2)
                        };

                        Grid.SetColumn(rectangle, column);
                        Grid.SetRow(rectangle, row);
                        ScheduleGrid.Children.Add(rectangle); // Add the element

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
            // Return columns corresponding to day headers
            Console.WriteLine("DayOfWeek: " + dayOfWeek);
            switch (dayOfWeek)
            {
                case "Monday": return 1;
                case "Tuesday": return 2;
                case "Wednesday": return 3;
                case "Thursday": return 4;
                case "Friday": return 5;
                default: return 0; // No column for other days
            }
        }

        private int GetRowForTime(TimeSpan startTime)
        {
            Console.WriteLine("StartTime: " + startTime);
            // Return rows corresponding to times
            if (startTime.Hours >= 8 && startTime.Hours < 10)
                return 1;
            else if (startTime.Hours >= 10 && startTime.Hours < 12)
                return 2;
            else if (startTime.Hours >= 12 && startTime.Hours < 14)
                return 3;
            else if (startTime.Hours >= 14 && startTime.Hours < 16)
                return 4;
            else if (startTime.Hours >= 16 && startTime.Hours < 18)
                return 5;
            else if (startTime.Hours >= 18 && startTime.Hours < 20)
                return 6;
            else
                return 0; // Not a valid time slot
        }

        private Color GetColorForDay(string dayOfWeek)
        {
            if (dayOfWeek == "Monday") return Colors.LightBlue;
            if (dayOfWeek == "Tuesday") return Colors.LightGreen;
            if (dayOfWeek == "Wednesday") return Colors.LightYellow;
            if (dayOfWeek == "Thursday") return Colors.LightPink;
            if (dayOfWeek == "Friday") return Colors.LightCoral;
            return Colors.Transparent;
        }

        public static class PrintHelper
        {
            public static void PrintFlowDocument(FlowDocument document)
            {
                PrintDialog printDialog = new PrintDialog();

                if (printDialog.ShowDialog() == true)
                {
                    // Adjusting printing dimensions
                    document.PageHeight = printDialog.PrintableAreaHeight;
                    document.PageWidth = printDialog.PrintableAreaWidth;

                    IDocumentPaginatorSource idpSource = document;
                    printDialog.PrintDocument(idpSource.DocumentPaginator, "Print Document");
                }
            }
        }
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            FlowDocument document = new FlowDocument();

            Paragraph title = new Paragraph(new Run("Schedule"));
            title.FontSize = 18;
            title.FontWeight = FontWeights.Bold;
            title.TextAlignment = TextAlignment.Center;
            document.Blocks.Add(title);

            using (var context = new ApplicationDbContext())
            {
                dynamic scheduleData;
                if (!_isTeacher)
                {
                    scheduleData = from Enrollment in context.Enrollments
                                   join Course in context.Courses on Enrollment.CourseId equals Course.CourseId
                                   where Enrollment.StudentId == _studentId
                                      && Course.Semester == _semester
                                     && Course.Year == _year
                                   select new
                                   {
                                       Course.CourseName,
                                       Course.CourseDOW,
                                       Course.StartTime,
                                       Course.EndTime
                                   };
                }
                else
                {
                    scheduleData = from Course in context.Courses
                                   where Course.TeacherId == _studentId
                                   && Course.Semester == _semester
                                   && Course.Year == _year
                                   select new
                                   {
                                       Course.CourseName,
                                       Course.CourseDOW,
                                       Course.StartTime,
                                       Course.EndTime
                                   };
                }
                foreach (var course in scheduleData)
                {
                    Paragraph courseDetails = new Paragraph();
                    courseDetails.Inlines.Add(new Bold(new Run("Course Name: ")));
                    courseDetails.Inlines.Add(new Run(course.CourseName + "\n"));
                    courseDetails.Inlines.Add(new Bold(new Run("Day: ")));
                    courseDetails.Inlines.Add(new Run(course.CourseDOW + "\n"));
                    courseDetails.Inlines.Add(new Bold(new Run("Time: ")));
                    courseDetails.Inlines.Add(new Run($"{course.StartTime:hh\\:mm} - {course.EndTime:hh\\:mm}\n"));
                    courseDetails.Margin = new Thickness(0, 0, 0, 10);
                    document.Blocks.Add(courseDetails);
                }

                PrintHelper.PrintFlowDocument(document);
            }

        }


        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboardWindow = new DashboardWindow(LoggedInUser.UserId, LoggedInUser.UserName, _isTeacher);
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
