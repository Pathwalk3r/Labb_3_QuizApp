using QuizApp.Models;
using System.Collections.ObjectModel;

namespace QuizApp.ViewModels;

class QuestionPackViewModel : ViewModelBase
{
    private readonly QuestionPack _model;

    public QuestionPackViewModel(QuestionPack model)
    {
        _model = model;
        Questions = new ObservableCollection<Question>(_model.Questions);
    }

    public string Name
    {
        get => _model.Name; 
        set 
        {
            if (_model.Name == value)
                return;

            _model.Name = value;
            RaisePropertyChanged();
        }
    }

    public Difficulty Difficulty
    {
        get => _model.Difficulty; 
        set
        {
            if (Difficulty == value)
                return;

            _model.Difficulty = value;
            RaisePropertyChanged();
        }
    } 
    
    public int TimeLimitInSeconds
    {
        get => _model.TimeLimitInSeconds;
        set
        {
            if (_model.TimeLimitInSeconds == value)
                return;

            _model.TimeLimitInSeconds = value;
            RaisePropertyChanged();
        }
    }

    public ObservableCollection<Question> Questions { get; }

    public QuestionPack ToModel() =>
    new(Name, Difficulty, TimeLimitInSeconds)
    {
        Questions = Questions.ToList()
    };
}
