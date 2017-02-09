using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace org.flamerat.GrandWild {
    public interface ITimerBased {
        void TimeTickBehavior(Timer timer);
    }

    public class Timer {

        public delegate void OnTimeTickEvent(Timer timer);
        public event OnTimeTickEvent OnTimeTick;
        public TimeSpan CurrentTimeFromStart {
            get {
                return _CurrentTickTimeFromStart;
            }
        }
        public TimeSpan PreviousTimeFromStart {
            get {
                return _PreviousTickTimeFromStart;
            }
        }
        public TimeSpan ActualInteval {
            get {
                return _CurrentTickTimeFromStart - _PreviousTickTimeFromStart;
            }
        }
        public TimeSpan TargetInteval {
            get {
                return _TargetInteval;
            }set {
                if (value.Ticks > 0) {
                    _TargetInteval = value;
                    _Timer.Interval /*in millesecond*/ = _TargetInteval.Ticks * 1e-4F;
                }
            }
        }
        public float SpeedScale {
            get {
                return _TimeFlowSpeedScale;
            }
            set {
                if (System.Math.Abs(value) > 1e-6F) _TimeFlowSpeedScale = value;
            }
        }
        public bool CanSkipTick {
            get; private set;
        } = false;
        public bool IsRunning {
            get;private set;
        }
        public bool IsBlocking {
            get;private set;
        }
        
        //Block won't affect time from start but stop will, aka block is more like skipping events while stop will pause the whole timer completely.

        public void Start() {
            IsRunning = true;
            _PreviousTickTime = DateTime.Now;
            _Timer.Start();
        }
        public void Stop() {
            _Timer.Stop();
            IsRunning = false;
        }
        public void Block() {
            IsBlocking = true;
            _Timer.Stop();
        }
        public void UnBlock() {
            IsBlocking = false;
            _Timer.Start();
        }
        public void ResetTimeFromStart() {
            _PreviousTickTimeFromStart=new TimeSpan(0);
            _CurrentTickTimeFromStart = new TimeSpan(0);
        }

        public Timer(bool canSkipTick=false, float speedScale = 1.0F, TimeSpan? targetInteval=null){
            _Timer = new System.Timers.Timer();
            CanSkipTick = canSkipTick;
            SpeedScale = speedScale;
            if (targetInteval.HasValue) TargetInteval = targetInteval.Value;

            if (CanSkipTick) {
                _Timer.Elapsed += _Skippable;
            }else {
                _Timer.Elapsed += _Unskippable;
            }
        }

        //reason for two callbacks is for time optimization

        //If previous event hasn't complete the timer will still try to execute the new event when possible
        private void _Unskippable(object sender, System.Timers.ElapsedEventArgs e) {
            _PreviousTickTimeFromStart = _CurrentTickTimeFromStart;
            _CurrentTickTimeFromStart = _PreviousTickTimeFromStart + new TimeSpan((long)(_TimeFlowSpeedScale*(e.SignalTime - _PreviousTickTime).Ticks));
            OnTimeTick(this);
        }

        //If previous event hasn't complete the timer will be jammed and skip the new event;
        private void _Skippable(object sender, System.Timers.ElapsedEventArgs e) {
            if (!_Busy) {
                _Busy = true;
                _Unskippable(sender, e);
                _Busy = false;
            }
        }

        private System.Timers.Timer _Timer;
        private TimeSpan _PreviousTickTimeFromStart = new TimeSpan(0);
        private TimeSpan _CurrentTickTimeFromStart = new TimeSpan(0);
        private DateTime _PreviousTickTime;
        private float _TimeFlowSpeedScale = 1.0F;
        private TimeSpan _TargetInteval = new TimeSpan(0, 0, 0, 0, 50);
        private volatile bool _Busy=false;
    }
}
