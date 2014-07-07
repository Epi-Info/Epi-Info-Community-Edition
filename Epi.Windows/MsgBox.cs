using System;
using System.IO;
using System.Windows.Forms;

using Epi.Windows;
using Epi.Diagnostics;

namespace Epi.Windows
{

	/// <summary>
	/// Class containing common methods for messaging.
	/// </summary>
	public static class MsgBox
	{

		#region Public Methods

		/// <summary>
		/// Displays an exception
		/// </summary>
		/// <param name="ex">The Exception</param>
		public static void ShowException(System.Exception ex)
		{
			if (ex == null)
				return;

			Debugger.LogException(ex);
            //Logger.Log(ex.StackTrace);

			PrepareToShow();

				// FileNotFoundException
			if (ex is FileNotFoundException)
			{
				ShowException((FileNotFoundException)ex);
				return;
			}

				// NullReferenceException
			else if (ex is NullReferenceException)
			{
				ShowException((NullReferenceException)ex);
				return;
			}

				// OutOfMemoryException
			else if (ex is OutOfMemoryException)
			{
				ShowException((OutOfMemoryException) ex);
				return;
			}

				// ParseException
			else if (ex is ParseException)
			{
				ShowException((ParseException)ex);
				return;
			}
                // ReservedWordException
            else if (ex is ReservedWordException)
            {
                ShowException(ex as ReservedWordException);
                return;
            }

            // GeneralException
            else if (ex is GeneralException)
            {
                ShowGeneralException((GeneralException)ex);
            }

            else if (ex is System.ApplicationException)
            {
                ShowApplicationException((System.ApplicationException)ex);
            }

            // ApplicationException or Exception
            else
            {
                ShowError(Util.GetComprehensiveExceptionMessage(ex));
            }
		}

		/// <summary>
		/// Displays an error message when DirectoryNotFoundException is thrown.
		/// </summary>
		/// <param name="ex">The exception</param>
		private static void ShowException(DirectoryNotFoundException ex)
		{
			PrepareToShow();
            string errorMessage = Util.CombineMessageParts(SharedStrings.DIRECTORY_NOT_FOUND, ex.Message);
			ShowError(errorMessage, false);
		}

		/// <summary>
		/// Displays an error message when NullReferenceException is thrown.
		/// </summary>
		/// <param name="ex">The exception</param>
		private static void ShowException(NullReferenceException ex)
		{
			PrepareToShow();
            string msg = Util.GetComprehensiveExceptionMessage(ex);
			ShowError(msg, false);
		}

		/// <summary>
		/// Displays an error message when FileNotFoundException is thrown.
		/// </summary>
		/// <param name="ex">The exception</param>
        private static void ShowException(FileNotFoundException ex)
		{
			PrepareToShow();
            string msg = Util.CombineMessageParts(SharedStrings.FILE_NOT_FOUND, ex.FileName);
			ShowError(msg, false);
		}

		/// <summary>
		/// Displays an error message when OutOfMemoryException is thrown.
		/// </summary>
		/// <param name="ex">The exception</param>
        private static void ShowException(OutOfMemoryException ex)
		{
			PrepareToShow();
            string errorMessage = Util.CombineMessageParts(SharedStrings.OUT_OF_MEMORY, ex.Source);
			ShowError(errorMessage, false);
		}

		/// <summary>
		/// Displays a ParseException
		/// </summary>
		/// <param name="ex">The exception</param>
        private static void ShowException(ParseException ex)
		{
			PrepareToShow();
			//string errorMessage = LocalizeMessage(Strings.UNEXPECTED_TOKEN, ex.UnexpectedToken.Text);
			string errorMessage = Util.GetComprehensiveExceptionMessage(ex);
			ShowError(errorMessage, false);
		}

        /// <summary>
		/// Displays a ReservedWordException
		/// </summary>
		/// <param name="ex">The exception</param>
        private static void ShowException(ReservedWordException ex)
        {
            PrepareToShow();
            string errorMessage = Util.CombineMessageParts(SharedStrings.RESERVED_WORD_ERROR, ex.ReservedWordUsed);
            ShowError(errorMessage, false);
        }

		/// <summary>
		/// Displays GeneralException
		/// </summary>
		/// <param name="ex"></param>
        private static void ShowGeneralException(GeneralException ex)
		{
            string msg = Util.GetComprehensiveExceptionMessage(ex);
			ShowError(msg, false);
		}

		/// <summary>
		/// Displays a System.ApplicationException
		/// </summary>
		/// <param name="ex"></param>
        private static void ShowApplicationException(System.Exception ex)
		{
            PrepareToShow();
            string msg = Util.GetComprehensiveExceptionMessage(ex);
            ShowError(msg, false);
		}

		/// <summary>
		/// Displays an information box
		/// </summary>
		/// <param name="message">The message to be displayed in the information box</param>
        /// <param name="translate">Boolean indicating whether message requires translation</param>
        private static void ShowInformation(string message, bool translate)
		{
			PrepareToShow();
			string msg = string.Empty;
            //if (translate)
            //{
            //    msg = Localization.T(message);
            //}
            //else
            //{
				msg = message;
            //}
			MessageBox.Show(
				msg,
                SharedStrings.INFORMATION,
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}

        /// <summary>
        /// Makes a call to show information box 
        /// </summary>
        /// <param name="message">The message to be displayed in the information box</param>
		public static void ShowInformation(string message)
		{
			ShowInformation(message, true);
		}

		/// <summary>
		/// Displays a warning box
		/// </summary>
		/// <param name="message">The warning ato be displayed in the information box</param>
        /// <param name="translate">Boolean indicating whether message requires translation</param>
        private static void ShowWarning(string message, bool translate)
		{
			string msg = message;
            //msg = translate ? Localization.T(message) : message;
       
			MessageBox.Show(
				msg,
                SharedStrings.WARNING,
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}

        /// <summary>
        /// Calls method to show warning in information box
        /// </summary>
        /// <param name="message">Passes the warning that is to be displayed in the information box</param>
		public static void ShowWarning(string message)
		{
			ShowWarning(message, true);
		}

		/// <summary>
		/// Dispalys an error box
		/// </summary>
		/// <param name="message">The message to be displayed in the information box</param>
        /// <param name="translate">Boolean indicating whether error requires translation</param>
		private static void ShowError(string message, bool translate)
		{
            //Logger.Log(message);
			MessageBox.Show(
				message,
				SharedStrings.ERROR,
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		}

        /// <summary>
        /// Makes a call to show error in an information box
        /// </summary>
        /// <param name="message">The error to be displayed in the information box</param>
		public static void ShowError(string message)
		{
			ShowError(message, true);
		}

		/// <summary>
		/// Displays an error box
		/// </summary>
		/// <param name="errorMessage">The error to be displayed</param>
        /// <param name="ex">The error exception</param>
		public static void ShowError(string errorMessage, Exception ex)
		{
            string formattedMsg = errorMessage;
            string additionalInfo = GetInnerMostException(ex).Message;
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                formattedMsg += Environment.NewLine + Environment.NewLine;
                formattedMsg += SharedStrings.ADDITIONAL_INFORMATION;
                formattedMsg += Environment.NewLine;
                formattedMsg += additionalInfo;
            }

            ShowError(formattedMsg);
		}

		/// <summary>
		/// Displays a question
		/// </summary>
		/// <param name="question">The question to be displayed</param>
        /// <param name="buttons">Buttons to show on msg box</param>
		/// <returns>The user's response from the dialog</returns>
		public static DialogResult ShowQuestion(string question, MessageBoxButtons buttons)
		{
			PrepareToShow();
			return MessageBox.Show(
				question,
                SharedStrings.QUESTION,
				buttons,
				MessageBoxIcon.Question);
		}

        /// <summary>
        /// Calls the method to display a question
        /// </summary>
        /// <param name="question">The question to be displayed</param>
        /// <returns>The user's response from the dialog</returns>
		public static DialogResult ShowQuestion(string question)
		{
			return ShowQuestion(question, MessageBoxButtons.YesNo);
		}

		/// <summary>
		/// Displays a message box by localizing the text and caption.
		/// </summary>
		/// <param name="text">Text of the message box</param>
		/// <param name="caption">Caption of MessageBox</param>
		/// <param name="buttons">MessageBox buttons</param>
        /// <returns>The user's response from the dialog</returns>
		public static DialogResult Show(string text,string caption,MessageBoxButtons buttons)
		{
            PrepareToShow();
            Form topForm = TopMostMessageBox.GetTopMostInstance();
            DialogResult dialogResult = MessageBox.Show(topForm, text, caption, buttons);
            topForm.Dispose();
            return dialogResult;
		}

		/// <summary>
		/// Displays a message box by localizing the text and caption.
		/// </summary>
		/// <param name="text">Text of the message box</param>
		/// <param name="caption">Caption of MessageBox</param>
		/// <param name="buttons">MessageBox buttons</param>
		/// <param name="icon">MessageBoxIcon icon</param>
        /// <returns>The user's response from the dialog</returns>
		public static DialogResult Show(string text,string caption,MessageBoxButtons buttons, MessageBoxIcon icon)
		{
            PrepareToShow();
            Form topForm = TopMostMessageBox.GetTopMostInstance();
            DialogResult dialogResult = MessageBox.Show(topForm, text, caption, buttons, icon);
            topForm.Dispose();
            return dialogResult;
		}

		/// <summary>
		/// Displays a message box by localizing the text and caption.
		/// </summary>
		/// <param name="text">Text of the message box</param>
		/// <param name="caption">Caption of MessageBox</param>
        /// <returns>The user's response from the dialog</returns>
		public static DialogResult Show(string text,string caption)
		{
			PrepareToShow();
            Form topForm = TopMostMessageBox.GetTopMostInstance();
            DialogResult dialogResult = MessageBox.Show(topForm, text, caption);
            topForm.Dispose();
            return dialogResult;
		}

		#endregion Public Methods

		#region Private Methods

		/// <summary>
		/// Digs through all inner exceptions and retrieves the original exception that was thrown in the first place.
		/// </summary>
        /// <param name="ex">Inner exception</param>		
		/// <returns>Returns original exception</returns>
		private static Exception GetInnerMostException(Exception ex)
		{
			while (ex.InnerException != null)
			{
				ex = ex.InnerException;
			}
			return ex;
		}

		private static void PrepareToShow()
		{
			Cursor.Current = Cursors.Default;
		}

		#endregion Private Methods

	}

    static public class TopMostMessageBox
    {
        static public Form GetTopMostInstance()
        {
            Form topmostForm = new Form();

            topmostForm.Size = new System.Drawing.Size(1, 1);
            topmostForm.StartPosition = FormStartPosition.Manual;
            System.Drawing.Rectangle screenRect = SystemInformation.VirtualScreen;

            topmostForm.Location = new System.Drawing.Point(screenRect.Bottom + 10, screenRect.Right + 10);
            topmostForm.Show();
            topmostForm.Focus();
            topmostForm.BringToFront();
            topmostForm.TopMost = true;

            return topmostForm;
        }
    }
}
