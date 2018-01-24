using System;
using System.Collections.Generic;
using System.Text;
//обеспечивает ИИ для компьютера
namespace Reversi.Classes
{
    class MoveSolver
    {
        #region ReadOnly

        private const int MAX_BOARD_VALUE = Int32.MaxValue;//плюс бесконечность
        private const int MIN_BOARD_VALUE = - MAX_BOARD_VALUE;//минус бесконечность

        #endregion        

        #region Fields

        private Player mPlayer;
        private int mMaxDepth;        

        #endregion

        #region Constructors

        public MoveSolver(Player player, int maxDepth)
        {
            this.mPlayer = player;
            this.mMaxDepth = maxDepth;
        }

        #endregion        

        #region Properties

        public Player Player
        {
            get
            {
                return this.mPlayer;
            }
        }

        public int MaxDepth
        {
            get
            {
                return this.mMaxDepth;
            }
        }

        #endregion

        #region Methods

        public void GetNextMove(out int rowIndex, out int columnIndex)//вызывается в классе ComputerPlayer в процедуре DoNextMove
        {
            this.GetNextMove(this.Player.Game.Board, true, 1, MIN_BOARD_VALUE, MAX_BOARD_VALUE, out rowIndex, out columnIndex);
        }
        //процедура для осуществления следующего хода. алгоритм АВ-отсечения. ищет лучший ход. возвращает BestBoardValue в виде числа (вероятности)
        private int GetNextMove(Board board, bool isMaximizing, int currentDepth, int alpha, int beta, out int resultRowIndex, out int resultColumnIndex)
        {
            resultRowIndex = 0;
            resultColumnIndex = 0;
            //цвет фишки
            DiscColor color = isMaximizing ? this.Player.Color : DiscColor.GetOpposite(this.Player.Color);//если true DiskColor = Player.Color иначе противоположный цвет
            bool playerSkipsMove = false;
            List<int[]> possibleMoves = new List<int[]>();
        //проверка на последний ход
            bool isFinalMove = (currentDepth >= this.MaxDepth) || this.Player.Game.IsStopped || this.Player.Game.IsPaused;
            //если ход не последний
            if (!isFinalMove)
            {   //получение возможных ходов.          
                possibleMoves = this.GetPossibleMoves(board, color);
                if (possibleMoves.Count == 0)//если нет возможных ходов
                {
                    playerSkipsMove = true;
                    possibleMoves = this.GetPossibleMoves(board, DiscColor.GetOpposite(color));//получить список возможных ходов для противоположного игрока
                }
               
                isFinalMove = (possibleMoves.Count == 0); //проверка на последний ход (если список пуст вернет true)
            }
            //если используем на самом "дне" глубины подсчета
            if (isFinalMove)
            {
                resultRowIndex = -1;
                resultColumnIndex = -1;
                return this.EvaluateBoard(board);//возвращает оценочную эвристическую функцию (mobility+stability)
            }
            else//если макс. глубине не достигнута
            {//поиск наилучшего хода (просмотр дерева)
                int bestBoardValue = isMaximizing ? MIN_BOARD_VALUE : MAX_BOARD_VALUE;//в начале bestBoardValue присваивается + или - бесконечность 
                int bestMoveRowIndex = -1;//инициализация координат лучшего хода
                int bestMoveColumnIndex = -1;

                foreach (int[] nextMove in possibleMoves)
                {
                    int rowIndex = nextMove[0];
                    int columnIndex = nextMove[1];

                    Board nextBoard = (Board)board.Clone();//копия доски
                    nextBoard.SetFieldColor(rowIndex, columnIndex, color);//установка фишек на доску

                    bool nextIsMaximizing = playerSkipsMove ? isMaximizing : !isMaximizing;

                    int dummyIndex; // значения resultRowIndex и resultColumnIndex не нуждаются в рекурсивном вызове функции 
                    //рекурсивный вызов
                    
                    int currentBoardValue = this.GetNextMove(nextBoard, nextIsMaximizing, currentDepth + 1, alpha, beta, out dummyIndex, out dummyIndex);
                    if (isMaximizing)
                    {
                        if (currentBoardValue > bestBoardValue)
                        {
                            bestBoardValue = currentBoardValue;
                            bestMoveRowIndex = rowIndex;
                            bestMoveColumnIndex = columnIndex;
                            //Найти оценку текущей ситуации, в предположении, что она находится между alpha и beta. 
                            if (bestBoardValue > alpha)
                            {
                                alpha = bestBoardValue;
                            }

                            if (bestBoardValue >= beta)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (currentBoardValue < bestBoardValue)
                        {
                            bestBoardValue = currentBoardValue;
                            bestMoveRowIndex = rowIndex;
                            bestMoveColumnIndex = columnIndex;

                            if (bestBoardValue < beta)

                            {
                                beta = bestBoardValue;
                            }

                            if (bestBoardValue <= alpha)
                            {
                                break;
                            }
                        }
                    }
                }

                resultRowIndex = bestMoveRowIndex;
                resultColumnIndex = bestMoveColumnIndex;
                return bestBoardValue;
            }
      }
        //создание списка возможных ходов
        private List<int[]> GetPossibleMoves(Board board, DiscColor color)
        {
            List<int[]> possibleMoves = new List<int[]>();
            for (int rowIndex = 0; rowIndex < board.Size; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < board.Size; columnIndex++)
                {
                    if (board.CanSetFieldColor(rowIndex, columnIndex, color))
                    {
                        possibleMoves.Add(new int[2] { rowIndex, columnIndex });
                    }
                }
            }            
            //перемешиване списка PossibleMoves и запись его в result
            List<int> indexes = this.GetRandomIndexes(possibleMoves.Count);
            List<int[]> result = new List<int[]>();
            foreach (int index in indexes)
            {
                result.Add(possibleMoves[index]);
            }
            return result;
        }
        //генерация списка случайных чисел на заданном диапазоне
        private List<int> GetRandomIndexes(int count)
        {
            int minValue = 0;
            int maxValue = count;
            List<int> result = new List<int>();
            Random random = new Random();

            while (result.Count < count)
            {
                int next = random.Next(minValue, maxValue);
                if (!result.Contains(next))
                {
                    result.Add(next);
                }

                if (next == minValue)
                {
                    minValue++;
                }
                else if (next == maxValue - 1)
                {
                    maxValue--;
                }
            }

            return result;
        }

        //функция оценки доски
        private int EvaluateBoard(Board board)
        {
            DiscColor color = this.Player.Color;
            DiscColor oppositeColor = DiscColor.GetOpposite(this.Player.Color);
            //списки предположительных ходов игроков
            List<int[]> oppositePlayerPossibleMoves = this.GetPossibleMoves(board, oppositeColor);
            List<int[]> possibleMoves = this.GetPossibleMoves(board, color);

            if ((possibleMoves.Count == 0) && (oppositePlayerPossibleMoves.Count == 0))//если списки пусты
            {
                int result = board.GetDiscsCount(color) - board.GetDiscsCount(oppositeColor);//от количества фишек ии отнимает количество фишек противника
                int addend = (int)Math.Pow(board.Size, 4) + (int)Math.Pow(board.Size, 3); // потому что это конечное состояние, его вес должен быть больше эвристического
                if (result < 0)//если разность меньше 0
                {
                    addend = -addend;
                }
                return result + addend;
            }
            else//если ходы есть
            {
                //количество фишек противника, которые могут быть преобразованы
                int mobility = this.GetPossibleConvertions(board, color, possibleMoves) 
                    - this.GetPossibleConvertions(board, oppositeColor, oppositePlayerPossibleMoves);
                //число стабильных фишек на доске игрока
                int stability = (this.GetStableDiscsCount(board, color) - this.GetStableDiscsCount(board, oppositeColor)) * board.Size * 2 / 3;

                return mobility + stability;
            }
        }
        //возвращает количество фишек, которые могут быть "захвачены"
        private int GetPossibleConvertions(Board board, DiscColor color, List<int[]> possibleMoves)
        {
            int result = 0;
            foreach (int[] move in possibleMoves)
            {
                Board newBoard = board.Clone() as Board;
                int rowIndex = move[0];
                int columnIndex = move[1];

                newBoard.SetFieldColor(rowIndex, columnIndex, color);
                result += newBoard.InvertedDiscsLastMove;
            }
            return result;
        }
        //возвращает количество стабильных фишек (фишки, которые не могут быть перевернуты в дальнейшей игре никаким образом)
        public int GetStableDiscsCount(Board board, DiscColor color)
        {
            return this.GetStableDiscsFromCorner(board, color, 0, 0)//левый верхний угол
                + this.GetStableDiscsFromCorner(board, color, 0, board.Size - 1)//правый верхний
                + this.GetStableDiscsFromCorner(board, color, board.Size - 1, 0)//левый нижний
                + this.GetStableDiscsFromCorner(board, color, board.Size - 1, board.Size - 1)//правый нижний
                + this.GetStableDiscsFromEdge(board, color, 0, true)//верх
                + this.GetStableDiscsFromEdge(board, color, board.Size - 1, true)//низ
                + this.GetStableDiscsFromEdge(board, color, 0, false)//левая граница
                + this.GetStableDiscsFromEdge(board, color, board.Size - 1, false);//правая граница
        }
// Области в углу и по краям позволят легко завладеть большим
//количеством фишек противника сразу.Таким образом, ии
//должен занимать эти позиции, если это возможно.
        //Получить стабильные фишки из углов
        private int GetStableDiscsFromCorner(Board board, DiscColor color, int cornerRowIndex, int cornerColumnIndex)
        {
            int result = 0;

            int rowIndexChange = (cornerRowIndex == 0) ? 1 : -1;
            int columnIndexChange = (cornerColumnIndex == 0) ? 1 : -1;

            int rowIndex = cornerRowIndex;
            int rowIndexLimit = (cornerRowIndex == 0) ? board.Size : 0;
            int columnIndexLimit = (cornerColumnIndex == 0) ? board.Size : 0;
            for (rowIndex = cornerRowIndex; rowIndex != rowIndexLimit; rowIndex += rowIndexChange)
            {
                int columnIndex;
                for (columnIndex = cornerColumnIndex; columnIndex != columnIndexLimit; columnIndex += columnIndexChange)//цикл подсчета результата 
                {
                    if (board[rowIndex, columnIndex] == color)//если цвет поля нужный
                    {
                        result++;
                    }
                    else
                    {//иначе переход на следующую строку (выход из цикла)
                        break;
                    }
                }

                if ((columnIndexChange > 0 && columnIndex < board.Size) || (columnIndexChange < 0 && columnIndex > 0))
                {
                    columnIndexLimit = columnIndex - columnIndexChange;

                    if (columnIndexChange > 0 && columnIndexLimit == 0)
                    {
                        columnIndexLimit++;
                    }
                    else if (columnIndexChange < 0 && columnIndexLimit == board.Size - 1)
                    {
                        columnIndexLimit--;
                    }

                    if ((columnIndexChange > 0 && columnIndexLimit < 0)
                        || (columnIndexChange < 0 && columnIndexLimit > board.Size - 1))
                    {
                        break;
                    }
                }
            }

            return result;
        }
        //Получить стабильные фишки с края (подсчет количества)
        private int GetStableDiscsFromEdge(Board board, DiscColor color, int edgeCoordinate, bool isHorizontal)
        {
            int result = 0;

            if (IsEdgeFull(board, edgeCoordinate, isHorizontal))
            {
                bool oppositeColorDiscsPassed = false;
                for (int otherCoordinate = 0; otherCoordinate < board.Size; otherCoordinate++)
                {                
                    DiscColor fieldColor = (isHorizontal) ? board[edgeCoordinate, otherCoordinate] : board[otherCoordinate, edgeCoordinate];
                    if (fieldColor != color)
                    {
                        oppositeColorDiscsPassed = true;
                    }
                    else if (oppositeColorDiscsPassed)
                    {
                        int consecutiveDiscsCount = 0;
                        while ((otherCoordinate < board.Size) && (fieldColor == color))
                        {
                            consecutiveDiscsCount++;

                            otherCoordinate++;
                            if (otherCoordinate < board.Size)
                            {
                                fieldColor = (isHorizontal) ? board[edgeCoordinate, otherCoordinate] : board[otherCoordinate, edgeCoordinate];
                            }
                        }
                        if (otherCoordinate != board.Size)
                        {
                            result += consecutiveDiscsCount;
                            oppositeColorDiscsPassed = true;
                        }                                             
                    }
                }
            }

            return result;
        }
        //проверка если край заполнен
        private bool IsEdgeFull(Board board, int edgeCoordinate, bool isHorizontal)
        {
            for (int otherCoordinate = 0; otherCoordinate < board.Size; otherCoordinate++)
            {
                if (isHorizontal && (board[edgeCoordinate, otherCoordinate] == DiscColor.None)
                    || !isHorizontal && (board[otherCoordinate, edgeCoordinate] == DiscColor.None))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
