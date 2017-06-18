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
	public partial class ViewModel : BaseObject
		{
		private string _connectionLabel = "You are not connected to any device";
		private const int PORT = 2055;
		private ICommand _ExitCommand;
		private ICommand _AddCommand;
		private ICommand _DelCommand;
		private ICommand _DCCommand;
		private ObservableCollection<AppEntry> _appList = new ObservableCollection<AppEntry>();
		private Thread _thread;
		private MainWindow _main;
		private NetworkHandler _networkHandler;

		public bool isConnected = false;
		public Socket socket;
		public TcpListener listener;
		public NetworkStream stream;
		public StreamReader sReader;
		public StreamWriter sWriter;

		public ViewModel(MainWindow main)
			{
			listener = new TcpListener(IPAddress.Any, PORT);
			_thread = new Thread(new ThreadStart(StartConnection));
			_thread.Start();
			_main = main;
			LoadList();
			}

		public void StartConnection()
			{
			_networkHandler = new NetworkHandler(this);
			}

		private void LoadList()
			{
			try
				{
				string loadPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\LazyPcAppList.txt";
				if (File.Exists(loadPath))
					{
					TextReader tReader = new StreamReader(loadPath);
					string[] appArray = File.ReadAllLines(loadPath);
					foreach (string appLine in appArray)
						{
						if (appLine != "~~")
							{
							AppEntry app;
							string[] appProp = appLine.Split(';');
							Uri path = new Uri(appProp[1]);
							if (path.IsFile)
								{
								app = new AppEntry(path);
								}
							else
								{
								Exception ex = new Exception("Wrong file from list.");
								throw ex;
								}
							_appList.Add(app);
							}
						}
					}
				}
			catch (Exception ex)
				{
				MessageBox.Show("Error when loading save file: " + ex.ToString());
				}
			}

		

		public void ChangeConnectionStatus()
			{
			if (isConnected == true)
				{
				connectionLabel = "You are not connected to any device";
				isConnected = false;
				}
			else
				{
				connectionLabel = "You are connected to " + IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString()) + " through the port: " + PORT;
				isConnected = true;
				}
			}

		public ICommand ExitCommand
			{
			get
				{
				return _ExitCommand ?? (_ExitCommand = new CommandHandler(() => ExitApp(), true));
				}
			}

		private void ExitApp()
			{
			listener.Stop();
			if (stream != null)
				{
				Disconnect();
				}
			SaveList();
			Environment.Exit(0);
			}

		public void SaveList()
			{
			try
				{
				if (_appList.Count > 0)
					{
					string savePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\LazyPcAppList.txt";
					if (File.Exists(savePath))
						{
						File.Delete(savePath);
						}
					File.Create(savePath).Dispose();
					TextWriter tWriter = new StreamWriter(savePath);
					foreach (AppEntry app in _appList)
						{
						tWriter.WriteLine(app.GetName() + ";" + app.GetPath());
						}
					tWriter.WriteLine("~~");
					tWriter.Close();
					}
				}
			catch (Exception ex)
				{
				MessageBox.Show("Error when creating save file: " + ex.ToString());
				}
			}

		public void OnWindowClosing(object sender, CancelEventArgs e)
			{
			ExitApp();
			}

		public ICommand AddCommand
			{
			get
				{
				return _AddCommand ?? (_AddCommand = new CommandHandler(() => Add(), true));
				}
			}

		private void Add()
			{
			try
				{
				OpenFileDialog openDialog = new OpenFileDialog();
				openDialog.Filter = "Executables (*.exe) | *.exe";
				if (openDialog.ShowDialog() == true)
					{
					Uri file = new Uri(openDialog.FileName);
					if (file.IsFile)
						{
						AppEntry app = new AppEntry(file);
						appList.Add(app);
						}
					}
				}
			catch (Exception ex)
				{
				MessageBox.Show("Error when opening file: " + ex.ToString());
				}
			}


		public ICommand DelCommand
			{
			get
				{
				return _DelCommand ?? (_DelCommand = new CommandHandler(() => Delete(), true));
				}
			}

		private void Delete()
			{
			if (_main.listBoxApps.SelectedItem != null)
				{
				appList.Remove(appList[_main.listBoxApps.SelectedIndex]);
				}
			}

		public ICommand DCCommand
			{
			get
				{
				return _DCCommand ?? (_DCCommand = new CommandHandler(() => Disconnect(), true));
				}
			}


		public void Disconnect()
			{
			try
				{
				if (stream != null && isConnected == true)
					{
					sWriter.Close();
					sReader.Close();
					stream.Close();
					ChangeConnectionStatus();
					if (socket.Connected == true)
						{
						socket.Disconnect(true);
						}
					}
				}
			catch (Exception ex)
				{
				MessageBox.Show("Exception when trying to disconnect: " + ex.ToString());
				}
			}

		public string connectionLabel
			{
			get
				{
				return _connectionLabel;
				}
			set
				{
				_connectionLabel = value;
				OnPropertyChanged("connectionLabel");
				}
			}

		public ObservableCollection<AppEntry> appList
			{
			get
				{
				return _appList;
				}
			set
				{
				_appList = value;
				OnPropertyChanged("appList");
				}
			}

		}
	}
