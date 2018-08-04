using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModInjector_MoreQuickSlots
{
	public struct InjectorStatus
	{
		public bool Overall;
		public bool Mod;
		public string ModMessage;
		public bool Target;
		public string TargetMessage;
		public bool Injected;
		public string InjectedMessage;
	}

	public class Injector
	{
		private const string mainFilename = "Assembly-CSharp.dll";
		private const string backupFilename = "Assembly-CSharp.rk_original.dll";
		private const string subnauticaManagedDir = "/Subnautica_Data/Managed/";
		private const string mainPatchingClass = "GameInput";
		private const string mainPatchingMethod = "Awake";

		private string subnauticaDir;
		private string managedDir;
		private string assemblyPath;
		private string backupAssemblyPath;
		private string modAssemblyPath;
		private string modAssemblyFilename;
		private string modClass;
		private string modMethod;

		public Injector(string filePath, string modAssemblyFilename, string modClass, string modMethod)
		{
			subnauticaDir = filePath;
			managedDir = filePath + subnauticaManagedDir;
			assemblyPath = managedDir + mainFilename;
			backupAssemblyPath = managedDir + backupFilename;
			this.modAssemblyFilename = modAssemblyFilename;
			modAssemblyPath = managedDir + modAssemblyFilename;
			this.modClass = modClass;
			this.modMethod = modMethod;
		}

		public InjectorStatus GetStatus()
		{
			InjectorStatus status = new InjectorStatus();

			if (!File.Exists(modAssemblyPath))
			{
				status.Mod = false;
				status.ModMessage = modAssemblyFilename + " does not exist!";
			}
			else
			{
				status.Mod = true;
				status.ModMessage = modAssemblyFilename + " exists";
			}

			if (File.Exists(assemblyPath))
			{
				status.Target = true;
				status.TargetMessage = mainFilename + " exists";
				if (IsInjected())
				{
					status.Injected = true;
					status.InjectedMessage = "Injected";
				}
				else
				{
					status.Injected = false;
					status.InjectedMessage = "Not Injected";
				}
			}
			else
			{
				status.Target = false;
				status.Injected = false;
				status.TargetMessage = mainFilename + " does not exist!";
				status.InjectedMessage = "Not Injected";
			}

			return status;
		}

		public bool CanInject()
		{
			return File.Exists(assemblyPath) && File.Exists(modAssemblyPath) && !IsInjected();
		}

		internal bool TryInject(out string message)
		{
			if (!Directory.Exists(managedDir))
			{
				message = "Managed directory does not exist!";
				return false;
			}
			if (!File.Exists(assemblyPath))
			{
				message = mainFilename + " does not exist!";
				return false;
			}
			if (!File.Exists(modAssemblyPath))
			{
				message = modAssemblyFilename + " does not exist! Install " + modAssemblyFilename + " to\n" + managedDir + "\nbefore injecting!";
				return false;
			}
			if (IsInjected())
			{
				message = "Mod is already injected!";
				return false;
			}

			try
			{
				Inject();
			}
			catch (Exception e)
			{
				message = "An exception occurred: " + e.Message;
				return false;
			}
			
			message = mainFilename + " successfully patched!";
			return true;
		}

		private void Inject()
		{
			var target = AssemblyDefinition.ReadAssembly(assemblyPath);

			// delete old backups
			if (File.Exists(backupAssemblyPath))
			{
				File.Delete(backupAssemblyPath);
			}

			// save a copy of the dll as a backup
			File.Copy(assemblyPath, backupAssemblyPath);

			// load patcher module
			var installer = AssemblyDefinition.ReadAssembly(modAssemblyPath);
			var patchMethod = installer.MainModule.GetType(modClass).Methods.Single(x => x.Name == modMethod);

			// target the injection method
			var type = target.MainModule.GetType(mainPatchingClass);
			var method = type.Methods.First(x => x.Name == mainPatchingMethod);

			// inject
			var injectMethodCall = Instruction.Create(Mono.Cecil.Cil.OpCodes.Call, method.Module.Import(patchMethod));
			method.Body.GetILProcessor().InsertBefore(method.Body.Instructions[0], injectMethodCall);

			// save changes under original filename
			target.Write(assemblyPath);
		}

		public bool IsInjected()
		{
			if (!File.Exists(assemblyPath) || !File.Exists(modAssemblyPath))
			{
				return false;
			}

			var game = AssemblyDefinition.ReadAssembly(assemblyPath);

			var type = game.MainModule.GetType(mainPatchingClass);
			var method = type.Methods.First(x => x.Name == mainPatchingMethod);

			var installer = AssemblyDefinition.ReadAssembly(modAssemblyPath);
			var patchMethod = installer.MainModule.GetType(modClass).Methods.FirstOrDefault(x => x.Name == modMethod);

			bool patched = false;

			foreach (var instruction in method.Body.Instructions)
			{
				if (instruction.OpCode.Equals(OpCodes.Call) && instruction.Operand.ToString().Equals("System.Void " + modClass + "::" + modMethod + "()"))
				{
					return true;
				}
			}

			return patched;
		}

		public bool TryUninstall(out string message)
		{
			if (File.Exists(backupAssemblyPath))
			{
				File.Delete(assemblyPath);
				File.Move(backupAssemblyPath, assemblyPath);

				message = "Uninstall Successful";
				return true;
			}

			message = "Uninstall Failed! Backup file is missing!";
			return false;
		}
	}
}
