﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lab1;

public class Map
{
    private static Map instance;

    // Приватный конструктор
    private Map()
    {
    }

    public static Map Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Map();
            }
            return instance;
        }
    }

    public static string[] LevelMap =  {
        //"########",
        //"#X.....#",
        //"#@.....#",
        //"#....@.#",
        //"#......#",
        //"#*...*.#",
        //"#......#",
        //"########"
    };

    public List<(int x, int y)> AllBox { get; set; }
    public List<(int x, int y)> AllPoint { get; set; }

    public void DrawClearMap()
    {
        for (int y = 1; y < LevelMap.Length; y++)//y
        {
            for (int x = 1; x < LevelMap[y].Length; x++)//x
            {
                if (LevelMap[y][x] == 'X' || LevelMap[y][x] == '@' || LevelMap[y][x] == '*')//Инверсия коодинат
                {
                    ChangeMap(x, y, '.');
                }
            }
        }
    }

    public void DrawMap(string[] map)
    {
        foreach (var str in map)
        {
            Console.WriteLine(str);
        }
    }

    public void ChangeMap(int x, int y, char c) // функция для изменения карты
    {
        char[] row = LevelMap[y].ToCharArray();
        row[x] = c;
        LevelMap[y] = new string(row);
    }

    public (int x, int y) GetPlayerPos() // находит игрока на карте и его позицию
    {
        for (int i = 0; i < LevelMap.Length; i++)
        {
            for (int j = 0; j < LevelMap[i].Length; j++)
            {
                if (LevelMap[i][j] == 'X')
                {
                    return (j, i); // (column x, row y)
                }
            }
        }
        return (-1, -1); // Или выбросить исключение по вашему выбору
    }

    public List<(int x, int y)> GetBoxPosition() //находит все ящики на карте и возвращает лист с их координатами 
    {
        List<(int x, int y)> res = new List<(int x, int y)>();
        for (int i = 0; i < LevelMap.Length; i++)
        {
            for (int j = 0; j < LevelMap[i].Length; j++)
            {
                if (LevelMap[i][j] == '@')
                {
                    res.Add((j, i)); // (column x, row y)
                }
            }
        }
        return res;
    }

    public List<(int x, int y)> GetPointPosition() //находит все точки на карте и возвращает лист с их координатами 
    {
        List<(int x, int y)> res = new List<(int x, int y)>();
        for (int i = 0; i < LevelMap.Length; i++)
        {
            for (int j = 0; j < LevelMap[i].Length; j++)
            {
                if (LevelMap[i][j] == '*')
                {
                    res.Add((j, i)); // (column x, row y)
                }
            }
        }
        return res;
    }

    public bool BoxIsFind(int x, int y) //проверяет есть ли ящик по данным координатам
    {
        int index = AllBox.FindIndex(box => box.x == x && box.y == y); // Fix: (x, y)
        return index != -1;
    }

    public bool BoxIsFind(int x, int y, List<(int x, int y)> boxes) //проверяет есть ли ящик по данным координатам
    {
        int index = boxes.FindIndex(box => box.x == x && box.y == y); // Fix: (x, y)
        return index != -1;
    }

    public void ChangeBoxOnMap()
    {
        foreach (var vec in AllBox)
        {
            ChangeMap(vec.x, vec.y, '@'); // Fix: (x, y)
        }
    }

    public void MoveBox((int x, int y) playerPos, (int x, int y) delta)
    {
        int index = AllBox.FindIndex(box =>
            box.x == playerPos.x + delta.x && box.y == playerPos.y + delta.y); // Fix: (x, y)
        if (index != -1)
        {
            AllBox[index] = (playerPos.x + 2 * delta.x, playerPos.y + 2 * delta.y); // Fix: (x, y)
        }
        ChangeMap(playerPos.x, playerPos.y, '.'); // Fix: (x, y)
    }

    public List<(int x, int y)> MoveBox((int x, int y) playerPos, (int x, int y) delta, List<(int x, int y)> boxes)
    {
        List<(int x, int y)> tmpBoxes = new List<(int x, int y)>(boxes);
        int index = tmpBoxes.FindIndex(box =>
            box.x == playerPos.x + delta.x && box.y == playerPos.y + delta.y); // Fix: (x, y)
        if (index != -1)
        {
            tmpBoxes[index] = (playerPos.x + 2 * delta.x, playerPos.y + 2 * delta.y); // Fix: (x, y)
        }

        return tmpBoxes;
    }

    public void MoveBoxRevers((int x, int y) playerPos, (int x, int y) delta)
    {
        int index = AllBox.FindIndex(box =>
            box.x == playerPos.x - delta.x && box.y == playerPos.y - delta.y); // Fix: (x, y)
        if (index != -1)
        {
            ChangeMap(AllBox[index].x, AllBox[index].y, '.'); // Fix: (x, y)
            AllBox[index] = (playerPos.x, playerPos.y); // Fix: (x, y)
        }
    }

    public List<(int x, int y)> MoveBoxRevers((int x, int y) playerPos, (int x, int y) delta, List<(int x, int y)> boxes)
    {
        List<(int x, int y)> tmpBoxes = new List<(int x, int y)>(boxes);
        int index = tmpBoxes.FindIndex(box =>
            (box.x == playerPos.x - delta.x) && (box.y == playerPos.y - delta.y)); // Fix: (x, y)
        if (index != -1)
        {
            tmpBoxes[index] = (playerPos.x, playerPos.y); // Fix: (x, y)
        }

        return tmpBoxes;
    }

    public bool IsWin()
    {
        int count = 0;
        foreach (var point in AllPoint)
        {
            if (BoxIsFind(point.x, point.y)) // Fix: (x, y)
                count++;
        }
        return count == AllBox.Count;
    }


    //Отрисовка коробки Относительно State ++
    public void DrawBox((int x, int y) boxPos)
    {
        ChangeMap(boxPos.x, boxPos.y, '@');//инверсия координат
    }

    //функция по отрисовке коробки на новой пустой позиции
    public void DrawBox((int x,int y) boxPosition,(int x,int y) boxPositionPrev)
    {
        ChangeMap(boxPositionPrev.x,boxPositionPrev.y,'.');//Удаление прошлого места коробки
        ChangeMap(boxPosition.x,boxPosition.y,'@');//инвертированны координаты
    }
    //функция по отрисовке point на пустой позиции
    public void DrawPoint((int x, int y) pointPosition)
    {
        ChangeMap(pointPosition.x, pointPosition.y, '*');
    }


}