using UnityEngine;
using UnityEngine.InputSystem;
using RBitUtils;
using RBitUtils.ResponseTypes;
using System;

public class Controller1 : MonoBehaviour
{
	public float naturalFrequency, damping, response;
	public float naturalFrequencyLast, dampingLast, responseLast;
	public Transform targ;
	Vec3Response<Spring> controller;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		ResetResponse();
	}

	void ResetResponse()
	{
		var current = new Vector3(Mouse.current.position.value.x, Mouse.current.position.value.y, 10);
		controller = new(x0 => new Spring(x0, naturalFrequency, damping, response), targ.position);
	}

	// Update is called once per frame
	void Update()
    {
		var current = new Vector3(Mouse.current.position.value.x, Mouse.current.position.value.y, 10);
		transform.position = controller.Update(Time.deltaTime, targ.position);
		naturalFrequency.CheckChange(ref naturalFrequencyLast, ResetResponse);
		damping.CheckChange(ref dampingLast, ResetResponse);
		response.CheckChange(ref responseLast, ResetResponse);
	}
}

