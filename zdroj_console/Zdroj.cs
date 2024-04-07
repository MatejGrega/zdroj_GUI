/*
 * This class realizes communication interface with power supply.
 * Power supply is found on an available serial ports of computer automatically
 * after calling method Connect(). Method Disconnect() should be called before
 * exiting the program. All parameters of power supply like current and
 * voltage limit, measured voltage and current, enabled output etc. are implemented
 * as properties. Actual value of parameter is checked each time a property is
 * accessed. An exception is thrown if getting value of property from power supply
 * fail or setting property value to power supply violates rules of safe operation.
 * Log file with all communication over serial port is created each time the
 * constructior is called and successfully finds power supply. Log file path is
 * AppData/Roaming/PowerSupplyLog/YYYY-MM-DD_HH-MM-SS.log, where YYYY-MM-DD is actual date
 * and HH-MM-SS is actual time.
 * 
 * List of properties:
 *		double						MeasCurrent		read only
 *		double						MeasPower		read only
 *		double						MeasVoltage		read only
 *		double						CurrentLimit
 *		double						VoltageLimit
 *		bool						DigMode			read only
 *		int							VoltSlew
 *		Zdroj.OverLimitProtection	OverLimProt
 *		bool						CalState		read only
 * List of public methods:
 *		bool	Connect()
 *		void	Disconnect()
 *		void	Reset()
 *		void	CalCurrent(double StartCurrent, double StopCurrent)
 *		void	CalVoltage(double StartVoltage, double StopVoltage)
 */

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

	private const double _minCurrLimit = 0.001;
	private const double _maxCurrLimit = 1.000;
	private const double _minVoltLimit = 0.01;
	private const double _maxVoltLimit = 30.00;
	private const int _minVoltSlew = 1;
	private const int _maxVoltSlew = 30000;

	private SerialPort _serPort = new SerialPort(); //serial port used for communication with power supply
	private string _logFilePath;					//file path to log
	private StreamWriter _strWr;					//stream writer for writing to log file

	public Zdroj()
	{
		//power supply is communicating via virtual serial port over USB physical layer
		//so serial port setting like baud rate, stop bits, etc. does not matter
		_serPort.ReadTimeout = 1500;    //timeout for reading from serial port - used after sending a command and reading answer
		_serPort.WriteTimeout = 1000;

		string filePath = Path.Combine(Environment.GetFolderPath(
			Environment.SpecialFolder.ApplicationData), "PowerSupplyLog");
		string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log";
		_logFilePath = Path.Combine(filePath, fileName);
		Directory.CreateDirectory(filePath);
		_strWr = File.CreateText(_logFilePath);
		_strWr.AutoFlush = true;
	}

	//Path to log file
	public string LogFilePath
	{
		get
		{
			return _logFilePath;
		}
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
			checkCal();
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
			checkCal();
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
			checkCal();
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

	//calibration is in progress
	public bool CalState
	{
		get
		{
			return getBoolAnswerCommand("SYST:CAL?");
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
				_strWr.WriteLine("Opening serial port:" + tmpPort);
				_serPort.PortName = tmpPort;
				_serPort.Open();
				_serPort.Write("*IDN?\n");              //send identification SCPI command
				_strWr.WriteLine(DateTime.Now.ToString("HH:mm:ss.ff") + " -> *IDN?");
				string tmpStr = _serPort.ReadLine();
				_strWr.WriteLine(DateTime.Now.ToString("HH:mm:ss.ff") + " <- " + tmpStr);
				if(tmpStr == "MATEJ GREGA,Power supply 0-30V 1A,1,1.0")	//compare with supposed answer
				{
					found = true;						//power supply was found
				}
			}
			catch(Exception ex)
			{
				_strWr.WriteLine("EXCEPTION THROWN: " + ex.Message);
			}
			finally
			{
				if (_serPort.IsOpen && found == false)
				{
					_strWr.WriteLine("Closing serial port: " + tmpPort);
					_serPort.Close();
				}
			}
			if (found)
			{
				_strWr.WriteLine("Serial port with power supply found: " + _serPort.PortName);
				_strWr.WriteLine();
				return true;
			}
		}
		_strWr.WriteLine("Serial port with power supply not found.");
		_strWr.WriteLine();
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

	//set power supply to defined state
	public void Reset()
	{
		sendCommand("*RST");		//reset power supply
		_serPort.ReadExisting();	//clear input buffer and serial stream
	}

	//initiate calibration of current in specified range
	public void CalCurrent(double StartCurrent, double StopCurrent)
	{
		if (getBoolAnswerCommand("SYST:CAL?"))
		{
			throw new Exception("Cannot initiate new calibration, because calibration is already in progress!");
		}
		if(StartCurrent < _minCurrLimit)
		{
			StartCurrent = _minCurrLimit;
		}
		if(StartCurrent > _maxCurrLimit)
		{
			StartCurrent = _maxCurrLimit;
		}
		if (StopCurrent < _minCurrLimit)
		{
			StopCurrent = _minCurrLimit;
		}
		if (StopCurrent > _maxCurrLimit)
		{
			StopCurrent = _maxCurrLimit;
		}
		if(StartCurrent > StopCurrent)
		{
			double tmpDouble = StartCurrent;
			StartCurrent = StopCurrent;
			StopCurrent = tmpDouble;
		}
		sendCommand(string.Format(CultureInfo.GetCultureInfo("en-US"),
			"SYST:CAL:CURR {0:0.000},{1:0.000}", StartCurrent, StopCurrent));
	}

	//initiate calibration of voltage in specified range
	public void CalVoltage(double StartVoltage, double StopVoltage)
	{
		if (getBoolAnswerCommand("SYST:CAL?"))
		{
			throw new Exception("Cannot initiate new calibration, because calibration is already in progress!");
		}
		if (StartVoltage < _minCurrLimit)
		{
			StartVoltage = _minCurrLimit;
		}
		if (StartVoltage > _maxCurrLimit)
		{
			StartVoltage = _maxCurrLimit;
		}
		if (StopVoltage < _minCurrLimit)
		{
			StopVoltage = _minCurrLimit;
		}
		if (StopVoltage > _maxCurrLimit)
		{
			StopVoltage = _maxCurrLimit;
		}
		if (StartVoltage > StopVoltage)
		{
			double tmpDouble = StartVoltage;
			StartVoltage = StopVoltage;
			StopVoltage = tmpDouble;
		}
		sendCommand(string.Format(CultureInfo.GetCultureInfo("en-US"),
			"SYST:CAL:VOLT {0:0.000}, {1:0.000}", StartVoltage, StopVoltage));
	}

	//send SCPI command to power supply
	private void sendCommand(string command)
	{
		try
		{
			_serPort.ReadExisting();            //clear input buffer - read all available data from input buffer and stream
			_serPort.Write(command + "\n");
			_strWr.WriteLine(DateTime.Now.ToString("HH:mm:ss.ff") + " -> " + command);
		}
		catch
		{
			throw new Exception("Power supply is not connected or does not answer correctly to the command \"" +
				command + "\"");
		}
	}

	//send command and parse answer as double precision floating point number
	private double getDoubleAnswerCommand(string command)
	{
		try
		{
			_serPort.ReadExisting();            //clear input buffer - read all available data from input buffer and stream
			sendCommand(command);
			string tmpStr = _serPort.ReadLine();
			_strWr.WriteLine(DateTime.Now.ToString("HH:mm:ss.ff") + " <- " + tmpStr);
			return double.Parse(tmpStr, NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		catch
		{
			throw new Exception("Power supply is not connected or does not answer correctly to the command \"" +
				command + "\"");
		}
	}

	//send command and parse answer as integer
	private int getIntAnswerCommand(string command)
	{
		try
		{
			_serPort.ReadExisting();            //clear input buffer - read all available data from input buffer and stream
			sendCommand(command);
			string tmpStr = _serPort.ReadLine();
			_strWr.WriteLine(DateTime.Now.ToString("HH:mm:ss.ff") + " <- " + tmpStr);
			return int.Parse(tmpStr, NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		catch
		{
			throw new Exception("Power supply is not connected or does not answer correctly to the command \"" +
				command + "\"");
		}
	}

	//send command and parse answer as boolean value
	private bool getBoolAnswerCommand(string command)
	{
		try
		{
			_serPort.ReadExisting();            //clear input buffer - read all available data from input buffer and stream
			sendCommand(command);
			string tmpStr = _serPort.ReadLine();
			_strWr.WriteLine(DateTime.Now.ToString("HH:mm:ss.ff") + " <- " + tmpStr);
			if (tmpStr == "0")
			{
				return false;
			}
			if(tmpStr == "1")
			{
				return true;
			}
			throw new Exception("Power supply incorrect answer to command \"" + command + "\"");
		}
		catch
		{
			throw new Exception("Power supply is not connected or does not answer correctly to the command \"" +
				command + "\"");
		}
	}

	//throw exception with correct message if power supply is analog controlled
	private void checkDigitalControl()
	{
		if (!getBoolAnswerCommand("SYST:MODE:DIG?"))
		{
			throw new Exception("Cannot set the value, because power supply is analog controlled." +
				"Change to digital control manually.");
		}
	}

	//throw exception if power supply calibration is in progress
	private void checkCal()
	{
		if (getBoolAnswerCommand("SYST:CAL?"))
		{
			throw new Exception("Cannot set the value, because power supply calibration is in progress!");
		}
	}
}
