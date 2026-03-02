using UnityEngine;
using UnityEngine.InputSystem;
using RBitUtils;
using RBitUtils.ResponseTypes;
using System;

public class AdaptiveStepController : MonoBehaviour
{
	public float rate;
	public float rateLast;
	public AnimationCurve adaptiveRateCurve;
	public AnimationCurve adaptiveRateCurveLast;

	public Transform targ;

	AdaptiveStepV3 controller;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		
		ResetResponse();
	}

	private void ResetResponse()
	{
		var current = new Vector3(Mouse.current.position.value.x, Mouse.current.position.value.y, 10);
		controller = new AdaptiveStepV3(targ.position, rate, v => adaptiveRateCurve.Evaluate(v));
	}

	// Update is called once per frame
	void Update()
	{
		var current = new Vector3(Mouse.current.position.value.x, Mouse.current.position.value.y, 10);
		transform.position = controller.Update(Time.deltaTime, targ.position);
		rate.CheckChange(ref rateLast, ResetResponse);
		adaptiveRateCurve.CheckChange(ref adaptiveRateCurveLast, ResetResponse);
	}
}

