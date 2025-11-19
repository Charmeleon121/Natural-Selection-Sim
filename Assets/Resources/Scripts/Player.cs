using System;
using UnityEngine;

public class Player : MonoBehaviour {
	private Keybinds input;
	private readonly float speed = 10f;

	void Awake() {
		input = new();
		input.Gameplay.Enable();
	}

	void FixedUpdate() {
		int horizontal = Convert.ToInt16(input.Gameplay.Right.IsPressed()) - Convert.ToInt16(input.Gameplay.Left.IsPressed());
		int vertical = Convert.ToInt16(input.Gameplay.Forwards.IsPressed()) - Convert.ToInt16(input.Gameplay.Backwards.IsPressed());
		
		float newX = transform.position.x + horizontal * speed * Time.fixedDeltaTime;
		float newY = transform.position.y;
		float newZ = transform.position.z + vertical * speed * Time.fixedDeltaTime;

		if (input.Gameplay.ZoomIn.IsPressed()) {
			--newY;
		}

		if (input.Gameplay.ZoomOut.IsPressed()) {
			++newY;
		}

		transform.position = new(newX, newY, newZ);
	}
}
