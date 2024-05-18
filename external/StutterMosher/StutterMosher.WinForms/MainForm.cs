using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StutterMosher.WinForms
{
    public partial class MainForm : Form
    {
        private bool InputFileSelected;
        private bool OutputFileSelected;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InputFileButton_Click(object sender, EventArgs e)
        {
            InputFileDialog.ShowDialog();
        }

        private void InputFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            InputFileTextBox.Text = InputFileDialog.FileName;
            InputFileSelected = true;
        }

        private void OutputFileButton_Click(object sender, EventArgs e)
        {
            OutputFileDialog.ShowDialog();
        }

        private void OutputFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            OutputFileTextBox.Text = OutputFileDialog.FileName;
            OutputFileSelected = true;
        }

        private void Mosher_ProgressChanged(object sender, Mosher.ProgressEventArgs e)
        {
            BeginInvoke((Action)(() =>
            {
                ProgressBar.Value = (int)(100 * e.Progress);
            }));
        }

        private async void GoButton_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            GoButton.Enabled = false;

            await Task.Factory.StartNew(ConvertInputFile);
            if (!File.Exists("input.avi"))
            {
                OperationFailed();
                return;
            }

            using (FileStream inputFile = File.OpenRead("input.avi"))
            using (FileStream outputFile = File.Create("output.avi"))
            {
                Mosher mosher = new Mosher(inputFile, outputFile);
                mosher.ProgressChanged += Mosher_ProgressChanged;
                await mosher.MoshAsync((int)MoshPicker.Value);
            }

            await Task.Factory.StartNew(ConvertOutputFile);
            if (File.Exists(OutputFileDialog.FileName))
                OperationSuccess();
            else
                OperationFailed();

            ProgressBar.Value = 0;
            GoButton.Enabled = true;
        }

        private bool ValidateInput()
        {
            if (!(InputFileSelected && OutputFileSelected))
            {
                MessageBox.Show(
                    "You must have an input and output file selected before you can continue.", "Warning!",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning
                    );
                return false;
            }
            else if (File.Exists(OutputFileDialog.FileName))
            {
                DialogResult result = MessageBox.Show(
                    "Proceeding with the selected options can result in data loss!  Are you sure you want to overwrite the destination file?", "Warning!",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning
                    );
                if (result == DialogResult.No)
                    return false;
            }
            else if (!(StartTime.MaskCompleted && EndTime.MaskCompleted))
            {
                MessageBox.Show(
                    "The specified timespan is invalid.  Please make sure that all spaces in each prompt are full.", "Warning!",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning
                    );
                return false;
            }

            return true;
        }

        private void OperationSuccess()
        {
            DialogResult result = MessageBox.Show(
                        "Moshing Completed Successfully! Would you like to view the results?", "Success!",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information
                        );
            if (result == DialogResult.Yes)
                Process.Start(OutputFileDialog.FileName);
        }

        private void OperationFailed()
        {
            MessageBox.Show(
                    "An error occured while converting the file.", "Error!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
        }

        private void ConvertInputFile()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-y -i \"{InputFileDialog.FileName}\" -ss {StartTime.Text} -to {EndTime.Text} \"input.avi\""
            }).WaitForExit();
        }

        private void ConvertOutputFile()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-y -i \"output.avi\" \"{OutputFileDialog.FileName}\""
            }).WaitForExit();
        }
    }
}
