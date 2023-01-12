using System;
using System.Linq;
using MPI;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class Program
    {
            static int countUnit;
            static int countThreads;            

            public static double calculate(int[] C)
            {
                    int i, j;
                    double res = 0;
                    
                    for (i = 0; i < C.Length; i++)
                    {                                                      
                      if ((C[i] % 2) == 0) res = res + C[i];      
                    }

                    return res;
            }
        static void Main(string[] args)
            {
                    bool flag;

                    if (args.Count() == 0)
                    {
                            Console.Write("Введите количество элементов массива (от 100000 до 1000000): ");
                            do
                            {
                                    string numberStr = Console.ReadLine();

                                    flag = Int32.TryParse(numberStr, out countUnit);
                                    if (!flag)
                                    {
                                        Console.WriteLine("Количество элементов массива указано некорректно. Попробуйте еще раз.");
                                    }
                                    else
                                    {
                                        if (countUnit > 100000 & countUnit < 1000000) { flag = true; } else { flag = false; }
                                        if (!flag) Console.WriteLine("Количество элементов массива быть > 100 и < 100000. Попробуйте еще раз.");
                                    }
                            } while (!flag);
                            Console.WriteLine("Количество элементов массива = " + countUnit);
                            
                            
                            Console.Write("Введите количество потоков (от 1 до 40): ", countUnit);
                            do
                            {
                                    string numberStr = Console.ReadLine();

                                    flag = Int32.TryParse(numberStr, out countThreads);
                                    if (!flag)
                                    {
                                            Console.WriteLine("Количество потоков указано некорректно. Попробуйте еще раз.");
                                    }
                                    else
                                    {
                                            if (countThreads >= 1 && countThreads <= 40) { flag = true; } else { flag = false; }
                                            if (!flag) Console.WriteLine("Количество потоков должно быть меньше 40. Попробуйте еще раз.");
                                    }
                            }  while (!flag);
                            Console.WriteLine("Количество потоков = " + countThreads);
                            
                            
                            Process.Start("CMD.exe", "/C cd "+ AppDomain.CurrentDomain.BaseDirectory + " && mpiexec -n " + countThreads + " ConsoleApplication1.exe " + countUnit);
                            Console.ReadLine();
                    }
                    else
                    {
                            Stopwatch sWatch = new Stopwatch();
                            sWatch.Start();

                            Random rnd1 = new Random();

                            countUnit = Convert.ToInt32(args[0]);
                            int N = countUnit;

                            int[] A = new int[N];
                            int[] C = new int[N];
                            int[,] Nums = new int[2, countUnit];
                            
                            for (int i = 0; i < N; i++)
                            {
                                    A[i] = rnd1.Next(100, 10000000);
                                    C[i] = rnd1.Next(100, 10000000);
                            }
                            for (int j = 0; j < 100; j++)
                            {
                                Nums[0, j] = rnd1.Next(100, 10000000);
                                Nums[1, j] = rnd1.Next(100, 10000000);
                            }
                            
                            
                            using (new MPI.Environment(ref args))
                            {
                                Intracommunicator comm = Communicator.world;
                                int rank = comm.Rank; //текущий поток
                                int size = comm.Size; //кол-во потоков
                                
                                int h = countUnit / size;
                                int start;
                                int end;

                                if (rank == 0)
                                {
                                        for (int i = 1; i < size; i++)                          
                                        {
                                                start = h * i;
                                                end = start + h - 1;
                                                if (i == size - 1) end = countUnit - 1;

                                                comm.Send(end - start + 1, i, 1);
                                        }
                                        for (int i = 1; i < size; i++)                          
                                        {
                                                start = h * i;
                                                end = start + h - 1;
                                                if (i == size - 1) end = countUnit - 1;
                                           
                                                int[] array_part1 = new int[end - start + 1];
                                                int[] array_part2 = new int[end - start + 1];

                                                Array.Copy(A, start, array_part1, i, end - start + 1);
                                                Array.Copy(C, start, array_part2, i, end - start + 1);
                                                
                                                comm.Send(array_part1, i, 2);
                                                comm.Send(array_part2, i, 3);
                                        }
                                        
                                        int[] arr_part1 = new int[h];
                                        int[] arr_part2 = new int[h];
                                        Array.Copy(A, 0, arr_part1, 0, h);
                                        Array.Copy(C, 0, arr_part2, 0, h);
                                        
                                        
                                        double sum;
                                        
                                        
                                        Console.WriteLine("Задание № 10. Поиск суммы всех четных чисел массива");
                                        Console.WriteLine("Дана последовательность натуральных чисел {a0…an–1}. \nСоздать многопоточное приложение для поиска суммы ai, где ai – четные числа");
                                        sWatch.Reset();
                                        sWatch.Start();
                                        double rez10 = calculate(C);
                                        sWatch.Stop();
                                        Console.WriteLine("Один поток. Сумма всех четных чисел массива = " + rez10 + ". Время: " + sWatch.ElapsedMilliseconds.ToString() + " мс.");
                                        
                                        sWatch.Reset();
                                        sWatch.Start();
                                        rez10 = calculate(arr_part2);
                                        for (int i = 1; i < size; i++)
                                        {
                                            sum = 0;
                                            comm.Receive(i, 9, out sum);
                                            rez10 += sum;
                                        }
                                        sWatch.Stop();
                                        Console.WriteLine(countThreads + " потоков. Сумма всех четных чисел массива = " + rez10 + ". Время: " + sWatch.ElapsedMilliseconds.ToString() + " мс.\r\n\r\n");
                                }
                                else
                                {
                                        int N1 = 0;
                                        comm.Receive(0, 1, out N1);
                                        int[] array1 = new int[N1];
                                        comm.Receive(0, 2, ref array1);
                                        int[] array2 = new int[N1];
                                        comm.Receive(0, 3, ref array2);
                                        
                                        double sum;
                                        
                                    
                                        sum = calculate(array2);
                                        comm.Send(sum, 0, 9);
                                }
                            }
                            Console.ReadLine();
                    }
        }
    }
}
