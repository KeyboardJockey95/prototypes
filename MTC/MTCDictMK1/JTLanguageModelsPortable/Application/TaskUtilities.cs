using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Service;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Helpers;

namespace JTLanguageModelsPortable.Application
{
    public class TaskUtilities
    {
        public string UserName { get; set; }
        public string TaskName { get; set; }
        public SoftwareTimer Timer { get; set; }
        public double OperationTime { get; set; }
        public bool SaveLog { get; set; }
        public string LogFilePath { get; set; }
        public bool CanDoBackground { get; set; }
        public int ProgressCountBase = 0;
        public int ProgressCount = 0;
        public int ProgressIndex = 0;
        public string ProgressStage = String.Empty;
        public bool ThrowIfInterrupted = false;
        public bool WasCanceled = false;
#if DEBUG
        public bool ShowElapsedTime = true;
        public bool ShowDisplaySuffix = true;
#else
        public bool ShowElapsedTime = false;
        public bool ShowDisplaySuffix = false;
#endif

        // Dump delegate.
        public ApplicationData.DumpString DumpStringDelegate = null;

        public TaskUtilities(UserRecord userRecord)
        {
            ClearTaskUtilities();
            if (userRecord != null)
                UserName = userRecord.UserName;
        }

        public TaskUtilities(TaskUtilities other)
        {
            CopyTaskUtilities(other);
        }

        public TaskUtilities()
        {
            ClearTaskUtilities();
        }

        public void ClearTaskUtilities()
        {
            UserName = String.Empty;
            TaskName = String.Empty;
            Timer = null;
            OperationTime = 0;
            SaveLog = false;
            LogFilePath = String.Empty;
            CanDoBackground = true;
            ProgressCountBase = 0;
            ProgressCount = 0;
            ProgressIndex = 0;
            ProgressStage = String.Empty;
        }

        public void CopyTaskUtilities(TaskUtilities other)
        {
            UserName = other.UserName;
            TaskName = other.TaskName;
            Timer = null;
            OperationTime = 0;
            SaveLog = other.SaveLog;
            LogFilePath = other.LogFilePath;
            ProgressCountBase = 0;
            ProgressCount = 0;
            ProgressIndex = 0;
            ProgressStage = String.Empty;
        }

        public void SetCanceled(string message)
        {
            if (!string.IsNullOrEmpty(TaskName))
                ApplicationData.Global.SetCanceled(UserName, TaskName, message);
        }

        public void SetCanceled(string operationName, string message)
        {
            ApplicationData.Global.SetCanceled(UserName, operationName, message);
        }

        public void ClearCanceled()
        {
            if (!string.IsNullOrEmpty(TaskName))
                ApplicationData.Global.ClearCanceled(UserName, TaskName);
        }

        public void ClearCanceled(string operationName)
        {
            ApplicationData.Global.ClearCanceled(UserName, operationName);
        }

        public bool IsCanceled()
        {
            if (!string.IsNullOrEmpty(TaskName))
            {
                if (ApplicationData.Global.IsCanceled(UserName, TaskName))
                    return true;
            }
            return false;
        }

        public bool IsCanceled(string operationName)
        {
            if (ApplicationData.Global.IsCanceled(UserName, operationName))
                return true;
            return false;
        }

        public void CheckForCancel()
        {
            if (IsCanceled())
                throw new OperationCanceledException(GetCanceledMessage());
        }

        public void CheckForCancel(string operationName)
        {
            if (IsCanceled(operationName))
                throw new OperationCanceledException(GetCanceledMessage(operationName));
        }

        public string GetCanceledMessage()
        {
            if (!string.IsNullOrEmpty(TaskName))
                return ApplicationData.Global.GetCanceledMessage(UserName, TaskName);
            return String.Empty;
        }

        public string GetCanceledMessage(string operationName)
        {
            return ApplicationData.Global.GetCanceledMessage(UserName, operationName);
        }

        public void GetOperationStatus(out string state, out string statusLabel)
        {
            if (!string.IsNullOrEmpty(TaskName))
                ApplicationData.Global.GetOperationStatus(UserName, TaskName, out state, out statusLabel);
            else
            {
                state = String.Empty;
                statusLabel = String.Empty;
            }
        }

        public void GetOperationStatus(string operationName, out string state, out string statusLabel)
        {
            ApplicationData.Global.GetOperationStatus(UserName, operationName, out state, out statusLabel);
        }

        public void SetOperationStatus(string state, string statusLabel)
        {
            if (!string.IsNullOrEmpty(TaskName))
                ApplicationData.Global.SetOperationStatus(UserName, TaskName, state, statusLabel);
        }

        public void SetOperationStatus(string operationName, string state, string statusLabel)
        {
            ApplicationData.Global.SetOperationStatus(UserName, operationName, state, statusLabel);
        }

        public string GetOperationStatusState()
        {
            if (!string.IsNullOrEmpty(TaskName))
                return ApplicationData.Global.GetUserOptionString(UserName, TaskName + "OperationStatusState");
            return String.Empty;
        }

        public string GetOperationStatusState(string operationName)
        {
            return ApplicationData.Global.GetUserOptionString(UserName, operationName + "OperationStatusState");
        }

        public void SetOperationStatusState(string state)
        {
            if (!string.IsNullOrEmpty(TaskName))
                ApplicationData.Global.SetUserOptionString(UserName, TaskName + "OperationStatusState", state);
        }

        public void SetOperationStatusState(string operationName, string state)
        {
            ApplicationData.Global.SetUserOptionString(UserName, operationName + "OperationStatusState", state);
        }

        public string GetOperationStatusLabel()
        {
            if (!string.IsNullOrEmpty(TaskName))
                return ApplicationData.Global.GetOperationStatusLabel(UserName, TaskName);
            return String.Empty;
        }

        public string GetOperationStatusLabel(string operationName)
        {
            return ApplicationData.Global.GetOperationStatusLabel(UserName, operationName);
        }

        public void SetOperationStatusLabel(string statusLabel)
        {
            if (!string.IsNullOrEmpty(TaskName))
                ApplicationData.Global.SetOperationStatusLabel(UserName, TaskName, statusLabel);
        }

        public void SetOperationStatusLabelElapsed(string statusLabel)
        {
            if (!string.IsNullOrEmpty(TaskName))
                ApplicationData.Global.SetOperationStatusLabel(UserName, TaskName, statusLabel + ElapsedTimeStringOptional);
        }

        public void SetOperationStatusLabel(string operationName, string statusLabel)
        {
            ApplicationData.Global.SetOperationStatusLabel(UserName, operationName, statusLabel);
        }

        public void ClearOperationStatus()
        {
            if (!string.IsNullOrEmpty(TaskName))
                ApplicationData.Global.ClearOperationStatus(UserName, TaskName);
        }

        public void ClearOperationStatus(string operationName)
        {
            ApplicationData.Global.ClearOperationStatus(UserName, operationName);
        }

        public void CreateTimerCheck()
        {
            if (Timer == null)
                CreateTimer();
        }

        public void CreateTimerCheckAndStart()
        {
            if (Timer == null)
                CreateTimer();

            TimerStart();
        }

        public void CreateTimer()
        {
            Timer = new SoftwareTimer();
        }

        public void TimerStart()
        {
            OperationTime = 0;

            if (Timer != null)
                Timer.Start();
        }

        public double TimerStop()
        {
            if (Timer != null)
            {
                Timer.Stop();
                OperationTime = Timer.GetTimeInSeconds();
            }
            else
                OperationTime = 0;

            return OperationTime;
        }

        public string FormatOperationTime(string label)
        {
            //string timeString = OperationTime.ToString() + " sec, " + (1000 * OperationTime).ToString() + " msec\r\n";
            string timeString = OperationTime.ToString() + " sec.\r\n";

            if (!String.IsNullOrEmpty(label))
                return label + ": " + timeString;
            else
                return timeString;
        }

        public void TimerStopAndDumpReport(string label)
        {
            double result = TimerStop();
            DumpString(FormatOperationTime(label));
        }

        public bool IsProgressInitialized = false;

        public virtual void InitializeProgress(
            string operation,
            bool doContinue,
            int progressCount)
        {
        }

        public virtual void FinishProgress(
            string operation,
            bool doContinue)
        {
        }

        // Progress is 0.0f to 1.0f, where 1.0f means complete.
        public virtual float GetProgress()
        {
            if (ProgressCount == 0)
                return 1.0f;

            return (float)ProgressIndex / ProgressCount;
        }

        public virtual string GetProgressMessage()
        {
            return ProgressStage;
        }

        public virtual bool StartProgress(int count)
        {
            ProgressCount = count;
            ProgressIndex = 0;
            ProgressStage = "Starting..." + ProgressDisplaySuffixOptional();
            ApplicationData.Global.ProgressOperation_Dispatch(ProgressMode.Start, ProgressCount, ProgressStage);
            SetOperationStatusLabel(ProgressStage);
            WriteLog(ProgressStage);
            DumpString(ProgressStage);
            return ProgressCancelCheck();
        }

        public virtual bool ContinueProgress(int count)
        {
            ProgressCount += count;
            ProgressStage = "Continuing..." + ProgressDisplaySuffixOptional();
            ApplicationData.Global.ProgressOperation_Dispatch(ProgressMode.Start, ProgressCount, ProgressStage);
            SetOperationStatusLabel(ProgressStage);
            WriteLog(ProgressStage);
            DumpString(ProgressStage);
            return ProgressCancelCheck();
        }

        public virtual bool UpdateProgress(string stage)
        {
            ProgressIndex++;
            ProgressStage = stage + ProgressDisplaySuffixOptional();
            ApplicationData.Global.ProgressOperation_Dispatch(ProgressMode.Update, ProgressIndex, ProgressStage);
            SetOperationStatusLabel(ProgressStage);
            WriteLog(ProgressStage);
            DumpString(ProgressStage);
            return ProgressCancelCheck();
        }

        public virtual bool UpdateProgressElapsed(string stage)
        {
            ProgressIndex++;
            ProgressStage = stage + ElapsedTimeStringOptional + ProgressDisplaySuffixOptional();
            ApplicationData.Global.ProgressOperation_Dispatch(ProgressMode.Update, ProgressIndex, ProgressStage);
            SetOperationStatusLabel(ProgressStage);
            WriteLog(ProgressStage);
            DumpString(ProgressStage);
            return ProgressCancelCheck();
        }

        public virtual bool UpdateProgressMessage(string stage)
        {
            ProgressStage = stage + ProgressDisplaySuffixOptional();
            ApplicationData.Global.ProgressOperation_Dispatch(ProgressMode.Update, ProgressIndex, ProgressStage);
            SetOperationStatusLabel(ProgressStage);
            WriteLog(ProgressStage);
            DumpString(ProgressStage);
            return ProgressCancelCheck();
        }

        public virtual bool UpdateProgressMessageElapsed(string stage)
        {
            ProgressStage = stage + ElapsedTimeStringOptional + ProgressDisplaySuffixOptional();
            ApplicationData.Global.ProgressOperation_Dispatch(ProgressMode.Update, ProgressIndex, ProgressStage);
            SetOperationStatusLabel(ProgressStage);
            WriteLog(ProgressStage);
            DumpString(ProgressStage);
            return ProgressCancelCheck();
        }

        public virtual bool EndProgress()
        {
            ProgressIndex = ProgressCount;
            ProgressStage = "Completed." + ProgressDisplaySuffixOptional();
            ApplicationData.Global.ProgressOperation_Dispatch(ProgressMode.Stop, ProgressIndex, ProgressStage);
            SetOperationStatusLabel(ProgressStage);
            WriteLog(ProgressStage);
            DumpString(ProgressStage);
            return ProgressCancelCheck();
        }

        public virtual bool EndContinuedProgress()
        {
            // Ignore for now.
            return ProgressCancelCheck();
        }

        public virtual bool ResetProgress()
        {
            ProgressCount = 0;
            ProgressIndex = 0;
            ProgressStage = String.Empty;
            SetOperationStatusLabel(String.Empty);
            return ProgressCancelCheck();
        }

        public virtual string ProgressDisplaySuffix()
        {
            return " (stage " + ProgressIndex.ToString() + " of " + ProgressCount.ToString() + ")";
        }

        public virtual string ProgressDisplaySuffixOptional()
        {
            if (ShowDisplaySuffix)
                return " (stage " + ProgressIndex.ToString() + " of " + ProgressCount.ToString() + ")";
            else
                return String.Empty;
        }

        public virtual void PutStatusMessage(string message)
        {
            //string msg = ProgressStage + ":\n    " + message;
            string msg = message + ProgressDisplaySuffixOptional();
            SetOperationStatusLabel(msg);
            WriteLog(msg);
            DumpString(msg);
        }

        public virtual void PutStatusMessageElapsed(string message)
        {
            //string msg = ProgressStage + ":\n    " + message;
            string msg = message + ElapsedTimeStringOptional + ProgressDisplaySuffixOptional();
            SetOperationStatusLabel(msg);
            WriteLog(msg);
            DumpString(msg);
        }
        public virtual bool ProgressSetupThrow()
        {
            ThrowIfInterrupted = true;

            if (ProgressCancelCheck())
                return true;

            return false;
        }

        // Returns true if cancelled.
        public bool ProgressCancelCheck()
        {
            if (WasCanceled ||
                ApplicationData.Global.ProgressCancelCheck() ||
                IsCanceled() ||
                ApplicationData.Global.CanceledCheck())
            {
                if (ThrowIfInterrupted)
                    throw new OperationCanceledException(GetCanceledMessage());

                WasCanceled = true;

                return true;
            }

            return false;
        }

        public void TransferProgress(TaskUtilities other)
        {
            TaskName = other.TaskName;
            ProgressCountBase = other.ProgressCountBase;
            ProgressCount = other.ProgressCount;
            ProgressIndex = other.ProgressIndex;
            ProgressStage = other.ProgressStage;
            Timer = other.Timer;
        }

        public double ElapsedTime
        {
            get
            {
                if (Timer != null)
                    return Timer.GetElapsedTimeInSeconds();
                return 0.0;
            }
        }

        public string ElapsedTimeString
        {
            get
            {
                double elapsedTime = ElapsedTime;

                if (elapsedTime != 0.0)
                    return ": Elapsed time: " + elapsedTime.ToString();

                return String.Empty;
            }
        }

        public string ElapsedTimeStringOptional
        {
            get
            {
                if (ShowElapsedTime)
                {
                    double elapsedTime = ElapsedTime;

                    if (elapsedTime != 0.0)
                        return ": Elapsed time: " + elapsedTime.ToString();
                }

                return String.Empty;
            }
        }

        public virtual void ClearLog()
        {
            if (SaveLog && !String.IsNullOrEmpty(LogFilePath))
            {
                FileSingleton.DirectoryExistsCheck(LogFilePath);
                FileSingleton.Delete(LogFilePath);
            }
        }

        public virtual void WriteLog(string message)
        {
            if (SaveLog && !String.IsNullOrEmpty(LogFilePath))
            {
                FileSingleton.AppendAllText(LogFilePath, message + "\r\n");
            }
        }

        public virtual void DumpString(string text)
        {
            if (DumpStringDelegate != null)
                DumpStringDelegate(text);
        }

        public virtual void DumpBaseObject(IBaseObject baseObject)
        {
            if (baseObject == null)
                DumpString("(null)");
            else
                DumpString(baseObject.Xml.ToString());
        }

        public void SetUpDumpStringCheck()
        {
            if (DumpStringDelegate == null)
                DumpStringDelegate = ApplicationData.Global.PutConsoleMessage;
        }
    }
}
