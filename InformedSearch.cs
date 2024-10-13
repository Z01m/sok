using System;
using System.Collections.Generic;
using System.Linq;
using Lab1;
public class infs
{
    public class State
    {
        public (int x, int y) PlayerPosition;
        public List<(int x, int y)> BoxPositions;
        public List<(int x, int y)> PointPosition;
        public State prevMove = null;
        public int CountMoves = 0;

        public State((int x, int y) playerPos, List<(int x, int y)> boxPos, List<(int x, int y)> pointPos, int CountMoves, State state)
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
            State other = obj as State;
            if (other == null) return false;

            if (PlayerPosition != other.PlayerPosition) return false;
            if (CountMoves != other.CountMoves) return false;

            // Compare box positions using a sorted list or HashSet for faster comparison
            var thisBoxes = new HashSet<(int x, int y)>(BoxPositions);
            var otherBoxes = new HashSet<(int x, int y)>(other.BoxPositions);
            return thisBoxes.SetEquals(otherBoxes);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + PlayerPosition.x.GetHashCode();
            hash = hash * 31 + PlayerPosition.y.GetHashCode();

            foreach (var box in BoxPositions)
            {
                hash = hash * 31 + box.x.GetHashCode();
                hash = hash * 31 + box.y.GetHashCode();
            }

            return hash;
        }

        public int Heuristic()
        {
            int heuristicValue = 0;
            foreach (var box in BoxPositions)
            {
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
    public class PriorityQueue<T>
    {
        private List<Tuple<T, int>> elements = new List<Tuple<T, int>>();

        public void Enqueue(T item, int priority)
        {
            elements.Add(Tuple.Create(item, priority));
        }

        public T Dequeue()
        {
            int bestIndex = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Item2 < elements[bestIndex].Item2)
                {
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].Item1;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }

        public int Count()
        {
            return elements.Count;
        }
        public bool Contains(T item)
        {
            foreach (var element in elements)
            {
                if (EqualityComparer<T>.Default.Equals(element.Item1, item))
                {
                    return true;
                }
            }
            return false;
        }

    }

    public class InformedSearch
    {
        private PriorityQueue<State> OpenSet = new PriorityQueue<State>();
        private HashSet<State> Visited = new HashSet<State>();

        public List<State> MovesWin { get; set; } = new List<State>();//массив шагов для победы для шага вперёд
        public List<State> MovesWinReverse { get; set; } = new List<State>();//массив шагов для победы для шага назад
        Player player = new Player();

        int countNode = 0;
        int countIteration = 0;



        public bool IsGoalState(State state)
        {
            return state.BoxPositions.All(box => state.PointPosition.Contains(box));
        }

        public void Search()
        {
            OpenSet.Enqueue(new State(Map.Instance.GetPlayerPos(), Map.Instance.GetBoxPosition(),
                Map.Instance.GetPointPosition(), 0, null), 0);

            while (OpenSet.Count() > 0)
            {
                countIteration++;
                var current = OpenSet.Dequeue();

                if (IsGoalState(current))
                {
                    Console.WriteLine("win!");
                    fillMovesWin(current);
                    WriteMovesWin();
                    return;
                }

                Visited.Add(current);

                foreach (var move in GetPossibleMoves(current))
                {
                    if (Visited.Contains(move)) continue;
                    int tentativeCost = current.Cost() + 1;
                    if (!OpenSet.Contains(move))
                    {
                        if (tentativeCost <= move.Cost())
                        {
                            OpenSet.Enqueue(move, tentativeCost + move.Heuristic());
                        }
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
                    yield return new State(newPos, tmpBoxes, current.PointPosition, current.Cost() + 1, current);//в конце current -> это предыдущий шаг
                    countNode++;
                }
            }
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

        public void DrawNextStep(int indexMovesWin)
        {
            if (indexMovesWin < MovesWin.Count)
            {
                Map.Instance.DrawClearMap();//функция по очистке карты -> всё что не точка и не стена делать точкой
                player.DrawPlayer(MovesWin[indexMovesWin].PlayerPosition);//отрисовка игрока
                for (int i = 0; i < MovesWin[indexMovesWin].PointPosition.Count; i++)
                    Map.Instance.DrawPoint(MovesWin[indexMovesWin].PointPosition[i]);//отрисовка точек
                for (int i = 0; i < MovesWin[indexMovesWin].BoxPositions.Count; i++)
                    Map.Instance.DrawBox(MovesWin[indexMovesWin].BoxPositions[i]);//отрисовка коробок
            }
            else Console.WriteLine("Следующего шага нет!");
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
            if (indexMovesWin == MovesWin.Count)
                return false;
            else return true;
        }


    }
}