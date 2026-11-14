using QuizApp.Command;
using QuizApp.Models;
using System.Collections.ObjectModel;
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
            if (SetProperty(ref _activePack, value))
            {
                ConfigurationViewModel?.RaiseActivePackChanged();
                MenuViewModel?.RaiseActivePackChanged();
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

        var questionPack = new QuestionPack("Mina frågor!");
        questionPack.Questions.Add(new Question("Vad heter Sveriges huvudstad?", "Stockholm", "Oslo", "Göteborg", "London"));
        questionPack.Questions.Add(new Question("Vad är 2 + 3?", "5", "10", "8", "1"));

        ActivePack = new QuestionPackViewModel(questionPack);

        PlayerViewModel = new PlayerViewModel(this);
        ConfigurationViewModel = new ConfigurationViewModel(this);
        MenuViewModel = new MenuViewModel(this);

        ShowStartCommand = new RelayCommand(_ => CurrentView = this);
        ShowPlayerCommand = new RelayCommand(_ =>
        {
            if (PlayerViewModel is null)
                return;

            PlayerViewModel.StartQuestionTimer();
            CurrentView = PlayerViewModel;
        });
        ShowConfigurationCommand = new RelayCommand(_ => CurrentView = ConfigurationViewModel);

        CurrentView = this;
 
    }

    public QuestionPack LoadQuestionPack(string filePath)
    {
        var pack = JsonSerializer.Deserialize<QuestionPack>(
            File.ReadAllText(filePath),
            JsonOptions);

        if (pack?.Questions is null)
            throw new InvalidDataException("The file is not a valid question pack.");

        return pack;
    }

    public void SaveActivePack()
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

        File.WriteAllText(filePath, json);
    }

}
