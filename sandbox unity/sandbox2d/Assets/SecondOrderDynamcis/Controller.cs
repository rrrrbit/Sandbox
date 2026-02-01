using UnityEngine;
using UnityEngine.InputSystem;
using RBitUtils;
using System;

public class Controller : MonoBehaviour
{
	public float naturalFrequency, damping, response;
	public float naturalFrequencyLast, dampingLast, responseLast;

	SecondOrderDynamics.V3 controller;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		var current = new Vector3(Mouse.current.position.value.x, Mouse.current.position.value.y, 10);
		controller = new(Camera.main.ScreenToWorldPoint(current), naturalFrequency, damping, response);
    }

    // Update is called once per frame
    void Update()
    {
		var current = new Vector3(Mouse.current.position.value.x, Mouse.current.position.value.y, 10);
		transform.position = controller.Update(Time.deltaTime, Camera.main.ScreenToWorldPoint(current));
		Action reset = () => { controller = new(Camera.main.ScreenToWorldPoint(current), naturalFrequency, damping, response); };
		naturalFrequency.CheckChange(ref naturalFrequencyLast, reset);
		damping.CheckChange(ref dampingLast, reset);
		response.CheckChange(ref responseLast, reset);
	}
}

