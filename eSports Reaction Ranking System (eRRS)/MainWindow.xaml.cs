using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace eSports_Reaction_Ranking_System__eRRS_
{
    public class Student
    {
        public required string Name { get; set; }
        public required double ReactionTime_ms { get; set; }
        public string? Category { get; set; }
    }

    public partial class MainWindow : Window
    {
        private ObservableCollection<Student> students; // Stores all students
        public MainWindow()
        {
            InitializeComponent();
            students = new ObservableCollection<Student>();
            studentDataList.ItemsSource = students; // Binds students as source for ListView
            // Hide all error messages
            reactionNotPosNumberErrorMsg.Visibility = Visibility.Hidden;
            noStudentNameErrorMsg.Visibility = Visibility.Hidden;
            noStudentSelectedErrorMsg.Visibility = Visibility.Hidden;
            noStudentsErrorMsg.Visibility = Visibility.Hidden;
        }

        private void addStudent_Click(object sender, RoutedEventArgs e)
        {
            noStudentSelectedErrorMsg.Visibility = Visibility.Hidden;  // Hides possibly overlapping error
            if (double.TryParse(studentReactionEntry.Text, out double reactionTime))
            { 
                if (reactionTime > 0)
                {
                    reactionNotPosNumberErrorMsg.Visibility = Visibility.Hidden;
                    if (studentNameEntry.Text != "")
                    {
                        Student student = new Student() { Name = studentNameEntry.Text, ReactionTime_ms = reactionTime, Category = "" };
                        students.Add(student);
                        noStudentNameErrorMsg.Visibility = Visibility.Hidden;
                        // Clears user input text boxes
                        studentNameEntry.Text = "";
                        studentReactionEntry.Text = "";
                        studentDataList.ScrollIntoView(student);
                    } else { noStudentNameErrorMsg.Visibility = Visibility.Visible; }
                } else { reactionNotPosNumberErrorMsg.Visibility = Visibility.Visible; }
            } else { reactionNotPosNumberErrorMsg.Visibility = Visibility.Visible; }
        }

        private void studentNameEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Hides placeholder label when user inputs text into name box or shows in opposite case
            if (studentNameEntry.Text != "")
            {
                studentNameEntryPlaceholder.Visibility = Visibility.Hidden;
            } 
            else
            {
                studentNameEntryPlaceholder.Visibility = Visibility.Visible;
            }
        }
        private void studentReactionEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Hides placeholder label when user inputs text into reaction time box or shows in opposite case
            if (studentReactionEntry.Text != "")
            {
                studentReactionEntryPlaceholder.Visibility = Visibility.Hidden;
            }
            else
            {
                studentReactionEntryPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void deleteStudent_Click(object sender, RoutedEventArgs e)
        {
            int selection = studentDataList.SelectedIndex;  // User highlighted item index from list
            if (selection == -1) // If no item selected
            {
                // Hide errors that may be displayed in the same location
                noStudentNameErrorMsg.Visibility = Visibility.Hidden;
                reactionNotPosNumberErrorMsg.Visibility = Visibility.Hidden;

                noStudentSelectedErrorMsg.Visibility = Visibility.Visible;
            }
            else
            {
                noStudentSelectedErrorMsg.Visibility = Visibility.Hidden;
                students.RemoveAt(selection);
            }
        }

        private void calculateCategories_Click(object sender, RoutedEventArgs e)
        {
            if (students.Count <= 0) 
            {
                noStudentsErrorMsg.Visibility = Visibility.Visible; 
            }
            else 
            {
                noStudentNameErrorMsg.Visibility = Visibility.Hidden;
                double mean = calculateArithmeticMean();
                double standardDeviation = calculateStandardDeviation(mean);
                assignRankings(mean, standardDeviation);
            }
        }

        private void assignRankings(double mean, double standardDeviation)
        {
            if (standardDeviation == 0) // Prevent division by 0
            {
                foreach (Student student in students) { student.Category = "Standard"; }
            }
            else
            {
                foreach (Student student in students)
                {
                    double zScore = (student.ReactionTime_ms - mean) / standardDeviation;  // Eq 2. From Analysis
                                                                                           // Inequalities from Objectives
                    if (zScore <= -2) { student.Category = "Outstanding"; }
                    else if (zScore <= -1) { student.Category = "Good"; }
                    else if (zScore < 1) { student.Category = "Standard"; }
                    else if (zScore < 2) { student.Category = "Requires Improvement"; }
                    else { student.Category = "Inadequate"; }
                }
            }
            // In order to update ListView
            studentDataList.ItemsSource = null;
            studentDataList.ItemsSource = students;
        }

        private double calculateArithmeticMean()
        {
            double sum = 0;
            foreach (Student student in students) { sum += student.ReactionTime_ms; }
            return sum / students.Count;
        }

        private double calculateStandardDeviation(double mean)
        {
            // Eq. 1 from analysis
            double sum = 0;
            foreach (Student student in students) { sum += Math.Pow(student.ReactionTime_ms - mean, 2); }
            return Math.Pow(sum / students.Count, 0.5);
        }

        private void exportCSV_Click(object sender, RoutedEventArgs e)
        {
            List<string> lines = ["Name,Reaction Time [ms],Category"]; // Starts with just headers
            // Append each student
            foreach (Student student in students) {
             lines.Add($"{student.Name},{student.ReactionTime_ms},{student.Category}");
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "student-categories";
            saveFileDialog.DefaultExt = ".csv";
            saveFileDialog.Filter = "Comma Separated Values|*.csv";
            if (saveFileDialog.ShowDialog() == true) // If user accepts prompt to save file
            {
                File.WriteAllLines(saveFileDialog.FileName, lines);
            }
        }

        private void resetData_Click(object sender, RoutedEventArgs e)
        {
            students = [];
            // In order to update ListView
            studentDataList.ItemsSource = null;
            studentDataList.ItemsSource = students;
        }
    }
}