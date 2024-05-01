/*
 * This class realizes automatic power supply controlling from a script file.
 * Script file format is .csv with semicolon (';') separator.
 * First value in a row of script file is time and the second is command to be sent to the
 * power supply as a string. Other columns, blank lines and lines with empty first and
 * second columns are ignored.
 * Format of time in the script file:
 *		ss.ff			seconds from the start of the script
 *		mm:ss.ff		minutes : seconds from the start of the script
 *		hh:mm:ss.ff		hours : minutes : seconds from the start of the script
 *		+ss.ff			seconds from the last command
 *		+mm:ss.ff		minutes : seconds from the start last command
 *		+hh:mm:ss.ff	hours : minutes : seconds from the start last command
 * Script entries (commands) must be ordered sequentially by the time of the execution.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

public class SkriptPrikaz
{
    public double TimeOffset;
    public string Command;

    public SkriptPrikaz(double timeOffset, string command)
    {
        TimeOffset = timeOffset;
        Command = command;
    }
}

public static class ZdrojSkript
{
    private static bool _initialized = false;
    //list with SCPI commands and time differences of particular command from the last command
    private static List<SkriptPrikaz> _scriptCommands = new List<SkriptPrikaz>();
    private static double _totalTime = 0.0;
    private static int _totalScriptCommands = 0;

    private static System.Timers.Timer commandTimer = new System.Timers.Timer();
    private static bool _scriptRunning = false;
    private static int _actualCommandIndex = 0;
    private static Zdroj _zdroj;

    //Total time of the script to be executed
    public static double TotalTime
    {
        get
        {
            return _totalTime;
        }
    }

    //script file has been parsed successfully and is ready to be started
    public static bool Initialized
    {
        get
        {
            return _initialized;
        }
    }

    //number of SCPI commands loaded from a script file
    public static int TotalScriptCommands
    {
        get
        {
            return _totalScriptCommands;
        }
    }

    //number of SCPI commands from a script already executed
    public static int ExecutedScriptCommands
    {
        get
        {
            return _actualCommandIndex + 1;
        }
    }

    //flag of running script
    public static bool ScriptRunning
    {
        get
        {
            return _scriptRunning;
        }
    }

    //Method loads script file
    public static void Init(string ScriptPath)
    {
        if (_scriptRunning)
        {
            throw new Exception("Some Script is already running! Abort script before new is loaded.");
        }

        _scriptCommands.Clear();
        _initialized = false;
        _totalTime = 0.0;

        StreamReader sr = File.OpenText(ScriptPath);
        string tmpStr = null;
        string[] tmpStrings;
        bool parseError = false;
        uint parsingLine = 1;
        double absTimeLast = 0.0;
        bool timeAbsoluteValue = true;              //time in script is absolute value from start of the script, not increment of time from tha last command
        while ((tmpStr = sr.ReadLine()) != null)
        {
            tmpStrings = tmpStr.Split(';');
            if (tmpStrings[0] == string.Empty)
            {
                if ((tmpStrings.Length > 1) && (tmpStrings[1] != string.Empty)) //command without defined time
                {
                    parseError = true;
                    break;
                }
            }
            else
            {                                       //line of script contains time
                                                    //check whether command is present (empty string is accepted too)
                if (tmpStrings.Length < 2)
                {
                    parseError = true;
                    break;
                }
                //check whether time starts with character '+' incicating that time is offset from the last command not an absolute value from the start of the script
                timeAbsoluteValue = true;
                if (tmpStrings[0].StartsWith("+"))
                {
                    timeAbsoluteValue = false;
                    tmpStrings[0] = tmpStrings[0].Substring(1);
                }
                //check characters - number possibly with decimal point or ':'
                foreach (char ch in tmpStrings[0])
                {
                    if (((ch < '0') || (ch > '9')) && (ch != ':') && (ch != '.'))   //time consist unsupported character
                    {
                        parseError = true;
                        break;
                    }
                }
                //time consists of valid characters
                string[] timeNumbers = tmpStrings[0].Split(':');    //spit to hours, minutes and seconds
                if (timeNumbers.Length > 3)                         //time format is longer than hh:mm:ss.ff
                {
                    parseError = true;
                    break;
                }
                double parsedTime = 0.0;
                //parse time from seconds through minutes to hours
                for (int i = timeNumbers.Length; i > 0; i--)
                {
                    parsedTime += double.Parse(timeNumbers[i - 1],
                        System.Globalization.NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture) * Math.Pow(60, timeNumbers.Length - i);
                }
                //convert time to time difference from tha last command
                if (timeAbsoluteValue)
                {
                    parsedTime -= absTimeLast;
                }
                //check if next command is subsequent in time
                if (parsedTime < 0)
                {
                    parseError = true;
                    break;
                }
                absTimeLast += parsedTime;
                SkriptPrikaz skrPrkz = new SkriptPrikaz(parsedTime, tmpStrings[1].Trim());
                _scriptCommands.Add(skrPrkz);
            }
            parsingLine++;
        }
        sr.Close();
        _totalTime = absTimeLast;
        _totalScriptCommands = _scriptCommands.Count;
        if (_totalScriptCommands == 0)
        {
            parseError = true;
        }

        if (parseError)
        {
            throw new Exception(string.Format("Parse error on line {0}. Script file not initialized!", parsingLine));
        }
        _initialized = true;
    }

    //Method starts loaded script
    public static void Start(Zdroj zdroj)
    {
        _zdroj = zdroj;
        if (!_initialized)
        {
            throw new Exception("Script for controlling power supply not initialized!");
        }
        _zdroj.ReadLogEnabled = true;
        _actualCommandIndex = -1;       //initialization value of index is -1
        commandTimer.Elapsed += OnTimedEvent;
        commandTimer.AutoReset = false;
        actualizeTimer();
    }

    //Method stops actually running script
    public static void Abort()
    {
        commandTimer.Stop();
        _scriptRunning = false;
        _zdroj.ReadLogEnabled = false;
    }

    //handler of Timer elapsed period
    private static void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        actualizeTimer();
    }

    //execute SCPI command, initialize and start timer for next SCPI command
    private static void actualizeTimer()
    {
        if (_actualCommandIndex >= 0)       //not execute command during timer initialization (_currentCommandIndex = -1)
        {
            _zdroj.SendCommand(_scriptCommands[_actualCommandIndex].Command);
        }

        _actualCommandIndex++;
        if (_actualCommandIndex > _scriptCommands.Count - 1)    //end of script
        {
            commandTimer.Stop();
            _scriptRunning = false;
            _zdroj.ReadLogEnabled = false;
            return;
        }
        if (_scriptCommands[_actualCommandIndex].TimeOffset > 0.0)  //if there is a time delay to a next SCPI command
        {
            commandTimer.Interval = _scriptCommands[_actualCommandIndex].TimeOffset * 1000;     //Interval is in milliseconds
            commandTimer.Start();
            _scriptRunning = true;
        }
        else
        {
            actualizeTimer();   //execute next SCPI command immediatelly if there is not a time delay
        }
    }
}