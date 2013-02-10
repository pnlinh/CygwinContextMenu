﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: CLSCompliant(true)]
namespace CygwinContextMenu
{
    static class Program
    {
        // file type to register
        const string FileType = @"Directory\Background";

        // context menu name in the registry
        const string KeyName = "opencygwin";

        // context menu text
        const string MenuText = "Open cygwin window Here";

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // process register or unregister commands
                if (args.Length == 0)
                {
                    bool isRegistered = FileShellExtension.IsRegistered(FileType, KeyName);

                    if (isRegistered)
                    {
                        // unregister the context menu
                        FileShellExtension.Unregister(FileType, KeyName);

                        MessageBox.Show(string.Format("The {0} shell extension was unregistered.", KeyName), KeyName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // full path to self, %L is placeholder for selected file
                        string menuCommand = string.Format("\"{0}\" \"%V\"", Application.ExecutablePath);

                        // register the context menu
                        FileShellExtension.Register(FileType, KeyName, MenuText, menuCommand, true);

                        MessageBox.Show(string.Format("The {0} shell extension was registered.", KeyName), KeyName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    OpenCygwinHere(args[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception caught", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void OpenCygwinHere(string path)
        {
            //Find the location of cygwin.

            string cygwinPath = @"C:\cygwin";

            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (RegistryKey subKey = baseKey.OpenSubKey(@"SOFTWARE\Cygwin\setup\"))
                {
                    if (subKey != null)
                    {
                        cygwinPath = subKey.GetValue("rootdir").ToString();
                    }
                }
            }

            string cygwinTTY = Path.Combine(cygwinPath, @"bin\mintty.exe");

            if (!File.Exists(cygwinTTY))
            {
                MessageBox.Show("Cygwin is not installed.", "Cygwin not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //see http://code.google.com/p/mintty/wiki/Tips#Starting_mintty_from_a_batch_file
            ProcessStartInfo info = new ProcessStartInfo(cygwinTTY, "/bin/env CHERE_INVOKING=1 /bin/bash -l");
            info.WorkingDirectory = path;
            Process.Start(info);
        }
    }
}