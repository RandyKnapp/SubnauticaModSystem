using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModInjector_MoreQuickSlots
{
	public partial class Form1 : Form
	{
		private const string steamInstallRegistryPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam";
		private const string steamInstallRegistryKey = "InstallPath";
		private const string steamConfigFile = @"\config\config.vdf";
		private const string subnauticaDir = @"\steamapps\common\Subnautica";
		private const string steamConfigBaseInstallKey = "BaseInstallFolder_1";

		private Injector injector;

		public Form1()
		{
			InitializeComponent();

			string steamDir = (string)Microsoft.Win32.Registry.GetValue(steamInstallRegistryPath, steamInstallRegistryKey, "");
			string configPath = steamDir + steamConfigFile;
			string subnauticaDirectory = steamDir + subnauticaDir;

			if (File.Exists(configPath))
			{
				foreach (var line in File.ReadAllLines(configPath))
				{
					if (line.Contains(steamConfigBaseInstallKey))
					{
						int start = line.LastIndexOf("\t\t\"") + 3;
						string dir = line.Substring(start, line.Length - start - 1);
						dir = dir.Replace("\\\\", "\\");
						subnauticaDirectory = dir + subnauticaDir;
						break;
					}
				}
			}

			if (Directory.Exists(subnauticaDirectory))
			{
				textBox1.Text = subnauticaDirectory;
			}
			textBox1.Select(0, 0);

			UpdateInjector();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			string dir = textBox1.Text;
			if (!Directory.Exists(dir))
			{
				Error("Directory specified does not exist!");
				return;
			}

			string message;
			bool success = injector.TryInject(out message);
			if (!success)
			{
				Error(message);
			}
			else
			{
				Success(message);
			}

			UpdateStatus();
		}

		private void UpdateInjector()
		{
			injector = new Injector(textBox1.Text, "MoreQuickSlots.dll", "MoreQuickSlots.InjectorPatcher", "Patch");

			UpdateStatus();
		}

		private void UpdateStatus()
		{
			InjectorStatus status = injector.GetStatus();

			modStatusLabel.Text = status.ModMessage;
			modStatusGood.Visible = status.Mod;
			modStatusBad.Visible = !status.Mod;

			targetStatusLabel.Text = status.TargetMessage;
			targetStatusGood.Visible = status.Target;
			targetStatusBad.Visible = !status.Target;

			injectedStatusLabel.Text = status.InjectedMessage;
			injectedStatusGood.Visible = status.Injected;
			injectedStatusBad.Visible = !status.Injected;

			injectButton.Enabled = !injector.IsInjected();
			uninstallButton.Visible = injector.IsInjected();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			DialogResult result = folderBrowserDialog1.ShowDialog();
			if (result == DialogResult.OK)
			{
				textBox1.Text = folderBrowserDialog1.SelectedPath;
			}
		}

		private void Error(string message)
		{
			MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private void Success(string message)
		{
			MessageBox.Show(message, "Success!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			UpdateInjector();
		}

		private void uninstallButton_Click(object sender, EventArgs e)
		{
			string message;
			bool success = injector.TryUninstall(out message);
			if (success)
			{
				Success(message);
			}
			else
			{
				Error(message);
			}

			UpdateStatus();
		}
	}
}
