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

namespace ScholarView
{
    /// <summary>
    /// Interaction logic for UpdateGrade.xaml
    /// </summary>
    public partial class UpdateGrade : Window
    {
        Enrollment enrollment;
        public UpdateGrade(Enrollment currEnrollment)
        {
            InitializeComponent();
            this.enrollment = currEnrollment;
            LblStudentName.Content = currEnrollment.User.Name;
            TbxGrade.Text = currEnrollment.Grade.ToString();
        }

        private void btnDialogSave_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TbxGrade.Text, out decimal newGrade))
            {
                enrollment.Grade = newGrade; 
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter a valid grade.");
            }
        }
    }
}
