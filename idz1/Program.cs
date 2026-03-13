using System;
using System.ComponentModel.DataAnnotations;
while (true)
{
    Console.WriteLine("Введите первую строку:");
    string s1 = Console.ReadLine();
    if (s1 == "exit") break;
    Console.WriteLine("Введите вторую строку:");
    string s2 = Console.ReadLine();
    Console.WriteLine($"Расстояние Дамерау-Левенштейна составляет {Distance(s1,s2)}");
    Console.WriteLine();
}

static int Distance (string s11, string s22)
{
    if (s11 == null || s22 == null) return -1;
    string s1 = s11.ToUpper();
    string s2 = s22.ToUpper();
    int cost;
    int l1 = s1.Length;
    int l2 = s2.Length;
    int[,] matrix = new int[l1+1, l2+1];
    for (int i = 0; i <= l1; i++) matrix[i, 0] = i;
    for (int i = 0; i <= l2; i++) matrix[0, i] = i;
    for (int i = 1; i <= l1; i++)
    {
        for (int j = 1; j <= l2; j++)
        {
            if (s1[i - 1] == s2[j - 1]) cost = 0;
            else cost = 1;
            matrix[i,j] = Math.Min(Math.Min(matrix[i-1,j] + 1, matrix[i,j-1]+1), matrix[i-1,j-1] + cost);

            if (i > 1 && j > 1 && s1[i - 1] == s2[j - 2] && s1[i - 2] == s2[j - 1])
                matrix[i, j] = Math.Min(matrix[i-2,j-2] + cost, matrix[i,j]);
        }
    }
    return matrix[l1, l2];
}
