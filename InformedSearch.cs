using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1
{
    public class InformedSearch
    {
        public List<State> OpenSet { get; set; } = new List<State>();
        public HashSet<State> Visited { get; set; } = new HashSet<State>();
        public List<State> MovesWin { get; set; } = new List<State>();//массив шагов для победы для шага вперёд
        public List<State> MovesWinReverse { get; set; } = new List<State>();//массив шагов для победы для шага назад

        Stack<State> stack = new Stack<State>();

        Player player = new Player();

        int countNode = 0;
        int countIteration = 0;
        
        public class State
        {
            public (int x, int y) PlayerPosition;
            public List<(int x, int y)> BoxPositions;
            public List<(int x, int y)> PointPosition;
            public State prevMove = null;
            public int CountMoves = 0;

            public State((int x, int y) playerPos, List<(int x, int y)> boxPos, List<(int x, int y)> pointPos,int CountMoves, State state)
            {
                this.PlayerPosition = (playerPos.x, playerPos.y);
                this.BoxPositions = new List<(int x, int y)>(boxPos);
                this.PointPosition = new List<(int x, int y)>(pointPos);
                this.CountMoves = CountMoves;
                this.prevMove = state;//предыдущий шаг
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                State s = (State)obj;
                if (PlayerPosition != s.PlayerPosition) return false;
                foreach (var b in BoxPositions)
                {
                    if (!s.BoxPositions.Contains(b)) return false;
                }
                return true;
            }
            
            
            public override int GetHashCode()
            {
                int hash = 17;

                // Хеширование позиции игрока
                hash = hash * 31 + PlayerPosition.x.GetHashCode();
                hash = hash * 31 + PlayerPosition.y.GetHashCode();

                // Хеширование позиций ящиков
                foreach (var box in BoxPositions)
                {
                    hash = hash * 31 + box.x.GetHashCode();
                    hash = hash * 31 + box.y.GetHashCode();
                }

                // Хеширование позиций точек
                foreach (var point in PointPosition)
                {
                    hash = hash * 31 + point.x.GetHashCode();
                    hash = hash * 31 + point.y.GetHashCode();
                }

                hash = hash * 31 + CountMoves.GetHashCode();

                // Если используется состояние предыдущего хода (если необходимо)
                if (prevMove != null)
                {
                    hash = hash * 31 + prevMove.GetHashCode(); // опционально, если State тоже переопределяет GetHashCode
                }

                return hash;
            }

            public int Heuristic()
            {
                int heuristicValue = 0;
                foreach (var box in BoxPositions)
                {
                    // Находим ближайшую точку
                    int minDistance = PointPosition
                        .Select(point => Math.Abs(point.x - box.x) + Math.Abs(point.y - box.y))
                        .Min();
                    heuristicValue += minDistance;
                }
                return heuristicValue;
            }

            public int Cost()
            {
                return CountMoves;
            }
        }

        public bool IsGoalState(State state)
        {
            int count = 0;
            foreach (var box in state.BoxPositions)
            {
                foreach (var point in state.PointPosition)
                {
                    if (box.x == point.x && box.y == point.y)
                    {
                        count++;
                        break;
                    }
                }
            }
            return count == state.BoxPositions.Count;
        }

        public void Search()
        {
            OpenSet.Add(new State(Map.Instance.GetPlayerPos(), Map.Instance.GetBoxPosition(),
                Map.Instance.GetPointPosition(), 0, null));

            while (OpenSet.Count>0)
            {
                countIteration++;
                var current = OpenSet.OrderBy(state => state.Cost() + state.Heuristic()).First();
                if (IsGoalState(current))
                {
                    Console.WriteLine("win!");
                    fillMovesWin(current);
                    WriteMovesWin();
                    return;
                }
                OpenSet.Remove(current);
                Visited.Add(current);
                
                foreach (var move in GetPossibleMoves(current))
                {
                    if(Visited.Contains(move)) continue;
                    int tentativeCost = current.Cost() + 1;
                    if (!OpenSet.Contains(move) || tentativeCost < move.Cost())
                    {
                        OpenSet.Add(move);
                    }
                }
            }
        }
        
        public IEnumerable<State> GetPossibleMoves(State current)
        {
            Player player = new Player();
            List<State> moves = new List<State>();
            var directions = new (int x, int y)[]
            {
                (0, 1),  //down
                (0, -1), //up
                (1, 0),  //right
                (-1, 0), //left    
            };
            foreach (var dir in directions)
            {
                var newPos = (current.PlayerPosition.x + dir.x, current.PlayerPosition.y + dir.y);
                if (player.CanMove(current.PlayerPosition, dir, current.BoxPositions))
                {
                    var tmpBoxes = Map.Instance.MoveBox(current.PlayerPosition, dir, current.BoxPositions);
                    moves.Add(new State(newPos, tmpBoxes, current.PointPosition,current.Cost() + 1, current));//в конце current -> это предыдущий шаг
                    countNode++;
                }
            }
            return moves;
        }
        
        public void DrawPrevStep(int indexMovesWin)
        {
            Map.Instance.DrawClearMap();//функция по очистке карты -> всё что не точка и не стена делать точкой
            player.DrawPlayer(MovesWinReverse[indexMovesWin].PlayerPosition);//отрисовка игрока
            for (int i = 0; i < MovesWinReverse[indexMovesWin].PointPosition.Count; i++)
                Map.Instance.DrawPoint(MovesWinReverse[indexMovesWin].PointPosition[i]);//отрисовка точек
            for (int i = 0; i < MovesWinReverse[indexMovesWin].BoxPositions.Count; i++)
                Map.Instance.DrawBox(MovesWinReverse[indexMovesWin].BoxPositions[i]);//отрисовка коробок

            if (indexMovesWin == 0) Console.WriteLine("Стартовая позциия");
        }

        public bool NumberNextStep(int indexMovesWin)
        {
            if (indexMovesWin == MovesWin.Count - 1)
                return false;
            else return true;
        }

        public int GetCountSteps()
        {
            return MovesWin.Count;
        }
        
        public void fillMovesWin(State current)
        {
            MovesWin.Add(current);//добавление позиции
            if (current.prevMove != null)//если существует ссылка на предыдущий шаг
                fillMovesWin(current.prevMove);//вызываем рекурсию с ссылкой на пред.шаг
            else
            {
                MovesWinReverse = MovesWin;
                MovesWin.Reverse();//если пред.шага нет, тогда разворачиваем массив шагов, тем самым на первой позиции будет стартовая точка
            }
        }
        
        public void WriteMovesWin()
        {
            for (int i = 0; i < MovesWin.Count; i++)
            {
                Console.WriteLine($"({MovesWin[i].PlayerPosition.x} {MovesWin[i].PlayerPosition.y})");
            }
        }

        public int GetCountNode()
        {
            return countNode;
        }

        public int GetCountIteration()
        {
            return countIteration;
        }
        
        
    }
}