using QuizApp.Command;
using QuizApp.Models;
using System.Diagnostics;
using System.Windows.Threading;

namespace QuizApp.ViewModels;

class PlayerViewModel : ViewModelBase
{
    private readonly MainWindowViewModel? _mainWindowViewModel;

    public QuestionPackViewModel? ActivePack
        => _mainWindowViewModel?.ActivePack;

    private readonly DispatcherTimer _advanceTimer = new()
    {
        Interval = TimeSpan.FromSeconds(5)
    };

    private int _currentQuestionIndex;

    public int CurrentQuestionIndex
    {
        get => _currentQuestionIndex;
        private set
        {
            if (SetProperty(ref _currentQuestionIndex, value))
                RaisePropertyChanged(nameof(CurrentQuestion));
        }
    }

    public Question? CurrentQuestion
    {
        get
        {
            var questions = ActivePack?.Questions;
            if (questions is not null && CurrentQuestionIndex < questions.Count)
            {
                return (Question?)questions[CurrentQuestionIndex];
            }
            else
            {
                return null;
            }
        }
    }

    private string _answer1 = "";
    private string _answer2 = "";
    private string _answer3 = "";
    private string _answer4 = "";


    public string Answer1
    {
        get => _answer1;
        set
        {
            _answer1 = value;
            RaisePropertyChanged(nameof(Answer1));
        }
    }

    public string Answer2
    {
        get => _answer2;
        set
        {
            _answer2 = value;
            RaisePropertyChanged(nameof(Answer2));
        }
    }

    public string Answer3
    {
        get => _answer3;
        set
        {
            _answer3 = value;
            RaisePropertyChanged(nameof(Answer3));
        }
    }

    public string Answer4
    {
        get => _answer4;
        set
        {
            _answer4 = value;
            RaisePropertyChanged(nameof(Answer4));
        }
    }

    private int _correctAnswerIndex = -1;
    public int CorrectAnswerIndex
    {
        get => _correctAnswerIndex;
        set
        {
            _correctAnswerIndex = value;
            RaisePropertyChanged(nameof(CorrectAnswerIndex));
        }
    }

    private int _incorrectAnswerIndex = -1;
    public int IncorrectAnswerIndex
    {
        get => _incorrectAnswerIndex;
        set
        {
            _incorrectAnswerIndex = value;
            RaisePropertyChanged(nameof(IncorrectAnswerIndex));
        }
    }

    private List<string> _answers = new();

    public List<string> Answers
    {
        get => _answers;
        set
        {
            _answers = value;
            RaisePropertyChanged(nameof(Answers));
        }
    }

    private bool _areAnswersEnabled = true;

    public bool AreAnswersEnabled
    {
        get => _areAnswersEnabled;
        set
        {
            _areAnswersEnabled = value;
            RaisePropertyChanged(nameof(AreAnswersEnabled));
        }
    }

    public RelayCommand CheckAnswerCommand {  get; set; }

    public PlayerViewModel(MainWindowViewModel? mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;

        CheckAnswerCommand = new RelayCommand(CheckAnswer);

        _advanceTimer.Tick += (_, _) =>
        {
            _advanceTimer.Stop();
            AdvanceToNextQuestion();
        };
        
        RandomizeAnswerPosition();
    }

    public void RandomizeAnswerPosition()
    {
        if (ActivePack?.Questions == null ||
            ActivePack.Questions.Count == 0)
            return;

        CorrectAnswerIndex = -1;
        IncorrectAnswerIndex = -1;

        AreAnswersEnabled = true;
        CheckAnswerCommand.RaiseCanExecuteChanged();

        var question = CurrentQuestion;
        if (question is null)
            return;

        List<string> answers = new()
        {
            question.CorrectAnswer
        };

        answers.AddRange(question.IncorrectAnswer);

        Answers = answers.OrderBy(x => Random.Shared.Next()).ToList();

        if (answers.Count != 4)
            return;

        Answer1 = answers[0];
        Answer2 = answers[1];
        Answer3 = answers[2];
        Answer4 = answers[3];
    }

    public void CheckAnswer(object? parameter)
    {
        var question = CurrentQuestion;
        if (!AreAnswersEnabled || question is null)
            return;

        if (!int.TryParse(parameter?.ToString(), out int selectedIndex))
            return;

        if (selectedIndex < 0 || selectedIndex >= Answers.Count)
            return;

        CorrectAnswerIndex = Answers.IndexOf(question.CorrectAnswer);

        if (selectedIndex != CorrectAnswerIndex)
        {
            IncorrectAnswerIndex = selectedIndex;
        }
        else
        {
            IncorrectAnswerIndex = -1;
        }

        AreAnswersEnabled = false;
        CheckAnswerCommand.RaiseCanExecuteChanged();
        StopQuestionTimer();
        _advanceTimer.Start();
    }

    private void AdvanceToNextQuestion()
    {
        if (ActivePack?.Questions is not { Count: > 0 } questions)
            return;

        CurrentQuestionIndex = (CurrentQuestionIndex + 1) % questions.Count;
        RandomizeAnswerPosition();
        StartQuestionTimer();
    }

    private CancellationTokenSource? _questionTimerCts;

    private int _timeRemainingSeconds;
    public int TimeRemainingSeconds
    {
        get => _timeRemainingSeconds;
        private set => SetProperty(ref _timeRemainingSeconds, value);
    }

    public void StartQuestionTimer()
    {
        StopQuestionTimer();

        int limitSeconds = Math.Max(1, ActivePack?.TimeLimitInSeconds ?? 1);
        TimeRemainingSeconds = limitSeconds;

        _questionTimerCts = new CancellationTokenSource();
        _ = RunQuestionTimerAsync(limitSeconds, _questionTimerCts.Token);
    }

    private void StopQuestionTimer()
    {
        var timer = _questionTimerCts;
        _questionTimerCts = null;
        timer?.Cancel();
        timer?.Dispose();
    }

    private void HandleTimeExpired()
    {
        if (!AreAnswersEnabled || CurrentQuestion is not { } question)
            return;

        CorrectAnswerIndex = Answers.IndexOf(question.CorrectAnswer);
        IncorrectAnswerIndex = -1;
        AreAnswersEnabled = false;
        CheckAnswerCommand.RaiseCanExecuteChanged();
        _advanceTimer.Start();
    }

    private async Task RunQuestionTimerAsync(
        int limitSeconds,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var limit = TimeSpan.FromSeconds(limitSeconds);

        try
        {
            while (true)
            {
                TimeSpan remaining = limit - stopwatch.Elapsed;

                if (remaining <= TimeSpan.Zero)
                {
                    TimeRemainingSeconds = 0;
                    HandleTimeExpired();
                    return;
                }

                TimeRemainingSeconds = (int)Math.Ceiling(remaining.TotalSeconds);
                await Task.Delay(100, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

}