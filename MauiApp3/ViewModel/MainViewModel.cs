
using MauiApp3.Commands;
using MauiApp3.Model;
using MauiApp3.Queries;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiApp3.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly GetPlayerByIdQueryHandler _getPlayerByIdQueryHandler;
        private readonly GetPlayersQueryHandler _getPlayersQueryHandler;
        private readonly AddPlayerCommandHandler _addPlayerCommandHandler;
        private readonly DeletePlayerCommandHandler _deletePlayerCommandHandler;
        private readonly EditPlayerCommandHandler _editPlayerCommandHandler;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Player> Players { get; } = new();

        public string PlayerName { get; set; } = "";
        public string PlayerLevel { get; set; } = "";
        public string PlayerId { get; set; } = "";

        public string DeletePlayerId { get; set; } = "";
        public string EditPlayerId { get; set; } = "";
        public string EditPlayerName { get; set; } = "";
        public string EditPlayerLevel { get; set; } = "";

        private string _searchResult = "";
        public string SearchResult
        {
            get => _searchResult;
            set
            {
                _searchResult = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddPlayerCommandButton { get; }
        public ICommand FindPlayerCommandButton { get; }
        public ICommand DeletePlayerCommandButton { get; }
        public ICommand EditPlayerCommandButton { get; }

        public MainViewModel(
            GetPlayerByIdQueryHandler getPlayerByIdQueryHandler,
            GetPlayersQueryHandler getPlayersQueryHandler,
            AddPlayerCommandHandler addPlayerCommandHandler,
            DeletePlayerCommandHandler deletePlayerCommandHandler,
            EditPlayerCommandHandler editPlayerCommandHandler)
        {
            _getPlayerByIdQueryHandler = getPlayerByIdQueryHandler;
            _getPlayersQueryHandler = getPlayersQueryHandler;
            _addPlayerCommandHandler = addPlayerCommandHandler;
            _deletePlayerCommandHandler = deletePlayerCommandHandler;
            _editPlayerCommandHandler = editPlayerCommandHandler;

            AddPlayerCommandButton = new Command(AddPlayer);
            FindPlayerCommandButton = new Command(FindPlayer);
            DeletePlayerCommandButton = new Command(DeletePlayer);
            EditPlayerCommandButton = new Command(EditPlayer);

            LoadPlayers();
        }

        private void AddPlayer()
        {
            if (string.IsNullOrWhiteSpace(PlayerName))
            {
                StatusMessage = "Podaj nazwę gracza.";
                return;
            }

            if (!int.TryParse(PlayerLevel, out int playerLevel) || playerLevel < 0)
            {
                StatusMessage = "Podaj poprawny poziom (0 lub więcej).";
                return;
            }

            _addPlayerCommandHandler.Handle(
                new AddPlayerCommand(PlayerName.Trim(), playerLevel));

            PlayerName = "";
            PlayerLevel = "";
            OnPropertyChanged(nameof(PlayerName));
            OnPropertyChanged(nameof(PlayerLevel));

            LoadPlayers();
            StatusMessage = "Dodano gracza.";
        }

        private void FindPlayer()
        {
            if (!int.TryParse(PlayerId, out int playerId) || playerId <= 0)
            {
                SearchResult = "Niepoprawne ID.";
                return;
            }

            Player? player = _getPlayerByIdQueryHandler.Handle(
                new GetPlayerByIdQuery(playerId));

            SearchResult = player == null
                ? "Nie znaleziono gracza."
                : $"{player.Name} - poziom {player.Level}";
        }

        private void DeletePlayer()
        {
            if (!int.TryParse(DeletePlayerId, out int id) || id <= 0)
            {
                StatusMessage = "Podaj poprawne ID gracza do usunięcia.";
                return;
            }

            bool deleted = _deletePlayerCommandHandler.Handle(
                new MauiApp3.Commands.DeletePlayerCommand(id));

            if (!deleted)
            {
                StatusMessage = "Nie znaleziono gracza o takim ID.";
                return;
            }

            DeletePlayerId = "";
            OnPropertyChanged(nameof(DeletePlayerId));
            LoadPlayers();
            StatusMessage = "Usunięto gracza.";
        }

        private void EditPlayer()
        {
            if (!int.TryParse(EditPlayerId, out int id) || id <= 0)
            {
                StatusMessage = "Podaj poprawne ID gracza do edycji.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditPlayerName))
            {
                StatusMessage = "Podaj nową nazwę gracza.";
                return;
            }

            if (!int.TryParse(EditPlayerLevel, out int level) || level < 0)
            {
                StatusMessage = "Podaj poprawny poziom (0 lub więcej).";
                return;
            }

            bool edited = _editPlayerCommandHandler.Handle(
                new MauiApp3.Commands.EditPlayerCommand(
                    id, EditPlayerName.Trim(), level));

            if (!edited)
            {
                StatusMessage = "Nie znaleziono gracza o takim ID.";
                return;
            }

            EditPlayerId = "";
            EditPlayerName = "";
            EditPlayerLevel = "";

            OnPropertyChanged(nameof(EditPlayerId));
            OnPropertyChanged(nameof(EditPlayerName));
            OnPropertyChanged(nameof(EditPlayerLevel));

            LoadPlayers();
            StatusMessage = "Zaktualizowano gracza.";
        }

        private void LoadPlayers()
        {
            List<Player> players =
                _getPlayersQueryHandler.Handle(new GetPlayersQuery());

            Players.Clear();

            foreach (Player player in players)
            {
                Players.Add(player);
            }
        }

        protected void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
