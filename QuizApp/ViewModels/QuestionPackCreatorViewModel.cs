using QuizApp.Models;

namespace QuizApp.ViewModels;

internal class QuestionPackCreatorViewModel : ViewModelBase
{
    private string _name = "";

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public IReadOnlyList<Difficulty> Difficulties { get; } =
        Enum.GetValues<Difficulty>();

    private Difficulty _selectedDifficulty = Difficulty.Medium;

    public Difficulty SelectedDifficulty
    {
        get => _selectedDifficulty;
        set => SetProperty(ref _selectedDifficulty, value);
    }

    private double _timeLimitSliderValue = 30;

    public double TimeLimitSliderValue
    {
        get => _timeLimitSliderValue;
        set
        {
            if (SetProperty(ref _timeLimitSliderValue, value))
                RaisePropertyChanged(nameof(TimeLimitInSeconds));
        }
    }

    public int TimeLimitInSeconds =>
        (int)Math.Round(TimeLimitSliderValue);

    public QuestionPackCreatorViewModel()
        : this("", Difficulty.Medium, 30)
    {
    }

    public QuestionPackCreatorViewModel(
        string name,
        Difficulty difficulty,
        int timeLimitInSeconds)
    {
        _name = name;
        _selectedDifficulty = difficulty;
        _timeLimitSliderValue = Math.Clamp(timeLimitInSeconds, 5, 180);
    }
}