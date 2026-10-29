using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Xml.Serialization;

public class Matrix
{
    private double[,] data; // Единственное поле класса.
    public int Rows { get { return data.GetLength(0); } }
    public int Columns { get { return data.GetLength(1); } }
    public Matrix(int n, int m)
    {
        if (n < 1 || m < 1) throw new ArgumentException("Размеры должны быть положительными.");
        data = new double[n, m];
        for (int i = 0; i < n; i++)
            for (int j = m - 1; j >= 0; j--)
                data[i,j] = Program.ReadNumber("Элемент [" + (i+1) + "," + (j+1) + "]", -10000, 10000);
    }
    public Matrix(int n, Random random)
    {
        if (n < 1 || random == null) throw new ArgumentException("Неверные параметры.");
        data = new double[n,n];
        for (int i=0;i<n;i++) for (int j=0;j<n;j++)
            data[i,j] = i>j ? random.Next(-17,37) : random.Next(100,10001);
    }
    public Matrix(int n)
    {
        if (n < 1) throw new ArgumentException("Размер должен быть положительным.");
        data = new double[n,n];
        int value=1;
        for (int offset=n-1;offset>=0;offset--)
            for (int j=0;j<n-offset;j++) data[j+offset,j]=value++;
    }
    private Matrix(double[,] values) { data=values; }
    public bool TryMaxUnique(out double maximum)
    {
        bool found=false; maximum=0;
        for (int i=0;i<Rows;i++) for (int j=0;j<Columns;j++)
        {
            int count=0;
            for (int r=0;r<Rows;r++) for (int c=0;c<Columns;c++)
                if (data[r,c]==data[i,j]) count++;
            if (count==1 && (!found || data[i,j]>maximum)) { maximum=data[i,j]; found=true; }
        }
        return found;
    }
    public static Matrix operator *(double factor, Matrix a)
    {
        double[,] result=new double[a.Rows,a.Columns];
        for(int i=0;i<a.Rows;i++) for(int j=0;j<a.Columns;j++) result[i,j]=factor*a.data[i,j];
        return new Matrix(result);
    }
    public static Matrix operator *(Matrix a, Matrix b)
    {
        if(a.Columns!=b.Rows) throw new ArgumentException("Несовместимые размеры для умножения.");
        double[,] result=new double[a.Rows,b.Columns];
        for(int i=0;i<a.Rows;i++) for(int j=0;j<b.Columns;j++)
            for(int k=0;k<a.Columns;k++) result[i,j]+=a.data[i,k]*b.data[k,j];
        return new Matrix(result);
    }
    public static Matrix operator -(Matrix a, Matrix b)
    {
        if(a.Rows!=b.Rows || a.Columns!=b.Columns) throw new ArgumentException("Размеры должны совпадать.");
        double[,] result=new double[a.Rows,a.Columns];
        for(int i=0;i<a.Rows;i++) for(int j=0;j<a.Columns;j++) result[i,j]=a.data[i,j]-b.data[i,j];
        return new Matrix(result);
    }
    public override string ToString()
    {
        StringBuilder text=new StringBuilder();
        for(int i=0;i<Rows;i++)
        {
            for(int j=0;j<Columns;j++) text.Append(data[i,j].ToString("G10",CultureInfo.InvariantCulture).PadLeft(18));
            text.AppendLine();
        }
        return text.ToString();
    }
}

public struct BaggageItem
{
    public string Name;
    public double Weight;
    public BaggageItem(string name,double weight) { Name=name; Weight=weight; }
}
public struct Passenger
{
    public string Name;
    public BaggageItem[] Items;
    public Passenger(string name,BaggageItem[] items) { Name=name; Items=items; }
}

public static class FileTasks
{
    public static void FillBinary(string path,int count,int min,int max,Random random)
    {
        using(BinaryWriter writer=new BinaryWriter(File.Create(path)))
            for(int i=0;i<count;i++) writer.Write(random.Next(min,max+1));
    }
    public static void SelectEven(string source,string target)
    {
        using(BinaryReader reader=new BinaryReader(File.OpenRead(source)))
        using(BinaryWriter writer=new BinaryWriter(File.Create(target)))
            while(reader.BaseStream.Position<reader.BaseStream.Length)
            { int value=reader.ReadInt32(); if(value%2==0) writer.Write(value); }
    }
    public static string ShowBinary(string path)
    {
        StringBuilder text=new StringBuilder();
        using(BinaryReader reader=new BinaryReader(File.OpenRead(path)))
            while(reader.BaseStream.Position<reader.BaseStream.Length) text.Append(reader.ReadInt32()+" ");
        return text.Length==0 ? "(пустой файл)" : text.ToString();
    }
    public static void FillBaggage(string binaryPath,string xmlPath,Passenger[] passengers)
    {
        XmlSerializer serializer=new XmlSerializer(typeof(Passenger[]));
        using(MemoryStream buffer=new MemoryStream())
        {
            serializer.Serialize(buffer,passengers);
            byte[] bytes=buffer.ToArray();
            File.WriteAllBytes(xmlPath,bytes);
            using(BinaryWriter writer=new BinaryWriter(File.Create(binaryPath)))
            { writer.Write(bytes.Length); writer.Write(bytes); }
        }
    }
    public static Passenger[] ReadBaggage(string path)
    {
        using(BinaryReader reader=new BinaryReader(File.OpenRead(path)))
        {
            int length=reader.ReadInt32();
            if(length<0 || length!=reader.BaseStream.Length-reader.BaseStream.Position)
                throw new InvalidDataException("Повреждён файл багажа.");
            using(MemoryStream buffer=new MemoryStream(reader.ReadBytes(length)))
                return (Passenger[])new XmlSerializer(typeof(Passenger[])).Deserialize(buffer);
        }
    }
    public static string FindBaggage(string path,double tolerance)
    {
        Passenger[] passengers=ReadBaggage(path);
        double total=0; int count=0;
        for(int i=0;i<passengers.Length;i++) for(int j=0;j<passengers[i].Items.Length;j++)
        { total+=passengers[i].Items[j].Weight; count++; }
        if(count==0) return "В файле нет единиц багажа.";
        double average=total/count;
        StringBuilder text=new StringBuilder("Общая средняя масса единицы: "+average+" кг\n");
        int matches=0;
        for(int i=0;i<passengers.Length;i++)
        {
            if(passengers[i].Items.Length==0) continue;
            double sum=0;
            for(int j=0;j<passengers[i].Items.Length;j++) sum+=passengers[i].Items[j].Weight;
            double local=sum/passengers[i].Items.Length;
            if(Math.Abs(local-average)<=tolerance)
            { text.AppendLine(passengers[i].Name+": средняя масса "+local+" кг"); matches++; }
        }
        if(matches==0) text.AppendLine("Подходящего багажа нет.");
        return text.ToString();
    }
    public static void FillText(string path,int count,int perLine,int min,int max,Random random)
    {
        using(StreamWriter writer=new StreamWriter(path,false,Encoding.UTF8))
            for(int i=0;i<count;i++)
            {
                writer.Write(random.Next(min,max+1));
                if((i+1)%perLine==0 || i==count-1) writer.WriteLine(); else writer.Write(" ");
            }
    }
    public static long HalfDifference(string path)
    {
        string[] lines=File.ReadAllLines(path);
        if(lines.Length%2!=0) throw new InvalidDataException("Количество элементов должно быть чётным.");
        long first=0,second=0;
        for(int i=0;i<lines.Length;i++)
        {
            int value=int.Parse(lines[i]);
            if(i<lines.Length/2) first+=value; else second+=value;
        }
        return first-second;
    }
    public static long SumText(string path)
    {
        long sum=0;
        using(StreamReader reader=new StreamReader(path))
        {
            string line;
            while((line=reader.ReadLine())!=null)
            {
                string[] parts=line.Split(new char[]{' ','\t'},StringSplitOptions.RemoveEmptyEntries);
                for(int i=0;i<parts.Length;i++) sum+=int.Parse(parts[i]);
            }
        }
        return sum;
    }
    public static void ShortestLongest(string source,string target)
    {
        using(StreamReader reader=new StreamReader(source))
        {
            string shortest=reader.ReadLine();
            if(shortest==null) throw new InvalidDataException("Исходный файл пуст.");
            string longest=shortest,line;
            while((line=reader.ReadLine())!=null)
            {
                if(line.Length<shortest.Length) shortest=line;
                if(line.Length>longest.Length) longest=line;
            }
            File.WriteAllLines(target,new string[]{shortest,longest},Encoding.UTF8);
        }
    }
}
