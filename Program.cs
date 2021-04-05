using System;
using System.Threading;
using System.Collections.Generic;

namespace Karimova_pr3
{
  class Program
  {
    static bool check = true;

    static List<Int32> producer_goods = new List<Int32>();//  Числа производителей
    static List<Int32> consumer_goods = new List<Int32>();//  Числа потребителей (потреблённые числа производителей)
    static List<Int32> control = new List<Int32>();// Копия чисел производителей (для проверки)

    static object Producer_locker = new object();
    static object Consumer_locker = new object();
    static void Producer(object i) // Производитель
    { 
      bool sleep_producer = false;
      bool prod_lock;
      int number = (int)i, value;
      Random random = new Random();
      while (check) // если check = false, то поток заканчиваетс свою работу
      {
        if (producer_goods.Count >= 100) // Проверка кол-ва чисел производителей (если больше 100, то производитель спит)
        {
          Console.WriteLine($"Производитель {number + 1} уснул");
          sleep_producer = true;
          Thread.Sleep(1200);
          continue;
        }
        else
          if ((producer_goods.Count <= 80) && (sleep_producer)) // Проверка кол-ва чисел производителя (если меньше 80 и производитель спит, то произовдитель просыпается)
        {
          Console.WriteLine($"Производитель {number + 1} проснулся");
          sleep_producer = false;
        }
        value = random.Next(1, 100); // Генерируется некоторое число
        prod_lock = false;
        try // Блокировка потока
        {
          Monitor.Enter(Producer_locker, ref prod_lock);
          producer_goods.Add(value); // добавление некоторого числа в массив чисел производителей
          control.Add(value); // добавление числа в массив для проверка (вдруг элементы массива потребителей отличаются от массива производителей)
          Thread.Sleep(0);
        }
        finally
        {
          if (prod_lock) // Разблокировка потока
          {
            Monitor.Exit(Producer_locker);
          }
        }
        Console.WriteLine($"Производитель {number + 1} произвел число {value}");
      }
      Console.WriteLine($"Производитель {number + 1} закончил");
    }
    static void Consumer(object j)
    {
      int value, number = (int)j;
      bool consumer_sleep = false, cons_lock;

      while ((producer_goods.Count != 0) || (check))
      {
        if (producer_goods.Count == 0) // Если в массиве производителей нет чисел, то потребитель спит
        {
          Console.WriteLine($"Потребитель {number + 1} уснул");
          consumer_sleep = true;
          Thread.Sleep(500);
          continue;
        }
        else
          if (consumer_sleep) // потребитель просыпается
        {
          Console.WriteLine($"Потребитель {number + 1} проснулся");
          consumer_sleep = false;
        }
        cons_lock = false;
        try // Блокировка потока
        {
          Monitor.Enter(Consumer_locker, ref cons_lock);
          if (producer_goods.Count != 0)
          {
            value = producer_goods[0]; // Считывание некоторого числа из массива производителей в value
            producer_goods.RemoveAt(0); // Удаление этого элемента из массива производителей
            Thread.Sleep(0);
          }
          else
          {
            Thread.Sleep(0);
            continue;
          }
        }
        finally
        {
          if (cons_lock) // разблокировка потока
          {
            Monitor.Exit(Consumer_locker);
          }
        }
        consumer_goods.Add(value); // Добавление некоторого числа (value) в массив потребителей
        Console.WriteLine($"Потребитель {number + 1} получил число {value}");
      }
      Console.WriteLine($"Потребитель {number + 1} закончил");
    }

    static void Main()
    {
      char key;
      bool check_alive_threads = true;

      List<Thread> producers = new List<Thread>();
      List<Thread> consumers = new List<Thread>();

      for (int i = 0; i < 3; i++) // создание потоков производителей
      {
        producers.Add(new Thread(new ParameterizedThreadStart(Producer)));
        producers[i].Start(i);
      }
      for (int j = 0; j < 2; j++) // создание потоков потребителей
      {
        consumers.Add(new Thread(new ParameterizedThreadStart(Consumer)));
        consumers[j].Start(j);
      }
      while (check)//Конец работы при нажатии q
      {
        key = Console.ReadKey().KeyChar;
        if (key == 'q')
        {
          check = false;
        }
      }
      while (check_alive_threads)
      {
        if (!((producers[0].IsAlive)||(producers[1].IsAlive)||(producers[2].IsAlive)||(consumers[0].IsAlive)||(consumers[1].IsAlive)))
        {
          check_alive_threads = false;
          consumer_goods.Sort();
          control.Sort();
          bool correct = true;
          if (consumer_goods.Count == control.Count)//Проверка потреблённых чисел
          {
            for (int i = 0; i < consumer_goods.Count; i++)
              if (consumer_goods[i] != control[i])
              {
                Console.WriteLine("Разные элементы! Ошибка!");
                correct = false;
                break;
              }
            if (correct)
              Console.WriteLine("\nЗавершение!");
            else
              Console.WriteLine("Ошибка!");
          }
        }
      }
    }
  }
}
