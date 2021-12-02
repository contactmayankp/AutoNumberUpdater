using System;
using System.Diagnostics;
using System.Windows.Forms;
using Logging;

namespace Sdmsols.XTB.AutoNumberUpdater
{
    internal static class LogTextBoxAndProgressBar
    {
        #region Progress Bar and Status text

        

        internal static void SetProgressBar(ProgressBar progressBar, int progBarCount)
        {
            progressBar.Visible = true;
            progressBar.Value = 0;
            progressBar.Minimum = 0;
            progressBar.Maximum = progBarCount;
            progressBar.Step = 1;

            Application.DoEvents();
        }

        internal static void AddProgressStep(ProgressBar progressBar)
        {
            progressBar.PerformStep();
            Application.DoEvents();
        }

        internal static void UpdateStatusMessage(TextBox statusText, string logMessage)
        {
            statusText.Text = statusText.Text + logMessage + Environment.NewLine;

            statusText.Focus();
            statusText.ScrollToCaret();
            ErrorLog.ReportRoutine(false, logMessage, EventLogEntryType.Information);

            Application.DoEvents();

        }

        #endregion Progress Bar and Status text
    }
}
