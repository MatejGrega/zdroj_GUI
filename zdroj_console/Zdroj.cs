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

	const double _minCurrLimit = 0.001;
	const double _maxCurrLimit = 1.000;
	const double _minVoltLimit = 0.01;
	const double _maxVoltLimit = 30.00;
	const int _minVoltSlew = 1;
	const int _maxVoltSlew = 30000;

	private SerialPort _serPort = new SerialPort(); //serial port used for communication with power supply

	public Zdroj()
	{
		//power supply is communicating via virtual serial port over USB physical layer
		//so serial port setting like baud rate, stop bits, etc. does not matter
		_serPort.ReadTimeout = 5000;    //timeout for reading from serial port - used after sending a command and reading answer
										//Connect();
	}

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

	//current set on power supply
	public double CurrentLimit
	{
		get
		{
			return getDoubleAnswerCommand("CURR?");
		}
		set
		{
			checkDigitalControl();
			if (value > _maxCurrLimit)
			{
				throw new Exception("New value of current exceeds maximum. New value was not set.");
			}
			if(value < _minCurrLimit)
			{
				sendCommand("CURR MIN");
				return;
			}
			sendCommand(string.Format("CURR " + value.ToString("0.000", CultureInfo.GetCultureInfo("en-US"))));
		}
	}

	//voltage set on power supply
	public double VoltageLimit
	{
		get
		{
			return getDoubleAnswerCommand("VOLT?");
		}
		set
		{
			checkDigitalControl();
			if (value > _maxVoltLimit)
			{
				throw new Exception("New value of current exceeds maximum. New value was not set.");
			}
			if (value < _minVoltLimit)
			{
				sendCommand("VOLT MIN");
				return;
			}
			sendCommand(string.Format("VOLT " + value.ToString("0.00", CultureInfo.GetCultureInfo("en-US"))));
		}
	}

	//whether digital or analog colntroll is selected on power supply
	//(manual swith - not possible to change by software)
	public bool DigMode
	{
		get
		{
			return getBoolAnswerCommand("SYST:MODE:DIG?");
		}
	}

	//whether is output of power supply enabled or disabled
	public bool Output
	{
		get
		{
			return getBoolAnswerCommand("OUTP?");
		}
		set
		{
			sendCommand(value ? "OUTP 1" : "OUTP 0");
		}
	}

	//slew rate of output voltage
	public int VoltSlew
	{
		get
		{
			return getIntAnswerCommand("VOLT:SLEW?");
		}
		set
		{
			int tmpSlewRate = value;
			if(tmpSlewRate < _minVoltSlew)
			{
				tmpSlewRate = _minVoltSlew;
			}
			if(tmpSlewRate > _maxVoltSlew)
			{
				tmpSlewRate = _maxVoltSlew;
			}
			sendCommand("VOLT:SLEW " + tmpSlewRate);
		}
	}

	//OCP and OVP state
	public OverLimitProtection OverLimProt
	{
		get
		{
			bool tmpOCP = getBoolAnswerCommand("CURR:PROT:STAT?");
			bool tmpOVP = getBoolAnswerCommand("VOLT:PROT:STAT?");
			if(tmpOCP)
			{
				return OverLimitProtection.OCP;
			}
			if(tmpOVP)
			{
				return OverLimitProtection.OVP;
			}
			return OverLimitProtection.Disabled;
		}
		set
		{
			bool tmpOVP = false;
			bool tmpOCP = false;
			if(value == OverLimitProtection.OCP)
			{
				tmpOCP = true;
			}
			if(value == OverLimitProtection.OVP)
			{
				tmpOVP = true;
			}
			sendCommand("VOLT:PROT:STAT 0");
			sendCommand("CURR:PROT:STAT 0");
			sendCommand(tmpOVP ? "VOLT:PROT:STAT 1" : "VOLT:PROT:STAT 0");
			sendCommand(tmpOCP ? "CURR:PROT:STAT 1" : "CURR:PROT:STAT 0");
		}
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
			if (found)
			{
				return true;
			}
		}
		return false;	//power supply was not found
	}

	//Disconnect power supply, disable it's output and close serial port
	public void Disconnect()
	{
		if (_serPort.IsOpen)
		{
			try
			{
				sendCommand("OUTP 0");		//disable output of power supply if possible
			}
			catch { }						//ignore exceptions
			_serPort.Close();
		}
	}

	public void Reset()
	{
		sendCommand("*RST");
		_serPort.ReadExisting();
	}

	//send SCPI command to power supply
	private void sendCommand(string command)
	{
		try
		{
			_serPort.ReadExisting();            //clear input buffer - read all available data from input buffer and stream
			_serPort.Write(command + "\n");
		}
		catch
		{
			throw new Exception("Power supply is not connected or does not answer correctly to the command \"" + command + "\"");
		}
	}

	//send command and parse answer as double precision floating point number
	private double getDoubleAnswerCommand(string command)
	{
		try
		{
			_serPort.ReadExisting();            //clear input buffer - read all available data from input buffer and stream
			sendCommand(command);
			return double.Parse(_serPort.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		catch
		{
			throw new Exception("Power supply is not connected or does not answer correctly to the command \"" + command + "\"");
		}
	}

	//send command and parse answer as integer
	private int getIntAnswerCommand(string command)
	{
		try
		{
			_serPort.ReadExisting();            //clear input buffer - read all available data from input buffer and stream
			sendCommand(command);
			return int.Parse(_serPort.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		catch
		{
			throw new Exception("Power supply is not connected or does not answer correctly to the command \"" + command + "\"");
		}
	}

	//send command and parse answer as boolean value
	private bool getBoolAnswerCommand(string command)
	{
		try
		{
			_serPort.ReadExisting();            //clear input buffer - read all available data from input buffer and stream
			sendCommand(command);
			string answr = _serPort.ReadLine();
			if(answr == "0")
			{
				return false;
			}
			if(answr == "1")
			{
				return true;
			}
			throw new Exception("Power supply incorrect answer to command \"" + command + "\"");
		}
		catch
		{
			throw new Exception("Power supply is not connected or does not answer correctly to the command \"" + command + "\"");
		}
	}

	//throw exception with correct message if power supply is analog controlled
	private void checkDigitalControl()
	{
		if (!getBoolAnswerCommand("SYST:MODE:DIG?"))
		{
			throw new Exception("Cannot set the value, because power supply is analog controlled. Change to digital control manually.");
		}
	}

}
