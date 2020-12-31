using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MusicParameter {
	public float timePerUnit;
	public int unitPerBeat;
	public int beatPerBar;
	public int unitPerBar { get { return unitPerBeat * beatPerBar; } }
	public MusicParameter(float BPM, int unitPerBeat, int beatPerBar) {
		this.unitPerBeat = unitPerBeat;
		this.beatPerBar = beatPerBar;
		var timePerBeat = 60f / BPM;
		timePerUnit = timePerBeat / unitPerBeat;
	}
}

public struct Timing {
	public float bar;
	public float beat;
	public float unit;
	public Timing(float bar, float beat, float unit) {
		this.bar = bar;
		this.beat = beat;
		this.unit = unit;
	}
}

public class TimingUtility {
	public MusicParameter param;
	public TimingUtility(MusicParameter param) {
		this.param = param;
	}

	public float ToUnit(Timing timing) {
		return timing.bar * param.unitPerBar + timing.beat * param.unitPerBeat + timing.unit;
	}
	public float ToTime(float unit) {
		return unit * param.timePerUnit;
	}
	public float ToTime(Timing timing) {
		return ToTime(ToUnit(timing));
	}
	public float ToTime(float bar, float beat, float unit) {
		return ToTime(new Timing(bar, beat, unit));
	}
}
