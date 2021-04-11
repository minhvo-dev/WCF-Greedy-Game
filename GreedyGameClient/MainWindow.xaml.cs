/** Author:     Vo, Dinh Tue Minh
 *  Date:       March 25, 2021
 *  Purpose:    Implement GUI for Greedy Game application
 *              Settings are stored in the App.config file
 */

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using System.ServiceModel;
using GreedyGameLibrary;

namespace GreedyGameClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback
    {
        private const int MAXIMUM_NAME_LENGTH = 7;
        private SolidColorBrush LIGHT_PURPLE = new SolidColorBrush(Color.FromRgb(239, 215, 247));
        private SolidColorBrush PURPLE = new SolidColorBrush(Color.FromRgb(153, 0, 204));
        private SolidColorBrush LIGHT_BLUE = new SolidColorBrush(Color.FromRgb(171, 217, 255));
        private SolidColorBrush WHITE = new SolidColorBrush(Color.FromRgb(255, 255, 255));

        private IGreedyGame _greedyGame;
        private Player _player; // object that store this player's information
        private int _minimumRequiredPlayers;
        private int _maximumRequiredPlayers;

        private List<Label> _nameLabels;
        private List<Label> _lastPickLabels;
        private List<Label> _statusLabels;
        private List<Rectangle> _playerRectangles;
        private List<RadioButton> _optionsRadioButtons;

        public MainWindow()
        {
            InitializeComponent();

            // init values
            _greedyGame = null;
            _player = null;
            _maximumRequiredPlayers = 0;
            _minimumRequiredPlayers = 0;

            _nameLabels = new List<Label>();
            _nameLabels.Add(player1Name);
            _nameLabels.Add(player2Name);
            _nameLabels.Add(player3Name);
            _nameLabels.Add(player4Name);
            _nameLabels.Add(player5Name);
            _nameLabels.Add(player6Name);
            _nameLabels.Add(player7Name);

            _lastPickLabels = new List<Label>();
            _lastPickLabels.Add(player1LastPick);
            _lastPickLabels.Add(player2LastPick);
            _lastPickLabels.Add(player3LastPick);
            _lastPickLabels.Add(player4LastPick);
            _lastPickLabels.Add(player5LastPick);
            _lastPickLabels.Add(player6LastPick);
            _lastPickLabels.Add(player7LastPick);

            _statusLabels = new List<Label>();
            _statusLabels.Add(player1Status);
            _statusLabels.Add(player2Status);
            _statusLabels.Add(player3Status);
            _statusLabels.Add(player4Status);
            _statusLabels.Add(player5Status);
            _statusLabels.Add(player6Status);
            _statusLabels.Add(player7Status);

            _playerRectangles = new List<Rectangle>();
            _playerRectangles.Add(player1Tile);
            _playerRectangles.Add(player2Tile);
            _playerRectangles.Add(player3Tile);
            _playerRectangles.Add(player4Tile);
            _playerRectangles.Add(player5Tile);
            _playerRectangles.Add(player6Tile);
            _playerRectangles.Add(player7Tile);

            _optionsRadioButtons = new List<RadioButton>();
            _optionsRadioButtons.Add(option1RadioButton);
            _optionsRadioButtons.Add(option2RadioButton);
            _optionsRadioButtons.Add(option3RadioButton);
            _optionsRadioButtons.Add(option4RadioButton);

            // hide the options panel
            HideOptionsPanel_();
        }

        // ---------------------------------------------------------------------
        // ICALLBACK IMPLEMENTATION
        // ---------------------------------------------------------------------
        #region ICALLBACK_INTERFACE_IMPLEMENTATION

        private delegate void UpdatePlayersDashboardDelegate(Player[] players);
        public void UpdatePlayersDashboard(Player[] players)
        {
            if (Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                // executing on main thread: can modify data
                UpdatePlayersDashboard_(players);
            }
            else
            {
                // not executing on main thread: cannot modify data
                // delegate the execution to the main thread
                Dispatcher.BeginInvoke(new UpdatePlayersDashboardDelegate(UpdatePlayersDashboard), new object[] { players });
            }
        }

        private delegate void UpdateControlPanelsDelegate(bool enableHost, int targetScore);
        public void UpdateControlPanels(bool enableHost, int targetScore)
        {
            if (Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                // executing on main thread: can modify data
                UpdateControlPanels_(enableHost, targetScore);
            }
            else
            {
                // not executing on main thread: cannot modify data
                // delegate the execution to the main thread
                Dispatcher.BeginInvoke(new UpdateControlPanelsDelegate(UpdateControlPanels), new object[] { enableHost, targetScore });
            }
        }

        private delegate void UpdateGameDashboardDelegate(Player[] players, string status);
        public void UpdateGameDashboard(Player[] players, string status)
        {
            if (Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                // executing on main thread: can modify data
                UpdateGameDashboard_(players, status);
            }
            else
            {
                // not executing on main thread: cannot modify data
                // delegate the execution to the main thread
                Dispatcher.BeginInvoke(new UpdateGameDashboardDelegate(UpdateGameDashboard), new object[] { players, status });
            }
        }

        private delegate void UpdatePlaygroundPanelDelegate(int numberOptions);
        public void UpdatePlaygroundPanel(int numberOptions)
        {
            if (Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                // executing on main thread: can modify data
                UpdatePlaygroundPanel_(numberOptions);
            }
            else
            {
                // not executing on main thread: cannot modify data
                // delegate the execution to the main thread
                Dispatcher.BeginInvoke(new UpdatePlaygroundPanelDelegate(UpdatePlaygroundPanel), new object[] { numberOptions });
            }
        }

        #endregion ICALLBACK_INTERFACE_IMPLEMENTATION

        // ---------------------------------------------------------------------
        // EVENT HANDLERS
        // ---------------------------------------------------------------------
        #region EVENT_HANDLERS

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_greedyGame != null && _player != null)
            {
                _greedyGame.Leave();
            }
        }

        private void joinButton_Click(object sender, RoutedEventArgs e)
        {
            string name = nameTextBox.Text;
            if (string.IsNullOrWhiteSpace(name) == false && name.Length <= MAXIMUM_NAME_LENGTH)
            {
                ConnectToService_(name);
            }
            else
            {
                nameTextBox.Text = "";
                MessageBox.Show($"Name cannot be either empty or longer than {MAXIMUM_NAME_LENGTH}");
            }
        }

        private void hostButton_Click(object sender, RoutedEventArgs e)
        {
            if (_greedyGame.BecomeHost())
            {
                // enable host panel
                var minScore = _greedyGame.MinimumTargetScore;
                targetScoreSlider.Value = minScore;
                targetScoreLabel.Content = minScore;
                targetScoreSlider.IsEnabled = true;
                becomeHostButton.IsEnabled = false;
                startButton.IsEnabled = true;
            }
            else
            {
                // this should not be shown
                MessageBox.Show("The game already has a host");
            }
        }

        private void targetScoreSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            targetScoreLabel.Content = targetScoreSlider.Value;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (_greedyGame.Start((int)targetScoreSlider.Value) == false)
            {
                MessageBox.Show($"The game requires at least {_minimumRequiredPlayers} players to start", "Error");
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedOption = 0;
            for (int i = 0, n = _optionsRadioButtons.Count; i < n; ++i)
            {
                if (_optionsRadioButtons[i].IsChecked.HasValue && _optionsRadioButtons[i].IsChecked.Value)
                {
                    selectedOption = i + 1;
                    break;
                }
            }
            if (selectedOption != 0)
            {
                // only show selected option, hide the others
                // and inform the user
                HideOptionsPanel_();
                _optionsRadioButtons[selectedOption - 1].Visibility = Visibility.Visible;
                _optionsRadioButtons[selectedOption - 1].IsChecked = true;
                instructionLabel.Content = $"You played Card #{selectedOption}";

                _greedyGame.Play(selectedOption);
            }
            else
            {
                MessageBox.Show("You must select a Card to play", "Error");
            }
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"To connect to a game, select your unique name in the 'Name' textbox then click 'Join' button. " +
                $"If you would like to be the host of the game, click 'Become Host' button. " +
                $"Wait for other players to connect, then you can enjoy the game together." +
                $"\n\nFor more information, please visit https://github.com/minhvo-dev/WCF-Greedy-Game" +
                $"\nGood luck, have fun 😀",
                "Information");
        }

        #endregion EVENT_HANDLERS

        // ---------------------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------------------
        #region HELPERS

        private void ConnectToService_(string clientName)
        {
            try
            {
                // Configure the ABCs
                DuplexChannelFactory<IGreedyGame> channel = new DuplexChannelFactory<IGreedyGame>(this, "GreedyGameService");
                // Connect to the endpoint
                _greedyGame = channel.CreateChannel();
                var response = _greedyGame.Join(clientName);
                switch (response.Item1)
                {
                    case JoinResponseCode.GameIsFull:
                        MessageBox.Show("Game is full", "Error");
                        _greedyGame = null;
                        nameTextBox.Text = "";
                        break;
                    case JoinResponseCode.GameIsStarted:
                        MessageBox.Show("Game is already started", "Error");
                        _greedyGame = null;
                        nameTextBox.Text = "";
                        break;
                    case JoinResponseCode.NameIsInvalid:
                        MessageBox.Show("Name is invalid", "Error");
                        _greedyGame = null;
                        nameTextBox.Text = "";
                        break;
                    case JoinResponseCode.NameIsUsed:
                        MessageBox.Show("Name is already used", "Error");
                        _greedyGame = null;
                        nameTextBox.Text = "";
                        break;
                    case JoinResponseCode.Accept:
                        // set player
                        _player = response.Item2;

                        // disable join function
                        nameTextBox.Text = _player.Name;
                        nameTextBox.IsEnabled = joinButton.IsEnabled = false;

                        // setup the slider
                        targetScoreSlider.Minimum = _greedyGame.MinimumTargetScore;
                        targetScoreSlider.Maximum = _greedyGame.MaximumTargetScore;

                        // retrieve the game state
                        // update the UI:
                        //  - control panels
                        //  - live update board

                        _maximumRequiredPlayers = _greedyGame.MaximumRequiredPlayers;
                        _minimumRequiredPlayers = _greedyGame.MinimumRequiredPlayers;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void UpdateControlPanels_(bool enableHost, int targetScore)
        {
            if (targetScore == 0)
            {
                targetScoreLabel.Content = "";
                targetScoreSlider.Value = targetScoreSlider.Minimum;
            }
            else
            {
                targetScoreLabel.Content = targetScore;
                targetScoreSlider.Value = targetScore;
            }
            targetScoreSlider.IsEnabled = startButton.IsEnabled = false;
            becomeHostButton.IsEnabled = enableHost;
        }

        private void SetDefaultRectangle_(Rectangle rectangle)
        {
            rectangle.StrokeThickness = 1;
            rectangle.Stroke = LIGHT_BLUE;
            rectangle.Fill = WHITE;
        }

        private void SetWinnerRectangle_(Rectangle rectangle)
        {
            rectangle.StrokeThickness = 3;
            rectangle.Stroke = PURPLE;
            rectangle.Fill = LIGHT_PURPLE;
        }

        private void UpdatePlayersDashboard_(Player[] players)
        {
            for (int i = 0; i < _maximumRequiredPlayers; ++i)
            {
                if (players[i] == null)
                {
                    _nameLabels[i].Content = _lastPickLabels[i].Content = _statusLabels[i].Content = "";
                    SetDefaultRectangle_(_playerRectangles[i]);
                }
                else
                {
                    _nameLabels[i].Content = players[i].Name;
                    _lastPickLabels[i].Content = players[i].LastPick == 0 ? "" : $"Card #{players[i].LastPick}";
                    _statusLabels[i].Content = players[i].Status;
                    if (players[i].IsLastRoundWinner)
                    {
                        SetWinnerRectangle_(_playerRectangles[i]);
                    }
                    else
                    {
                        SetDefaultRectangle_(_playerRectangles[i]);
                    }
                }

            }
        }

        private void UpdateGameDashboard_(Player[] players, string status)
        {
            if (status != null)
            {
                gameStatusLabel.Content = status;
            }

            // clear the last dashboard content
            dashboardListBox.Items.Clear();

            // dashboard items must be in descending order
            List<Tuple<string, int>> pairs = new List<Tuple<string, int>>();
            foreach (var player in players)
            {
                if (player != null)
                {
                    pairs.Add(new Tuple<string, int>(player.Name, player.Score));
                }
            }
            pairs.Sort((t1, t2) =>
            {
                if (t1.Item2 == t2.Item2) return t1.Item1.CompareTo(t2.Item1);
                return t2.Item2.CompareTo(t1.Item2);
            });

            // prepare the listbox item
            List<ListBoxItem> listBoxItems = new List<ListBoxItem>();
            foreach (var pair in pairs)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = $"{pair.Item1,-20}{pair.Item2,3}";
                // highlight this player in the list
                if (pair.Item1 == _player.Name)
                {
                    item.Background = new SolidColorBrush(Color.FromRgb(171, 217, 255));
                }
                listBoxItems.Add(item);
            }

            // add new items into it
            foreach (var item in listBoxItems)
            {
                dashboardListBox.Items.Add(item);
            }

        }

        private void UpdatePlaygroundPanel_(int numberOptions)
        {
            HideOptionsPanel_();
            if (numberOptions > 0)
            {
                ShowOptionsPanel_(numberOptions);
            }
        }

        private void HideOptionsPanel_()
        {
            playButton.Visibility = Visibility.Hidden;
            foreach (var btn in _optionsRadioButtons)
            {
                btn.IsChecked = false;
                btn.Visibility = Visibility.Hidden;
            }
            instructionLabel.Content = "";
        }

        private void ShowOptionsPanel_(int numberOptions)
        {
            if (numberOptions > 4 || numberOptions < 1)
            {
                MessageBox.Show($"Number of options ({numberOptions}) is invalid", "Error");
            }
            else
            {
                playButton.Visibility = Visibility.Visible;
                for (int i = 0; i < numberOptions; ++i)
                {
                    _optionsRadioButtons[i].Visibility = Visibility.Visible;
                    _optionsRadioButtons[i].IsChecked = false;
                }
            }
            instructionLabel.Content = "Pick your Card and Play!";
        }

        #endregion HELPERS

    }
}
