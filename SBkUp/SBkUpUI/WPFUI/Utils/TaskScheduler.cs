using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32.TaskScheduler;
using SBkUpUI.WPFUI.Controls;

namespace SBkUpUI.WPFUI.Utils
{
    class Task_Scheduler
    {
        private static TaskService _service;

        public static TaskService TaskService
        {
            get
            {
                if (_service == null)
                {
                    _service = new TaskService();
                }
                return _service;
            }
        }

        private static Task TryGetTask(JobItem jobItem)
        {
            string taskName = string.Format(Properties.Resources.TASK_SCHEDULER_PRE, jobItem.Name);
            return TaskService.FindTask(taskName);
        }

        public static void TryDeleteTask(JobItem jobItem)
        {
            string taskName = string.Format(Properties.Resources.TASK_SCHEDULER_PRE, jobItem.Name);
            TaskService.RootFolder.DeleteTask(taskName, false);
        }

        private static Task CreateTask(JobItem jobItem, IntPtr owner)
        {
            string taskName = string.Format(Properties.Resources.TASK_SCHEDULER_PRE, jobItem.Name);
            var td = TaskService.NewTask();
            var trigger = new DailyTrigger((short)jobItem.Frequency);
            trigger.StartBoundary = jobItem.Date - jobItem.Date.TimeOfDay + jobItem.Time.TimeOfDay;
            td.Triggers.Add(trigger);

            var process = Process.GetCurrentProcess();
            string winzipPath = Path.Combine(Path.GetDirectoryName(process.MainModule.FileName), WinZipMethods.Is32Bit? "WinZip32.exe" : "WinZip64.exe");

#if WZ_APPX
            var program = winzipPath;
            var argu = Util.RunJobArguments(jobItem, owner);
            WinZipMethods.ProcessCommand(IntPtr.Zero, ref program, ref argu);
            td.Actions.Add(new ExecAction(program, argu));
#else
            td.Actions.Add(new ExecAction(winzipPath, Util.RunJobArguments(jobItem, owner)));
#endif
            var task = TaskService.RootFolder.RegisterTaskDefinition(taskName, td);
            task.Enabled = jobItem.IsEnabled;
            return task;
        }

        public static void TryInitJobItemFromTask(JobItem jobItem)
        {
            var task = TryGetTask(jobItem);
            jobItem.ModifiedByUser = false;
            jobItem.IsEnabled = false;
            if (task != null)
            {
                jobItem.IsEnabled = task.Enabled;
                foreach (var trigger in task.Definition.Triggers)
                {
                    if (trigger is DailyTrigger td)
                    {
                        jobItem.Frequency = td.DaysInterval;
                        jobItem.Date = td.StartBoundary;
                        jobItem.Time = td.StartBoundary;
                    }
                    else
                    {
                        jobItem.ModifiedByUser = true;
                    }
                }
            }
        }

        public static void TrySaveJobItemToTask(JobItem jobItem, IntPtr owner)
        {
            var task = TryGetTask(jobItem);
            if (task != null)
            {
                task.Enabled = jobItem.IsEnabled;
                if (!jobItem.ModifiedByUser)
                {
                    TryDeleteTask(jobItem);
                    CreateTask(jobItem, owner);
                }
            }
            else
            {
                if (jobItem.IsEnabled)
                {
                    CreateTask(jobItem, owner);
                }
            }
        }
    }
}
