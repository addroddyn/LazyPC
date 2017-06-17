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

namespace LazyPC
	{
	class NetworkHandler
		{

		ViewModel _viewModel;


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
							GetMessage();
							}
						}
					catch (Exception ex)
						{
						MessageBox.Show("Exception handling existing connection: " + ex.ToString());
						}
					}
				}
			}

		public void GetMessage()
			{
			try
				{
				string _incomingMessage = _viewModel.sReader.ReadLine();
				if (_incomingMessage == null && _viewModel.isConnected == true)
					{
					_viewModel.Disconnect();
					}
				else if (_incomingMessage.StartsWith("system"))
					{
					MessageBox.Show("System command");
					}
				else if (_incomingMessage.StartsWith("application"))
					{
					MessageBox.Show("Application command");
					}
				else if (_incomingMessage.Equals("disconnect"))
					{
					_viewModel.Disconnect();
					}
				else
					{
					MessageBox.Show("Unknown messagetype.");
					}
				}
			catch (Exception ex)
				{
				MessageBox.Show("Error when getting message from client: " + ex.ToString());
				}
			}
		}
	}
