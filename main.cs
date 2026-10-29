using System;
using System.IO;
using System.Text;
using System.Globalization;

public class Program
{
    public static string ReadLine()
    {
        string value=Console.ReadLine();
        if(value==null) throw new EndOfStreamException();
        return value;
    }
    public static int ReadInt(string name,int min,int max)
    {
        while(true)
        {
            Console.Write(name+" ("+min+".."+max+"): ");
            int value;
            if(int.TryParse(ReadLine(),out value) && value>=min && value<=max) return value;
            Console.WriteLine("Введите целое число в указанном диапазоне.");
        }
    }
    public static double ReadNumber(string name,double min,double max)
    {
        while(true)
        {
            Console.Write(name+" ("+min+".."+max+"): ");
            double value;
            if(double.TryParse(ReadLine().Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out value)
                && !double.IsNaN(value) && !double.IsInfinity(value) && value>=min && value<=max) return value;
            Console.WriteLine("Введите допустимое число. Разделитель: точка или запятая.");
        }
    }
    private static string ReadName(string prompt)
    {
        while(true) { Console.Write(prompt+": "); string s=ReadLine().Trim(); if(s.Length>0) return s; Console.WriteLine("Строка не должна быть пустой."); }
    }
    public static void Main()
    {
        Console.OutputEncoding=Encoding.UTF8;
        Console.InputEncoding=Encoding.UTF8;
        Random random=new Random();
        try
        {
            while(true)
            {
                Console.WriteLine("\nЛабораторная №3\n1 — Заполнение массивов\n2 — Максимальный уникальный элемент\n3 — 2*A - 3*B*C\n4 — Чётные числа в бинарном файле\n5 — Багаж\n6 — Разность сумм половин\n7 — Сумма элементов\n8 — Короткая и длинная строки\n0 — Выход");
                int task=ReadInt("Задание",0,8);
                if(task==0) return;
                Console.WriteLine("1 — Задача 1\n0 — Назад");
                if(ReadInt("Номер",0,1)==0) continue;
                try { Run(task,random); }
                catch(EndOfStreamException) { throw; }
                catch(Exception e) { Console.WriteLine("Не удалось выполнить задание: "+e.Message); }
            }
        }
        catch(EndOfStreamException) { Console.WriteLine("Ввод завершён."); }
    }
    private static void Run(int task,Random random)
    {
        if(task<=3)
        {
            int n=ReadInt("Число строк",1,20),m=ReadInt("Число столбцов",1,20);
            if(task==1)
            {
                Console.WriteLine("Первый массив: ввод по строкам справа налево.");
                Matrix first=new Matrix(n,m);
                Console.WriteLine(first);
                Console.WriteLine("Второй массив n x n, случайные значения:\n"+new Matrix(n,random));
                Console.WriteLine("Третий массив n x n, по образцу:\n"+new Matrix(n));
            }
            else if(task==2)
            {
                Matrix matrix=new Matrix(n,m); Console.WriteLine(matrix);
                double maximum;
                if(matrix.TryMaxUnique(out maximum)) Console.WriteLine("Максимальный неповторяющийся: "+maximum);
                else Console.WriteLine("Неповторяющихся элементов нет.");
            }
            else
            {
                int k=ReadInt("Общий размер B и C (B: n x k, C: k x m)",1,20);
                Console.WriteLine("Матрица A:"); Matrix a=new Matrix(n,m);
                Console.WriteLine("Матрица B:"); Matrix b=new Matrix(n,k);
                Console.WriteLine("Матрица C:"); Matrix c=new Matrix(k,m);
                Matrix result=2*a-3*b*c;
                Console.WriteLine("A:\n"+a+"B:\n"+b+"C:\n"+c+"2*A - 3*B*C:\n"+result);
            }
            return;
        }
        // Каждый запуск сохраняется отдельно, существующие пользовательские файлы не затираются.
        string folder=Path.Combine(Environment.CurrentDirectory,"lab3_data_"+DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")+"_"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        Console.WriteLine("Папка результатов: "+folder);
        string source=Path.Combine(folder,"source.txt"),target=Path.Combine(folder,"result.txt");
        if(task==4 || task==6 || task==7)
        {
            int count=ReadInt("Количество чисел",task==6?2:1,1000);
            while(task==6 && count%2!=0) { Console.WriteLine("Нужно чётное количество."); count=ReadInt("Количество",2,1000); }
            int min=ReadInt("Минимум",-1000000,1000000),max=ReadInt("Максимум",min,1000000);
            if(task==4)
            {
                source=Path.Combine(folder,"source.bin"); target=Path.Combine(folder,"even.bin");
                FileTasks.FillBinary(source,count,min,max,random);
                FileTasks.SelectEven(source,target);
                Console.WriteLine("Исходные числа: "+FileTasks.ShowBinary(source));
                Console.WriteLine("Чётные числа: "+FileTasks.ShowBinary(target));
            }
            else
            {
                int perLine=task==6?1:ReadInt("Чисел в строке",2,20);
                FileTasks.FillText(source,count,perLine,min,max,random);
                Console.WriteLine("Исходный файл:\n"+File.ReadAllText(source));
                if(task==6) Console.WriteLine("Сумма первой половины минус сумма второй: "+FileTasks.HalfDifference(source));
                else Console.WriteLine("Сумма: "+FileTasks.SumText(source));
            }
        }
        else if(task==5)
        {
            int count=ReadInt("Количество пассажиров",1,100);
            Passenger[] passengers=new Passenger[count];
            for(int i=0;i<count;i++)
            {
                string name=ReadName("Имя пассажира "+(i+1));
                BaggageItem[] items=new BaggageItem[ReadInt("Количество мест багажа",1,100)];
                for(int j=0;j<items.Length;j++) items[j]=new BaggageItem(ReadName("Название места "+(j+1)),ReadNumber("Масса, кг",0.001,10000));
                passengers[i]=new Passenger(name,items);
            }
            double tolerance=ReadNumber("Допустимое отклонение m, кг",0,10000);
            string path=Path.Combine(folder,"baggage.bin");
            FileTasks.FillBaggage(path,Path.Combine(folder,"baggage.xml"),passengers);
            Console.WriteLine(FileTasks.FindBaggage(path,tolerance));
        }
        else
        {
            int count=ReadInt("Количество строк",1,1000);
            string[] lines=new string[count];
            for(int i=0;i<count;i++) { Console.Write("Строка "+(i+1)+" (можно пустую): "); lines[i]=ReadLine(); }
            File.WriteAllLines(source,lines,Encoding.UTF8);
            FileTasks.ShortestLongest(source,target);
            Console.WriteLine("Результат: сначала кратчайшая, затем длиннейшая строка:\n"+File.ReadAllText(target));
        }
    }
}
