// ============================================================================
//  WPF StopWatch / Timer Control v1.0                                       //
//                                                                           //
//  Last Updated at 12th Jan, 2013                                           //
//  Copyright (c) 2013 Attila Papai (Rokuum Labs)                            //
//  All code falls under the MIT license                                     //
// ============================================================================

using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Rokuum.WPF.Controls
{
    /// <summary>
    /// Interaction logic for StopWatch.xaml
    /// </summary>
    public partial class StopWatch : UserControl
    {
        #region Private Fields

        private DispatcherTimer timer;
        private TimeSpan startedTimeSpan;
        private TimeSpan pausedTimeSpan;
        private StopWatchState state;

        static DependencyProperty FormatProperty;
        static DependencyProperty IntervalProperty;

        #endregion

        #region Enumerations

        public enum StopWatchState
        {
            Stopped = 0,
            Started = 1,
            Paused = 2
        }

        #endregion

        #region Constructor

        public StopWatch()
        {
            InitializeComponent();                      
            state = StopWatchState.Stopped;
            timer = new DispatcherTimer();

            // this.Interval is not available here yet
            // timer.Interval = new TimeSpan(this.Interval * 10000);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Static constructor.s
        /// </summary>
        static StopWatch()
        {
            PropertyChangedCallback formatChangedCallback = new PropertyChangedCallback(FormatChanged);
            FormatProperty = DependencyProperty.Register("Format", typeof(string), typeof(StopWatch), new UIPropertyMetadata("HH:mm:ss", formatChangedCallback));
            IntervalProperty = DependencyProperty.Register("Interval", typeof(int), typeof(StopWatch), new UIPropertyMetadata(1000));
        }

        /// <summary>
        /// Raised when Format property changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        static void FormatChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            StopWatch thisStopWatch = (StopWatch)sender;
            if (thisStopWatch != null)
            {
                thisStopWatch.timerLabel.Content = new DateTime(0L).ToString(thisStopWatch.Format);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the display format of elapsed time.
        /// </summary>
        public string Format
        {
            get
            {
                return (string)base.GetValue(FormatProperty);
            }
            set
            {
                base.SetValue(FormatProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the measure of duration between each tick.
        /// </summary>
        public int Interval
        {
            get
            {
                return (int)base.GetValue(IntervalProperty);
            }
            set
            {
                base.SetValue(IntervalProperty, value);
            }
        }

        /// <summary>
        /// Gets the state of StopWatch. It can be either Stopped, Started or Paused.
        /// </summary>        
        public StopWatchState State
        {
            get { return this.state; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the StopWatch.
        /// </summary>
        /// <exception cref="InvalidOperationException">It is thrown when trying to start the StopWatch while is in Started state.</exception>
        public void Start()
        {
            if (this.state == StopWatchState.Started)
            {
                throw new InvalidOperationException("Cannot start StopWatch. It is already started.");
            }

            // If StopWatch is stopped...
            if (this.state == StopWatchState.Stopped)
            {
                timer.Tick += timer_Tick;
                startedTimeSpan = new TimeSpan(DateTime.Now.Ticks);
            }

            // Or if it is Paused...
            else if (this.state == StopWatchState.Paused)
            {
                startedTimeSpan = new TimeSpan(DateTime.Now.Ticks).Subtract(pausedTimeSpan).Add(startedTimeSpan);
                pausedTimeSpan = TimeSpan.Zero;
            }

            // Start StopWatch
            if (timer.Interval.Ticks == 0)
            {
                timer.Interval = new TimeSpan(this.Interval * 10000);
            }

            timer.Start();            
            state = StopWatchState.Started;
        }       

        /// <summary>
        /// Pauses the StopWatch.
        /// </summary>
        /// <exception cref="InvalidOperationException">It is thrown when trying to pause the StopWatch when it is not in Started state.</exception>
        public void Pause()
        {
            if (this.state != StopWatchState.Started)
            {
                throw new InvalidOperationException("Cannot pause StopWatch. It is not started.");
            }

            timer.Stop();
            state = StopWatchState.Paused;
            pausedTimeSpan = new TimeSpan(DateTime.Now.Ticks);
        }

        /// <summary>
        /// Stops the StopWatch.
        /// </summary>
        /// <exception cref="InvalidOperationException">It is thrown when trying to stop the StopWatch when it is not in Started or Paused state.</exception>
        public void Stop()
        {
            if (this.state == StopWatchState.Stopped)
            {
                throw new InvalidOperationException("Cannot stop StopWatch. It is not started or paused.");
            }

            timer.Stop();
            timer.Tick -= timer_Tick;
            timerLabel.Content = new DateTime(0L).ToString(this.Format);
            state = StopWatchState.Stopped;
        }

        public void Dispose()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= timer_Tick;
            }
        }

        #endregion

        #region Private Implementations

        private void timer_Tick(object sender, EventArgs e)
        {
            timerLabel.Content = DateTime.Now.Subtract(startedTimeSpan).ToString(this.Format);
        }

        #endregion
    }
}
