using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ScholarView
{
    /// <summary>
    /// Interaction logic for CourseDetails.xaml
    /// </summary>
    public partial class CourseDetails : Window
    {
        private int _studentId;
        private int _courseId;
        private Cours _currentCourse;

        public CourseDetails(int courseId, int studentId)
        {
            InitializeComponent();
            try
            {
                using (var context = new ApplicationDbContext()){

                    _currentCourse = context.Courses
                        .Include("User")
                        .FirstOrDefault(c => c.CourseId == courseId);

                    if (_currentCourse != null)
                    {
                        this.DataContext = _currentCourse;
                    }
                    _courseId = courseId;
                    _studentId = studentId;
                        if (CheckTrueIfEnrolled(context))
                        {
                            BtnRegister.Visibility = Visibility.Hidden;
                            LblPrevRegister.Visibility = Visibility.Visible;
                        }
                }
            }
            catch (SystemException ex)
            {
                MessageBox.Show(this, "Error reading from database\n" + ex.Message, "Fatal error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(ex.Message);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var newRegister = new Enrollment
                    {
                        StudentId = _studentId,
                        CourseId = _courseId,
                    };

                    // check for course TimeSpan overlap
                    List<Cours> matchingCourseTimes = context.Enrollments
                        .Where(n => n.StudentId == _studentId)
                        .Select(n => n.Cours)
                        .Where(c => c.Semester == _currentCourse.Semester
                        && c.Year == _currentCourse.Year
                        && c.CourseDOW == _currentCourse.CourseDOW)
                        .ToList();
                    foreach (Cours course in matchingCourseTimes)
                    {
                        if (DoCoursesOverlap(_currentCourse, course))
                        {
                            MessageBox.Show(this, "Another course you attend has a time overlap.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    // check that capacity of class has not been reached
                    var updateRegistered = context.Courses.FirstOrDefault(c => c.CourseId == _courseId);
                    if (updateRegistered.NumRegistered < updateRegistered.Capacity)
                    {
                        updateRegistered.NumRegistered++;
                    }
                    else
                    {
                        MessageBox.Show(this, "This class is at max capacity.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    context.Enrollments.Add(newRegister);
                    context.SaveChanges();
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error reading from database\n" + ex.Message, "Database error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(ex.Message);
            }
        }
        public static class PrintHelper
        {
            public static void PrintFlowDocument(FlowDocument document)
            {
                PrintDialog printDialog = new PrintDialog();

                if (printDialog.ShowDialog() == true)
                {
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

            Paragraph title = new Paragraph(new Run("Course Details"));
            title.FontSize = 18;
            title.FontWeight = FontWeights.Bold;
            title.TextAlignment = TextAlignment.Center;
            document.Blocks.Add(title);

            if (this.DataContext is Cours course)
            {
                Paragraph courseDetails = new Paragraph();
                courseDetails.Inlines.Add(new Bold(new Run("Course Name: ")));
                courseDetails.Inlines.Add(new Run(course.CourseName + "\n"));
                courseDetails.Inlines.Add(new Bold(new Run("Teacher: ")));
                courseDetails.Inlines.Add(new Run(course.User?.Name ?? "N/A" + "\n"));
                courseDetails.Inlines.Add(new Bold(new Run("Days: ")));
                courseDetails.Inlines.Add(new Run(course.CourseDOW + "\n"));
                courseDetails.Inlines.Add(new Bold(new Run("Start Time: ")));
                courseDetails.Inlines.Add(new Run(course.StartTime.ToString(@"hh\:mm") + "\n"));
                courseDetails.Inlines.Add(new Bold(new Run("End Time: ")));
                courseDetails.Inlines.Add(new Run(course.EndTime.ToString(@"hh\:mm") + "\n"));
                courseDetails.Inlines.Add(new Bold(new Run("Year: ")));
                courseDetails.Inlines.Add(new Run(course.Year.ToString() + "\n"));
                courseDetails.Inlines.Add(new Bold(new Run("Semester: ")));
                courseDetails.Inlines.Add(new Run(course.Semester.ToString() + "\n"));

                document.Blocks.Add(courseDetails);
            }
            else
            {
                MessageBox.Show("No course details available to print.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PrintHelper.PrintFlowDocument(document);
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private bool DoCoursesOverlap(Cours beingRegistered, Cours alreadyEnrolled)
        {
            return beingRegistered.StartTime < alreadyEnrolled.EndTime
                && beingRegistered.EndTime > alreadyEnrolled.StartTime;
        }

        private bool CheckTrueIfEnrolled(ApplicationDbContext context)
        {
            Enrollment matchingEnrollment = context.Enrollments
                        .FirstOrDefault(n => n.StudentId == _studentId && n.CourseId == _courseId);
            return (matchingEnrollment != null);
        }
    }
}
