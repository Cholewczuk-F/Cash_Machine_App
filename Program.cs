using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zadanie_3
{
    class Bankomat
    {
        int[] nominaly =  { 10, 20, 50, 100, 200, 500 };
        int[] ilosc = new int[6];
        Klient[] konto = new Klient[] { new Klient("Waldy", 1234, 12200), new Klient("Filip", 1234, 440), new Klient("El Cretino", 1234, 560) };

        public Bankomat()
        {
            Random losowanie = new Random();
            for(int i = 0; i < 6; i++)
            {
                ilosc[i] = losowanie.Next(2, 8);
            }    
        }

        public void login(uint clientId)
        {
            uint tries = 0; uint pin = 0;
            if(clientId < konto.Length && !konto[clientId].isAccoutLocked())
            {
                do
                {
                    Console.Write("Podaj Pin: ");
                    pin = inputUint();
                    if (konto[clientId].checkPin(pin))
                    {
                        Console.WriteLine("Zalogowano pomyślnie!\n");
                        userInterface(clientId);    
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Pin nieprawidłowy.\n");
                        tries++;
                    }
                } while (tries < 3);
                if(tries == 3)
                {
                    konto[clientId].lockAccout();
                    Console.WriteLine("Konto zostało zablokowane. Prosimy zgłosić się do najbliższego oddziału.\n");
                }
            }else if(clientId >= konto.Length)
            {
                Console.WriteLine("Podane konto nie istnieje.\n");
            }
            else if(konto[clientId].isAccoutLocked())
            {
                Console.WriteLine("Konto zostało zablokowane. Prosimy zgłosić się do najbliższego oddziału.\n");
            }
        }

        void displayState()
        {
            for(int i = 5; i >= 0; i--)
            {
                Console.Write("{0} Ilość: {1},   ", nominaly[i], ilosc[i]);
            }
            Console.WriteLine();
        }

        private void deposit(uint clientId)
        {
            uint deposit = 0; uint sum = 0;
            Console.Write("Bankomat akceptuje nominały [10, 20, 50, 100, 200, 500].\n");
            while(deposit == 0)
            {
                Console.WriteLine("Ile chciałbyś wpłacić: ");
                deposit = inputUint();

                if(isDepositable(deposit))
                {
                    sum = deposit;
                    displayState();
                    for(int i = 5; i >= 0; i--)
                    {
                        ilosc[i] += (int)(sum / nominaly[i]);
                        sum -= (uint)(sum / nominaly[i] * nominaly[i]);
                    }
                    displayState();

                    konto[clientId].deposit(deposit);
                    Console.WriteLine("Wpłacono pomyślnie.");
                }else
                {
                    Console.WriteLine("Kwota nie może zostać zdeponowana w obsługiwanych nominałach. Podaj poprawną.");
                    deposit = 0;
                }
            }
            Console.WriteLine();
            
        }

        private bool isDepositable(uint value)
        {
            if(value % 10 == 0)
            {
                return true;
            }else
            {
                return false;
            }
        }

        private uint inputUint()
        {
            uint value = 0;
            try
            {
                value = uint.Parse(Console.ReadLine());
            }catch(ArgumentNullException)
            {
                Console.WriteLine("Błąd: Nie wprowadzono wartości.");
            }catch(FormatException)
            {
                Console.WriteLine("Błąd: Nieprawidłowy format.");
            }catch(OverflowException)
            {
                Console.WriteLine("Błąd: Nieprawidłowa wartość.");
            }
            return value;
        }

        private void withdraw(uint clientId)
        {
            uint amount = 0;

            if (konto[clientId].showSum() > 0)
            {
                uint sumOfNotes = 0;
                do
                {
                    Console.WriteLine("Podaj sumę do wypłaty: ");
                    amount = inputUint();
                    if(!isDepositable(amount))
                    {
                        Console.WriteLine("Nie da rady wypłacić kwoty z obsługiwanych nominałach.");
                    }else if(amount == 0)
                    {
                        return;
                    }
                } while (!isDepositable(amount));

                for(int i = 0; i < 6; i++)
                {
                    sumOfNotes += (uint)(nominaly[i] * ilosc[i]);
                }
                if(sumOfNotes < amount)
                {
                    Console.WriteLine("Bankomat nie posiada środków aby wypłacić podaną sumę. Maksymalnie można wypłacić: {0}", sumOfNotes);
                    displayState();
                    Console.WriteLine();
                    return;
                }

                uint[] zestaw = { 0, 0, 0, 0, 0, 0 };
                uint roznicaZestawu = amount;

                displayState();
                for(int i = 5;  i >= 0; i--)
                {
                    uint notesNeeded = (uint)(roznicaZestawu / nominaly[i]);
                    if(notesNeeded >= ilosc[i])
                    {
                        zestaw[i] = (uint)ilosc[i];
                        ilosc[i] = 0;
                        roznicaZestawu -= (uint)(zestaw[i] * nominaly[i]);
                    }else if(notesNeeded < ilosc[i])
                    {
                        ilosc[i] -= (int)notesNeeded;
                        zestaw[i] += notesNeeded;
                        roznicaZestawu -= (uint)(notesNeeded * nominaly[i]);
                    }

                    if (zestaw[i] > 0)
                    {
                        Console.WriteLine("Nominal: {0}     Wypłacono: {1}     Zostało: {2}     Do wypłacenia: {3}", nominaly[i], zestaw[i], ilosc[i], roznicaZestawu);
                    }
                }
                displayState();

            }else
            {
                Console.WriteLine("Brak środków na koncie.");
            }
            
        }

        private void showSum(uint clientId)
        {
            Console.WriteLine("Stan konta wynosi: {0}$\n", konto[clientId].showSum());
        }

        private void changePin(uint clientId)
        {
            uint pin = 0;
            do
            {
                Console.Write("Podaj nowy numer pin[4-6 cyfr]: ");
                pin = inputUint();
            } while (pin <= 999 || pin >= 1000000);
            konto[clientId].setPin(pin);
            Console.WriteLine("Pin zmieniono pomyślnie.\n");
        }


        private void userInterface(uint clientId)
        {
            uint opcja = 0;
            while(opcja != 99)
            {
                Console.Write("1 - Wpłata\n2 - Wypłata\n3 - Stan konta\n4 - Zmień pin\n5 - Wyloguj\nOpcja: ");
                opcja = inputUint();

                switch(opcja)
                {
                    case 1:
                        {
                            deposit(clientId);
                            break;
                        }
                    case 2:
                        {
                            withdraw(clientId);
                            break;
                        }
                    case 3:
                        {
                            showSum(clientId);
                            break;
                        }
                    case 4:
                        {
                            changePin(clientId);
                            break;
                        }
                    case 5:
                        {
                            return;
                        }
                    default:
                        {
                            Console.WriteLine("Nieprawidłowa opcja.\n");
                            break;
                        }
                }
            }
        }

    }

    class Klient
    {
        private string name;
        private uint pin;
        private uint sum;
        private bool locked = false;
        public Klient(string name, uint pin, uint sum)
        {
            this.name = name;
            this.pin = pin;
            this.sum = sum;
        }

        public bool checkPin(uint inputPin)
        {
            if(this.pin == inputPin)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public uint showSum()
        {
            return sum;
        }

        public string getName()
        {
            return name;
        }

        public void setPin(uint nowyPin)
        {
            this.pin = nowyPin;
            Console.WriteLine("Nowy pin: {0}\n", this.pin);
        }

        public void lockAccout()
        {
            locked = true;
        }

        public bool isAccoutLocked()
        {
            return locked;
        }

        public void deposit(uint value)
        {
            sum += value;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Bankomat elektronicznySzmaciarz = new Bankomat();
            uint clientId = 10;
            while(clientId != 3)
            {
                Console.Write("Wybierz 4 aby zakończyć.\nNumer klienta [1-3]: ");

                clientId = inputUint() - 1;
                if(clientId != 3)
                {
                    elektronicznySzmaciarz.login(clientId);
                }

                Console.WriteLine("");
            }
        }

        private static uint inputUint()
        {
            uint value = 0;
            try
            {
                value = uint.Parse(Console.ReadLine());
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Błąd: Nie wprowadzono wartości.");
            }
            catch (FormatException)
            {
                Console.WriteLine("Błąd: Nieprawidłowy format.");
            }
            catch (OverflowException)
            {
                Console.WriteLine("Błąd: Nieprawidłowa wartość.");
            }
            return value;
        }
    }
}
