using System;
using System.Collections.Generic;
using System.Threading;

namespace Leson_11_dz_07_
{
   public class BarberShop
    {
        private readonly int _waitingRoomSeats;
        private Queue<string> _waitingCustomers;
        private Semaphore _barberSemaphore;
        private object _lock = new object();
        private bool _barberSleeping = true;

        // Счётчики обслуженных и ушедших клиентов
        private int _servedCustomers = 0;
        private int _leftCustomers = 0;

        public BarberShop(int waitingRoomSeats)
        {
            _waitingRoomSeats = waitingRoomSeats;
            _waitingCustomers = new Queue<string>();
            _barberSemaphore = new Semaphore(0, 1);
        }

        public void EnterShop(string customerName)
        {
            lock (_lock)
            {
                if (_waitingCustomers.Count < _waitingRoomSeats)
                {
                    Console.WriteLine($"{customerName} заходит в парикмахерскую и садится в зале ожидания.");
                    _waitingCustomers.Enqueue(customerName);

                    if (_barberSleeping)
                    {
                        _barberSleeping = false;
                        _barberSemaphore.Release(); // Будим парикмахера
                    }
                }
                else
                {
                    Console.WriteLine($"{customerName} уходит, так как нет свободных мест в зале ожидания.");
                    _leftCustomers++; // Увеличиваем счётчик ушедших клиентов
                }
            }
        }

        public void StartBarber()
        {
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        _barberSemaphore.WaitOne(); // Ожидаем, пока парикмахера разбудят

                        while (true)
                        {
                            string customer;
                            lock (_lock)
                            {
                                if (_waitingCustomers.Count > 0)
                                {
                                    customer = _waitingCustomers.Dequeue();
                                }
                                else
                                {
                                    _barberSleeping = true;
                                    Console.WriteLine("Парикмахер засыпает, так как нет клиентов.");
                                    break;
                                }
                            }

                            Console.WriteLine($"Парикмахер начинает стрижку для {customer}.");
                            Thread.Sleep(4000); // Имитация времени на стрижку
                            Console.WriteLine($"Парикмахер закончил стрижку для {customer}.");
                            _servedCustomers++; // Увеличиваем счётчик обслуженных клиентов
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Произошла ошибка: {ex.Message}");
                    }
                }
            }).Start();
        }

        public void PrintStatistics()
        {
            Console.WriteLine($"Всего обслужено клиентов: {_servedCustomers}");
            Console.WriteLine($"Всего ушло клиентов: {_leftCustomers}");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {

            var random_time = new Random();
            
            const int waitingRoomSeats = 3;
            const int number_of_clients = 10;


            BarberShop shop = new BarberShop(waitingRoomSeats);
            shop.StartBarber();

            // Имитируем приход клиентов
            for (int i = 1; i <= number_of_clients; i++)
            {
                string customerName = $"Клиент {i}";
                new Thread(() => shop.EnterShop(customerName)).Start();
                Thread.Sleep(random_time.Next(500, 2000)); // Задержка между приходами клиентов
            }

            // Ждём завершения обработки всех клиентов, после чего выводим статистику
            Thread.Sleep(15000);
            shop.PrintStatistics();
        }
    }
}
