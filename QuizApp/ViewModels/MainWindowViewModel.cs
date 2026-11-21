using QuizApp.Command;
using QuizApp.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace QuizApp.ViewModels;

internal class MainWindowViewModel : ViewModelBase
{

    private static readonly string QuestionPacksDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Labb_3_QuizApp",
    "QuestionPacks");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public IReadOnlyList<SavedPackFile> GetSavedPackFiles()
    {
        Directory.CreateDirectory(QuestionPacksDirectory);

        return Directory
            .EnumerateFiles(QuestionPacksDirectory, "*.json")
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .Select(path => new SavedPackFile(
                Path.GetFileNameWithoutExtension(path),
                path))
            .ToList();
    }

    private object _currentView;

    public object CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            RaisePropertyChanged();
        }
    }
    public ObservableCollection<QuestionPackViewModel>? Packs { get; }

	private QuestionPackViewModel? _activePack;

    public QuestionPackViewModel? ActivePack
    {
        get => _activePack;
        set
        {
            if (ReferenceEquals(_activePack, value))
                return;

            if (_activePack is not null)
                _activePack.Questions.CollectionChanged -= OnQuestionsChanged;

            if (SetProperty(ref _activePack, value))
            {
                if (_activePack is not null)
                    _activePack.Questions.CollectionChanged += OnQuestionsChanged;

                ConfigurationViewModel?.RaiseActivePackChanged();
                MenuViewModel?.RaiseActivePackChanged();

                PlayerViewModel?.RaiseActivePackChanged(
                    startTimer: ReferenceEquals(_currentView, PlayerViewModel));

                ShowPlayerCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    public PlayerViewModel? PlayerViewModel { get; }

    public ConfigurationViewModel? ConfigurationViewModel { get; }

    public MenuViewModel? MenuViewModel { get; }

    public RelayCommand ShowStartCommand { get; }

    public RelayCommand ShowPlayerCommand { get; }
    public RelayCommand ShowConfigurationCommand { get; }

    public MainWindowViewModel()
    {

        PlayerViewModel = new PlayerViewModel(this);
        ConfigurationViewModel = new ConfigurationViewModel(this);
        MenuViewModel = new MenuViewModel(this);

        ShowStartCommand = new RelayCommand(_ => CurrentView = this);
        ShowPlayerCommand = new RelayCommand(
        _ =>
        {
            if (ActivePack is null || ActivePack.Questions.Count == 0 ||
                PlayerViewModel is null)
                return;

            PlayerViewModel.StartQuiz();
            CurrentView = PlayerViewModel;
        },
        _ => ActivePack is not null && ActivePack.Questions.Count > 0);
        ShowConfigurationCommand = new RelayCommand(_ => CurrentView = ConfigurationViewModel);

        CurrentView = this;
    }

    public async Task<QuestionPack> LoadQuestionPackAsync(string filePath)
    {
        string json = await File.ReadAllTextAsync(filePath);

        var pack = JsonSerializer.Deserialize<QuestionPack>(json, JsonOptions);

        if (pack?.Questions is null)
            throw new InvalidDataException("The file is not a valid question pack.");

        return pack;
    }

    public async Task SaveActivePackAsync()
    {
        if (ActivePack is null)
            throw new InvalidOperationException("There is no active question pack.");

        Directory.CreateDirectory(QuestionPacksDirectory);

        string fileName = string.Concat(
            ActivePack.Name.Select(character =>
                Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = "QuestionPack";

        string filePath = Path.Combine(QuestionPacksDirectory, $"{fileName}.json");
        string json = JsonSerializer.Serialize(ActivePack.ToModel(), JsonOptions);

        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task LoadFirstSavedPackAsync()
    {
        try
        {
            var firstPackFile = GetSavedPackFiles().FirstOrDefault();

            if (firstPackFile is null)
                return;

            var pack = await LoadQuestionPackAsync(firstPackFile.FilePath);
            ActivePack = new QuestionPackViewModel(pack);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not load a saved question pack: {ex.Message}");
        }
    }

    private void OnQuestionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ShowPlayerCommand?.RaiseCanExecuteChanged();
    }

    public void ShowQuizResults(int correctAnswers, int totalQuestions)
    {
        CurrentView = new QuizResultViewModel(
            correctAnswers,
            totalQuestions,
            this);
    }
}
