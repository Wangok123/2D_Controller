﻿using UnityEngine;
using System.Collections;

namespace EP1
{
	[RequireComponent (typeof (Controller2D))]
	public class Player : MonoBehaviour {


		Controller2D controller;

		void Start() {
			controller = GetComponent<Controller2D> ();
		}
	}

}
