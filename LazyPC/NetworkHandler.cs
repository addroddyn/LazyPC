using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using LazyPC;
using System.Net.NetworkInformation;
using System.ComponentModel;
using Microsoft.Win32;
using System.Diagnostics;

namespace LazyPC
	{
	class NetworkHandler
		{

		private ViewModel _viewModel;
		private string[] _parsedMessage;
		private string _incomingMessage;
		private DateTime _shutdownTime;


		public NetworkHandler(ViewModel viewModel)
			{
			_viewModel = viewModel;
			HandleConnection();
			}

		public void HandleConnection()
			{
			while (true)
				{
				if (_viewModel.isConnected == false)
					{
					try
						{
						_viewModel.listener.Start();
						_viewModel.socket = _viewModel.listener.AcceptSocket();
						_viewModel.listener.Stop();
						_viewModel.stream = new NetworkStream(_viewModel.socket);
						_viewModel.sReader = new StreamReader(_viewModel.stream);
						_viewModel.sWriter = new StreamWriter(_viewModel.stream);
						_viewModel.sWriter.AutoFlush = true;
						_viewModel.sWriter.WriteLine("Are you the one I'm waiting for?");
						if (_viewModel.sReader.ReadLine().ToLower() == "yes")
							{
							_viewModel.ChangeConnectionStatus();
							}
						else
							{
							_viewModel.sWriter.WriteLine("Sorry, I'm here for someone else.");
							_viewModel.socket.Disconnect(true);
							}
						}
					catch (Exception ex)
						{
						MessageBox.Show("Exception when handling new connection: " + ex.ToString());
						}
					}
				if (_viewModel.isConnected == true)
					{
					try
						{
						if (_viewModel.socket.Connected == true)
							{
							ParseMessage();
							}
						}
					catch (Exception ex)
						{
						MessageBox.Show("Exception handling existing connection: " + ex.ToString());
						}
					}
				}
			}

		public void ParseMessage()
			{
			try
				{
				_incomingMessage = _viewModel.sReader.ReadLine();
				if (_incomingMessage == null && _viewModel.isConnected == true)
					{
					_viewModel.Disconnect();
					}
				else
					{
					_parsedMessage = _incomingMessage.ToLower().Split(';');
					}
				if (_parsedMessage.Count() != 3 && _incomingMessage != null)
					{
					_viewModel.sWriter.WriteLine("Wrong format. Should be:\n\rmessageType;firstArgument;secondArgument");
					return;
					}
				else if (_parsedMessage.Count() == 3 && _incomingMessage != null)
					{
					switch (_parsedMessage[0])
						{
						case "system":
							_viewModel.sWriter.WriteLine("this is a system message.");
							ParseSystemMessage();
							break;
						case "application":
							_viewModel.sWriter.WriteLine("this is an application message.");
							ParseAppMessage();
							break;
						default:
							_viewModel.sWriter.WriteLine("Unknown message type.");
							_viewModel.sWriter.WriteLine("Your message:");
							foreach (string token in _parsedMessage)
								_viewModel.sWriter.WriteLine(token);
							break;
						}
					}
				}
			catch (Exception ex)
				{
				if (ex.ToString().Contains("An established connection was aborted by the software in your host machine."))
					{
					_viewModel.Disconnect();
					}
				}
			}

		private void ParseAppMessage()
			{
			bool foundApp = false;
			try
				{
				foreach (AppEntry app in _viewModel.appList)
					{
					if (_parsedMessage[1].Equals(app.GetName().ToLower()))
						{
						foundApp = true;
						break;
						}
					}
				if (foundApp)
					{
					switch (_parsedMessage[2])
						{
						case "start":
							if (IsAppRunning(_parsedMessage[1]) == false)
								{
								StartApp(_parsedMessage[1]);
								}
							else
								{
								_viewModel.sWriter.WriteLine("App is already running");
								}
							break;
						case "stop":
							if (IsAppRunning(_parsedMessage[1]))
								{
								StopApp(_parsedMessage[1]);
								}
							else
								{
								_viewModel.sWriter.WriteLine("App is already stopped");
								}
							break;
						default:
							_viewModel.sWriter.WriteLine("command not recognized");
							break;
						}
					}
				else
					{
					_viewModel.sWriter.WriteLine("No app found");
					}
				}
			catch (Exception ex)
				{
				MessageBox.Show("Error when handling application message: " + ex.ToString());
				}
			}

		private void StopApp(string processName)
			{
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
				{
				string tempName = process.ProcessName.ToLower();
				if (tempName.Equals(processName.ToLower()))
					{
					_viewModel.sWriter.WriteLine("Found matching process");
					process.CloseMainWindow();
					process.Close();
					return;
					}
				}
			}

	private void StartApp(string processName)
		{
		foreach (AppEntry app in _viewModel.appList)
			{
			if (app.GetName().ToLower().Equals(processName))
				{
				Process.Start(app.GetPath());
				return;
				}
			}
		}

		private bool IsAppRunning(string processName)
		{
		Process[] processes = Process.GetProcesses();
		foreach (Process process in processes)
			{
			string tempName = process.ProcessName.ToLower();
			if (tempName.Equals(processName.ToLower()))
				return true;
			}
		return false;
		}

	private void ParseSystemMessage()
		{
		try
			{
			switch (_parsedMessage[1])
				{
				case "disconnect":
					_viewModel.sWriter.WriteLine("User disconnect");
					_viewModel.Disconnect();
					break;
				case "shutdown":
					if (_parsedMessage[2] != "")
						{
						if (CheckShutdownTime())
							{
							_shutdownTime = DateTime.Parse(_parsedMessage[2]);
							_viewModel.sWriter.WriteLine("Shutdown is scheduled for: " + _shutdownTime.ToString());
							Thread shutdownThread = new Thread(new ThreadStart(ShutDownPC));
							shutdownThread.Start();
							}
						else
							{
							_viewModel.sWriter.WriteLine("Incorrect time format. It should hh:mm .");
							}
						}
					break;
				default:
					_viewModel.sWriter.WriteLine("Unrecognized system message.");
					_viewModel.sWriter.WriteLine("Your message:");
					foreach (string token in _parsedMessage)
						_viewModel.sWriter.WriteLine(token);
					break;
				}
			}
		catch (Exception ex)
			{
			MessageBox.Show("Error when handling system message: " + ex.ToString());
			}
		}

	private void ShutDownPC()
		{
		while (DateTime.Now.CompareTo(_shutdownTime) < 0)
			{
			}
		Process.Start("shutdown", "/s /t 0");
		}

	private bool CheckShutdownTime()
		{
		string[] timeParse = _parsedMessage[2].Split(':');
		if (timeParse.Count() != 2)
			return false;
		foreach (string time in timeParse)
			{
			int temp;
			if (!int.TryParse(time, out temp))
				return false;
			}
		if ((int.Parse(timeParse[0]) > 23 || int.Parse(timeParse[0]) < 0) || (int.Parse(timeParse[1]) < 0 || int.Parse(timeParse[1]) > 59))
			return false;
		return true;
		}
	}
	}
