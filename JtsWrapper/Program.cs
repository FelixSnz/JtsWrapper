using NLog;
using System;
using System.Linq;
using Ted.Sdk.Tracking;
using System.Drawing;
using GuiTools.Popups;
using JtsWrapper.Configuration;
using JtsWrapper.Models;

namespace JtsWrapper
{
    internal class Program
    {
        public static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        static JtsIpcFile uuidFile = new JtsIpcFile("jts_temp_received_uuid.txt");
        static readonly JtsIpcFile statusFile = new JtsIpcFile("jts_init_response_status.txt");
        static readonly JtsIpcFile resultFile = new JtsIpcFile("jts_status_to_send.txt");


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application.</param>
        static void Main(string[] args)
        {
            Logger.Info("Executing JtsWrapper...");
            Logger.Info($"Simulation Mode: {(Process.SimulationOn ? "on" : "off")}");
            try
            {
                string recv_args = string.Join(", ", args);
                Logger.Debug($"received args: {recv_args}");
                string command = args[0];
                string[] commandArgs = new string[args.Length - 1];
                Array.Copy(args, 1, commandArgs, 0, args.Length - 1);
                Logger.Info($"received command: {command}");
                switch (command.ToLower())
                {
                    case "--initialize":
                        InitializeProcessCaller(commandArgs);
                        break;
                    case "--set-output":
                        SetOutputCaller(commandArgs);
                        break;
                    case "--display-msg":
                        DisplayMsg(commandArgs);
                        break;
                    case "--set-result":
                        SetResult(commandArgs);
                        break;
                    default:
                        string args_string = string.Join(", ", args);
                        Logger.Warn($"invalid args: {args_string}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                Logger.Info("Execution ends...");
            }
        }

        /// <summary>
        /// Calls the InitializeProcess method in the Jts class.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the method.</param>
        public static void InitializeProcessCaller(string[] args)
        {
            try
            {
                ValidateArgs(1, args.Count());
                string serial = args[0];
                string response;
                string uuid;
                bool call_status;
                if (!Process.SimulationOn) //if the simulation mode is off, calls the jts initialize process method
                {
                    Logger.Info($"calling from Ted.Sdk.Tracking.dll");
                    call_status = Jts.InitializeProcess(serial, Process.OperationId, Process.LineSegmentId, Process.ProcessedBy, out response, out uuid);
                }
                else
                {
                    Logger.Info("simulating connection...");
                    response = "simulated init response for test";
                    uuid = "mensaje de prueba";
                    call_status = true;
                }
                string call_status_str = call_status ? "Successfull" : "Failed";
                Logger.Info($"call status result: {call_status_str}");
                if (response.ToLower().Contains("error"))
                {
                    Logger.Warn("error received");
                    string debug_info = $"args that raised the error:\n" +
                                        $"serial: {serial}, OperationId: {Process.OperationId}\n" +
                                        $"LineSegmentId: {Process.LineSegmentId}, ProcessedBy: {Process.ProcessedBy}";
                    Logger.Debug(debug_info);
                    string message_to_show = $"{response}\n{uuid}";
                    Message.Show("error response from 'Jts.InitializeProcess'", message_to_show, Color.Red, new Size(800, 600));
                    uuidFile.Clear();
                    statusFile.Write("FAIL");
                }
                else
                {
                    uuidFile.Write(uuid);
                    statusFile.Write("PASS");
                }
                Logger.Info($"received uuid: '{uuid}'");
                Logger.Info($"received response: '{response}'");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Calls the SetOperationOutput method in the Jts class.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the method.</param>
        public static void SetOutputCaller(string[] args)
        {
            try
            {
                ValidateArgs(2, args.Count());
                var uuid = uuidFile.Read();
                //output serial of the uut, depending on the process if uut doesnt change, is the same that was introduced
                string outputSerial = args[0];
                //result of the test process 'F' for fail and 'P' for pass
                string operationResult = args[1];
                if (operationResult == "F" ||  operationResult == "P")
                {
                    if (uuid != null)
                    {
                        string response;
                        bool call_status;
                        if (!Process.SimulationOn) //if the simulation mode is off, calls the jts SetOperationOutput method
                        {
                            Logger.Info($"calling from Ted.Sdk.Tracking.dll");
                            call_status = Jts.SetOperationOutput(uuid, outputSerial, operationResult, Process.ProcessedBy, out response);
                        }
                        else
                        {
                            Logger.Info("Simulating connection...");
                            response = "simulated out response for test";
                            call_status = true;
                        }
                        string call_status_str = call_status ? "Successfull" : "Failed";
                        Logger.Info($"call status result: {call_status_str}");
                        if (response.ToLower().Contains("error"))
                        {
                            Logger.Warn("error received");
                            string debug_info = $"args that raised the error:\n" +
                                                $"trackNTraceId: {uuid}, outputSerialNumber: {outputSerial}\n" +
                                                $"ProcessedBy: {Process.ProcessedBy}";
                            Logger.Debug(debug_info);
                            Message.Show("error response from 'Jts.SetOperationOutput'", response, Color.Red, new Size(800, 600));
                        }
                        else
                        {
                            //here could be an green message saying the insertion to jts was successfull
                        }
                        Logger.Info($"response from call: {response}");
                    }
                    else
                    {
                        Logger.Warn("Failed to read UUID file or the file format is incorrect.");
                    }
                }
                else
                {
                    Logger.Warn($"invalid operation result received, expected: F or P, received: {operationResult}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                uuidFile.Delete();
            }
        }

        /// <summary>
        /// Displays a message using the Message class.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the method.</param>
        public static void DisplayMsg(string[] args)
        {
            try
            {
                string args_to_display = string.Join(", ", args);
                Logger.Info($"Displaying: '{args_to_display}'...");
                Message.Show("debug msg", args_to_display, Color.Blue, new Size(900, 500));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Sets the result of the process in the resultFile.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the method.</param>
        public static void SetResult(string[] args)
        {
            try
            {
                ValidateArgs(1, args.Count());
                int numOfErrors = Convert.ToInt32(args[0]);
                if (numOfErrors > 0)
                {
                    Logger.Info("generating 'F' result...");
                    resultFile.Clear();
                    resultFile.Write("F");
                }
                else
                {
                    Logger.Info("generating 'P' result...");
                    resultFile.Clear();
                    resultFile.Write("P");
                }
            }
            catch (Exception ex)
            {
                // Log the exception with an appropriate log level (e.g., Error)
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Validates if the number of received arguments matches the expected number.
        /// </summary>
        /// <param name="expected">The expected number of arguments.</param>
        /// <param name="received">The received number of arguments.</param>
        /// <returns>Returns true if the number of received arguments matches the expected number; otherwise, returns false.</returns>
        private static bool ValidateArgs(int expected, int received)
        {
            if (expected != received)
            {
                Logger.Warn($"expected: {expected}, received: {received}");
                return false;
            }
            Logger.Info("Valid arg ammount!");
            return true;
        }
    }
}
