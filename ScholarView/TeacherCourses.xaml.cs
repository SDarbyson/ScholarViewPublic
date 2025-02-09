using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using ScholarView.windows;

namespace ScholarView
{
    /// <summary>
    /// Interaction logic for TeacherCourses.xaml
    /// </summary>
    public partial class TeacherCourses : Window
    {
        private int _teacherId;

        public TeacherCourses(int teacherId)
        {
            InitializeComponent();
            _teacherId = teacherId;

            try
            {
                ApplicationDbContext db = new ApplicationDbContext();

                LvCourses.ItemsSource = db.Courses
                          .Where(course => course.TeacherId == _teacherId)
                          .ToList();
            }
            catch (SystemException ex)
            {
                MessageBox.Show(this, "Error reading from database\n" + ex.Message, "Fatal error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }

        private void LvCourses_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LvCourses.SelectedItem is Cours selectedCourse)
            {
                StudentsGrades dialog = new StudentsGrades(selectedCourse.CourseId);

                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    Console.WriteLine($"Opened details for Course");
                }
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

            Paragraph title = new Paragraph(new Run("Teacher's Courses"));
            title.FontSize = 18;
            title.FontWeight = FontWeights.Bold;
            title.TextAlignment = TextAlignment.Center;
            document.Blocks.Add(title);

            Table coursesTable = new Table();
            coursesTable.CellSpacing = 10;
            coursesTable.Columns.Add(new TableColumn { Width = new GridLength(150) });
            coursesTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
            coursesTable.Columns.Add(new TableColumn { Width = new GridLength(100) });

            TableRow headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Course Name")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Capacity")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Registered Students")) { FontWeight = FontWeights.Bold }));
            coursesTable.RowGroups.Add(new TableRowGroup { Rows = { headerRow } });

            foreach (var item in LvCourses.Items)
            {
                if (item is Cours course)
                {
                    TableRow row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run(course.CourseName))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(course.Capacity.ToString()))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(course.NumRegistered.ToString()))));
                    coursesTable.RowGroups[0].Rows.Add(row);
                }
            }

            document.Blocks.Add(coursesTable);
            PrintHelper.PrintFlowDocument(document);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboard = new DashboardWindow(_teacherId, "Teacher", true);
            dashboard.Show();
            this.Close();
        }
    }
}
