using System;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using NetEti.ApplicationControl;

namespace NetEti.DemoApplications
{
    /// <summary>
    /// Funktion: Demoprogramm für die Klasse ProgressTimer
    /// </summary>
    /// <remarks>
    /// File: Form1.cs<br></br>
    /// Autor: Erik Nagel<br></br>
    ///<br></br>
    /// 09.03.2012 Erik Nagel: erstellt<br></br>
    /// 19.12.2016 Erik Nagel: überarbeitet<br></br>
    /// </remarks>
    public partial class Form1 : Form
    {
        private ProgressTimer _progressTimer_1; // Drei ProgressTimer,
        private ProgressTimer _progressTimer_2; // die sich den ProgressBar
        private ProgressTimer _progressTimer_3; // auf der Form aufteilen.
        private ProgressTimer _activeProgressTimer;

        private ProgressChangedEventArgs lastProgressChangedEventArgs;

        private bool closeAllowed;

        private BackgroundWorker _backgroundWorker; // Arbeitet die drei Test-Jobs
                                                    // in einem eigenen Thread ab.

        private DoWorkEventHandler _all_Jobs_DoWorkEventHandler;

        /// <summary>
        /// Constructor
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            this.UpdateProgressBarAndLabel(0, "");

            this.closeAllowed = true;
            this.btnStartAll.Enabled = true;

            this._progressTimer_1 = new ProgressTimer() { ProcessName = "Fortschritt: Teil 1" };
            this._progressTimer_1.WorkingRoutine = new DoWorkEventHandler(BackgroundWorker_DoJob_1);
            this._progressTimer_1.Weight = 200; // doppelte Länge
            this._progressTimer_1.Head = 40; // Soll für die ersten 40% des zugehörigen Teils
                                              // des ProgressBars Timer-gesteuert laufen
            this._progressTimer_1.PercentMilliseconds = 250; // 1 Prozent pro Viertelsekunde
            this._progressTimer_1.ProgressChanged += this.ProgressTimer_ProgressChanged;
            this._progressTimer_1.ProgressFinished += this.ProgressTimer_ProgressFinished;

            this._progressTimer_2 = new ProgressTimer() { ProcessName = "Fortschritt: Teil 2" };
            this._progressTimer_2.WorkingRoutine = new DoWorkEventHandler(BackgroundWorker_DoJob_2);
            this._progressTimer_2.ProgressChanged += this.ProgressTimer_ProgressChanged;
            this._progressTimer_2.ProgressFinished += this.ProgressTimer_ProgressFinished;

            this._progressTimer_3 = new ProgressTimer() { ProcessName = "Fortschritt: Teil 3" };
            this._progressTimer_3.WorkingRoutine = new DoWorkEventHandler(BackgroundWorker_DoJob_3);
            this._progressTimer_3.Tail = 60; // Soll für die letzten 60% des zugehörigen Teils
                                              // des ProgressBars Timer-gesteuert laufen
            this._progressTimer_3.PercentMilliseconds = 100; // 1 Prozent pro Zehntelsekunde
            this._progressTimer_3.ProgressChanged += this.ProgressTimer_ProgressChanged;
            this._progressTimer_3.ProgressFinished += this.ProgressTimer_ProgressFinished;

            this._progressTimer_1.Chain(_progressTimer_2); // Verketten der drei
            this._progressTimer_2.Chain(_progressTimer_3); // ProgressTimer

            this._all_Jobs_DoWorkEventHandler = new DoWorkEventHandler(this.All_Jobs_DoWork);

            this._backgroundWorker = new BackgroundWorker();
            this._backgroundWorker.DoWork += this._all_Jobs_DoWorkEventHandler; // Der BackgroundWorker wird auf Job 1 eingestellt.
            this._backgroundWorker.RunWorkerCompleted += this.All_Jobs_ProgressFinished;

        }

        private void btnStartAll_Click(object sender, EventArgs e)
        {
            try
            {
                // Hier wird die Verarbeitung des 1. Test-Jobs gestartet. Ist dieser beendet,
                // startet er seinerseits in myThreadWorker_RunWorkerCompleted_1 den Test-Job 2.
                this.btnStartAll.Enabled = false; // Die Form darf nicht geschlossen werden,
                                                  // solange die Jobs laufen.
                // this.lblPartInfo.Text = "Fortschritt: Teil 1";
                this.closeAllowed = false;
                this._backgroundWorker.RunWorkerAsync(); // Mache Jobs 1 bis 3.
            }
            catch (Exception ex)
            {
                this.btnStartAll.Enabled = true;
                this.closeAllowed = true;
                MessageBox.Show(ex.Message);
            }
        }

        private void All_Jobs_DoWork(object sender, DoWorkEventArgs e)
        {
            // Hier wird der erste der verketteten Jobs gestartet (Job 1).
            // Ist dieser ohne Fehler oder Abbruch beendet, startet er
            // seinerseits den darauffolgenden Job (Job 2).
            this._progressTimer_1.Start();
        }

        private void BackgroundWorker_DoJob_1(object sender, DoWorkEventArgs e)
        {
            this._activeProgressTimer = this._progressTimer_1;

            // am Anfang hängt's eine Zeit lang...
            Thread.Sleep(5000);

            int percentDone = 1;
            do
            {
                Thread.Sleep(100);
                this._activeProgressTimer.Value = percentDone;
            } while (percentDone++ < 100);
            // throw new ApplicationException("Testexception");
        }

        private void BackgroundWorker_DoJob_2(object sender, DoWorkEventArgs e)
        {
            this._activeProgressTimer = this._progressTimer_2;

            int percentDone = 1;
            do
            {
                Thread.Sleep(50);
                this._activeProgressTimer.Value = percentDone;
            } while (percentDone++ < 100);
        }

        private void BackgroundWorker_DoJob_3(object sender, DoWorkEventArgs e)
        {
            this._activeProgressTimer = this._progressTimer_3;

            int percentDone = 1;
            do
            {
                Thread.Sleep(50);
                this._activeProgressTimer.Value = percentDone;
            } while (percentDone++ < 100);

            // am Ende hängt's eine Zeit lang...
            Thread.Sleep(5000);
        }

        private void ProgressTimer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.lastProgressChangedEventArgs = e;
            this.OnProgressChanged(sender, e);
        }

        private void ProgressTimer_ProgressFinished(object sender, ProgressChangedEventArgs e)
        {
            // Ende einer Einzelverarbeitung, normalerweise muss hier nichts gemacht werden.
        }

        private void All_Jobs_ProgressFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            // Hier endet die Verarbeitung des BackroundWorkers, also die Verarbeitung aller Jobs.
            if (e.Error != null)
            {
                this.OnProgressFinished(sender, new ProgressChangedEventArgs(this.lastProgressChangedEventArgs.ProgressPercentage, e.Error));
            }
            else
            {
                if (e.Cancelled)
                {
                    this.OnProgressFinished(sender, new ProgressChangedEventArgs(this.lastProgressChangedEventArgs.ProgressPercentage, "Die Verarbeitung wurde abgebrochen."));
                }
                else
                {
                    this.OnProgressFinished(sender, new ProgressChangedEventArgs(100, "Die Verarbeitung ist abgeschlossen."));
                }
            }
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.UpdateProgressBarAndLabel(e.ProgressPercentage, (e.UserState ?? "").ToString());
        }

        private void OnProgressFinished(object sender, ProgressChangedEventArgs e)
        {
            string endMessage = (e.UserState ?? "").ToString().PadRight(60).Substring(0, 60).TrimEnd();
            this.UpdateProgressBarAndLabel(e.ProgressPercentage, endMessage);
            if (e.UserState is Exception)
            {
                MessageBox.Show(String.Format($"{(e.UserState as Exception).Message}", "Task-Exception", MessageBoxButtons.OK, MessageBoxIcon.Error));
                this.btnStartAll.Enabled = true;
                this.closeAllowed = true;
            }
            else
            {
                this.btnStartAll.Enabled = true;
                this.closeAllowed = true;
            }
        }

        private void UpdateProgressBarAndLabel(int value, string name)
        {
            if (this.theProgressBar.InvokeRequired)
            {
                this.BeginInvoke(new Action<int, string>(this.UpdateProgressBarAndLabel), value, name);
            }
            else
            {
                this.lblPartInfo.Text = name;
                this.theProgressBar.Value = value;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.closeAllowed)
            {
                e.Cancel = true;
                MessageBox.Show("Nö!");
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this._backgroundWorker.Dispose();
        }
    }
}
