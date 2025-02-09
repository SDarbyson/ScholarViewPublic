using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ScholarView
{
    /// <summary>
    /// Interaction logic for StudentsGrades.xaml
    /// </summary>
    public partial class StudentsGrades : Window
    {
        private int _courseId;

        public StudentsGrades(int courseId)
        {
            InitializeComponent();
            _courseId = courseId;

            try
            {
                ApplicationDbContext db = new ApplicationDbContext();

                var course = db.Courses.FirstOrDefault(c => c.CourseId == courseId);
                if (course != null)
                {
                    DataContext = new Cours
                    {
                        CourseName = course.CourseName,
                        Semester = course.Semester,
                        Year = course.Year
                    };
                }

                RefreshListView(db, courseId);
            }
            catch (SystemException ex)
            {
                MessageBox.Show(this, "Error reading from database\n" + ex.Message, "Fatal error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItemUpdateGrade(object sender, RoutedEventArgs e)
        {
            if (LvStudents.SelectedItem is Enrollment selectedEnrollment)
            {
                using (var db = new ApplicationDbContext())
                {
                    var enrollment = db.Enrollments.Find(selectedEnrollment.EnrollmentId);

                    if (enrollment == null)
                    {
                        MessageBox.Show("Failed to retrieve enrollment data.");
                        return;
                    }

                    UpdateGrade gradeDialog = new UpdateGrade(enrollment);
                    gradeDialog.Owner = this;

                    if (gradeDialog.ShowDialog() == true)
                    {
                        db.SaveChanges();
                        MessageBox.Show("Grade updated successfully.");
                        RefreshListView(db, enrollment.CourseId);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a student to update.");
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

            Paragraph title = new Paragraph(new Run("Student Grades"));
            title.FontSize = 18;
            title.FontWeight = FontWeights.Bold;
            title.TextAlignment = TextAlignment.Center;
            document.Blocks.Add(title);

            Table gradesTable = new Table();
            gradesTable.CellSpacing = 10;
            gradesTable.Columns.Add(new TableColumn { Width = new GridLength(200) });
            gradesTable.Columns.Add(new TableColumn { Width = new GridLength(100) });

            TableRow headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Student")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Grade")) { FontWeight = FontWeights.Bold }));
            gradesTable.RowGroups.Add(new TableRowGroup { Rows = { headerRow } });

            foreach (var item in LvStudents.Items)
            {
                if (item is Enrollment enrollment)
                {
                    TableRow row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run(enrollment.User.Name))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(enrollment.Grade.ToString()))));
                    gradesTable.RowGroups[0].Rows.Add(row);
                }
            }

            document.Blocks.Add(gradesTable);

            PrintHelper.PrintFlowDocument(document);
        }

        private void RefreshListView(ApplicationDbContext db, int courseId)
        {
            LvStudents.ItemsSource = db.Enrollments
                .Where(enrollment => enrollment.CourseId == courseId)
                .Include("User")
                .ToList();
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
