using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tween;
using UnityEngine.Timeline;
using Unity.VisualScripting;
using UnityEngine;

static public class VectorExtension
{
    public static Vector2Int ToIntVector(this Vector2 self) { return new Vector2Int((int)self.x, (int)self.y); }
    public static Vector3Int ToIntVector(this Vector3 self) { return new Vector3Int((int)self.x, (int)self.y, (int)self.z); }
}

public class TypeWrapper { }

public class TypeWrapperT<T> : TypeWrapper { 
    public T x;
    public TypeWrapperT(T x) { this.x = x; }
    public static T To<U>(U x) { 
        return ((TypeWrapperT<T>)(TypeWrapper)(new TypeWrapperT<U>(x))).x;
    }
}


public class TweenUtils {
    static int _Lerp(int start, int end, float alpha) { return (int)(start + (end - start) * alpha); }
    static float _Lerp(float start, float end, float alpha) { return (start + (end - start) * alpha); }
    static double _Lerp(double start, double end, float alpha) { return (start + (end - start) * alpha); }
    static Vector2 _Lerp(Vector2 start, Vector2 end, float alpha) { return (start + (end - start) * alpha); }
    static Vector3 _Lerp(Vector3 start, Vector3 end, float alpha) { return (start + (end - start) * alpha); }
    static Vector4 _Lerp(Vector4 start, Vector4 end, float alpha) { return (start + (end - start) * alpha); }
    static Vector2Int _Lerp(Vector2Int start, Vector2Int end, float alpha) 
        { return (start + ((Vector2)(end - start) * alpha)).ToIntVector(); }
    static Vector3Int _Lerp(Vector3Int start, Vector3Int end, float alpha)
        { return (start + ((Vector3)(end - start) * alpha)).ToIntVector(); }
    static Color _Lerp(Color start, Color end, float alpha) { return (start + ((end - start) * alpha)); }

    static Type[] types = new Type[] { 
        typeof(int),
        typeof(float),
        typeof(double),
        typeof(Vector2),
        typeof(Vector3),
        typeof(Vector4),
        typeof(Vector2Int),
        typeof(Vector3Int),
        typeof(Color)};
    public static T Add<T>(T x, T y) { 
        var t = typeof(T);
        if (t == typeof(int))
            return TypeWrapperT<T>.To( TypeWrapperT<int>.To(x) + TypeWrapperT<int>.To(y));
        if (t == typeof(float))
            return TypeWrapperT<T>.To(TypeWrapperT<float>.To(x) + TypeWrapperT<float>.To(y));
        if (t == typeof(double))
            return TypeWrapperT<T>.To(TypeWrapperT<double>.To(x) + TypeWrapperT<double>.To(y));
        if (t == typeof(Vector2))
            return TypeWrapperT<T>.To(TypeWrapperT<Vector2>.To(x) + TypeWrapperT<Vector2>.To(y));
        if (t == typeof(Vector3))
            return TypeWrapperT<T>.To(TypeWrapperT<Vector3>.To(x) + TypeWrapperT<Vector3>.To(y));
        if (t == typeof(Vector4))
            return TypeWrapperT<T>.To(TypeWrapperT<Vector4>.To(x) + TypeWrapperT<Vector4>.To(y));
        if (t == typeof(Vector2Int))
            return TypeWrapperT<T>.To(TypeWrapperT<Vector2Int>.To(x) + TypeWrapperT<Vector2Int>.To(y));
        if (t == typeof(Vector3Int))
            return TypeWrapperT<T>.To(TypeWrapperT<Vector3Int>.To(x) + TypeWrapperT<Vector3Int>.To(y));
        if (t == typeof(Color))
            return TypeWrapperT<T>.To(TypeWrapperT<Color>.To(x) + TypeWrapperT<Color>.To(y));
        return default(T);
    }

    public static T Lerp<T>(T start, T end, float alpha) {
        if (typeof(T) == typeof(int))
            return TypeWrapperT<T>.To(_Lerp(TypeWrapperT<int>.To(start), TypeWrapperT<int>.To(end), alpha));
        if (typeof(T) == typeof(float))
            return TypeWrapperT<T>.To(_Lerp(TypeWrapperT<float>.To(start), TypeWrapperT<float>.To(end), alpha));
        if (typeof(T) == typeof(double))
            return TypeWrapperT<T>.To(_Lerp(TypeWrapperT<double>.To(start), TypeWrapperT<double>.To(end), alpha));
        if (typeof(T) == typeof(Vector2))
            return TypeWrapperT<T>.To(_Lerp(TypeWrapperT<Vector2>.To(start), TypeWrapperT<Vector2>.To(end), alpha));
        if (typeof(T) == typeof(Vector3))
            return TypeWrapperT<T>.To(_Lerp(TypeWrapperT<Vector3>.To(start), TypeWrapperT<Vector3>.To(end), alpha));
        if (typeof(T) == typeof(Vector4))
            return TypeWrapperT<T>.To(_Lerp(TypeWrapperT<Vector4>.To(start), TypeWrapperT<Vector4>.To(end), alpha));
        if (typeof(T) == typeof(Vector2Int))
            return TypeWrapperT<T>.To(_Lerp(TypeWrapperT<Vector2Int>.To(start), TypeWrapperT<Vector2Int>.To(end), alpha));
        if (typeof(T) == typeof(Vector3Int))
            return TypeWrapperT<T>.To(_Lerp(TypeWrapperT<Vector3Int>.To(start), TypeWrapperT<Vector3Int>.To(end), alpha));
        if (typeof(T) == typeof(Color))
            return TypeWrapperT<T>.To(_Lerp(TypeWrapperT<Color>.To(start), TypeWrapperT<Color>.To(end), alpha));
        return default(T);
    }
}
public class TweenValueNode<T> : TweenNodeBase {
    public Action<T> setter;
    public T start;
    public T end;


    public TweenValueNode(Action<T> setter, T start, T end, float time) {
        this.setter = setter;
        this.start = start;
        this.end = end;
        base.time = time;
    }

    public override void Call(float alpha) {
        setter(TweenUtils.Lerp<T>(start, end, alpha));
    }
}
public class TweenGetterNode<T> : TweenNodeBase
{
    public Action<T> setter;
    public Func<T> getter;
    public T start;
    public T end;


    public TweenGetterNode(Action<T> setter, Func<T> getter, T end, float time) {
        this.setter = setter;
        this.getter = getter;
        this.end = end;
        base.time = time;

    }

    public override void Call(float alpha) {
        if (master.lastActiveTween != this)
            start = getter();
        setter(TweenUtils.Lerp<T>(start, end, alpha));
    }
}

public class TweenOffsetNode<T> : TweenNodeBase
{
    public Action<T> setter;
    public Func<T> getter;
    public T start;
    public T end;
    public T offset;

    public TweenOffsetNode(Action<T> setter, Func<T> getter, T offset, float time) {
        this.setter = setter;
        this.getter = getter;
        this.offset = offset;
        base.time = time;

    }

    public override void Call(float alpha) {
        if (master.lastActiveTween != this) { 
            start = getter();
            end = TweenUtils.Add<T>(start, offset);
        }
        setter(TweenUtils.Lerp<T>(start, end, alpha));
    }
}

public class TweenDynamicNode<T> : TweenNodeBase
{
    public Action<T> setter;
    public Func<T> from;
    public Func<T> to;
    public T start;
    public T end;
    public T offset;

    public TweenDynamicNode(Action<T> setter, Func<T> from, Func<T> to, float time) {
        this.from = from;
        this.to = to;
        this.setter = setter;
        base.time = time;
    }

    public override void Call(float alpha) {
        var start = from();
        var end = to();
        setter(TweenUtils.Lerp<T>(start, end, alpha));
    }
}


public static class TweenDynamicNodeUtil {

    static public TweenTrack AddTweenDynamic<T>(this Tween tween,  Action<T> setter, Func<T> from, Func<T> to, float time,
        TransitionType transitionType = TransitionType.LINEAR, EaseType easeType = EaseType.IN) {
        return AddTweenDynamic<T>(tween, "__default", setter, from, to, time, transitionType, easeType);
    }
	static public TweenTrack AddTweenDynamic<T>(this Tween tween, string trackName, Action<T> setter, Func<T> from, Func<T> to, float time,
		TransitionType transitionType = TransitionType.LINEAR, EaseType easeType = EaseType.IN) {
		if (!tween.tweenDict.TryGetValue(trackName, out var track)) {
			track = new TweenTrack { name = trackName };
			tween.tweenDict.Add(trackName, track);
		}
		
		if (track._tweenState == Tween.TweenState.RUNNING) {
			Debug.LogError("Try to call AddTween while tween is running");
			return track;
		}

		var cTime = 0f;
		var tweenNodeList = track.tweenNodeList;
		if (tweenNodeList.Count > 0)
			cTime = tweenNodeList[tweenNodeList.Count - 1].time;

		var tweenNode = new TweenDynamicNode<T>(setter, from, to, time + cTime);
		tweenNode.easeType = easeType;
		tweenNode.transitionType = transitionType;
		tweenNode.master = track;
		tweenNodeList.Add(tweenNode);
		return track;
	}
}
