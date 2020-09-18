using System;
using System.ComponentModel;
using System.Threading;

namespace NetEti.ApplicationControl
{
    /// <summary>
    /// Simuliert einen kontinuierlich laufenden Fortschritt, auch wenn die Verarbeitung
    /// am Anfang eine Zeit lang hängt bevor das erste Mal das Event für die
    /// Fortschrittsanzeige ausgelöst wird. Ebenso kann, wenn die Verarbeitung
    /// am Ende (nach 100%) noch eine Zeit lang hängt, ein weiterlaufender Balken simuliert
    /// werden.
    /// Darüber hinaus können mehrere Verarbeitungen durch Verkettung einen fortlaufenden
    /// Gesamtprozess simulieren.
    /// </summary>
    /// <remarks>   
    /// Autor: Erik Nagel
    ///<br></br>
    /// 12.03.2012 Erik Nagel: erstellt<br></br>
    /// 19.12.2016 Erik Nagel: überarbeitet.
    /// </remarks>
    public class ProgressTimer : IDisposable
    {
        #region IDisposable Member

        private bool _disposed = false;

        /// <summary>
        /// Öffentliche Methode zum Aufräumen.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Abschlussarbeiten, ggf. Timer zurücksetzen.
        /// </summary>
        /// <param name="disposing">False, wenn vom eigenen Destruktor aufgerufen.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    // Läuft vielleicht noch ein Timer-Thread?,
                    // dann beenden.
                    if (!this._timerStop)
                    {
                        this._timerStop = true;
                        Thread.Sleep(this.PercentMilliseconds);
                    }
                }
                this._disposed = true;
            }
        }

        /// <summary>
        /// Destruktor
        /// </summary>
        ~ProgressTimer()
        {
            this.Dispose(false);
        }

        #endregion IDisposable Member

        #region public members

        private ProgressChangedEventHandler _progressChanged;
        /// <summary>
        /// Ereignis das eintritt, wenn sich der Gesamtfortschritt ändert.
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged
        {
            add
            {
                _progressChanged += value;
                if (this._successor != null)
                {
                    this._successor.ProgressChanged += value;
                }
            }
            remove
            {
                if (this._successor != null)
                {
                    this._successor.ProgressChanged -= value;
                }
                _progressChanged -= value;
            }
        }

        /// <summary>
        /// Ereignis das eintritt, wenn die Verarbeitung für diesen ProgressTimer beendet ist.
        /// </summary>
        public event ProgressChangedEventHandler ProgressFinished;

        /// <summary>
        /// Die Verarbeitung für diesen ProgressTimer.
        /// </summary>
        public DoWorkEventHandler WorkingRoutine;

        /// <summary>
        /// Bezeichner für die zugrundeliegende Verarbeitung.
        /// Wird in ProgressChangedEventArgs als userState-Object mitgegeben.
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// Legt fest, wieviel Raum diese Verarbeitung im Verhältnis zu anderen
        /// verketteten Verarbeitungsschritten einnimmt (Default: 100, enspricht Gleichverteilung).
        /// </summary>
        public long Weight { get; set; }

        /// <summary>
        /// Legt fest, wieviel Prozent dieser Verarbeitung für die Startverarbeitung
        /// (vor erstem Aufruf des Callbacks für die Fortschrittsanzeige)
        /// reserviert werden (Default: 0).
        /// </summary>
        public int Head { get; set; }

        /// <summary>
        /// Legt fest, wieviel Prozent dieser Verarbeitung für die Endverarbeitung
        /// (nach dem letzten Aufruf des Callbacks für die Fortschrittsanzeige)
        /// reserviert werden (Default: 0).
        /// </summary>
        public int Tail { get; set; }

        /// <summary>
        /// Legt fest, wie schnell der Fortschritt um ein Prozent erhöht
        /// werden soll, wenn die eigentliche Verarbeitung hängt (Default: 1000).
        /// </summary>
        public int PercentMilliseconds { get; set; }

        /// <summary>
        /// Wird auf true gesetzt, wenn FinishChain aufgerufen wurde.
        /// </summary>
        public bool IsBreaked { get; private set; }

        private int _value;
        /// <summary>
        /// Übernimmt den Prozentwert (0 bis 100) für diesen ProgressTimer und
        /// rechnet diesen in den passenden Wert für den Gesamtfortschritt um.
        /// Ist der übergebene Prozentwert = 0 wird, wenn der Head dieses
        /// ProgressTimers > 0 ist, ein "Timer" für den Frortschritt losgeschickt,
        /// der das Weitersetzen für den Head-Teil übernimmt.
        /// Ist der übergebene Prozentwert = 100, wird, wenn der Tail dieses
        /// ProgressTimers > 0 ist, ein "Timer" für den Gesamtfortschritt losgeschickt,
        /// der das Weitersetzen für den Tail-Teil übernimmt.
        /// </summary>
        public int Value
        {
            // Aus dokumentatorischen Gründen habe ich die Berechnung von
            // AllPercentDone nicht weiter zusammengefasst.
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
                float toHundred = 100f / this.ChainWeight;
                float toThisWeight = this.Weight / 100f;
                int body = 100 - this.Head - this.Tail;
                float toThisBody = body / 100f;
                if ((this.Head > 0) && (this.Value == 0))
                {
                    this._isFinishing = false;
                    if (this._isStarting)
                    {
                        _asyncMethod aM1 = new _asyncMethod(invokeTimerAsync);
                        this._timerStop = false;
                        IAsyncResult iar = aM1.BeginInvoke(null, null);
                        this.AllPercentDone = (int)(
                            (this.LeftBranchWeight - this.Weight) * toHundred
                            + 0.5);
                    }
                }
                else
                {
                    if ((this.Tail > 0) && (this.Value < 100) && this.Value > this.Head/* && (this.Value > 97)*/)
                    {
                        if (!this._isFinishing)
                        {
                            this._isFinishing = true;
                            _asyncMethod aM2 = new _asyncMethod(invokeTimerAsync);
                            this._timerStop = false;
                            IAsyncResult iar = aM2.BeginInvoke(null, null);
                            this.AllPercentDone = (int)(
                                (this.LeftBranchWeight - this.Weight) * toHundred
                                + this.Head * toThisWeight * toHundred
                                + 100 * toThisBody * toThisWeight * toHundred
                                + 0.5);
                        }
                    }
                    else
                    {
                        this._isFinishing = false;
                        this.AllPercentDone = (int)(
                            (this.LeftBranchWeight - this.Weight) * toHundred
                            + this.Head * toThisWeight * toHundred
                            + this._value * toThisBody * toThisWeight * toHundred
                            + 0.5);
                    }
                }
                if (this.AllPercentDone != this.AllPercentLast)
                {
                    this.AllPercentLast = this.AllPercentDone;
                    if (this._value < 100)
                    {
                        this.OnProgressChanged("");
                    }
                }
                if (this.Value > 99)
                {
                    this.FinishMe(false);
                    //this.OnProgressFinished();
                }
            }
        }

        /// <summary>
        /// Liefert bei verketteten ProgressTimern den Anteil der links vom
        /// aktuellen ProgressTimer liegenden ProgressTimer am Gesamtfortschritt
        /// plus den Anteil des aktuellen ProgessTimers.
        /// </summary>
        public long LeftBranchWeight
        {
            get
            {
                if (this._predecessor != null)
                {
                    return this.Weight + this._predecessor.LeftBranchWeight;
                }
                else
                {
                    return this.Weight;
                }
            }
        }

        /// <summary>
        /// Liefert bei verketteten ProgressTimern den Anteil der rechts vom
        /// aktuellen ProgressTimer liegenden ProgressTimer am Gesamtfortschritt
        /// plus den Anteil des aktuellen ProgessTimers.
        /// </summary>
        public long RightBranchWeight
        {
            get
            {
                if (this._successor != null)
                {
                    return this.Weight + this._successor.RightBranchWeight;
                }
                else
                {
                    return this.Weight;
                }
            }
        }

        /// <summary>
        /// Liefert bei verketteten ProgressTimern die Summe aller Gewichte.
        /// </summary>
        public long ChainWeight
        {
            get
            {
                return (this.LeftBranchWeight + this.RightBranchWeight - this.Weight);
            }
        }

        /// <summary>
        /// Standard Konstruktor.
        /// </summary>
        public ProgressTimer()
        {
            this.Weight = 100;
            this.Head = 0;
            this.Tail = 0;
            this.PercentMilliseconds = 1000;
            this._predecessor = null;
            this._successor = null;
            this._timerStop = true;
            this._isStarting = false;
            this._isFinishing = false;
            this.IsBreaked = false;
        }

        /// <summary>
        /// Verkettet mehrere ProgressTimer(-Ketten), so dass sie sich den Gesamtfortschritt teilen.
        /// </summary>
        /// <param name="successor">Der nachfolgende ProgressTimer</param>
        /// <returns>Liefert den übergebenen successor für weitere Verkettungen wieder zurück.</returns>
        public ProgressTimer Chain(ProgressTimer successor)
        {
            ProgressTimer leftEdge = this;
            while (leftEdge._successor != null)
            {
                leftEdge = leftEdge._successor;
            }
            leftEdge._successor = successor;
            leftEdge._successor._predecessor = leftEdge;
            while (leftEdge._successor != null)
            {
                leftEdge = leftEdge._successor;
            }
            return leftEdge;
        }

        /// <summary>
        /// Löst eine vorhandene Verkettung.
        /// </summary>
        public void Unchain()
        {
            if (this._successor != null)
            {
                this._successor._predecessor = null;
                this._successor = null;
            }
            if (this._predecessor != null)
            {
                this._predecessor._successor = null;
                this._predecessor = null;
            }
        }

        /// <summary>
        /// Startet die Verarbeitung für den von diesem ProgressTimer belegten
        /// Teil des gemeinsamen Gesamtfortschritts. Wenn der Head dieses ProgressTimers
        /// > 0 ist, wird ein "Timer" losgeschickt, der den Fortschritt solange
        /// steuert, bis die Hauptverarbeitung übernimmt. 
        /// </summary>
        public void Start()
        {
            this.IsBreaked = false;
            this._isStarting = true; // Zwingend erforderlich, obwohl this._isStarting drei Zeilen weiter wieder auf
            this.AllPercentLast = 0; // false gesetzt wird (sonst würde im Setter von this.Value kein Timer gestartet).
            this.Value = 0; // Die Property "Value" startet ggf. einen "Timer".
            this._isStarting = false;
            try
            {
                if (this.WorkingRoutine != null)
                {
                    this.WorkingRoutine(this, new DoWorkEventArgs(this.ProcessName));
                }
                this.FinishMe(false);
                this.OnProgressFinished();
                if (!this.IsBreaked && this._successor != null)
                {
                    this._successor.Start();
                }
            }
            catch
            {
                this.FinishMe(false);
                throw;
            }
        }

        /// <summary>
        /// Beendet für diesen ProgressTimer die Verarbeitung.
        /// </summary>
        /// <param name="setToHundred">Bei True wird der Gesamtfortschritt auf den Wert gesetzt,
        /// der 100% der Verarbeitung dieses ProgressTimers entspricht.</param>
        public void Finish(bool setToHundred)
        {
            this.IsBreaked = true;
            this.FinishMe(setToHundred);
            this.OnProgressFinished();
        }

        /// <summary>
        /// Beendet für diesen ProgressTimer die Verarbeitung.
        /// </summary>
        /// <param name="setToHundred">Bei True wird der Gesamtfortschritt auf den Wert gesetzt,
        /// der 100% der Verarbeitung dieses ProgressTimers entspricht.</param>
        private void FinishMe(bool setToHundred)
        {
            // Läuft vielleicht noch ein Timer-Thread?,
            // dann beenden.
            if (!this._timerStop)
            {
                this._timerStop = true;
                Thread.Sleep(this.PercentMilliseconds);
            }
            if (setToHundred)
            {
                long allWeights = (this.LeftBranchWeight + this.RightBranchWeight - this.Weight);
                float toHundred = 100f / allWeights;
                float toThisWeight = this.Weight / 100f;
                int body = 100 - this.Head - this.Tail;
                float toThisBody = body / 100f;
                this.AllPercentDone = (int)(
                    (this.LeftBranchWeight - this.Weight) * toHundred
                    + this.Head * toThisWeight * toHundred
                    + 100 * toThisBody * toThisWeight * toHundred
                    + this.Tail * toThisWeight * toHundred
                    + 0.5);
            }
        }

        /// <summary>
        /// Beendet für die gesamte ProgressTimer-Kette die Verarbeitung.
        /// </summary>
        /// <param name="setToHundred">Bei True wird der Gesamtfortschritt auf 100% gesetzt.</param>
        public void FinishChain(bool setToHundred)
        {
            if (this._predecessor != null)
            {
                this._predecessor.FinishChain(setToHundred);
            }
            else
            {
                this.finishSuccessors(setToHundred);
            }
        }

        #endregion public members

        #region private members

        private ProgressTimer _predecessor;
        private ProgressTimer _successor;

        volatile private bool _timerStop;
        private bool _isStarting;
        private bool _isFinishing;

        private int _allPercentDone;
        // Setzt und liefert einen für alle verketteten ProgressTimer gemeinsamen
        // Prozentwert für die Gesamt-Verarbeitung von 0 bis 100.
        // Auf diesen Wert wird auch der gemeinsame Gesamtfortschritt gesetzt.
        private int AllPercentDone
        {
            get
            {
                if (this._predecessor != null)
                {
                    return this._predecessor.AllPercentDone;
                }
                else
                {
                    return this._allPercentDone;
                }
            }
            set
            {
                if (this._predecessor != null)
                {
                    this._predecessor.AllPercentDone = value;
                }
                else
                {
                    this._allPercentDone = value;
                }
            }
        }

        private int _allPercentLast;
        // Interner Vergleichswert
        private int AllPercentLast
        {
            get
            {
                if (this._predecessor != null)
                {
                    return this._predecessor.AllPercentLast;
                }
                else
                {
                    return this._allPercentLast;
                }
            }
            set
            {
                if (this._predecessor != null)
                {
                    this._predecessor.AllPercentLast = value;
                }
                else
                {
                    this._allPercentLast = value;
                }
            }
        }

        // Beendet eventuell noch folgende Nutzer der gemeinsamen Verarbeitung.
        private void finishSuccessors(bool setToHundred)
        {
            if (this._successor != null)
            {
                this.Finish(false);
                this._successor.finishSuccessors(setToHundred);
            }
            else
            {
                this.Finish(setToHundred);
            }
        }

        private void OnProgressChanged(string additionalInfo)
        {
            string info = this.ProcessName;
            if (!String.IsNullOrEmpty(additionalInfo))
            {
                info += " (" + additionalInfo + ")";
            }
            // Der auskommentierte Teil führte dazu, dass bei kleinen Teilprozessen
            // am Ende der Kette keine Ausgabe mehr erfolgte:
            // if (this._progressChanged != null && this.AllPercentDone < 100)
            if (this._progressChanged != null && (this.AllPercentDone < 100 || this._value < 100))
            {
                this._progressChanged(this, new ProgressChangedEventArgs(this.AllPercentDone, info));
            }
        }

        private void OnProgressFinished()
        {
            if (ProgressFinished != null)
            {
                if (this._successor != null && this.AllPercentDone > 99)
                {
                    // Da die aufrufende Anwendung bei einem Prozentwert von 100 davon ausgehen kann,
                    // dass die Verarbeitung aller ProzessTimer beendet ist, darf hier solange nicht
                    // 100 % übergeben werden, wie noch ein nachfolgender ProzessTimer wartet.
                    ProgressFinished(this, new ProgressChangedEventArgs(99, this.ProcessName));
                }
                else
                {
                    ProgressFinished(this, new ProgressChangedEventArgs(this.AllPercentDone, this.ProcessName));
                }
            }
        }

        private delegate void _asyncMethod();
        // Hier läuft der selbst gebastelte "Timer" in einem eigenen Thread.
        // Er steuert den Gesamtfortschritt, solange die Anwendung hängt.
        // Aus dokumentatorischen Gründen habe ich die Berechnung von
        // AllPercentDone nicht weiter zusammengefasst.
        private void invokeTimerAsync()
        {
            int timerValue = 0;
            while (!this._timerStop)
            {
                if ((this.Value == 0) && (timerValue < this.Head))
                {
                    timerValue++;
                    long allWeights = (this.LeftBranchWeight + this.RightBranchWeight - this.Weight);
                    float toHundred = 100f / allWeights;
                    float toThisWeight = this.Weight / 100f;
                    this.AllPercentDone = (int)(
                        (this.LeftBranchWeight - this.Weight) * toHundred
                        + timerValue * toThisWeight * toHundred
                        + 0.5);
                }
                else
                {
                    if ((timerValue < this.Tail) && this.Value > this.Head/* && (this.Value > 97)*/)
                    {
                        timerValue++;
                        long allWeights = (this.LeftBranchWeight + this.RightBranchWeight - this.Weight);
                        float toHundred = 100f / allWeights;
                        float toThisWeight = this.Weight / 100f;
                        int body = 100 - this.Head - this.Tail;
                        float toThisBody = body / 100f;
                        this.AllPercentDone = (int)(
                            (this.LeftBranchWeight - this.Weight) * toHundred
                            + this.Head * toThisWeight * toHundred
                            + 100 * toThisBody * toThisWeight * toHundred
                            + timerValue * toThisWeight * toHundred
                            + 0.5);
                    }
                    if ((this.Value <= this.Head)/* (this.Value <= 97)*/ || (timerValue >= this.Tail))
                    {
                        this._timerStop = true;
                    }
                }
                if (!this._timerStop)
                {
                    if (this.AllPercentDone != this.AllPercentLast)
                    {
                        this.AllPercentLast = this.AllPercentDone;
                        this.OnProgressChanged("");
                    }
                    if (this._value < 100)
                    {
                        // Der Gesamtfortschritt soll bis zum Schluss getriggert wirken,
                        // auch wenn der Timer eigentlich am Ende ist, die Anwendung aber noch hängt.
                        //this.OnProgressChanged("Timer");
                        Thread.Sleep(this.PercentMilliseconds);
                    }
                    else
                    {
                        this.FinishMe(false);
                        //this.OnProgressFinished();
                    }
                }
            } // while (!this.timerStop);
        } // InvokeTimerAsync

        #endregion private members

    }
}
