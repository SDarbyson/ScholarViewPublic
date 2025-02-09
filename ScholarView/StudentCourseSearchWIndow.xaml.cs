using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using ScholarView.windows;

namespace ScholarView
{
    public partial class StudentCourseSearchWindow : Window
    {
        private List<Cours> courseList = new List<Cours>();
        private int _studentId;
        private string _studentName;

        public StudentCourseSearchWindow(int studentId, string userName)
        {
            _studentId = studentId;
            _studentName = userName;
            InitializeComponent();
            MessageBox.Show($"Welcome, {_studentName}!");
        }

        private void BtnSearchSemester_Click(object sender, RoutedEventArgs e)
        {
            var selectedYear = StudentCourseSPanel.Children.OfType<RadioButton>()
                .FirstOrDefault(rb => rb.GroupName == "YearFilter" && rb.IsChecked == true)?.Content.ToString();
            var selectedSemester = (ComboSemester.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(selectedYear) || string.IsNullOrEmpty(selectedSemester))
            {
                MessageBox.Show("Please select both a year and a semester.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int year = int.Parse(selectedYear);
            SemesterEnum semester = (SemesterEnum)Enum.Parse(typeof(SemesterEnum), selectedSemester);

            using (var context = new ApplicationDbContext())
            {
                var courses = context.Courses.Include("User")
                    .Where(c => c.Year == year && c.Semester == semester)
                    .ToList();

                LvCourses.ItemsSource = courses;

                if (courses.Any())
                {
                    Console.WriteLine($"Found {courses.Count} courses for {semester} {year}.");
                }
                else
                {
                    MessageBox.Show($"No courses found for {semester} {year}.");
                }
            }
        }

        private void BtnStudentSearchAllCourses_Click(object sender, RoutedEventArgs e)
        {
            RefreshSearchListView();
        }

        private void RefreshSearchListView()
        {
            using (var context = new ApplicationDbContext())
            {
                courseList = context.Courses.Include("User").ToList();
                LvCourses.ItemsSource = courseList;
            }
        }

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboard = new DashboardWindow(_studentId, _studentName, false);
            dashboard.Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
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

            Paragraph title = new Paragraph(new Run("Available Courses"));
            title.FontSize = 18;
            title.FontWeight = FontWeights.Bold;
            title.TextAlignment = TextAlignment.Center;
            document.Blocks.Add(title);

            Table coursesTable = new Table();
            coursesTable.CellSpacing = 10;
            coursesTable.Columns.Add(new TableColumn { Width = new GridLength(150) });
            coursesTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
            coursesTable.Columns.Add(new TableColumn { Width = new GridLength(50) });
            coursesTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
            coursesTable.Columns.Add(new TableColumn { Width = new GridLength(70) });
            coursesTable.Columns.Add(new TableColumn { Width = new GridLength(100) });

            TableRow headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Course Name")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Teacher")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Capacity")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Days")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Year")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Semester")) { FontWeight = FontWeights.Bold }));
            coursesTable.RowGroups.Add(new TableRowGroup { Rows = { headerRow } });

            foreach (var item in LvCourses.Items)
            {
                if (item is Cours course)
                {
                    TableRow row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run(course.CourseName))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(course.User?.Name ?? "N/A"))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(course.Capacity.ToString()))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(course.CourseDOW))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(course.Year.ToString()))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(course.Semester.ToString()))));
                    coursesTable.RowGroups[0].Rows.Add(row);
                }
            }

            document.Blocks.Add(coursesTable);

            PrintHelper.PrintFlowDocument(document);
        }


        private void LvCourses_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LvCourses.SelectedItem is Cours selectedCourse)
            {
                CourseDetails courseDetails = new CourseDetails(selectedCourse.CourseId, _studentId);
                courseDetails.Owner = this;

                if (courseDetails.ShowDialog() == true)
                {
                    MessageBox.Show("Registration successful.");
                    BtnSearchSemester_Click(this, new RoutedEventArgs());   // Refresh filtered view
                }
            }
        }
    }
}
