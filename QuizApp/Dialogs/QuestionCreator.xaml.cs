using QuizApp.Models;
using QuizApp.ViewModels;
using System.Windows;


namespace QuizApp.Dialogs;


public partial class QuestionPackCreatorWindow : Window
{
    internal QuestionPackCreatorViewModel ViewModel { get; }

    public QuestionPackCreatorWindow()
        : this(new QuestionPackCreatorViewModel(), isEditing: false)
    {
    }

    internal QuestionPackCreatorWindow(QuestionPack pack)
        : this(
            new QuestionPackCreatorViewModel(
                pack.Name,
                pack.Difficulty,
                pack.TimeLimitInSeconds),
            isEditing: true)
    {
    }

    private QuestionPackCreatorWindow(
        QuestionPackCreatorViewModel viewModel,
        bool isEditing)
    {
        InitializeComponent();

        ViewModel = viewModel;
        DataContext = ViewModel;

        Title = isEditing ? "Edit Question Pack" : "Create Question Pack";
        ActionButton.Content = isEditing ? "Save" : "Create";
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.Name))
        {
            MessageBox.Show(this, "Enter a name for the pack.");
            return;
        }

        DialogResult = true;
    }
}
