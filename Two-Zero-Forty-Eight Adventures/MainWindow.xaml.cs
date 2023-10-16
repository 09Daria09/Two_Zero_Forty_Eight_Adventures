using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using static Two_Zero_Forty_Eight_Adventures.Registration;

namespace Two_Zero_Forty_Eight_Adventures
{
    public partial class MainWindow : Window
    {
        private int _animationsInProgress = 0;
        private int _score = 0;
        private int _highscore = 0;
        private string NameUsers = null;
        private List<Tile> previousState = new List<Tile>();

        public MainWindow(User name)
        {
            NameUsers = name.Username;
            _highscore = name.Highscore;
            InitializeComponent();
            LoadAndDisplayHighscore(NameUsers);
            Tile newTile = CreateNewTile();

            if (newTile != null)
            {
                AddTileToGrid(newTile);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.Key)
            {
                case Key.Up:
                    MoveTilesUp();
                    SaveCurrentState();
                    break;
                case Key.Down:
                    MoveTilesDown();
                    SaveCurrentState();
                    break;
                case Key.Left:
                    MoveTilesLeft();
                    SaveCurrentState();
                    break;
                case Key.Right:
                    MoveTilesRight();
                    SaveCurrentState();
                    break;
            }

        }

        public class Tile
        {
            public int Value { get; set; }
            public int Row { get; set; }
            public int Column { get; set; }
        }
        private void SaveCurrentState()
        {
            previousState.Clear();

            foreach (var grid in GameGrid.Children.OfType<Grid>())
            {
                var tile = new Tile
                {
                    Value = int.Parse((grid.Children[1] as TextBlock).Text),
                    Row = Grid.GetRow(grid),
                    Column = Grid.GetColumn(grid)
                };
                previousState.Add(tile);
            }
        }
        private bool HasFieldChanged()
        {
            var currentState = GameGrid.Children.OfType<Grid>()
                .Select(g => new Tile
                {
                    Value = int.Parse((g.Children[1] as TextBlock).Text),
                    Row = Grid.GetRow(g),
                    Column = Grid.GetColumn(g)
                })
                .ToList();

            if (currentState.Count != previousState.Count)
            {
                return true;
            }

            for (int i = 0; i < currentState.Count; i++)
            {
                if (currentState[i].Row != previousState[i].Row ||
                    currentState[i].Column != previousState[i].Column ||
                    currentState[i].Value != previousState[i].Value)
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureTileIsOnTop(Grid gridTile)
        {
            GameGrid.Children.Remove(gridTile);
            GameGrid.Children.Add(gridTile);
        }

        private int GetRandomTileValue()
        {
            Random rand = new Random();
            return rand.Next(1, 101) <= 90 ? 2 : 4;
        }

        private List<(int, int)> GetEmptyCells()
        {
            var emptyCells = new List<(int, int)>();
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    if (IsCellEmpty(row, col))
                    {
                        emptyCells.Add((row, col));
                    }
                }
            }
            return emptyCells;
        }

        private Tile CreateNewTile()
        {
            var emptyCells = GetEmptyCells();
            if (emptyCells.Count == 0) return null;

            Random rand = new Random();
            var randomCell = emptyCells[rand.Next(emptyCells.Count)];

            return new Tile
            {
                Row = randomCell.Item1,
                Column = randomCell.Item2,
                Value = GetRandomTileValue()
            };
        }

        public void AddNewTile()
        {
            var emptyCells = GetEmptyCells();

            if (!CanMakeMove() && emptyCells.Count == 0)
            {
                ShowGameOverMessage();
                RestartGame();
            }
            if (HasFieldChanged())
            {
                Tile newTile = CreateNewTile();

                if (newTile != null)
                {
                    AddTileToGrid(newTile);
                }
            }

        }
        public void RestartGame()
        {
            GameGrid.Children.Clear();  

            _score = 0;
            _highscore = _score > _highscore ? _score : _highscore; 
            UpdateScore();
            previousState.Clear();
            Tile newTile = CreateNewTile();

            if (newTile != null)
            {
                AddTileToGrid(newTile);
            }
        }
        private bool CanMakeMove()
        {
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    var currentTile = GetTileAt(row, col);

                    if (currentTile == null)
                        continue;

                    var neighbors = new List<Tile>
            {
                GetTileAt(row - 1, col), 
                GetTileAt(row + 1, col), 
                GetTileAt(row, col - 1), 
                GetTileAt(row, col + 1)  
            };

                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor != null && neighbor.Value == currentTile.Value)
                        {
                            return true; 
                        }
                    }
                }
            }
            return false; 
        }

        private Tile GetTileAt(int row, int col)
        {
            if (row < 0 || row >= 4 || col < 0 || col >= 4)
                return null;

            return GameGrid.Children.OfType<Grid>()
                .Where(g => Grid.GetRow(g) == row && Grid.GetColumn(g) == col)
                .Select(g => new Tile
                {
                    Value = int.Parse((g.Children[1] as TextBlock).Text),
                    Row = row,
                    Column = col
                })
                .FirstOrDefault();
        }

        private void ShowGameOverMessage()
        {
            string message = $"{NameUsers}, вы проиграли! Ваш счет: {_score}. Ваш лучший счет: {_highscore}.";
            MessageBox.Show(message, "Игра окончена", MessageBoxButton.OK);
        }

        private void AddTileToGrid(Tile tile)
        {
            var rect = new Rectangle
            {
                Width = double.NaN,
                Height = double.NaN,
                Fill = new SolidColorBrush(GetColorForTileValue(tile.Value)),
                Stroke = new SolidColorBrush(Colors.DarkGray),
                StrokeThickness = 3
            };

            var text = new TextBlock
            {
                Text = tile.Value.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 36,
                Foreground = new SolidColorBrush(Colors.White)
            };

            var grid = new Grid();
            grid.Children.Add(rect);
            grid.Children.Add(text);

            grid.RenderTransform = new TranslateTransform();

            Grid.SetRow(grid, tile.Row);
            Grid.SetColumn(grid, tile.Column);

            GameGrid.Children.Add(grid);
        }
        private bool IsCellEmpty(int row, int col)
        {
            foreach (UIElement child in GameGrid.Children)
            {
                if (Grid.GetRow(child) == row && Grid.GetColumn(child) == col)
                    return false;
            }
            return true;
        }
        private void RemoveTileFromGrid(Tile tile)
        {
            Grid gridTile = GameGrid.Children.OfType<Grid>()
                .First(g => Grid.GetRow(g) == tile.Row && Grid.GetColumn(g) == tile.Column);
            GameGrid.Children.Remove(gridTile);
        }
        private void UpdateTileValueAndStyle(Tile tile, int newValue)
        {
            Grid gridTile = GameGrid.Children.OfType<Grid>()
                .First(g => Grid.GetRow(g) == tile.Row && Grid.GetColumn(g) == tile.Column);

            (gridTile.Children[1] as TextBlock).Text = newValue.ToString();

            Color newColor = GetColorForTileValue(newValue);

            (gridTile.Children[0] as Rectangle).Fill = new SolidColorBrush(newColor);
        }

        public void MoveTilesLeft()
        {
            for (int row = 0; row < 4; row++)
            {
                List<Tile> tilesInRow = GameGrid.Children.OfType<Grid>()
                    .Where(g => Grid.GetRow(g) == row)
                    .Select(g => new Tile
                    {
                        Value = int.Parse((g.Children[1] as TextBlock).Text),
                        Row = row,
                        Column = Grid.GetColumn(g)
                    })
                    .OrderBy(t => t.Column)
                    .ToList();

                int targetCol = 0;
                while (tilesInRow.Count > 0)
                {
                    Tile currentTile = tilesInRow[0];
                    tilesInRow.RemoveAt(0);

                    Tile nextTile = tilesInRow.FirstOrDefault(t => t.Value == currentTile.Value && t.Column > currentTile.Column);

                    if (nextTile != null && IsPathClear(currentTile, nextTile))
                    {
                        tilesInRow.Remove(nextTile);
                        int oldRow = currentTile.Row;
                        int oldColumn = currentTile.Column;

                        AnimateTileMerge(nextTile, currentTile, targetCol);
                        //
                        AnimateTileMovement(currentTile, targetCol);
                        //
                        _score += nextTile.Value * 2;
                        UpdateScore();
                    }
                    else
                    {
                        AnimateTileMovement(currentTile, targetCol);
                    }

                    targetCol++;
                }
            }
        }
        public void MoveTilesRight()
        {
            for (int row = 0; row < 4; row++)
            {
                List<Tile> tilesInRow = GameGrid.Children.OfType<Grid>()
                    .Where(g => Grid.GetRow(g) == row)
                    .Select(g => new Tile
                    {
                        Value = int.Parse((g.Children[1] as TextBlock).Text),
                        Row = row,
                        Column = Grid.GetColumn(g)
                    })
                    .OrderByDescending(t => t.Column)
                    .ToList();

                int targetCol = 3;
                while (tilesInRow.Count > 0)
                {
                    Tile currentTile = tilesInRow[0];
                    tilesInRow.RemoveAt(0);

                    Tile nextTile = tilesInRow.FirstOrDefault(t => t.Value == currentTile.Value && t.Column < currentTile.Column);

                    if (nextTile != null && IsPathClear(currentTile, nextTile))
                    {
                        tilesInRow.Remove(nextTile);
                        int oldRow = currentTile.Row;
                        int oldColumn = currentTile.Column;

                        AnimateTileMerge(nextTile, currentTile, targetCol);
                        //
                        AnimateTileMovement(currentTile, targetCol);
                        //
                        _score += nextTile.Value * 2;
                        UpdateScore();
                    }
                    else
                    {
                        AnimateTileMovement(currentTile, targetCol);
                    }

                    targetCol--;
                }
            }

        }

        private void AnimateTileMerge(Tile movingTile, Tile targetTile, int targetColumn)
        {
            Grid gridMovingTile = GameGrid.Children.OfType<Grid>()
                .First(g => Grid.GetRow(g) == movingTile.Row && Grid.GetColumn(g) == movingTile.Column);

            var currentTranslateTransform = gridMovingTile.RenderTransform as TranslateTransform;
            if (currentTranslateTransform == null)
            {
                currentTranslateTransform = new TranslateTransform();
                gridMovingTile.RenderTransform = currentTranslateTransform;
            }

            double currentX = currentTranslateTransform.X;
            double targetX = (targetColumn - movingTile.Column) * gridMovingTile.ActualWidth;

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                From = currentX,
                To = targetX,
                Duration = TimeSpan.FromMilliseconds(400)
            };

            moveAnimation.Completed += (sender, e) =>
            {
                RemoveTileFromGrid(movingTile);
                UpdateTileValueAndStyle(targetTile, targetTile.Value * 2);
                Grid.SetColumn(gridMovingTile, targetColumn);
                gridMovingTile.RenderTransform = new TranslateTransform();

                _animationsInProgress--;
                if (_animationsInProgress == 0)
                {
                    AddNewTile();
                }
            };

            _animationsInProgress++;
            EnsureTileIsOnTop(gridMovingTile);

            currentTranslateTransform.BeginAnimation(TranslateTransform.XProperty, moveAnimation);
        }
        private void AnimateTileMovement(Tile tile, int targetColumn)
        {
            Grid gridTile = GameGrid.Children.OfType<Grid>()
                .First(g => Grid.GetRow(g) == tile.Row && Grid.GetColumn(g) == tile.Column);

            var currentTranslateTransform = gridTile.RenderTransform as TranslateTransform;
            if (currentTranslateTransform == null)
            {
                currentTranslateTransform = new TranslateTransform();
                gridTile.RenderTransform = currentTranslateTransform;
            }

            double currentX = currentTranslateTransform.X;
            double targetX = (targetColumn - tile.Column) * gridTile.ActualWidth;
            _animationsInProgress++;

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                From = currentX,
                To = targetX,
                Duration = TimeSpan.FromMilliseconds(400)
            };

            moveAnimation.Completed += (sender, e) =>
            {
                Grid.SetColumn(gridTile, targetColumn);
                gridTile.RenderTransform = new TranslateTransform();

                _animationsInProgress--;
                if (_animationsInProgress == 0)
                {
                    AddNewTile();
                }
            };
            EnsureTileIsOnTop(gridTile);

            currentTranslateTransform.BeginAnimation(TranslateTransform.XProperty, moveAnimation);
        }
        private bool IsPathClear(Tile startTile, Tile endTile)
        {
            if (startTile.Value != endTile.Value)
            {
                return false;
            }

            int startCol = Math.Min(startTile.Column, endTile.Column);
            int endCol = Math.Max(startTile.Column, endTile.Column);

            for (int col = startCol + 1; col < endCol; col++)
            {
                if (!IsCellEmpty(startTile.Row, col))
                {
                    return false;
                }
            }
            return true;
        }

        public void MoveTilesUp()
        {
            for (int col = 0; col < 4; col++)
            {
                List<Tile> tilesInColumn = GameGrid.Children.OfType<Grid>()
                    .Where(g => Grid.GetColumn(g) == col)
                    .Select(g => new Tile
                    {
                        Value = int.Parse((g.Children[1] as TextBlock).Text),
                        Row = Grid.GetRow(g),
                        Column = col
                    })
                    .OrderBy(t => t.Row)
                    .ToList();

                int targetRow = 0;
                while (tilesInColumn.Count > 0)
                {
                    Tile currentTile = tilesInColumn[0];
                    tilesInColumn.RemoveAt(0);

                    Tile nextTile = tilesInColumn.FirstOrDefault(t => t.Value == currentTile.Value && t.Row > currentTile.Row);

                    if (nextTile != null && IsVerticalPathClear(currentTile, nextTile))
                    {
                        tilesInColumn.Remove(nextTile);
                        AnimateTileMergeVertical(nextTile, currentTile, targetRow);
                        //
                        AnimateTileMovementVertical(currentTile, targetRow);
                        //
                        _score += nextTile.Value * 2;
                        UpdateScore();
                    }
                    else
                    {
                        AnimateTileMovementVertical(currentTile, targetRow);
                    }

                    targetRow++;
                }
            }
        }
        public void MoveTilesDown()
        {
            for (int col = 0; col < 4; col++)
            {
                List<Tile> tilesInColumn = GameGrid.Children.OfType<Grid>()
                    .Where(g => Grid.GetColumn(g) == col)
                    .Select(g => new Tile
                    {
                        Value = int.Parse((g.Children[1] as TextBlock).Text),
                        Row = Grid.GetRow(g),
                        Column = col
                    })
                    .OrderByDescending(t => t.Row)
                    .ToList();

                int targetRow = 3;
                while (tilesInColumn.Count > 0)
                {
                    Tile currentTile = tilesInColumn[0];
                    tilesInColumn.RemoveAt(0);

                    Tile nextTile = tilesInColumn.FirstOrDefault(t => t.Value == currentTile.Value && t.Row < currentTile.Row);

                    if (nextTile != null && IsVerticalPathClear(currentTile, nextTile))
                    {
                        tilesInColumn.Remove(nextTile);
                        AnimateTileMergeVertical(nextTile, currentTile, targetRow);
                        //
                        AnimateTileMovementVertical(currentTile, targetRow);
                        //
                        _score += nextTile.Value * 2;
                        UpdateScore();
                    }
                    else
                    {
                        AnimateTileMovementVertical(currentTile, targetRow);
                    }

                    targetRow--;
                }
            }
        }

        private void AnimateTileMergeVertical(Tile movingTile, Tile targetTile, int targetRow)
        {
            Grid gridMovingTile = GameGrid.Children.OfType<Grid>()
                .First(g => Grid.GetRow(g) == movingTile.Row && Grid.GetColumn(g) == movingTile.Column);

            var currentTranslateTransform = gridMovingTile.RenderTransform as TranslateTransform;
            if (currentTranslateTransform == null)
            {
                currentTranslateTransform = new TranslateTransform();
                gridMovingTile.RenderTransform = currentTranslateTransform;
            }

            double currentY = currentTranslateTransform.Y;
            double targetY = (targetRow - movingTile.Row) * gridMovingTile.ActualHeight;

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                From = currentY,
                To = targetY,
                Duration = TimeSpan.FromMilliseconds(400)
            };

            moveAnimation.Completed += (sender, e) =>
            {
                RemoveTileFromGrid(movingTile);
                UpdateTileValueAndStyle(targetTile, targetTile.Value * 2);
                Grid.SetRow(gridMovingTile, targetRow);
                gridMovingTile.RenderTransform = new TranslateTransform();

                _animationsInProgress--;
                if (_animationsInProgress == 0)
                {
                    AddNewTile();
                }
            };

            _animationsInProgress++;
            EnsureTileIsOnTop(gridMovingTile);

            currentTranslateTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation);
        }
        private void AnimateTileMovementVertical(Tile tile, int targetRow)
        {
            Grid gridTile = GameGrid.Children.OfType<Grid>()
                .First(g => Grid.GetRow(g) == tile.Row && Grid.GetColumn(g) == tile.Column);

            var currentTranslateTransform = gridTile.RenderTransform as TranslateTransform;
            if (currentTranslateTransform == null)
            {
                currentTranslateTransform = new TranslateTransform();
                gridTile.RenderTransform = currentTranslateTransform;
            }

            double currentY = currentTranslateTransform.Y;
            double targetY = (targetRow - tile.Row) * gridTile.ActualHeight;
            _animationsInProgress++;

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                From = currentY,
                To = targetY,
                Duration = TimeSpan.FromMilliseconds(400)
            };

            moveAnimation.Completed += (sender, e) =>
            {
                Grid.SetRow(gridTile, targetRow);
                gridTile.RenderTransform = new TranslateTransform();

                _animationsInProgress--;
                if (_animationsInProgress == 0)
                {
                    AddNewTile();
                }
            };
            EnsureTileIsOnTop(gridTile);

            currentTranslateTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation);
        }
        private bool IsVerticalPathClear(Tile startTile, Tile endTile)
        {
            if (startTile.Value != endTile.Value)
            {
                return false;
            }

            int startRow = Math.Min(startTile.Row, endTile.Row);
            int endRow = Math.Max(startTile.Row, endTile.Row);

            for (int row = startRow + 1; row < endRow; row++)
            {
                if (!IsCellEmpty(row, startTile.Column))
                {
                    return false;
                }
            }
            return true;
        }


        #region Подбор цветов для плитки 
        private Color GetColorForTileValue(int value)
        {
            //switch (value)
            //{
            //    case 2: return Color.FromRgb(25, 25, 112);       
            //    case 4: return Color.FromRgb(70, 130, 180);      
            //    case 8: return Color.FromRgb(100, 149, 237);     
            //    case 16: return Color.FromRgb(127, 255, 212);    
            //    case 32: return Color.FromRgb(50, 205, 50);      
            //    case 64: return Color.FromRgb(124, 252, 0);      
            //    case 128: return Color.FromRgb(173, 255, 47);    
            //    case 256: return Color.FromRgb(255, 255, 0);     
            //    case 512: return Color.FromRgb(255, 223, 186);   
            //    case 1024: return Color.FromRgb(255, 182, 193);  
            //    case 2048: return Color.FromRgb(255, 105, 180);  
            //    case 4096: return Color.FromRgb(255, 20, 147);   
            //    case 8192: return Color.FromRgb(219, 112, 147);  
            //    case 16384: return Color.FromRgb(199, 21, 133);  
            //    case 32768: return Color.FromRgb(148, 0, 211);   
            //    case 65536: return Color.FromRgb(138, 43, 226);  
            //    default: return Colors.LightGray;
            //}
            switch (value)
            {
                case 2: return Color.FromRgb(8, 8, 48);
                case 4: return Color.FromRgb(25, 25, 112);
                case 8: return Color.FromRgb(39, 16, 96);
                case 16: return Color.FromRgb(75, 0, 130);
                case 32: return Color.FromRgb(148, 0, 211);
                case 64: return Color.FromRgb(138, 43, 226);
                case 128: return Color.FromRgb(123, 104, 238);
                case 256: return Color.FromRgb(106, 90, 205);
                case 512: return Color.FromRgb(72, 61, 139);
                case 1024: return Color.FromRgb(70, 130, 180);
                case 2048: return Color.FromRgb(64, 224, 208);
                case 4096: return Color.FromRgb(32, 178, 170);
                case 8192: return Color.FromRgb(0, 139, 139);
                default: return Color.FromRgb(176, 224, 230);  // PowderBlue
            }

        }
        #endregion

        #region Работа с файлом 
        private void UpdateScore()
        {
            ScoreTextBlock.Text = _score.ToString();

            if (_score > _highscore)
            {
                _highscore = _score;
                HighscoreTextBlock.Text = _highscore.ToString();
                UpdateUserHighscore(NameUsers, _highscore);
            }
        }
        private void LoadAndDisplayHighscore(string username)
        {
            List<User> users = LoadUsers();

            User user = users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                HighscoreTextBlock.Text = user.Highscore.ToString();
                _highscore = user.Highscore;
            }
            else
            {
                MessageBox.Show("Пользователь не найден!");
            }
        }

        private void UpdateUserHighscore(string username, int newHighscore)
        {

            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                user = new User { Username = username, Highscore = newHighscore };
                users.Add(user);
            }
            else if (newHighscore > user.Highscore)
            {
                user.Highscore = newHighscore;
            }

            SaveUsers(users);
        }

        private void SaveUsers(List<User> users)
        {
            string json = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText("users.json", json);
        }
        private List<User> LoadUsers()
        {
            string json = File.ReadAllText("users.json");
            if (string.IsNullOrWhiteSpace(json))
                return new List<User>();

            try
            {
                return JsonConvert.DeserializeObject<List<User>>(json);
            }
            catch
            {
                try
                {
                    User singleUser = JsonConvert.DeserializeObject<User>(json);
                    return new List<User> { singleUser };
                }
                catch
                {
                    return new List<User>();
                }
            }
        }
        #endregion

    }
}
