using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Globalization;
using System.Linq.Expressions;

public class Zdroj
{
	public enum OverLimitProtection		//definition of supported protections - disable output on OVP or OCP
	{
		Disabled,						//output of power supply is not disabled on over limit event
		OCP,							//overcurrent protection
		OVP								//overvoltage protection
	};

	private SerialPort _serPort = new SerialPort(); //serial port used for communication with power supply
	private double _currLimit;						//current set on power supply
	private double _voltLimit;                       //voltage set on power supply
	private OverLimitProtection _ovrLimProt;        //OCP and OVP state
	private int _voltSlew;                          //slew rate of output voltage
	private bool _output;                           //whether is output of power supply enabled or disabled                       
	private bool _digMode;                          //whether digital or analog colntroll is selected on power supply
													//(manual swith - not possible to change by software)

	//returns actual measured output current
	public double MeasCurrent
	{
		get
		{
			return getDoubleAnswerCommand("MEAS:CURR?");
		}
	}

	//returns actual measured output power
	public double MeasPower
	{
		get
		{
			return getDoubleAnswerCommand("MEAS:POW?");
		}
	}

	//returns actual measured output voltage
	public double MeasVoltage
	{
		get
		{
			return getDoubleAnswerCommand("MEAS:VOLT?");
		}
	}

	public Zdroj()
	{
		//power supply is communicating via virtual serial port over USB physical layer
		//so serial port setting like baud rate, stop bits, etc. does not matter
		_serPort.ReadTimeout = 5000;    //timeout for reading from serial port - used after sending a command and reading answer
		//Connect();
	}

	//return true if power supply was found on serial port and is successfully connected else return false
	//serial port with power supply remains opened
	public bool Connect()
	{
		bool found = false;
		string[] tmpPorts = SerialPort.GetPortNames();	//get names of available serial ports
		foreach(string tmpPort in tmpPorts)				//test if power supply is connected to the one of available serail ports
		{
			try
			{
				_serPort.PortName = tmpPort;
				_serPort.Open();
				_serPort.Write("*IDN?\n");				//send identification SCPI command
				if(_serPort.ReadLine() == "MATEJ GREGA,Power supply 0-30V 1A,1,1.0")	//compare with supposed answer
				{
					found = true;						//power supply was found
				}
			}
			catch{}		//catch exceptions so they do not propagate upwards, but they are not used (empty catch{})
			finally
			{
				if (_serPort.IsOpen && found == false)
				{
					_serPort.Close();
				}
			}
		}
		return false;	//power supply was not found
	}

	//Disconnect power supply and close serial port
	public void Disconnect()
	{
		if (_serPort.IsOpen)
		{
			_serPort.Close();
		}
	}

	private double getDoubleAnswerCommand(string command)
	{
		try
		{
			_serPort.Write(command + "\n");
			return double.Parse(_serPort.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		catch
		{
			throw new Exception("Power supply is not connected or does not answer correctly!");
		}
	}
	
}
