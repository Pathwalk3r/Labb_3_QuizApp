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
        Interval = TimeSpan.FromSeconds(3)
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

    private List<Question> _roundQuestions = [];

    private int _correctAnswers;
    public int CorrectAnswers
    {
        get => _correctAnswers;
        private set => SetProperty(ref _correctAnswers, value);
    }

    public int TotalQuestions => _roundQuestions.Count;

    public Question? CurrentQuestion =>
        CurrentQuestionIndex >= 0 && CurrentQuestionIndex < _roundQuestions.Count
            ? _roundQuestions[CurrentQuestionIndex]
            : null;

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

        CheckAnswerCommand = new RelayCommand(
            CheckAnswer,
            _ => CurrentQuestion is not null);

        _advanceTimer.Tick += (_, _) =>
        {
            _advanceTimer.Stop();
            AdvanceToNextQuestion();
        };
        
    }

    public void RandomizeAnswerPosition()
    {
        CorrectAnswerIndex = -1;
        IncorrectAnswerIndex = -1;
        AreAnswersEnabled = true;
        CheckAnswerCommand.RaiseCanExecuteChanged();

        var question = CurrentQuestion;
        if (question is null)
        {
            Answers = [];
            return;
        }

        var answers = new List<string> { question.CorrectAnswer };
        answers.AddRange(question.IncorrectAnswer);
        Shuffle(answers);

        if (answers.Count != 4)
        {
            Answers = [];
            return;
        }

        Answers = answers;
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

        if (selectedIndex == CorrectAnswerIndex)
        {
            CorrectAnswers++;
            IncorrectAnswerIndex = -1;
        }
        else
        {
            IncorrectAnswerIndex = selectedIndex;
        }

        AreAnswersEnabled = false;
        CheckAnswerCommand.RaiseCanExecuteChanged();
        StopQuestionTimer();
        _advanceTimer.Start();
    }

    private void AdvanceToNextQuestion()
    {
        _advanceTimer.Stop();

        if (CurrentQuestionIndex + 1 >= _roundQuestions.Count)
        {
            StopQuestionTimer();
            _mainWindowViewModel?.ShowQuizResults(
                CorrectAnswers,
                _roundQuestions.Count);
            return;
        }

        CurrentQuestionIndex++;
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

    public void RaiseActivePackChanged(bool startTimer)
    {
        _advanceTimer.Stop();
        StopQuestionTimer();

        _roundQuestions.Clear();
        CurrentQuestionIndex = 0;
        CorrectAnswers = 0;
        RaisePropertyChanged(nameof(CurrentQuestion));
        RaisePropertyChanged(nameof(TotalQuestions));

        CorrectAnswerIndex = -1;
        IncorrectAnswerIndex = -1;
        Answers = [];
        AreAnswersEnabled = true;

        if (startTimer)
            StartQuiz();
    }

    public void StartQuiz()
    {
        if (ActivePack?.Questions is not { Count: > 0 } questions)
            return;

        _advanceTimer.Stop();
        StopQuestionTimer();

        _roundQuestions = questions.ToList();
        Shuffle(_roundQuestions);

        CurrentQuestionIndex = 0;
        CorrectAnswers = 0;
        RaisePropertyChanged(nameof(CurrentQuestion));
        RaisePropertyChanged(nameof(TotalQuestions));

        RandomizeAnswerPosition();
        StartQuestionTimer();
    }

    private static void Shuffle<T>(IList<T> items)
    {
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }

}