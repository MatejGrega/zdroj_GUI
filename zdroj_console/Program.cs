
using System.Globalization;

public class Program
{
	static void Main(string[] args)
	{

        Zdroj zdroj = new Zdroj();
		if (zdroj.Connect())
		{
            Console.WriteLine("Uspesne pripojene");
        }
		else
		{
            Console.WriteLine("Zdroj nebol najdeny");
        }
        Console.WriteLine("Napatie = " + zdroj.MeasVoltage);
		Console.WriteLine("Prud = " + zdroj.MeasCurrent);
		Console.WriteLine("Vykon = " + zdroj.MeasPower);
		zdroj.Output = true;
        Console.WriteLine(zdroj.OverLimProt);
        Thread.Sleep(5000);
		zdroj.VoltSlew = 1;
		zdroj.VoltageLimit = 2.75;
		Console.WriteLine(zdroj.OverLimProt);
		Thread.Sleep(6000);
		zdroj.VoltSlew = 70;
		zdroj.VoltageLimit = 0.001;
		Console.WriteLine(zdroj.OverLimProt);
		Thread.Sleep(2000);
		zdroj.VoltageLimit = 7.123;
		Thread.Sleep(5000);
		Console.WriteLine("Nast napatie = " + zdroj.VoltageLimit);
		Console.WriteLine("Nast prud = " + zdroj.CurrentLimit);
		zdroj.Reset();
		Thread.Sleep(2000);

		zdroj.Disconnect();
	}
}
