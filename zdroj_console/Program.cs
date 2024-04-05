
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
	}
}
