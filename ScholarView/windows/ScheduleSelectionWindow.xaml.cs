using ScholarView.Validators;
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

namespace ScholarView.windows
{
    /// <summary>
    /// Interaction logic for ScheduleSelectionWindow.xaml
    /// </summary>
    public partial class ScheduleSelectionWindow : Window
    {
        private int _userId;
        private bool _isTeacher;
        public ScheduleSelectionWindow(int userId, bool isTeacher)
        {
            _userId = userId;
            _isTeacher = isTeacher;
            InitializeComponent();
        }

        private void BtnSearchSchedule_Click(object sender, RoutedEventArgs e)
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

            ScheduleWindow scheduleWindow = new ScheduleWindow(_userId, _isTeacher, year, semester);
            scheduleWindow.Show();
            this.Close();
        }
    }
}
