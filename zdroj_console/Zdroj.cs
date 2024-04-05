using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

public class Zdroj
{
	public void test()
    {
		SerialPort _serPortMultimeter = new SerialPort();                      //konfigurácia sériového portu multimetra

		_serPortMultimeter.PortName = "COM12";
		_serPortMultimeter.BaudRate = 115200;
		_serPortMultimeter.Parity = Parity.None;
		_serPortMultimeter.DataBits = 8;
		_serPortMultimeter.StopBits = StopBits.One;
		_serPortMultimeter.Handshake = Handshake.None;
		_serPortMultimeter.ReadTimeout = 5000;
		_serPortMultimeter.WriteTimeout = 5000;


		Console.WriteLine("\nOtvorenie sériových portov.");

		try
		{
			_serPortMultimeter.Open();                  //otvorenie portov

			Console.WriteLine("Èakanie 500ms.");

			Thread.Sleep(500);
			_serPortMultimeter.WriteLine(":MEAS:VOLT?");
			Console.WriteLine(_serPortMultimeter.ReadLine());
		}
		catch
		{

		}
		finally
		{
			Console.WriteLine("Zatváranie sériových portov.");

			_serPortMultimeter.Close();
		}

		Console.WriteLine("\nHOTOVO\n\nStlaète ¾ubovo¾ný kláves pre ukonèenie.");

		Console.ReadKey();
	}
    
}
