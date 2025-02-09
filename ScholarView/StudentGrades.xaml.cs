using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ScholarView.Validators;
using ScholarView.windows;

namespace ScholarView
{
    /// <summary>
    /// Interaction logic for StudentGrades.xaml
    /// </summary>
    public partial class StudentGrades : Window
    {
        public StudentGrades(int studentId)
        {
            InitializeComponent();

            try
            {
                ApplicationDbContext db = new ApplicationDbContext();

                LvGrades.ItemsSource = db.Enrollments
                    .Where(enrollment => enrollment.StudentId == studentId)
                    .Include("Cours")
                    .ToList();
            }
            catch (SystemException ex)
            {
                MessageBox.Show(this, "Error reading from database\n" + ex.Message, "Fatal error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(ex.Message);
                this.Close();
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
            gradesTable.Columns.Add(new TableColumn { Width = new GridLength(150) });
            gradesTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
            gradesTable.Columns.Add(new TableColumn { Width = new GridLength(70) });
            gradesTable.Columns.Add(new TableColumn { Width = new GridLength(70) });

            TableRow headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Course")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Semester")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Year")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Grade")) { FontWeight = FontWeights.Bold }));
            gradesTable.RowGroups.Add(new TableRowGroup { Rows = { headerRow } });

            foreach (var item in LvGrades.Items)
            {
                if (item is Enrollment enrollment)
                {
                    TableRow row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run(enrollment.Cours.CourseName))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(enrollment.Cours.Semester.ToString()))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(enrollment.Cours.Year.ToString()))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(enrollment.Grade?.ToString() ?? "N/A"))));
                    gradesTable.RowGroups[0].Rows.Add(row);
                }
            }

            document.Blocks.Add(gradesTable);

            PrintHelper.PrintFlowDocument(document);
        }

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboardWindow = new DashboardWindow(LoggedInUser.UserId, LoggedInUser.UserName, false);
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
