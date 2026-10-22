using System;
using System.Globalization;
using System.Text;

namespace PharmacyApp
{
    internal class Program
    {
        const int Capacity = 100;

        // Паралельні масиви: індекс i в усіх описує один лікарський засіб
        static int[] ids = new int[Capacity];
        static string[] names = new string[Capacity];
        static string[] manufacturers = new string[Capacity];
        static string[] forms = new string[Capacity];
        static int[] quantities = new int[Capacity];
        static decimal[] prices = new decimal[Capacity];
        static DateTime[] expiryDates = new DateTime[Capacity];
        static bool[] needsPrescription = new bool[Capacity];

        static int count = 0;   // скільки комірок заповнено
        static int nextId = 1;  // наступний код

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Демонстраційні дані (дати рахуємо від сьогодні)
            Store("Парацетамол", "Дарниця", "таблетки", 120, 35.50m, DateTime.Today.AddDays(20), false);
            Store("Амоксицилін", "Фармак", "капсули", 40, 142m, DateTime.Today.AddMonths(14), true);
            Store("Аскорбінка", "КВЗ", "драже", 200, 18.75m, DateTime.Today.AddDays(-5), false);

            bool running = true;
            while (running)
            {
                Console.WriteLine();
                Console.WriteLine("=== Аптека ===");
                Console.WriteLine("1 — додати");
                Console.WriteLine("2 — показати всі");
                Console.WriteLine("3 — знайти за кодом");
                Console.WriteLine("4 — видалити за кодом");
                Console.WriteLine("0 — вихід");
                Console.Write("Ваш вибір: ");

                switch ((Console.ReadLine() ?? "").Trim())
                {
                    case "1": AddMedicine(); break;
                    case "2": PrintAll(); break;
                    case "3": FindById(); break;
                    case "4": DeleteById(); break;
                    case "0": running = false; break;
                    default: Console.WriteLine("Немає такого пункту меню."); break;
                }
            }
        }

        static void Store(string name, string manufacturer, string form,
                          int quantity, decimal price, DateTime expiry, bool rx)
        {
            ids[count] = nextId;
            names[count] = name;
            manufacturers[count] = manufacturer;
            forms[count] = form;
            quantities[count] = quantity;
            prices[count] = price;
            expiryDates[count] = expiry;
            needsPrescription[count] = rx;
            count++;
            nextId++;
        }

        // Індекс засобу за кодом або -1, якщо коду немає
        static int IndexOfId(int id)
        {
            for (int i = 0; i < count; i++)
            {
                if (ids[i] == id) return i;
            }
            return -1;
        }

        static void AddMedicine()
        {
            if (count >= Capacity)
            {
                Console.WriteLine("Сховище заповнене.");
                return;
            }

            Console.Write("Назва: ");
            string name = (Console.ReadLine() ?? "").Trim();
            Console.Write("Виробник: ");
            string manufacturer = (Console.ReadLine() ?? "").Trim();
            Console.Write("Форма випуску: ");
            string form = (Console.ReadLine() ?? "").Trim();
            if (name.Length == 0 || manufacturer.Length == 0 || form.Length == 0)
            {
                Console.WriteLine("Текстові поля не можуть бути порожніми. Не додано.");
                return;
            }

            Console.Write("Кількість: ");
            if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity < 0)
            {
                Console.WriteLine("Кількість має бути цілим невід'ємним числом. Не додано.");
                return;
            }

            Console.Write("Ціна, грн: ");
            string priceText = (Console.ReadLine() ?? "").Trim().Replace(',', '.');
            if (!decimal.TryParse(priceText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price)
                || price < 0)
            {
                Console.WriteLine("Ціна має бути невід'ємним числом. Не додано.");
                return;
            }

            Console.Write("Термін придатності (дд.мм.рррр): ");
            if (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expiry))
            {
                Console.WriteLine("Невірний формат дати. Не додано.");
                return;
            }

            Console.Write("За рецептом? (так/ні): ");
            string rxText = (Console.ReadLine() ?? "").Trim().ToLower();
            if (rxText != "так" && rxText != "ні")
            {
                Console.WriteLine("Потрібно \"так\" або \"ні\". Не додано.");
                return;
            }

            Store(name, manufacturer, form, quantity, price, expiry, rxText == "так");
            Console.WriteLine($"Додано з кодом {ids[count - 1]}.");
        }

        static void PrintAll()
        {
            if (count == 0)
            {
                Console.WriteLine("Записів немає.");
                return;
            }

            Console.WriteLine("Код | Назва        | Виробник  | Форма     | К-сть |   Ціна | Придатний до | Рецепт | Примітка");
            Console.WriteLine("----+--------------+-----------+-----------+-------+--------+--------------+--------+---------");

            decimal total = 0;
            int expiringSoon = 0;

            for (int i = 0; i < count; i++)
            {
                // скільки днів лишилось до кінця терміну
                int days = (expiryDates[i].Date - DateTime.Today).Days;
                string note = "";
                if (days < 0) note = "прострочено";
                else if (days <= 30) { note = $"спливає за {days} дн."; expiringSoon++; }

                Console.WriteLine($"{ids[i],3} | {names[i],-12} | {manufacturers[i],-9} | {forms[i],-9} | " +
                                  $"{quantities[i],5} | {prices[i],6:F2} | {expiryDates[i]:dd.MM.yyyy}   | " +
                                  $"{(needsPrescription[i] ? "так" : "ні"),-6} | {note}");

                total += prices[i] * quantities[i];
            }

            Console.WriteLine($"Усього позицій: {count}, вартість залишків: {total:F2} грн.");
            Console.WriteLine($"Спливає за 30 днів: {expiringSoon}.");
        }

        static void FindById()
        {
            Console.Write("Код: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Код має бути числом.");
                return;
            }

            int i = IndexOfId(id);
            if (i == -1)
            {
                Console.WriteLine("Такого коду немає.");
                return;
            }

            Console.WriteLine($"Позиція {i}: {names[i]}, {manufacturers[i]}, {forms[i]}");
            Console.WriteLine($"К-сть {quantities[i]}, ціна {prices[i]:F2} грн, до {expiryDates[i]:dd.MM.yyyy}, " +
                              $"за рецептом: {(needsPrescription[i] ? "так" : "ні")}");
        }

        static void DeleteById()
        {
            Console.Write("Код: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Код має бути числом.");
                return;
            }

            int index = IndexOfId(id);
            if (index == -1)
            {
                Console.WriteLine("Такого коду немає.");
                return;
            }

            // Зсув уліво в усіх восьми масивах
            for (int i = index; i < count - 1; i++)
            {
                ids[i] = ids[i + 1];
                names[i] = names[i + 1];
                manufacturers[i] = manufacturers[i + 1];
                forms[i] = forms[i + 1];
                quantities[i] = quantities[i + 1];
                prices[i] = prices[i + 1];
                expiryDates[i] = expiryDates[i + 1];
                needsPrescription[i] = needsPrescription[i + 1];
            }
            count--;

            // Очищаємо звільнену комірку
            ids[count] = 0;
            names[count] = "";
            manufacturers[count] = "";
            forms[count] = "";
            quantities[count] = 0;
            prices[count] = 0m;
            expiryDates[count] = default;
            needsPrescription[count] = false;

            Console.WriteLine("Видалено.");
        }
    }
}