using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace LauncherUE4
{
    // [UE4]\Engine\Source\Programs\AutomationTool\Win\WinResources.cs
    public enum ResourceType
    {
        Icon = 3,
        RawData = 10,
        GroupIcon = 14,
        Version = 16,
    }

    // [UE4]\Engine\Source\Programs\AutomationTool\Win\WinResources.cs
    public class ModuleResourceUpdate : IDisposable
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr BeginUpdateResource(string pFileName, [MarshalAs(UnmanagedType.Bool)]bool bDeleteExistingResources);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool UpdateResource(IntPtr hUpdate, IntPtr lpType, IntPtr lpName, ushort wLanguage, IntPtr lpData, uint cbData);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

        const ushort DefaultLanguage = 1033; // en-us

        IntPtr UpdateHandle;
        List<IntPtr> UnmanagedPointers = new List<IntPtr>();

        public ModuleResourceUpdate(string OutputFile, bool bRemoveExisting)
        {
            UpdateHandle = BeginUpdateResource(OutputFile, bRemoveExisting);
        }

        public void SetData(int ResourceId, ResourceType Type, byte[] Data)
        {
            IntPtr UnmanagedPointer = Marshal.AllocHGlobal(Data.Length);
            UnmanagedPointers.Add(UnmanagedPointer);

            Marshal.Copy(Data, 0, UnmanagedPointer, Data.Length);

            if (!UpdateResource(UpdateHandle, new IntPtr((int)Type), new IntPtr(ResourceId), DefaultLanguage, UnmanagedPointer, (uint)Data.Length))
            {
                throw new Exception("Couldn't update resource");
            }
        }

        public void Dispose()
        {
            EndUpdateResource(UpdateHandle, false);
            foreach (IntPtr UnmanagedPointer in UnmanagedPointers)
            {
                Marshal.FreeHGlobal(UnmanagedPointer);
            }
            UnmanagedPointers.Clear();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Arguments not found!");
                Console.WriteLine("Usage: LauncherUE4.exe FileName StagedArguments");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File not found!");
                Console.WriteLine("Usage: LauncherUE4.exe FileName StagedArguments");
                return;
            }

            // [UE4]\Engine\Source\Programs\AutomationTool\Win\WinPlatform.Automation.cs
            // void StageBootstrapExecutable(DeploymentContext SC, string ExeName, FileReference TargetFile, StagedFileReference StagedRelativeTargetPath, string StagedArguments)
            using (ModuleResourceUpdate Update = new ModuleResourceUpdate(args[0], false))
            {
                const int ExecArgsResourceId = 202;

                string StagedArguments = "MedievalTales";
                for (int i = 1; i < args.Length; ++i)
                    StagedArguments += " " + args[i];
                StagedArguments += "\0";

                Update.SetData(ExecArgsResourceId, ResourceType.RawData, Encoding.Unicode.GetBytes(StagedArguments));
            }
        }
    }
}
