namespace SD_Sound_Card_Programmer.ViewModels;

public class MainViewModel : BaseViewModel
{
    #region Public Properties
    public ObservableCollection<Sound> AvailableSounds { get; set; } = new();
    public ObservableCollection<Sound> SelectedSounds { get; set; } = new();
    public ObservableCollection<string> DriveList { get; set; } = new();
    public int DriveSelection { get; set; } = 0;
    public int SeriesSelection { get; set; } = 0;
    public long SelectionSize { get; set; } = 0;
    public long AvailableSize { get; set; } = 0;
    public bool IsBottomInfoVisible { get; set; } = false;
    public bool IsSoundListEntryPopupOpen { get; set; } = false;
    public string NewSoundListName { get; set; } = string.Empty;
    public SolidColorBrush TextBoxBorderColor { get; set; } = new(Color.FromRgb(171, 173, 179));
    public string BottomInfoText { get; set; } = string.Empty;
    public int ProgressBarValue { get; set; }
    public int ProgressBarMaximumValue { get; set; }
    #endregion

    #region Command Declarations
    public Command SelectFileCommand { get; }
    public Command ResetSelectionsCommand { get; }
    public Command FindSDCardsCommand { get; }
    public Command ProgramSDCardCommand { get; }
    public Command SaveSoundListCommand { get; }
    public Command SoundListPopupOKCommand { get; }
    public Command SoundListPopupCancelCommand { get; }
    public Command SelectionChangedCommand { get; }
    public Command DriveChangedCommand { get; }
    public Command SeriesSelectionChangedCommand { get; }
    #endregion

    #region Constructor
    public MainViewModel()
    {
        SelectFileCommand = new(SelectFile);
        ResetSelectionsCommand = new(ResetSelections);
        FindSDCardsCommand = new(FindSDCards);
        ProgramSDCardCommand = new(ProgramSDCard);
        SaveSoundListCommand = new(SaveSoundList);

        SoundListPopupOKCommand = new(SoundListOK);
        SoundListPopupCancelCommand = new(ResetPopup);

        SelectionChangedCommand = new(OnSelectionChanged);
        DriveChangedCommand = new(OnDriveChanged);
        SeriesSelectionChangedCommand = new(LoadSound);

        LoadSound(null);
        FindSDCards(null);
    }
    #endregion

    #region Command Implementations
    private void SelectFile(object sender)
    {
        OpenFileDialog openFileDialog = new();
        if (openFileDialog.ShowDialog() == true)
        {
            string fileText = File.ReadAllText(openFileDialog.FileName);

            SeriesSelection = fileText[fileText.IndexOf("Product:") + 9] == 'B' ? 0 : 1;

            List<string>? soundList = fileText[(fileText.IndexOf("Sound name list:;") + 19)..].Replace(";" + Environment.NewLine + "End of file: \"EOF\";", "").Replace(Environment.NewLine, "").Split(';').ToList();
            foreach (string sound in soundList)
            {
                Sound? availableSound = AvailableSounds.Where(s => s.Name == NameConverter.ToFormalName(sound)).First();
                if (availableSound is not null)
                {
                    AvailableSounds.Where(s => s.Name == availableSound.Name).First().IsSelected = true;

                    RefreshList();
                }
                else
                {
                    MessageBox.Show($"The file \"{sound}\" could not be found!", "File missing", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            OnSelectionChanged(null);
        }
    }

    [SuppressPropertyChangedWarnings]
    private void OnSelectionChanged(object? sender)
    {
        SelectedSounds = new(AvailableSounds.Where(s => s.IsSelected).OrderBy(s => s.Name));
        //IEnumerable<AvailableSound>? collection = ((IList)sender).Cast<AvailableSound>();
        //SelectedSounds = new(collection.OrderBy(s => s.Name));
        SelectionSize = SelectedSounds.Sum(s => s.Size);
    }

    private void LoadSound(object? sender)
    {
        AvailableSounds.Clear();
        SelectedSounds.Clear();
        string fileType = SeriesSelection == 0 ? "mp3" : "ogg";
        string[]? files = Directory.GetFiles(Path.Combine(Assembly.GetExecutingAssembly().Location[..Assembly.GetExecutingAssembly().Location.IndexOf("bin")], "Sounds", fileType));
        foreach (string file in files)
        {
            AvailableSounds.Add(new()
            {
                Name = NameConverter.ToFormalName(new FileInfo(file).Name),
                Path = new FileInfo(file).FullName,
                ThumbnailSource = new FileInfo(file).FullName.Replace("Sounds", "Images")[0..^3] + "png",
                Size = new FileInfo(file).Length
            });
        }
    }

    private void ResetSelections(object sender)
    {
        LoadSound(null);
        SeriesSelection = 0;
    }

    private void FindSDCards(object? sender)
    {
        DriveList.Clear();
        IEnumerable<DriveInfo>? drives = DriveInfo.GetDrives().Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable);

        if (!drives.Any())
        {
            DriveList.Add("No SD Cards Available");
            DriveSelection = 0;
            return;
        }

        foreach (DriveInfo drive in drives)
        {
            DriveList.Add(drive.Name + " " + drive.VolumeLabel);
        }
        DriveSelection = 0;
        AvailableSize = new DriveInfo(DriveList[DriveSelection][0..2]).AvailableFreeSpace;
    }

    [SuppressPropertyChangedWarnings]
    private void OnDriveChanged(object sender)
    {
        if (DriveSelection < 0 || DriveList[0] == "No SD Cards Available")
        {
            AvailableSize = 0;
            return;
        }

        AvailableSize = new DriveInfo(DriveList[DriveSelection][0..2]).AvailableFreeSpace;
    }

    private async void ProgramSDCard(object sender)
    {
        if (SelectedSounds.Count == 0)
        {
            MessageBox.Show($"Select sounds before proceeding!", "No sounds selected", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (DriveList[DriveSelection] == "No SD Cards Available")
        {
            MessageBox.Show($"No SD cards available!  Click on \"Find SD Cards\" to find an SD card.", "SD card not inserted.", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!Directory.Exists(DriveList[DriveSelection][0..2]))
        {
            MessageBox.Show($"The drive \"{DriveList[DriveSelection][0..2]}\" could not be found!", "Drive not found", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (Directory.GetFiles(DriveList[DriveSelection][0..2]).Any())
        {
            if (MessageBox.Show($"The drive \"{DriveList[DriveSelection][0..2]}\" is not empty. Do you wish to continue?", "Drive not empty", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
            {
                return;
            }
        }

        if (new DriveInfo(DriveList[DriveSelection][0..2]).AvailableFreeSpace < SelectionSize)
        {
            MessageBox.Show($"Please delete some files from the SD card first.", "Not enough space", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        BottomInfoText = "Preparing...";
        ProgressBarMaximumValue = SelectedSounds.Count;
        ProgressBarValue = 0;
        IsBottomInfoVisible = true;
        foreach (Sound sound in SelectedSounds)
        {
            while (File.Exists(Path.Combine(DriveList[DriveSelection][0..2], new DirectoryInfo(sound.Path).Name)))
            {
                if (MessageBox.Show($"A file with the name {Path.Combine(DriveList[DriveSelection][0..2], new DirectoryInfo(sound.Path).Name)} already exists on the SD card. Please delete the file first.\nPress OK to continue.", "File exists", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            try
            {
                BottomInfoText = $"Writing {new DirectoryInfo(sound.Path).Name} to SD card...";
                await Task.Run(() => File.Copy(sound.Path, Path.Combine(DriveList[DriveSelection][0..2], new DirectoryInfo(sound.Path).Name)));
                ProgressBarValue++;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured while copying files to SD card.\nError message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IsBottomInfoVisible = false;
                ProgressBarValue = 0;
                return;
            }
        }

        BottomInfoText = "Completed...";
        MessageBox.Show($"SD card programmed!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        IsBottomInfoVisible = false;
        ProgressBarValue = 0;
    }

    private void SaveSoundList(object sender)
    {
        if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Saved Sound Cards")))
        {
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Saved Sound Cards"));
        }

        IsSoundListEntryPopupOpen = true;
    }

    private void SoundListOK(object sender)
    {
        if (string.IsNullOrWhiteSpace(NewSoundListName))
        {
            TextBoxBorderColor = new(Color.FromRgb(229, 86, 98));
            return;
        }

        TextBoxBorderColor = new(Color.FromRgb(171, 173, 179));

        if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Saved Sound Cards", NewSoundListName + ".txt")))
        {
            MessageBox.Show($"A file with that name already exists! Please choose a different name or delete the old file first!", "File exists", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        using StreamWriter sw = File.CreateText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Saved Sound Cards", NewSoundListName + ".txt"));
        sw.WriteLine("Sound card type: {0};", SeriesSelection == 0 ? "BST-100 / BST-400" : "S-680 / S-002");
        sw.WriteLine("Sound name list:;");
        foreach (Sound sound in SelectedSounds)
        {
            sw.WriteLine(new DirectoryInfo(sound.Path).Name + ";");
        }
        sw.WriteLine("End of file: \"EOF\";");

        MessageBox.Show($"Sound list saved to Desktop > Saved Sound Cards.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

        ResetPopup(null);
    }

    private void ResetPopup(object? sender)
    {
        IsSoundListEntryPopupOpen = false;
        NewSoundListName = string.Empty;
        TextBoxBorderColor = new(Color.FromRgb(171, 173, 179));
    }
    #endregion

    #region Helper Methods
    private void RefreshList()
    {
        AvailableSounds = new(AvailableSounds);
    }
    #endregion
}
