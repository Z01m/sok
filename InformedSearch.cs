using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Lab1
{
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
                State other = obj as State;
                if (other == null) return false;

                if (PlayerPosition != other.PlayerPosition) return false;
                //if (CountMoves != other.CountMoves) return false;
                foreach (var b in BoxPositions)
                {
                    if (!other.BoxPositions.Contains(b)) return false;
                }
                return true;
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

            private int BFS((int x, int y) start) // находит длину пути от ящика до любой ближайшей точки
            {
                Queue<((int x, int y) position, int distance)> queue = new Queue<((int, int), int)>();
                HashSet<(int x, int y)> visited = new HashSet<(int x, int y)>();
                queue.Enqueue((start, 0));
                var directions = new (int x, int y)[]
                {
                    (0, 1),  // down
                    (0, -1), // up
                    (1, 0),  // right
                    (-1, 0), // left    
                };

                while (queue.Count > 0)
                {
                    var (position, distance) = queue.Dequeue();

                    foreach (var point in PointPosition)
                    {
                        if (position == point)
                        {
                            return distance; // нашли ближайшую цель
                        }
                    }
                    visited.Add(position);

                    foreach (var dir in directions)
                    {
                        int newX = position.x + dir.x;
                        int newY = position.y + dir.y;
                        var newPos = (newX, newY);
                        
                        if (!visited.Contains(newPos) && Map.LevelMap[newY][newX] != '#' && !BoxPositions.Contains(newPos))
                        {
                            queue.Enqueue((newPos, distance + 1));
                        }
                    }
                }
                return 10000; // если недоступно
            }
            
            public int Heuristic() //возвращает суммарную дистанцию от каждого ящика до точки 
            {
                int totalDistance = 0;
                foreach (var box in BoxPositions)
                {
                    int minDistance = BFS(box); //дистанция считается через поиск в ширину 
                    totalDistance += minDistance; 
                }
                //Console.WriteLine(totalDistance);
                
                return totalDistance;
            }

            public int Cost() // возвращает кол-во пройденных шагов 
            {
                return CountMoves;
            }
        }
    public class PriorityQueue<State> 
    {
    private List<Tuple<State, int>> elements = new List<Tuple<State, int>>();

        public void Enqueue(State item, int priority)
        {
            elements.Add(Tuple.Create(item, priority));
        }
    
        public State Dequeue() //достает эллемент с мин приоритетом
        {
            int bestIndex = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Item2 < elements[bestIndex].Item2)
                {
                    bestIndex = i;
                }
            }
            State bestItem = elements[bestIndex].Item1;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }

        public int Count()
        {
            return elements.Count;
        }
        public bool Contains(State item)
        {
            foreach (var element in elements)
            {
                if (item.Equals(element.Item1))
                {
                    return true;
                }
            }
            return false;
        }
        
        public void Change(State current, int newPriority)
        {
            // Найти индекс элемента, который нужно изменить
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Item1.Equals(current))
                {
                    elements.RemoveAt(i);
                    elements.Add(Tuple.Create(current, newPriority));
                    return; 
                }
            }
        }


        public Tuple<State, int> Find(State current)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Item1.Equals(current))
                {
                    return elements[i];
                }
            }
            return null;
        }
    
    }
    
    public class InformedSearch
    {
        private PriorityQueue<State> OpenSet = new PriorityQueue<State>();
        private HashSet<State> Visited = new HashSet<State>();
        
        public List<State> MovesWin { get; set; } = new List<State>();//массив шагов для победы для шага вперёд
        public List<State> MovesWinReverse { get; set; } = new List<State>();//массив шагов для победы для шага назад

        int countNode = 0;
        int countIteration = 0;

        public bool IsGoalState(State state)
        {
            return state.BoxPositions.All(box => state.PointPosition.Contains(box));
        }

        public void Search()
        {
            var startState = new State(Map.Instance.GetPlayerPos(), Map.Instance.GetBoxPosition(),
                Map.Instance.GetPointPosition(), 0, null);
    
            OpenSet.Enqueue(startState, 0);

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
                    int newCost = move.Cost() + move.Heuristic();

                    // Проверяем, если move уже в OpenSet
                    if (OpenSet.Contains(move))
                    {
                        var existingEntry = OpenSet.Find(move);
                        if (existingEntry.Item2 > newCost)
                        {
                            OpenSet.Change(move, newCost);
                        }
                    }
                    // Проверяем, если move уже в Visited
                    else if (Visited.Contains(move))
                    {
                        var visitedEntry = IndexOf(Visited, move);
                        int visitedCost = visitedEntry.Cost() + visitedEntry.Heuristic();

                        if (visitedCost > newCost)
                        {
                            Visited.Remove(visitedEntry);
                            OpenSet.Enqueue(move, newCost); // Оставить только enqueue
                        }
                    }
                    // Если move ни в одном из множеств, добавляем его в OpenSet
                    else
                    {
                        OpenSet.Enqueue(move, newCost);
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
        
        public State IndexOf(HashSet<State> collection, State searchItem)
        {
            State index;

            foreach (var item in collection)
            {
                if (item.Equals(searchItem))
                {
                    return item;
                }
            }
            return null;
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